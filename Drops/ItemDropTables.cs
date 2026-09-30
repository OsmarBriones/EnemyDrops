#nullable enable
using BepInEx.Configuration;
using BepInEx.Logging;
using EnemyDrops.Configuration;
using RepoAPI.Items;
using System;
using System.Collections.Generic;

namespace EnemyDrops.Drops;

/// <summary>
/// Default in-code drop tables and configuration loader for enemy difficulties.
/// </summary>
internal static class ItemDropTables
{
	private static readonly IReadOnlyList<WeightedKey> commonItems = new[]
	{
		new WeightedKey(ItemKeys.GrenadeStun,        3f),
		new WeightedKey(ItemKeys.GrenadeShockwave,   3f),
		new WeightedKey(ItemKeys.GrenadeExplosive,   2f),
		new WeightedKey(ItemKeys.MineStun,           3f),
		new WeightedKey(ItemKeys.MineExplosive,      2f),
		new WeightedKey(ItemKeys.MineShockwave,      3f),

		new WeightedKey(ItemKeys.DroneZeroGravity,   1f),
		new WeightedKey(ItemKeys.DroneTorque,        1f),
		new WeightedKey(ItemKeys.OrbZeroGravity,     1f),

		new WeightedKey(ItemKeys.HealthPackSmall,    3f),
		new WeightedKey(ItemKeys.DuckBucket,         1f),

		new WeightedKey(ItemKeys.RubberDuck,         1f),
		new WeightedKey(ItemKeys.MeleeFryingPan,     1f),
		new WeightedKey(ItemKeys.MeleeInflatableHammer, 1f),
		new WeightedKey(ItemKeys.MeleeSword,         1f),

		new WeightedKey(ItemKeys.ValuableTracker,    1f),
		new WeightedKey(ItemKeys.ExtractionTracker,  1f),

		new WeightedKey(ItemKeys.LeafBlower,         1f)
	};

	private static readonly IReadOnlyList<WeightedKey> mediumItems = new[]
	{
		new WeightedKey(ItemKeys.CartSmall,          1f),

		new WeightedKey(ItemKeys.GrenadeDuctTaped,   3f),
		new WeightedKey(ItemKeys.GrenadeHuman,       3f),

		new WeightedKey(ItemKeys.GunHandgun,         1f),
		new WeightedKey(ItemKeys.GunTranq,           1f),
		new WeightedKey(ItemKeys.GunStun,            1f),
		new WeightedKey(ItemKeys.GunShockwave,       1f),

		new WeightedKey(ItemKeys.HealthPackMedium,   3f),
		new WeightedKey(ItemKeys.MeleeBaseballBat,   2f),
		new WeightedKey(ItemKeys.MeleeStunBaton,     2f),

		new WeightedKey(ItemKeys.UpgradePlayerTumbleClimb, 1f),
		new WeightedKey(ItemKeys.UpgradeDeathHeadBattery,  1f),

		new WeightedKey(ItemKeys.PhaseBridge,        1f),

		new WeightedKey(ItemKeys.VehicleSemiscooterSmall, 1f),

		new WeightedKey(ItemKeys.StaffTorque,        1f),
		new WeightedKey(ItemKeys.StaffVoid,          1f),
		new WeightedKey(ItemKeys.StaffZeroGravity,   1f),

		new WeightedKey(ItemKeys.WalkieTalkieBox,    1f)
	};

