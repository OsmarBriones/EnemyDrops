using BepInEx.Configuration;
using BepInEx.Logging;
using EnemyDrops.Configuration;
using EnemyDrops.Providers;
using System;
using System.Collections.Generic;

namespace EnemyDrops
{
	// Weighted entry for item keys
	public sealed class WeightedKey
	{
		public string Key { get; }
		public float Weight { get; }

		public WeightedKey(string key, float weight)
		{
			Key = key;
			Weight = weight;
		}
	}

	// Default, in-code drop tables. Users can override via config.
	public static class ItemDropTables
	{
		private static readonly IReadOnlyList<WeightedKey> s_commonItems = new[]
		{
			new WeightedKey(ItemKeys.GrenadeStun,        3f),
			new WeightedKey(ItemKeys.GrenadeShockwave,   3f),
			new WeightedKey(ItemKeys.GrenadeExplosive,   2f),
			new WeightedKey(ItemKeys.MineStun,           3f),
			new WeightedKey(ItemKeys.MineExplosive,      2f),
			new WeightedKey(ItemKeys.MineShockwave,      3f),

			new WeightedKey(ItemKeys.DroneTorque,        1f),

			new WeightedKey(ItemKeys.HealthPackSmall,    3f),
			new WeightedKey(ItemKeys.DuckBucket,         3f),
			new WeightedKey(ItemKeys.RubberDuck,         1f),

			new WeightedKey(ItemKeys.MeleeFryingPan,         1f),
			new WeightedKey(ItemKeys.MeleeInflatableHammer,  1f)
		};

		private static readonly IReadOnlyList<WeightedKey> s_mediumItems = new[]
		{
			new WeightedKey(ItemKeys.CartSmall,          1f),

			new WeightedKey(ItemKeys.DroneZeroGravity,   3f),

			new WeightedKey(ItemKeys.GrenadeDuctTaped,   1f),
			new WeightedKey(ItemKeys.GrenadeHuman,       2f),

			new WeightedKey(ItemKeys.GunHandgun,         1f),
			new WeightedKey(ItemKeys.GunTranq,           1f),
			new WeightedKey(ItemKeys.GunStun,            1f),
			new WeightedKey(ItemKeys.GunShockwave,       1f),

			new WeightedKey(ItemKeys.HealthPackMedium,   3f),
			new WeightedKey(ItemKeys.MeleeBaseballBat,   2f),
			new WeightedKey(ItemKeys.MeleeStunBaton,     2f),
			new WeightedKey(ItemKeys.MeleeSword,             1f),

			new WeightedKey(ItemKeys.OrbZeroGravity,     3f),

			new WeightedKey(ItemKeys.UpgradePlayerTumbleClimb,  1f),
			new WeightedKey(ItemKeys.UpgradeDeathHeadBattery,   1f),
		};

		private static readonly IReadOnlyList<WeightedKey> s_rareItems = new[]
		{
			new WeightedKey(ItemKeys.CartMedium,         1f),
			new WeightedKey(ItemKeys.CartCannon,         1f),
			new WeightedKey(ItemKeys.CartLaser,          1f),
			new WeightedKey(ItemKeys.DroneFeather,       2f),
			new WeightedKey(ItemKeys.DroneIndestructible,2f),
			new WeightedKey(ItemKeys.DroneBattery,       1f),

			new WeightedKey(ItemKeys.ExtractionTracker,  1f),
			new WeightedKey(ItemKeys.ValuableTracker,    1f),

			new WeightedKey(ItemKeys.GunShotgun,         1f),
			new WeightedKey(ItemKeys.GunLaser,           1f),
			new WeightedKey(ItemKeys.HealthPackLarge,    3f),

			new WeightedKey(ItemKeys.MeleeSledgeHammer,  1f),
			new WeightedKey(ItemKeys.PowerCrystal,       0f),
			new WeightedKey(ItemKeys.PhaseBridge,        1f),

			new WeightedKey(ItemKeys.UpgradePlayerHealth,        3f),
			new WeightedKey(ItemKeys.UpgradePlayerEnergy,        3f),
			new WeightedKey(ItemKeys.UpgradePlayerSprintSpeed,   3f),
			new WeightedKey(ItemKeys.UpgradePlayerGrabRange,     3f),
			new WeightedKey(ItemKeys.UpgradePlayerGrabStrength,  3f),
			new WeightedKey(ItemKeys.UpgradePlayerExtraJump,     3f),
			new WeightedKey(ItemKeys.UpgradePlayerTumbleLaunch,  3f),
			new WeightedKey(ItemKeys.UpgradePlayerTumbleWings,   3f),
			new WeightedKey(ItemKeys.UpgradePlayerCrouchRest,    3f),
			new WeightedKey(ItemKeys.UpgradeMapPlayerCount,      0f),
		};

