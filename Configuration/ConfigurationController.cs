#nullable enable
using BepInEx.Configuration;
using BepInEx.Logging;
using EnemyDrops.Drops;
using System;

namespace EnemyDrops.Configuration;

/// <summary>
/// Centralizes configuration initialization and reloads (used at startup and on each level start).
/// </summary>
internal static class ConfigurationController
{
	private static ConfigFile? configFile;
	private static ConfigEntry<int>? maxDropsPerLevelEntry;
	private static ConfigEntry<bool>? preserveItemsBetweenLevelsEntry;
	private static ConfigEntry<int>? maxPreservedItemsEntry;
	private static ConfigEntry<bool>? enableColorModificationEntry;
	private static ConfigEntry<float>? colorMultiplierREntry;
	private static ConfigEntry<float>? colorMultiplierGEntry;
	private static ConfigEntry<float>? colorMultiplierBEntry;
	private static ConfigEntry<float>? colorAddEntry;
	private static ConfigEntry<bool>? enableDropAuraEntry;
	private static ConfigEntry<float>? auraIntensityEntry;
	private static ConfigEntry<float>? auraRangeEntry;

	/// <summary>
	/// Exposes the configured max number of item drops per level (defaults to 200 if uninitialized).
	/// </summary>
	internal static int MaxDropsPerLevel => maxDropsPerLevelEntry?.Value ?? 200;

	/// <summary>
	/// Exposes whether items obtained from enemy drops and secured in the truck/inventory persist across levels.
	/// </summary>
	internal static bool PreserveItemsBetweenLevels => preserveItemsBetweenLevelsEntry?.Value ?? false;

	/// <summary>
	/// Exposes the maximum number of items obtained from enemy drops that can be preserved across levels.
	/// </summary>
	internal static int MaxPreservedItems => maxPreservedItemsEntry?.Value ?? 10;

	/// <summary>
	/// Exposes whether items dropped by enemies have their materials modified with a color multiplier and additive offset.
	/// </summary>
	internal static bool EnableColorModification => enableColorModificationEntry?.Value ?? true;

	/// <summary>
	/// Exposes the red channel multiplier applied to the material color of enemy drops.
	/// </summary>
	internal static float ColorMultiplierR => colorMultiplierREntry?.Value ?? 0.55f;

	/// <summary>
	/// Exposes the green channel multiplier applied to the material color of enemy drops.
	/// </summary>
	internal static float ColorMultiplierG => colorMultiplierGEntry?.Value ?? 0.55f;

	/// <summary>
	/// Exposes the blue channel multiplier applied to the material color of enemy drops.
	/// </summary>
	internal static float ColorMultiplierB => colorMultiplierBEntry?.Value ?? 0.55f;

	/// <summary>
	/// Exposes the additive offset applied to the material color of enemy drops.
	/// </summary>
	internal static float ColorAdd => colorAddEntry?.Value ?? 0.05f;

	/// <summary>
	/// Exposes whether items dropped by enemies emit a reddish light aura centered on the item.
	/// </summary>
	internal static bool EnableDropAura => enableDropAuraEntry?.Value ?? true;

	/// <summary>
	/// Exposes the intensity of the reddish light aura emitted by enemy drops.
	/// </summary>
	internal static float AuraIntensity => auraIntensityEntry?.Value ?? 0.8f;

	/// <summary>
	/// Exposes the range in meters of the reddish light aura emitted by enemy drops.
	/// </summary>
	internal static float AuraRange => auraRangeEntry?.Value ?? 3.5f;

