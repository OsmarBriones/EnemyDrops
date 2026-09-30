# QA Test Battery: EnemyDrops v1.2.0
> **Target Mod:** `EnemyDrops` (v1.2.0)  
> **Repository:** `EnemyDrops/`  
> **Network Scope:** Only Host, clients don't need it.  
> **Target Game:** R.E.P.O. (v0.4.0+)  
> **Includes:** New features (Persistence & Cap) + Full Backward Compatibility / Regression Suite  

---

## 1. Automated & Integrity Verification

### Tier 1: Decoupled Logic (RepoAPI)
- [x] **1.1 - Run Weighted Selection Tests:**
  ```powershell
  dotnet test RepoAPI/RepoAPI.Test/RepoAPI.Test.csproj
  ```
  *Pass Criteria:* 19/19 tests green. Validates `WeightedKey` construction, roulette pick algorithm, null safety, and zero-weight exclusions.

### Tier 2: Compilation & Assembly Contract
- [x] **1.2 - Debug Build & Deploy:**
  ```powershell
  dotnet build -c Debug EnemyDrops/EnemyDrops.csproj
  ```
  *Pass Criteria:* 0 warnings, 0 errors. Assembly copies to Steam and r2modman Debug plugins.
- [x] **1.3 - Release Build Verification:**
  ```powershell
  dotnet build -c Release EnemyDrops/EnemyDrops.csproj
  ```
  *Pass Criteria:* 0 warnings, 0 errors.

---

## 2. Regression & Backward Compatibility Suite (Comportamiento Anterior)

*Goal: Ensure that the migration to `RepoAPI` and the full architecture refactor did not break or alter any preexisting gameplay mechanics.*

### Test Group R1: Monster Danger Level & Drop Tables
- [x] **R1.1 - Tier 1 Monsters (Easy / Elsa):**
  - **Setup:** Launch level with basic monsters.
  - **Action:** Defeat low-danger monsters (Danger Level 1).
  - **Expected Outcome:** Drops correspond strictly to items defined in `[Easy Monsters (Elsa for example)]` (e.g., lower-tier tools, common items).
- [x] **R1.2 - Tier 2 Monsters (Medium / Chef):**
  - **Setup:** Encounter medium-danger monsters (Danger Level 2).
  - **Action:** Defeat them.
  - **Expected Outcome:** Drops draw strictly from `[Med. Monsters (Chef for example)]`.
- [x] **R1.3 - Tier 3 Monsters (Hard / Robe):**
  - **Setup:** Encounter high-danger monsters (Danger Level 3).
  - **Action:** Defeat them.
  - **Expected Outcome:** Drops draw strictly from `[Hard Monsters (Robe for example)]` (e.g., powerful weapons, high-tier gadgets).

### Test Group R2: Monster Exclusions (Blacklist)
- [x] **R2.1 - Non-Combat Entities:**
  - **Action:** Destroy or trigger death on excluded monsters (e.g., `Banger`, `Gnome`, neutral/harmless props).
  - **Expected Outcome:** **Zero** items drop. No null reference exceptions or errors logged in `LogOutput.log`.

### Test Group R3: Legacy Drop Cap & Level Reset
- [x] **R3.1 - Per-Level Cap (`MaxDropsPerLevel`):**
  - **Setup:** Set `MaxDropsPerLevel = 2` in `osmarbriones.EnemyDrops.cfg`.
  - **Action:** Kill 4 consecutive monsters in the same level.
  - **Expected Outcome:** The 1st and 2nd monsters drop loot. The 3rd and 4th monsters drop **nothing**.
- [x] **R3.2 - Counter Reset on New Level:**
  - **Setup:** Following R3.1, complete or transition to the next level.
  - **Action:** Kill an enemy in the new level.
  - **Expected Outcome:** The enemy drops loot normally (the per-level counter reset to 0 upon `EnemyDirector.Start`).

### Test Group R4: Weight Matrix & Zero-Weight Filtering
- [x] **R4.1 - Zero Weight Disables Drop:**
  - **Setup:** In `osmarbriones.EnemyDrops.cfg`, set an item's weight to `0` across all three difficulty sections (e.g. `Item Gun Shotgun = 0`).
  - **Action:** Defeat multiple monsters across tiers.
  - **Expected Outcome:** That specific item is **never** chosen or dropped.
- [x] **R4.2 - High Weight Bias:**
  - **Setup:** Set an item's weight to maximum `12` (e.g., `Item Gun Handgun = 12`) and others to `1` or `0`.
  - **Action:** Defeat monsters of that tier.
  - **Expected Outcome:** The high-weighted item drops consistently, confirming roulette picking proportional weights.

### Test Group R5: Legacy Scene Transition (Persistence Disabled)
- [x] **R5.1 - Temporary Drops Cleaned by Default:**
  - **Setup:** Verify `PreserveItemsBetweenLevels = false` (default setting).
  - **Action:** Defeat monsters, pick up dropped items, and bring them into the truck. Extract to the next level.
  - **Expected Outcome:** Dropped items are purged from `StatsManager.item` and `StatsManager.itemStatBattery`. They do **not** persist into the truck next level, preserving vanilla round-by-round clean state.

### Test Group R6: Multiplayer Authority & Replication
- [x] **R6.1 - Host-Only Execution Guard:**
  - **Setup:** Join a multiplayer lobby as a client (non-host).
  - **Action:** Defeat monsters.
  - **Expected Outcome:** Client's local mod does not attempt to roll drops (`SemiFunc.IsMasterClientOrSingleplayer()` check protects execution).