		// Nullable: assigned in InitializeConfig()
		private static DropTableConfigMatrix? s_configMatrix;

		// Nullable: assigned in EnsureFullDefaults()
		private static IReadOnlyList<WeightedKey>? s_fullDefaults1;
		private static IReadOnlyList<WeightedKey>? s_fullDefaults2;
		private static IReadOnlyList<WeightedKey>? s_fullDefaults3;
		private static bool s_fullDefaultsBuilt;

		/// Call once from your plugin (e.g., in Awake) to create per-item-per-difficulty config entries and load them.
		public static void InitializeConfig(ConfigFile config, string? header = null, bool saveImmediately = false)
		{
			// Build config entries with defaults (missing items default to 0)
			s_configMatrix = new DropTableConfigMatrix(config, s_commonItems, s_mediumItems, s_rareItems);
		}

		public static IReadOnlyList<WeightedKey> GetWeightsFor(EnemyParent.Difficulty difficulty)
		{
			// Use configured matrix if available
			if (s_configMatrix is not null)
				return s_configMatrix.Get(difficulty);

			// Otherwise, return full defaults (all items present, missing => weight 0)
			EnsureFullDefaults();

			var d1 = s_fullDefaults1!;
			var d2 = s_fullDefaults2!;
			var d3 = s_fullDefaults3!;

			return difficulty switch
			{
				EnemyParent.Difficulty.Difficulty1 => d1,
				EnemyParent.Difficulty.Difficulty2 => d2,
				_ => d3,
			};
		}

		private static void EnsureFullDefaults()
		{
			if (s_fullDefaultsBuilt) return;
			s_fullDefaults1 = BuildFullFromDefaults(s_commonItems);
			s_fullDefaults2 = BuildFullFromDefaults(s_mediumItems);
			s_fullDefaults3 = BuildFullFromDefaults(s_rareItems);
			s_fullDefaultsBuilt = true;
		}

		private static IReadOnlyList<WeightedKey> BuildFullFromDefaults(IReadOnlyList<WeightedKey> defaultsForLevel)
		{
			var map = new System.Collections.Generic.Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < defaultsForLevel.Count; i++)
				map[defaultsForLevel[i].Key] = defaultsForLevel[i].Weight;

			var arr = new WeightedKey[Providers.ItemKeys.All.Length];
			for (int i = 0; i < Providers.ItemKeys.All.Length; i++)
			{
				var key = Providers.ItemKeys.All[i];
				float weight = map.TryGetValue(key, out var w) ? w : 0f;
				arr[i] = new WeightedKey(key, weight);
			}
			return arr;
		}

		public static void LogWeights(ManualLogSource logger)
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
					logger.LogInfo($"DropTable {name}: (no non-zero entries)");
			}

			LogLevel("Difficulty1", EnemyParent.Difficulty.Difficulty1);
			LogLevel("Difficulty2", EnemyParent.Difficulty.Difficulty2);
			LogLevel("Difficulty3", EnemyParent.Difficulty.Difficulty3);
		}
	}
}
