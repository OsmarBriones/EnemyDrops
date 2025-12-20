# EnemyDrops Architecture

This document explains how the EnemyDrops mod is structured and how the main pieces interact at runtime.

---

## High-Level Concept

EnemyDrops adds extra loot to the game by:

1. Listening to enemy deaths.
2. Deciding **whether** something should drop (based on difficulty and configuration).
3. Picking **what** should drop from weighted tables.
4. Spawning the item in the world.
5. Tracking which spawned items are *level drops* so their battery state can be managed safely across scenes.

Only the **host** runs this logic; clients just see what the host spawns and syncs.

---

## Main Data Flow

### 1. Level Start

Entry point: `ReloadDropTablesOnLevelStart` (Harmony patch on `EnemyDirector.Start`)

- Checks `SemiFunc.RunIsLevel()` to ensure we are in a playable level.
- Reloads configuration via:
  - `ConfigurationController.Reload(EnemyDrops.Logger)`
- Resets per-level state:
  - `ItemDropper.ResetForNewLevel()`  
    Resets the per-level drop counter (`s_dropsThisLevel`).
  - `DroppedInstanceTracker.ClearForNewLevel()`  
    Clears the set of tracked dropped instance names from the previous level.
- Optionally logs the current `itemStatBattery` contents for debugging:
  - `LogBatteryTable()`

**Goal:** Start each level with a clean, predictable state and up-to-date config.

---

### 2. Enemy Death -> Drop Trigger

Entry point: `EnemyDeathPatch`

- Harmony-patches `EnemyHealth.Awake`:
  - Subscribes once to `EnemyHealth.onDeath` using a `ConditionalWeakTable` to avoid multiple subscriptions per instance.
- When an enemy dies:
  - `OnEnemyDeath(EnemyHealth health)` is called.
  - It checks `SemiFunc.IsMasterClientOrSingleplayer()`; if false, it does nothing (host-only behavior).
  - It looks up the `Enemy` component on the same GameObject.
  - Delegates to:
    - `ItemDropper.TrySpawnForEnemy(enemy, out spawned)`.

**Goal:** Central, reliable hook whenever an enemy actually dies, without duplicating listeners or requiring code changes in the base game.

---

### 3. Choosing and Spawning a Drop

Entry point: `ItemDropper.TrySpawnForEnemy`

Steps:

1. **Cap per-level drops**
   - Maintains `s_dropsThisLevel`.
   - If `s_dropsThisLevel >= MaxDropsPerLevel` (from config), it bails out early.

2. **Find spawn position and rotation**
   - Uses (in priority order):
     - `enemy.CustomValuableSpawnTransform`
     - `enemy.CenterTransform`
     - `enemy.transform`
   - Applies a small upward offset to avoid intersecting the ground.

3. **Determine enemy difficulty**
   - Uses `EnemyDifficultyAccessor.GetDangerLevel(enemy)` to map to an integer difficulty (1–3).
   - Logs the enemy name and danger level for debugging.

4. **Apply exclusions**
   - Some enemies are excluded from drops entirely (e.g., `"Gnome"`, `"Banger"`).
   - `IsExcludedEnemy` filters them by name.

5. **Pick an item key**
   - Selects the correct weighted table via:
     - `ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty1/2/3)`
   - Calls `PickWeightedKey`:
     - Sums weights for all items.
     - Rolls a random number in `[0, totalWeight]`.
     - Walks the list cumulatively to pick a single item key (e.g., `"Item Gun Shotgun"`).

6. **Spawn the item**
   - Delegates actual spawning to:
     - `ItemProvider.TrySpawnByKey(key, pos, rot, out spawned, 0f)`
   - If spawn succeeds:
     - Increments `s_dropsThisLevel`.
     - Tags the GameObject as a level drop:
       - `DroppedInstanceTracker.MarkDropped(spawned)`  
         Adds the `DroppedItemTag` component.

**Goal:** A single, deterministic place to decide “should this enemy drop something, and which item should that be?”

---

### 4. Tagging and Tracking Dropped Instances

#### Marker Component: `DroppedItemTag`

- A simple `MonoBehaviour` attached to GameObjects that came from EnemyDrops.
- Used purely as a runtime tag (`go.AddComponent<DroppedItemTag>()`).

#### Tracker: `DroppedInstanceTracker`

Responsibilities:

- **Mark dropped objects at spawn**
  - `MarkDropped(GameObject go)`:
    - Adds `DroppedItemTag` if it’s not already present.

- **Record final instance names**
  - The base game assigns a unique per-instance name later (e.g., `"Item Gun Shotgun/3"`).
  - Once that name exists, we call:
    - `RegisterInstance(string instanceName)`:
      - Stores the string in a `HashSet<string>`.
      - Logs a debug message when a new name is registered.

- **Check if an instance was a level drop**
  - `IsDropped(string instanceName)`:
    - Used by cleanup logic to decide which battery entries belong to level drops.

- **Reset per-level tracker**
  - `ClearForNewLevel()`:
    - Called on level start and after cleanup to avoid stale names leaking between levels.

