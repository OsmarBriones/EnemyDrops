namespace EnemyDrops.Providers
{
	/// <summary>
	/// Strongly-named constants for all known item dictionary keys.
	/// </summary>
	public static class ItemKeys
	{
		// Carts
		public const string CartCannon = "Item Cart Cannon";
		public const string CartLaser = "Item Cart Laser";
		public const string CartMedium = "Item Cart Medium";
		public const string CartSmall = "Item Cart Small";

		// Drone modifiers
		public const string DroneBattery = "Item Drone Battery";
		public const string DroneFeather = "Item Drone Feather";
		public const string DroneIndestructible = "Item Drone Indestructible";
		public const string DroneTorque = "Item Drone Torque";
		public const string DroneZeroGravity = "Item Drone Zero Gravity";

		// Misc single items
		public const string DuckBucket = "Item Duck Bucket";
		public const string ExtractionTracker = "Item Extraction Tracker";

		// Grenades
		public const string GrenadeDuctTaped = "Item Grenade Duct Taped";
		public const string GrenadeExplosive = "Item Grenade Explosive";
		public const string GrenadeHuman = "Item Grenade Human";
		public const string GrenadeShockwave = "Item Grenade Shockwave";
		public const string GrenadeStun = "Item Grenade Stun";

		// Guns
		public const string GunHandgun = "Item Gun Handgun";
		public const string GunLaser = "Item Gun Laser";
		public const string GunShockwave = "Item Gun Shockwave";
		public const string GunShotgun = "Item Gun Shotgun";
		public const string GunStun = "Item Gun Stun";
		public const string GunTranq = "Item Gun Tranq";

		// Health packs
		public const string HealthPackLarge = "Item Health Pack Large";
		public const string HealthPackMedium = "Item Health Pack Medium";
		public const string HealthPackSmall = "Item Health Pack Small";

		// Melee
		public const string MeleeBaseballBat = "Item Melee Baseball Bat";
		public const string MeleeFryingPan = "Item Melee Frying Pan";
		public const string MeleeInflatableHammer = "Item Melee Inflatable Hammer";
		public const string MeleeSledgeHammer = "Item Melee Sledge Hammer";
		public const string MeleeStunBaton = "Item Melee Stun Baton";
		public const string MeleeSword = "Item Melee Sword";

		// Mines
		public const string MineExplosive = "Item Mine Explosive";
		public const string MineShockwave = "Item Mine Shockwave";
		public const string MineStun = "Item Mine Stun";

		// Other functional items
		public const string OrbZeroGravity = "Item Orb Zero Gravity";
		public const string PhaseBridge = "Item Phase Bridge";
		public const string PowerCrystal = "Item Power Crystal";
		public const string RubberDuck = "Item Rubber Duck";

		// Upgrades
		public const string UpgradeDeathHeadBattery = "Item Upgrade Death Head Battery";
		public const string UpgradeMapPlayerCount = "Item Upgrade Map Player Count";
		public const string UpgradePlayerCrouchRest = "Item Upgrade Player Crouch Rest";
		public const string UpgradePlayerEnergy = "Item Upgrade Player Energy";
		public const string UpgradePlayerExtraJump = "Item Upgrade Player Extra Jump";
		public const string UpgradePlayerGrabRange = "Item Upgrade Player Grab Range";
		public const string UpgradePlayerGrabStrength = "Item Upgrade Player Grab Strength";
		public const string UpgradePlayerHealth = "Item Upgrade Player Health";
		public const string UpgradePlayerSprintSpeed = "Item Upgrade Player Sprint Speed";
		public const string UpgradePlayerTumbleClimb = "Item Upgrade Player Tumble Climb";
		public const string UpgradePlayerTumbleLaunch = "Item Upgrade Player Tumble Launch";
		public const string UpgradePlayerTumbleWings = "Item Upgrade Player Tumble Wings";

		// Valuable
		public const string ValuableTracker = "Item Valuable Tracker";

		/// <summary>
		/// Flat array of every key (order mirrors grouping above).
		/// </summary>
		public static readonly string[] All =
		{
			CartCannon, CartLaser, CartMedium, CartSmall,
			DroneBattery, DroneFeather, DroneIndestructible, DroneTorque, DroneZeroGravity,
			DuckBucket, ExtractionTracker,
			GrenadeDuctTaped, GrenadeExplosive, GrenadeHuman, GrenadeShockwave, GrenadeStun,
			GunHandgun, GunLaser, GunShockwave, GunShotgun, GunStun, GunTranq,
			HealthPackLarge, HealthPackMedium, HealthPackSmall,
			MeleeBaseballBat, MeleeFryingPan, MeleeInflatableHammer, MeleeSledgeHammer, MeleeStunBaton, MeleeSword,
			MineExplosive, MineShockwave, MineStun,
			OrbZeroGravity, PhaseBridge, PowerCrystal, RubberDuck,
			UpgradeDeathHeadBattery, UpgradeMapPlayerCount, UpgradePlayerCrouchRest, UpgradePlayerEnergy, UpgradePlayerExtraJump,
			UpgradePlayerGrabRange, UpgradePlayerGrabStrength, UpgradePlayerHealth, UpgradePlayerSprintSpeed,
			UpgradePlayerTumbleClimb, UpgradePlayerTumbleLaunch, UpgradePlayerTumbleWings,
			ValuableTracker
		};
	}
}