# EnemyDrops — Local Agent Context

This is EnemyDrops' canonical local agent context. Claude Code, Codex, Gemini,
and Antigravity use it through this file or the repository's `AGENTS.md` and
`GEMINI.md` entry points.

Before the first edit of every task, synchronize
`external/RepoKit` once and follow its synchronization gate. Read
its `VERSION.md`, `REPO_MODS_WORKSPACE.md`, and
`REPO_MODS_METHODOLOGY.md`; do not repeat that check during the same task
unless shared guidance changes. This repository remains standalone after
cloning, so do not depend on a parent workspace path for builds.

## Project Overview

EnemyDrops is a BepInEx 5.x mod for the Unity game **R.E.P.O.** that spawns loot items when enemies die. It uses HarmonyLib 2.4.1 for non-invasive patching of game code and Photon PUN 2 for multiplayer item spawning. Only the host runs the drop logic; clients see synced results automatically.

## Build and Deploy

```bash
# First clone: fetch the RepoAPI submodule (or set `git config --global submodule.recurse true` once)
git submodule update --init

# Debug or Release build — same single-file output either way
dotnet build EnemyDrops.csproj
dotnet build -c Release EnemyDrops.csproj
```

The `PostBuild` target copies `EnemyDrops.dll` to the Steam BepInEx plugins folder and the r2modman Debug profile plugins folder.

Testing is done in-game — build, launch R.E.P.O., and observe behavior. There is no automated test suite.

## Shared library: RepoAPI