**Goal:** Maintain a lightweight, per-level set of “these instance names are from our enemy drops.”

---

### 5. Capturing the Instance Name

Entry point: `PunManager_SetItemNameLOGIC_Patch`

- Harmony-patches `PunManager.SetItemNameLOGIC`.
- This method is where the **game** assigns the final instance name to `ItemAttributes` and notifies the network.
- In the postfix:
  - It resolves the `ItemAttributes` instance (`_itemAttributes` or via `PhotonView` in multiplayer).
  - Checks if the `ItemAttributes` GameObject has `DroppedItemTag`.
  - If yes:
    - Calls `DroppedInstanceTracker.RegisterInstance(name)`  
      where `name` is the full instance name: e.g., `"Item Gun Shotgun/1"`.

**Why this step matters:**

- Until `SetItemNameLOGIC` runs, we only have the prefab and no unique instance name.
- The battery dictionary (`StatsManager.itemStatBattery`) is keyed by this instance name, not by the base item name alone.
- Capturing it here gives us an exact link from “this spawned object” -> “this battery dictionary entry”.

---

### 6. Cleaning Up Batteries on Scene Switch

Entry point: `ClearDroppedBatteriesOnSceneSwitch` (patch on `SemiFunc.OnSceneSwitch`)

- Called whenever the game transitions scenes (e.g., end of level).
- In the postfix:
  - `DroppedInstanceTracker.ClearBatteriesForDroppedInstances()` is executed.

What `ClearBatteriesForDroppedInstances` does:

1. Fetches `StatsManager.instance` and its `itemStatBattery` dictionary.
2. Snapshots the current keys into a list (to avoid modifying while iterating).
3. For each key:
   - If `IsDropped(key)` returns `true`:
     - Adjusts the battery value for that instance (implementation choice):
       - Either **remove** the key, or
       - **reset** it to a default (e.g., `100`) while keeping the entry.
4. Logs which instance names were adjusted.
5. Calls `ClearForNewLevel()` to reset the tracker.

**Goal:** Ensure level-dropped instances do not persist stale battery values across scene transitions, while leaving owned/persisted items alone.

---

## Battery Model (Vanilla Game vs. Mod)

### Vanilla Behavior

- `StatsManager.itemStatBattery` is a `Dictionary<string, int>` keyed by:
  - Base item names: `"Item Gun Handgun"`, `"Item Power Crystal"`, etc. (usually seeded to 0).
  - Per-instance names: `"Item Gun Handgun/1"`, `"Item Gun Handgun/2"`, etc.
- When a new physical item is created via `ItemAdd`:
  - The game generates a unique name like `"Item X/3"`.
  - Internally, `PunManager.AddingItem` seeds `itemStatBattery[instanceName]`, typically to `100`.
- When an item is destroyed via `ItemRemove(instanceName)`:
  - It removes the entry from both `item` and `itemStatBattery`.
- Some entries (especially the base, unsuffixed keys) often stay at `0` and are not active battery values; active batteries are per-instance.

### EnemyDrops’ Interaction

- Does **not** change how batteries are created or drained during gameplay.
- Only touches `itemStatBattery` at **scene switches**, and only for:
  - Instance names that were tracked as EnemyDrops’ level drops.
- Owned or otherwise persisted items (that are not tagged as level drops) retain whatever battery value the base game gave them.

---

## Configuration and Item Keys

### Config File

- `osmarbriones.EnemyDrops.cfg` in `BepInEx/config`.
- Contains:
  - `MaxDropsPerLevel` (global per-level cap).
  - Weighted tables per monster difficulty.

### Item Keys

- Defined in `Providers/ItemKeys.cs` for safety and discoverability:
  - e.g., `ItemKeys.GunShotgun = "Item Gun Shotgun"`.
- `ItemKeys.All` is a flat array of all base keys.
- Used by `ItemDropTables`, `ItemDropper`, and config parsing so you don’t rely on magic strings scattered around.

---

## Build and Deployment

- Target framework: `.NET Standard 2.1`.
- Language version: C# 12.
- `EnemyDrops.csproj` defines a `PostBuild` target that copies the built `EnemyDrops.dll` to:
  - The game’s `BepInEx\plugins` folder.
  - The `r2modman` Debug profile’s `BepInEx\plugins` folder.

This lets you hit Build and immediately test in both the vanilla BepInEx setup and your r2modman profile without manual copying.

---

## Summary

- **EnemyDeathPatch + ItemDropper**: Decide *when* and *what* to drop.
- **DroppedItemTag + DroppedInstanceTracker**: Mark spawned items as “ours” and track their instance names.
- **PunManager_SetItemNameLOGIC_Patch**: Bridges from spawned GameObject to its unique battery key.
- **ClearDroppedBatteriesOnSceneSwitch**: Cleans up battery state only for tracked, level-dropped items when changing scenes.
- **Config + ItemKeys**: Provide a clean way to define and reason about what can drop and with what weight.