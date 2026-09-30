# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2026-09-30

### Added
- Material color modification (`EnemyDropVisuals`) applied to all renderers of monster-dropped items, tinting and darkening colors via RGB multipliers and additive offset while preserving original textures, displays, and surface details.
- Pulsating reddish light aura (`EnableDropAura`) dynamically centered directly inside dropped items via combined renderer bounds, with automatic dimming and deactivation when grabbed or equipped.
- Dedicated `[Visuals]` configuration section in `osmarbriones.EnemyDrops.cfg` (`EnableColorModification`, `ColorMultiplierR`, `ColorMultiplierG`, `ColorMultiplierB`, `ColorAdd`, `EnableDropAura`, `AuraIntensity`, `AuraRange`).

### Changed
- Rebalanced default drop table distributions according to in-game shop item values: moved budget grenades (`GrenadeHuman`, `GrenadeDuctTaped`) to Easy monsters, high-value tech/weapons (`OrbZeroGravity`, `DroneZeroGravity`, `BaseballBat`) to Hard monsters, and repositioned `Sword` and `UpgradePlayerEnergy` to Medium monsters.

## [2.0.3] - 2026-09-29

### Changed
- Enlarged announcement banner text on mod thumbnail ("NEW: NOW KEEP YOUR DROPS") for maximum legibility at small scale across Thunderstore and mod managers.

## [2.0.2] - 2026-09-29

### Changed
- Updated mod thumbnail with a new announcement banner highlighting the item persistence feature ("New: Now keep your drops").
- Added "What's new in v2.0" section to README.md detailing item preservation, configuration caps, and telemetry improvements.

## [2.0.1] - 2026-09-29

### Added
- Informative log message when a secured dropped item cannot be preserved due to reaching the `MaxPreservedItems` capacity limit.

### Fixed
- Restored missing contributor credits in README.md (special thanks to Rucio for REPO 0.4.0+ weapon items).
- Restored complete historical changelog entries (v1.0.0 through v1.2.0) in CHANGELOG.md.

## [2.0.0] - 2026-09-29

### Added
- Configuration `PreserveItemsBetweenLevels` allowing players to keep enemy-dropped items across levels.
- Configuration `MaxPreservedItems` (default: 10) limiting the maximum number of simultaneous enemy-dropped items preserved into the truck across levels.
- Detection logic verifying that dropped items are secured (inside the truck, in player inventory, or held) before preservation.
- Integration with `StatsManager.itemsPurchased` and `dictionaryOfDictionaries` for save game serialization of preserved items.
- Dynamic count tracking via `PreservedItemTracker` that decrements when preserved items are consumed or destroyed.

### Changed
- Replaced custom roulette item selection and spawning logic with `RepoAPI.Items` (`ItemProvider.TrySpawnWeightedItem`).
- Refactored entire codebase to comply with `REPO_MODS_METHODOLOGY.md` standards (standardized patch names, removed Hungarian notation prefixes, scoped mod internals).
- Transitioned `SemiFunc_OnSceneSwitch_Patch` to run as a Prefix so saved game data accurately reflects preserved items and cleaned drop tables before `SaveFileSave`.

## [1.2.0] - 2026-05-11
- Fixed compatibility with REPO 0.4.0+.
- Added new item drops: Leaf Blower, Semiscooter, Semiscooter Small, Staff Torque, Staff Void, Staff Zero Gravity, Walkie Talkie Box, and Revive Item.

## [1.1.0] - 2026-03-01
- Fixed bug causing items purchased from the store to have low battery.

## [1.0.0] - 2025-11-18
- Initial release. Monstes drop items upon death based on danger level.