- [x] **R6.2 - Network Synchronization:**
  - **Setup:** Host kills a monster with the mod active; connected vanilla clients watch.
  - **Action:** Item drops.
  - **Expected Outcome:** Clients see the item spawn simultaneously with smooth physics and can pick it up without desync.

---

## 3. New Features Suite (Persistencia & Límites v1.2.0)

*Goal: Verify the newly added level-to-level item preservation, securing criteria, and capacity caps.*

### Test Group N1: Item Securing & Extraction
*Set `PreserveItemsBetweenLevels = true` in `osmarbriones.EnemyDrops.cfg`.*

- [x] **N1.1 - Truck Room Volume Securing:**
  - **Action:** Kill a monster, pick up the dropped weapon (e.g. Handgun), carry it inside the truck and place it on the floor or a shelf (`RoomVolumeCheck.inTruck`).
  - **Action:** Extract / end the level.
  - **Expected Outcome:** In the shop or next level, the Handgun spawns in a truck item volume. Its battery charge matches the previous level.
- [x] **N1.2 - Player Inventory Securing:**
  - **Action:** Kill a monster, equip the dropped item into an inventory slot (Slot 1, 2, or 3).
  - **Action:** Extract while holding or carrying it.
  - **Expected Outcome:** In the next level, the item is restored directly into the player's hands/inventory slot.
- [x] **N1.3 - Abandoned Dungeon Drops Discarded:**
  - **Action:** Kill a monster in room 4. Leave the drop on the dungeon floor.
  - **Action:** Return to the truck without the item and extract.
  - **Expected Outcome:** The item was not secured; it is removed from `StatsManager` and **never** appears in the truck.
- [x] **N1.4 - Truck Ramp Proximity Safety Net:**
  - **Action:** Place a dropped item right on the back edge/ramp of the truck where colliders might be tight.
  - **Action:** Extract.
  - **Expected Outcome:** The fallback distance check (`<= 8m` to `TruckSafetySpawnPoint`) recognizes it as secured and preserves it.

### Test Group N2: Capacity Limit (`MaxPreservedItems`, Default 10)
- [x] **N2.1 - Cap Enforcement:**
  - **Setup:** Set `MaxPreservedItems = 2` (to test quickly) and `PreserveItemsBetweenLevels = true`.
  - **Action:** Defeat monsters and bring **3** dropped items into the truck.
  - **Action:** Extract.
  - **Expected Outcome:** Exactly 2 items are preserved into `itemsPurchased`. The 3rd item is cleaned up. Log emits: `EnemyDrops: Preserved item ... (2/2)`.
- [x] **N2.2 - Shop Purchases Independence:**
  - **Setup:** Maintain 2 preserved enemy drops in the truck.
  - **Action:** Buy 3 weapons with money at the extraction shop terminal.
  - **Expected Outcome:** Purchased items do **not** count towards the enemy drop limit. Total truck items = 5.
- [x] **N2.3 - Slot Reclamation on Consumption:**
  - **Setup:** At cap (2/2 preserved drops, including an enemy-dropped grenade or medkit).
  - **Action:** Throw the grenade or consume the medkit (`StatsManager.ItemRemove`).
  - **Action:** Defeat another monster in the level and bring its drop to the truck. Extract.
  - **Expected Outcome:** `PreservedItemTracker` decremented the consumed item; the new drop is accepted and preserved (total active preserved returns to 2/2).

### Test Group N3: State Resets & Run Lifecycles
- [x] **N3.1 - Game Over / Run Reset:**
  - **Setup:** Have preserved items active in a run.
  - **Action:** Die completely, triggering Game Over / return to main menu or new run.
  - **Expected Outcome:** `enemyDropsPreserved` is completely reset to 0 in `StatsManager_RunStartStats_Patch`. Starting a fresh run begins with clean baseline items.
- [x] **N3.2 - Live Config Hot-Reload:**
  - **Setup:** In the lobby or between rounds, edit `MaxPreservedItems` or weights in `osmarbriones.EnemyDrops.cfg` on disk.
  - **Action:** Start next level without restarting the game.
  - **Expected Outcome:** `EnemyDirector_Start_Patch` invokes `ConfigurationController.Reload()`, logging new values in `LogOutput.log`.

---

## 4. Telemetry & Log Audit Matrix

Search `%APPDATA%\r2modmanPlus-local\REPO\profiles\<Profile>\BepInEx\LogOutput.log` for these verification patterns:

| Step | Expected Log Pattern | Status |
|---|---|:---:|
| Mod Init | `[Info :EnemyDrops] EnemyDrops: Configuration initialized. MaxDropsPerLevel=200, PreserveItemsBetweenLevels=...` | [x] |
| Level Start | `[Info :EnemyDrops] EnemyDrops: Configuration reloaded.` | [x] |
| Item Spawn | `[Debug :EnemyDrops] ItemDropper: Dropped 'Item ...' for enemy '...'` | [x] |
| Name Sync | `[Debug :EnemyDrops] EnemyDrops: Registered dropped instance 'Item .../X'.` | [x] |
| Preservation | `[Info :EnemyDrops] EnemyDrops: Preserved item '...' (...) into truck/next level. Total preserved: X/Y` | [x] |
| Cleanup | `[Debug :EnemyDrops] EnemyDrops: Cleaned up X non-preserved dropped instance(s) from item + itemStatBattery` | [x] |
| Reclamation | `[Debug :EnemyDrops] EnemyDrops: Decremented preserved count for '...'` | [x] |
