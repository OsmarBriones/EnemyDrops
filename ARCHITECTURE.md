# EnemyDrops Architecture

This document explains how the EnemyDrops mod is structured and how the main pieces interact at runtime following the workspace methodology.

---

## High-Level Concept

EnemyDrops adds extra loot to the game by:

1. Listening to enemy deaths.
2. Deciding **whether** something should drop (based on difficulty, per-level drop cap, and configuration).
3. Delegating weighted selection and spawning to `RepoAPI`.
4. Tracking which spawned items are *level drops* via `DroppedItemTag` and `DroppedInstanceTracker`.
5. On scene switch:
   - If item preservation is enabled (`PreserveItemsBetweenLevels`), checking whether enemy-dropped items were successfully secured (brought to the truck or kept in player inventory) and preserving them into the player's persistent truck inventory (`StatsManager.itemsPurchased`) up to the configured limit (`MaxPreservedItems`, default 10).
   - Cleaning up non-preserved or abandoned dropped instances from `StatsManager.item` and `StatsManager.itemStatBattery` before the game saves.

Only the **host** runs drop selection and persistence logic; clients receive synchronized items through Photon and vanilla game network events.

---

## Project Structure

```
EnemyDrops/
├── Configuration/
│   ├── ConfigurationController.cs          # Centralized config initialization, reload, and accessors
│   └── DropTableConfigMatrix.cs            # Matrix binding item weights per monster difficulty
├── Drops/
│   ├── ItemDropper.cs                      # Difficulty checks, exclusions, and spawn delegation to RepoAPI
│   ├── ItemDropTables.cs                   # Default drop tables per difficulty level
│   └── ItemKeys.cs                         # Strongly-typed string constants for game item keys
├── Patches/
│   ├── EnemyDirector_Start_Patch.cs        # Resets per-level state & reloads config
│   ├── EnemyHealth_Awake_Patch.cs          # Subscribes to enemy death events
│   ├── PunManager_SetItemNameLOGIC_Patch.cs # Captures runtime instance names
│   ├── SemiFunc_OnSceneSwitch_Patch.cs     # Preserves secured items & cleans unpersisted drops before save
│   ├── StatsManager_ItemRemove_Patch.cs    # Decrements preserved drops count on item consumption/removal
│   └── StatsManager_RunStartStats_Patch.cs # Resets preserved drops tracker on new run
├── Reflection/
│   └── EnemyDifficultyAccessor.cs          # Reflection helper to safely read enemy danger levels
├── Tracking/
│   ├── DroppedInstanceTracker.cs           # Tracks level drops, validates secured status, handles scene switches
│   ├── DroppedItemTag.cs                   # Marker component attached to mod-spawned items
│   └── PreservedItemTracker.cs             # Manages active preserved drop counts via StatsManager
├── EnemyDropsPlugin.cs                     # BepInEx plugin entry point
└── EnemyDrops.csproj
```

---

## Main Data Flow

### 1. Level Start

Entry point: `EnemyDirector_Start_Patch` (Harmony patch on `EnemyDirector.Start`)

- Checks `SemiFunc.RunIsLevel()` to ensure we are in a playable level.
- Reloads configuration via:
  - `ConfigurationController.Reload(EnemyDropsPlugin.Logger)`
- Resets per-level state:
  - `ItemDropper.ResetForNewLevel()`
    Resets the per-level drop counter (`dropsThisLevel`).
  - `DroppedInstanceTracker.ClearForNewLevel()`
    Clears the set of tracked dropped instance names from the previous level.

**Goal:** Start each level with a clean, predictable state and up-to-date config.

---

### 2. Enemy Death -> Drop Trigger

Entry point: `EnemyHealth_Awake_Patch`

- Harmony-patches `EnemyHealth.Awake`:
  - Subscribes once to `EnemyHealth.onDeath` using a `ConditionalWeakTable` to avoid multiple subscriptions per instance.
- When an enemy dies:
  - `OnEnemyDeath(EnemyHealth health)` is called.
  - Checks `SemiFunc.IsMasterClientOrSingleplayer()`; if false, does nothing (host-only behavior).
  - Looks up the `Enemy` component on the same GameObject.
  - Delegates to `ItemDropper.TrySpawnForEnemy(enemy, out spawned)`.

**Goal:** Central, reliable hook whenever an enemy dies, without duplicating listeners or modifying base game assemblies.