	/// <summary>
	/// Initializes configuration-backed drop tables and logs active weights.
	/// </summary>
	internal static void Initialize(ConfigFile config, ManualLogSource logger)
	{
		if (config is null) throw new ArgumentNullException(nameof(config));
		if (logger is null) throw new ArgumentNullException(nameof(logger));

		configFile = config;

		// General plugin settings
		maxDropsPerLevelEntry = configFile.Bind(
			"General",
			nameof(MaxDropsPerLevel),
			200,
			new ConfigDescription(
				"Maximum number of items that can drop each level.",
				new AcceptableValueRange<int>(0, 1000)));

		preserveItemsBetweenLevelsEntry = configFile.Bind(
			"General",
			nameof(PreserveItemsBetweenLevels),
			false,
			new ConfigDescription(
				"Allow items obtained from enemy drops and secured in the truck or inventory to persist across levels."));

		maxPreservedItemsEntry = configFile.Bind(
			"General",
			nameof(MaxPreservedItems),
			10,
			new ConfigDescription(
				"Maximum number of enemy-dropped items that can be preserved simultaneously across levels.",
				new AcceptableValueRange<int>(0, 100)));

		// Visual settings
		enableColorModificationEntry = configFile.Bind(
			"Visuals",
			nameof(EnableColorModification),
			true,
			new ConfigDescription("Whether items dropped by enemies have their materials modified with a color multiplier and additive offset."));

		colorMultiplierREntry = configFile.Bind(
			"Visuals",
			nameof(ColorMultiplierR),
			0.55f,
			new ConfigDescription(
				"Red channel multiplier applied to material color (preserves texture details).",
				new AcceptableValueRange<float>(0.0f, 2.0f)));

		colorMultiplierGEntry = configFile.Bind(
			"Visuals",
			nameof(ColorMultiplierG),
			0.55f,
			new ConfigDescription(
				"Green channel multiplier applied to material color (preserves texture details).",
				new AcceptableValueRange<float>(0.0f, 2.0f)));

		colorMultiplierBEntry = configFile.Bind(
			"Visuals",
			nameof(ColorMultiplierB),
			0.55f,
			new ConfigDescription(
				"Blue channel multiplier applied to material color (preserves texture details).",
				new AcceptableValueRange<float>(0.0f, 2.0f)));

		colorAddEntry = configFile.Bind(
			"Visuals",
			nameof(ColorAdd),
			0.05f,
			new ConfigDescription(
				"Additive brightness offset applied to material color.",
				new AcceptableValueRange<float>(-1.0f, 1.0f)));

		enableDropAuraEntry = configFile.Bind(
			"Visuals",
			nameof(EnableDropAura),
			true,
			new ConfigDescription("Whether items dropped by enemies emit a reddish light aura centered on the item."));

		auraIntensityEntry = configFile.Bind(
			"Visuals",
			nameof(AuraIntensity),
			0.8f,
			new ConfigDescription(
				"Intensity of the reddish light aura emitted by enemy drops.",
				new AcceptableValueRange<float>(0.1f, 3.0f)));

		auraRangeEntry = configFile.Bind(
			"Visuals",
			nameof(AuraRange),
			3.5f,
			new ConfigDescription(
				"Range in meters of the reddish light aura emitted by enemy drops.",
				new AcceptableValueRange<float>(0.5f, 10.0f)));

		// Build or rebuild the runtime matrix from config entries
		ItemDropTables.InitializeConfig(configFile);

		// Persist any new defaults to disk (helps users discover the full set of keys)
		configFile.Save();

		// Log current weights
		ItemDropTables.LogWeights(logger);
		logger.LogInfo($"EnemyDrops: Configuration initialized. MaxDropsPerLevel={MaxDropsPerLevel}, PreserveItemsBetweenLevels={PreserveItemsBetweenLevels}, MaxPreservedItems={MaxPreservedItems}, EnableColorModification={EnableColorModification}, EnableDropAura={EnableDropAura}, AuraIntensity={AuraIntensity}, AuraRange={AuraRange}");
	}

	/// <summary>
	/// Reloads configuration from disk and rebuilds the drop tables.
	/// </summary>
	internal static void Reload(ManualLogSource logger)
	{
		if (configFile is null)
		{
			logger.LogWarning("EnemyDrops: Configuration reload requested before initialization.");
			return;
		}

		try
		{
			configFile.Reload();
			ItemDropTables.InitializeConfig(configFile);
			configFile.Save();

			ItemDropTables.LogWeights(logger);
			logger.LogInfo($"EnemyDrops: Configuration reloaded. MaxDropsPerLevel={MaxDropsPerLevel}, PreserveItemsBetweenLevels={PreserveItemsBetweenLevels}, MaxPreservedItems={MaxPreservedItems}, EnableColorModification={EnableColorModification}, EnableDropAura={EnableDropAura}, AuraIntensity={AuraIntensity}, AuraRange={AuraRange}");
		}
		catch (Exception ex)
		{
			logger.LogError($"EnemyDrops: Failed to reload configuration: {ex}");
		}
	}
}
