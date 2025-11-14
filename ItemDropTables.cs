using EnemyDrops.Providers;
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

	// Default, in-code drop tables. Replace with config loading later.
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

		public static IReadOnlyList<WeightedKey> GetWeightsFor(EnemyParent.Difficulty difficulty)
		{
			switch (difficulty)
			{
				case EnemyParent.Difficulty.Difficulty1:
					return s_commonItems;

				case EnemyParent.Difficulty.Difficulty2:
					return s_mediumItems;

				case EnemyParent.Difficulty.Difficulty3:
				default:
					return s_rareItems;
			}
		}
	}
}