`external/RepoAPI` is a git submodule ([OsmarBriones/RepoAPI](https://github.com/OsmarBriones/RepoAPI)). Its source is compiled directly into `EnemyDrops.dll` — `EnemyDrops.csproj` uses `<Compile Include>` to pull in only the module(s) this mod needs, not a project/assembly reference:

```xml
<Compile Include="external\RepoAPI\Items\ItemProvider.cs" />
<Compile Include="external\RepoAPI\Items\ItemKeysProvider.cs" />
<Compile Include="external\RepoAPI\Items\ItemName.cs" />
<Compile Include="external\RepoAPI\Items\WeightedKey.cs" />
<Compile Include="external\RepoAPI\Game\**\*.cs" />
```

There is no separate `RepoAPI.dll`, no merge step, no `ILRepack` — one build always produces exactly one `EnemyDrops.dll`. Edit the shared code directly under `external/RepoAPI/`; a normal rebuild of this mod picks up the change immediately, same as editing a local file. `RepoAPI.Items` depends on `RepoAPI.Game` (the `ItemName` enum is decorated with `[GameKey(...)]` attributes resolved there), which is why both folders are included. `Item.cs` and `ItemNames.cs` (Items/) and `ModConfig/`, `Patches/`, `Utils/`, and the root `ConfigurationController.cs` are unused here and stay excluded. See `external/RepoAPI/README.md` and the workspace-level `METHODOLOGY.md` for the full convention.

Not everything that looks shared was moved there: `ConfigurationController` and `Drops/ItemKeys.cs` stay mod-local — they're EnemyDrops-specific (difficulty-based config bindings, `MaxDropsPerLevel`, `PreserveItemsBetweenLevels`, `MaxPreservedItems`) even though similarly-named files exist in other mods. Weighted item selection, probability rolls, and item spawning are managed by `RepoAPI.Items`.

## Game Reference Files

Two environment variables point to local read-only reference material (not in the repo):

| Variable | Contents |
|---|---|
| `REPO_DECOMPILED` | ILSpy-exported C# source from `Assembly-CSharp.dll` |
| `REPO_ASSETS` | AssetRipper-extracted game assets |

Use these when you need to understand game internals (item pickup logic, `StatsManager`, `SemiFunc`, etc.) that aren't visible from the mod code alone.

## Dependencies

Game/BepInEx/Harmony references resolve via `Directory.Build.props` (`net48`, `$(RepoManaged)`, `$(RepoBepInExCore)`), which default to the Steam install path but honor a `REPO_GAME_DIR` environment variable override:
- `Assembly-CSharp.dll` and related UnityEngine DLLs — publicized via `BepInEx.AssemblyPublicizer.MSBuild`
- `0Harmony.dll` — from the game's BepInEx core folder
- `BepInEx.dll` — from the game's BepInEx install
- RepoAPI's `Items/` + `Game/` source — via the `external/RepoAPI` submodule, compiled directly in

## Architecture

The full data flow is documented in `ARCHITECTURE.md`. Here is the cross-file picture:

### Runtime Flow

```
Level Start
  └─ EnemyDirector_Start_Patch (patch on EnemyDirector.Start)
       ├─ ConfigurationController.Reload()      ← reads .cfg file
       ├─ ItemDropper.ResetForNewLevel()         ← resets dropsThisLevel counter
       └─ DroppedInstanceTracker.ClearForNewLevel()

Enemy Death
  └─ EnemyHealth_Awake_Patch (patch on EnemyHealth.Awake → subscribes to onDeath)
       └─ ItemDropper.TrySpawnForEnemy()
            ├─ checks dropsThisLevel < MaxDropsPerLevel
            ├─ EnemyDifficultyAccessor.GetDangerLevel()  ← reflection into internal field
            ├─ IsExcludedEnemy()                          ← skips Gnome, Banger, etc.
            ├─ ItemDropTables.GetWeightsFor(difficulty)  ← weighted table per difficulty
            ├─ ItemProvider.TrySpawnWeightedItem()        ← from RepoAPI.Items (weighted selection + spawn)
            └─ DroppedInstanceTracker.MarkDropped()       ← adds DroppedItemTag component

Item Name Assigned (Photon sync)
  └─ PunManager_SetItemNameLOGIC_Patch
       └─ DroppedInstanceTracker.RegisterInstance(name)  ← links GameObject to battery key

Scene Switch
  └─ SemiFunc_OnSceneSwitch_Patch (Prefix patch on SemiFunc.OnSceneSwitch)
       └─ DroppedInstanceTracker.ProcessSceneSwitch(gameOver, leaveGame)
            ├─ checks DroppedInstanceTracker.IsItemSecured() for each dropped item
            ├─ if PreserveItemsBetweenLevels && preservedCount < MaxPreservedItems:
            │    └─ preserves item into StatsManager.itemsPurchased and PreservedItemTracker
            ├─ cleans up unpersisted dropped instances from StatsManager.item + itemStatBattery
            └─ ClearForNewLevel()

Item Consumption / Removal
  └─ StatsManager_ItemRemove_Patch (Postfix on StatsManager.ItemRemove)
       └─ PreservedItemTracker.OnItemRemoved(instanceName)  ← frees up preserved slot
```

### Key Design Decisions

- **`ConditionalWeakTable` in `EnemyHealth_Awake_Patch`**: Prevents subscribing to `onDeath` multiple times per `EnemyHealth` instance across reloads.
- **`DroppedItemTag` marker component**: Used as a runtime tag on GameObjects; links spawned items with EnemyDrops.
- **Instance name registration is deferred**: The battery dictionary (`StatsManager.itemStatBattery`) is keyed by instance names like `"Item Gun Shotgun/1"` which are assigned asynchronously by `PunManager.SetItemNameLOGIC`, not at spawn time.
- **Prefix on `SemiFunc.OnSceneSwitch`**: Executes before `SemiFunc.SaveFileSave()` so preserved items are persisted to disk and unpersisted drops are cleaned up immediately.
- **Host-only guard**: Every drop and persistence decision is gated on `SemiFunc.IsMasterClientOrSingleplayer()`.

### Item Keys

All valid item key strings are defined as constants in `Drops/ItemKeys.cs`. Use `ItemKeys.<Name>` rather than raw strings anywhere items are referenced. `ItemKeys.All` is the flat array used to build config entries.

### Configuration

Generated at `BepInEx/config/osmarbriones.EnemyDrops.cfg`:
- `General`:
  - `MaxDropsPerLevel` (int, default: 200, range: 0..1000): Maximum item drops per level.
  - `PreserveItemsBetweenLevels` (bool, default: false): Enables keeping secured enemy drops across levels.
  - `MaxPreservedItems` (int, default: 10, range: 0..100): Maximum simultaneous enemy-dropped items preserved across levels.
- Per-difficulty weighted item tables (Difficulty 1, 2, 3).
Config is reloaded each level start, not just once at plugin load.
