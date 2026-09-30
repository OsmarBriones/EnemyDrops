# Enemy Drops
Make monsters drop items when they die — fully configurable and host-only.  
Only Host, clients don't need it. Native **BepInEx** mod.

Inspired by the original **REPO Enemy Drops** mod by ImVertro.  
This version is a fully native BepInEx implementation for users who prefer not to use MelonLoader, with additional usability improvements.

## Features
- Monsters drop items upon death based on their danger level.
- Configure drop chances and weights for every item per monster difficulty tier.
- Set a maximum limit on item drops per level.
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
Based on the idea from [REPO Enemy Drops by ImVertro](https://thunderstore.io/c/repo/p/ImVertro/REPO_Enemy_Drops/)  
Developed by **Osmar Briones**
