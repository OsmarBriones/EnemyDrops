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

		// Build or rebuild the runtime matrix from config entries
		ItemDropTables.InitializeConfig(configFile);

		// Persist any new defaults to disk (helps users discover the full set of keys)
		configFile.Save();

		// Log current weights
		ItemDropTables.LogWeights(logger);
		logger.LogInfo($"EnemyDrops: Configuration initialized. MaxDropsPerLevel={MaxDropsPerLevel}, PreserveItemsBetweenLevels={PreserveItemsBetweenLevels}, MaxPreservedItems={MaxPreservedItems}");
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
			logger.LogInfo($"EnemyDrops: Configuration reloaded. MaxDropsPerLevel={MaxDropsPerLevel}, PreserveItemsBetweenLevels={PreserveItemsBetweenLevels}, MaxPreservedItems={MaxPreservedItems}");
		}
		catch (Exception ex)
		{
			logger.LogError($"EnemyDrops: Failed to reload configuration: {ex}");
		}
	}
}
