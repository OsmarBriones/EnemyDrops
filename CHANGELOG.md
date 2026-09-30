# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-09-29

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
