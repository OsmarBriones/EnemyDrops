# Enemy Drops
Make monsters drop items when they die — fully configurable and host-only.  
Only Host, clients don't need it. Native **BepInEx** mod.

Inspired by the original **REPO Enemy Drops** mod by ImVertro.  
This version is a fully native BepInEx implementation for users who prefer not to use MelonLoader, with additional usability improvements.

## What's new in v2.0
- **Keep Enemy Drops Between Levels:** You can now bring dropped items into the truck or keep them in your inventory to preserve them across levels and save them with your run!
- **Configurable Preservation Cap:** New `MaxPreservedItems` setting (default: 10) to balance your run progression.
- **Dynamic Slot Recycling:** Consuming or destroying a preserved item frees up space for new enemy drops.
- **Informative Telemetry:** Clear console logs alert you when an item is secured or when the capacity limit is reached.
- **Engine Modernization:** Complete rewrite powered by `RepoAPI` for flawless host-only replication and performance.

## Features
- Monsters drop items upon death based on their danger level.
- Configure drop chances and weights for every item per monster difficulty tier.
- Set a maximum limit on item drops per level.
- **Visual Distinction (Color Modification)**: Items dropped by enemies have their material colors subtly darkened and tinted, clearly differentiating them from normal dungeon loot while keeping all original surface textures, screen displays, labels, and details intact.
- **Centered Light Aura**: A soft, pulsating reddish point light aura centered directly inside enemy-dropped items that automatically dims or switches off when held or equipped.
- **Item Preservation**: Option to keep items secured in the truck or inventory across subsequent levels.
- **Preservation Limit**: Configurable cap on the maximum number of enemy-dropped items that can be preserved simultaneously (default: 10).
- Only the host needs to have the mod installed — clients do not.
- 100% native **BepInEx** implementation.

## Requirements
- [BepInEx Pack for R.E.P.O.](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/)

## Installation
1. Install the latest [BepInEx Pack](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/).
2. Place `EnemyDrops.dll` into your `BepInEx/plugins` folder.
3. Launch the game once — the configuration file will be generated automatically inside `BepInEx/config`.

## Configuration
All settings are controlled through the generated file: `osmarbriones.EnemyDrops.cfg`
located in `BepInEx/config`.

### General Settings
- **`MaxDropsPerLevel`** (Default: `200`): Maximum number of items that can drop in a single level.
- **`PreserveItemsBetweenLevels`** (Default: `false`): When enabled (`true`), enemy-dropped items that players carry to safety inside the truck or keep in their inventory will persist into future levels and be saved with your run.
- **`MaxPreservedItems`** (Default: `10`): Maximum number of enemy-dropped items that can be preserved simultaneously. Standard shop-purchased items do not count toward this limit.

### Visual Settings
- **`EnableColorModification`** (Default: `true`): Enables custom color tinting and multipliers on enemy-dropped items while preserving original textures and displays.
- **`ColorMultiplierR`** (Default: `0.55`): Red multiplier applied to the material colors (0.0 to 2.0).
- **`ColorMultiplierG`** (Default: `0.55`): Green multiplier applied to the material colors (0.0 to 2.0).
- **`ColorMultiplierB`** (Default: `0.55`): Blue multiplier applied to the material colors (0.0 to 2.0).
- **`ColorAdd`** (Default: `0.05`): Additive offset applied to the material colors (-1.0 to 1.0).
- **`EnableDropAura`** (Default: `true`): Enables a soft, pulsating reddish light aura centered inside dropped items.
- **`AuraIntensity`** (Default: `0.8`): Controls the brightness and intensity of the reddish drop aura.
- **`AuraRange`** (Default: `3.5`): Controls how far the light aura reaches in meters.

### Difficulty-Based Drop Tables
The `.cfg` file includes three difficulty sections:

```
[Easy Monsters (Elsa for example)]
[Med. Monsters (Chef for example)]
[Hard Monsters (Robe for example)]
```

Each section contains entries like:

```
## Weight for "Item Gun Shotgun" on medium monsters. (range 0..12)
# Setting type: Int32
# Default value: 2
# Acceptable value range: From 0 to 12
Item Gun Shotgun = 2
```

### How weights work
- A value of **0** means the item **will not drop** for that monster difficulty.
- Higher numbers increase the **probability** of the item being selected when a monster dies.
- You have full control over which items appear for each difficulty tier.

## Issues & Bug Reports
Please report any bugs or suggest features on GitHub:  
https://github.com/OsmarBriones/EnemyDrops/issues

## Credits
- Based on the original concept from [REPO Enemy Drops by ImVertro](https://thunderstore.io/c/repo/p/ImVertro/REPO_Enemy_Drops/).
- Developed by **Osmar Briones**.
- Special thanks to **Rucio** for the REPO 0.4.0+ compatibility fix and weapon item additions.