---

### 3. Choosing and Spawning a Drop

Entry point: `ItemDropper.TrySpawnForEnemy`

Steps:

1. **Cap per-level drops**
   - Checks `dropsThisLevel < MaxDropsPerLevel`.
2. **Find spawn position and rotation**
   - Uses `enemy.CustomValuableSpawnTransform`, `enemy.CenterTransform`, or `enemy.transform` with a ground-offset.
3. **Delegate to RepoAPI**
   - Calls `ItemProvider.TrySpawnWeightedItem(table, pos, rot, out spawned, dropChance: 0f)`.
4. **Mark Spawned Object**
   - Attaches `DroppedItemTag` via `DroppedInstanceTracker.MarkDropped(spawned)`.

---

### 4. Tracking and Capturing Instance Names

Entry point: `PunManager_SetItemNameLOGIC_Patch`

- When `PunManager.SetItemNameLOGIC` assigns the runtime unique instance name (e.g. `"Item Gun Shotgun/1"`):
  - Checks if the target GameObject has `DroppedItemTag`.
  - If yes, registers the instance in `DroppedInstanceTracker.RegisterInstance(name)`.

---

### 5. Scene Transitions and Item Preservation

Entry points:
- `SemiFunc_OnSceneSwitch_Patch` (Prefix on `SemiFunc.OnSceneSwitch`)
- `DroppedInstanceTracker.ProcessSceneSwitch(bool gameOver, bool leaveGame)`

When transitioning between levels:
1. **Game Over or Leaving Game**:
   - Resets preserved counts via `PreservedItemTracker.Reset()`.
   - Clears in-memory level tracking via `ClearForNewLevel()`.
2. **Normal Scene Switch (Host / Singleplayer)**:
   - Evaluates all `DroppedItemTag` instances in the scene with `DroppedInstanceTracker.IsItemSecured(itemAttr)`:
     - In truck room volume (`RoomVolumeCheck.inTruck`)
     - Equipped or held by player (`ItemEquippable.IsEquipped()`, `PhysGrabObject.grabbed`, etc.)
     - In player inventory slots (`StatsManager.playerInventorySpot1/2/3`)
     - Within truck proximity fallback (`TruckSafetySpawnPoint`, distance <= 8m)
   - If `PreserveItemsBetweenLevels` is enabled and `PreservedItemTracker.GetTotalPreservedCount() < MaxPreservedItems`:
     - Adds secured items to `StatsManager.itemsPurchased` and `itemsPurchasedTotal`.
     - Tracks preserved count in `PreservedItemTracker`.
     - Preserves the item's identity in `StatsManager.item` and battery state in `StatsManager.itemStatBattery`.
   - Any non-preserved or abandoned dropped items are removed from `StatsManager.item` and `StatsManager.itemStatBattery` so stale data is not saved.
   - Because this runs as a **Prefix**, the subsequent call to `SemiFunc.SaveFileSave()` writes the exact updated state to disk.
   - Clears `DroppedInstanceTracker` for the next level. In subsequent levels, preserved items spawn cleanly via `TruckPopulateItemVolumes` like standard owned items.

---

### 6. Dynamic Preserved Count Tracking

Entry points:
- `StatsManager_ItemRemove_Patch`: Calls `PreservedItemTracker.OnItemRemoved(instanceName)` when an item is consumed or destroyed, freeing up a preserved item slot.
- `StatsManager_RunStartStats_Patch`: Resets the preserved count when starting a new run.

---

## Configuration

- `osmarbriones.EnemyDrops.cfg` in `BepInEx/config`:
  - `General`:
    - `MaxDropsPerLevel` (int, default: 200, range: 0..1000): Maximum item drops per level.
    - `PreserveItemsBetweenLevels` (bool, default: false): Enables keeping secured enemy drops across levels.
    - `MaxPreservedItems` (int, default: 10, range: 0..100): Maximum simultaneous enemy-dropped items preserved across levels.
  - Difficulty Drop Tables:
    - Weighted entries per monster difficulty (Difficulty 1, 2, 3).

---

## Build and Deployment

- Target framework: `net48`.
- Language version: C# 12.
- Publicized assembly references via `BepInEx.AssemblyPublicizer.MSBuild`.
- Automatic post-build deployment to Steam and r2modman Debug directories.