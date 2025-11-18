# Enemy Drops
Make monsters drop items when they die — fully configurable and host-only.  
Native **BepInEx** mod.

Inspired by the original **REPO Enemy Drops** mod by ImVertro.  
This version is a fully native BepInEx implementation for users who prefer not to use MelonLoader, with additional usability improvements.

## Features
- Monsters drop items upon death.
- Configure the maximum number of drops per level.
- Configure drop chances for every item based on monster difficulty.
- Configuration changes apply when loading a level.
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
The **MaxDropsPerLevel** value defines the maximum number of drops that can spawn per level.  
For example:

`MaxDropsPerLevel = 200`

### Difficulty-Based Drop Tables
The `.cfg` file includes three sections:

```
[Easy Monsters (Elsa for example)]
[Med. Monsters (Chef for example)]
[Hard Monsters (Robe for example)]
```

Each section contains entries like:

```
## Weight for "Item Cart Cannon" on medium monsters. (range 0..12)
# Setting type: Int32
# Default value: 0
# Acceptable value range: From 0 to 12
Item Cart Cannon = 0
```

### How weights work
- A value of **0** means the item **will not drop** for that monster difficulty.
- Higher numbers increase the **probability** of the item being selected when a monster dies.
- You have full control over which items appear for each difficulty tier.

## Support
If something doesn’t work, feel free to send an email to **osmarbriones@outlook.com** and I’ll do my best to fix it.

## Credits
Based on the idea from  
[REPO Enemy Drops by ImVertro](https://thunderstore.io/c/repo/p/ImVertro/REPO_Enemy_Drops/)

Developed by **Osmar Briones**