	private static readonly IReadOnlyList<WeightedKey> rareItems = new[]
	{
		new WeightedKey(ItemKeys.CartMedium,         1f),
		new WeightedKey(ItemKeys.CartCannon,         1f),
		new WeightedKey(ItemKeys.CartLaser,          1f),
		new WeightedKey(ItemKeys.DroneFeather,       2f),
		new WeightedKey(ItemKeys.DroneIndestructible, 2f),
		new WeightedKey(ItemKeys.DroneBattery,       1f),

		new WeightedKey(ItemKeys.GunShotgun,         1f),
		new WeightedKey(ItemKeys.GunLaser,           1f),
		new WeightedKey(ItemKeys.HealthPackLarge,    3f),

		new WeightedKey(ItemKeys.MeleeSledgeHammer,  1f),
		new WeightedKey(ItemKeys.PowerCrystal,       0f),

		new WeightedKey(ItemKeys.UpgradePlayerHealth,       1f),
		new WeightedKey(ItemKeys.UpgradePlayerEnergy,       1f),
		new WeightedKey(ItemKeys.UpgradePlayerSprintSpeed,  1f),
		new WeightedKey(ItemKeys.UpgradePlayerGrabRange,    1f),
		new WeightedKey(ItemKeys.UpgradePlayerGrabStrength, 1f),
		new WeightedKey(ItemKeys.UpgradePlayerExtraJump,    1f),
		new WeightedKey(ItemKeys.UpgradePlayerTumbleLaunch, 1f),
		new WeightedKey(ItemKeys.UpgradePlayerTumbleWings,  1f),
		new WeightedKey(ItemKeys.UpgradePlayerCrouchRest,   1f),
		new WeightedKey(ItemKeys.UpgradeMapPlayerCount,     0f),

		new WeightedKey(ItemKeys.VehicleSemiscooter, 1f),
		new WeightedKey(ItemKeys.ReviveItem,         1f)
	};

	private static DropTableConfigMatrix? configMatrix;
	private static IReadOnlyList<WeightedKey>? fullDefaults1;
	private static IReadOnlyList<WeightedKey>? fullDefaults2;
	private static IReadOnlyList<WeightedKey>? fullDefaults3;
	private static bool fullDefaultsBuilt;

	/// <summary>
	/// Initializes the config matrix for per-item drop table weights.
	/// </summary>
	internal static void InitializeConfig(ConfigFile config)
	{
		configMatrix = new DropTableConfigMatrix(config, commonItems, mediumItems, rareItems);
	}

	/// <summary>
	/// Retrieves the weighted key list configured for the given enemy difficulty.
	/// </summary>
	internal static IReadOnlyList<WeightedKey> GetWeightsFor(EnemyParent.Difficulty difficulty)
	{
		if (configMatrix is not null)
		{
			return configMatrix.Get(difficulty);
		}

		EnsureFullDefaults();

		return difficulty switch
		{
			EnemyParent.Difficulty.Difficulty1 => fullDefaults1!,
			EnemyParent.Difficulty.Difficulty2 => fullDefaults2!,
			_ => fullDefaults3!,
		};
	}

	private static void EnsureFullDefaults()
	{
		if (fullDefaultsBuilt) return;
		fullDefaults1 = BuildFullFromDefaults(commonItems);
		fullDefaults2 = BuildFullFromDefaults(mediumItems);
		fullDefaults3 = BuildFullFromDefaults(rareItems);
		fullDefaultsBuilt = true;
	}

	private static IReadOnlyList<WeightedKey> BuildFullFromDefaults(IReadOnlyList<WeightedKey> defaultsForLevel)
	{
		var map = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < defaultsForLevel.Count; i++)
		{
			map[defaultsForLevel[i].Key] = defaultsForLevel[i].Weight;
		}

		var arr = new WeightedKey[ItemKeys.All.Length];
		for (int i = 0; i < ItemKeys.All.Length; i++)
		{
			var key = ItemKeys.All[i];
			float weight = map.TryGetValue(key, out var w) ? w : 0f;
			arr[i] = new WeightedKey(key, weight);
		}
		return arr;
	}

	internal static void LogWeights(ManualLogSource logger)
	{
		if (logger is null) throw new ArgumentNullException(nameof(logger));

		void LogLevel(string name, EnemyParent.Difficulty diff)
		{
			var list = GetWeightsFor(diff);
			int count = 0;
			for (int i = 0; i < list.Count; i++)
			{
				var w = list[i];
				if (w.Weight > 0f)
				{
					if (count == 0) logger.LogInfo($"DropTable {name}:");
					logger.LogInfo($"  {w.Key} = {w.Weight}");
					count++;
				}
			}
			if (count == 0)
			{
				logger.LogInfo($"DropTable {name}: (no non-zero entries)");
			}
		}

		LogLevel("Difficulty1", EnemyParent.Difficulty.Difficulty1);
		LogLevel("Difficulty2", EnemyParent.Difficulty.Difficulty2);
		LogLevel("Difficulty3", EnemyParent.Difficulty.Difficulty3);
	}
}
