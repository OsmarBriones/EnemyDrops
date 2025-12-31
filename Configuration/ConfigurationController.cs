using BepInEx.Configuration;
using BepInEx.Logging;
using System;

namespace EnemyDrops.Configuration
{
	/// Centralizes config initialization and reloads (used at startup and on each level start).
	internal static class ConfigurationController
	{
		private static ConfigFile? _config;
		private static ConfigEntry<int>? _maxDropsPerLevel;

		/// Exposes the configured max number of item drops per level (defaults to 200 if uninitialized).
		internal static int MaxDropsPerLevel => _maxDropsPerLevel?.Value ?? 200;

		/// Initializes configuration-backed drop tables and logs active weights.
		internal static void Initialize(ConfigFile config, ManualLogSource logger)
		{
			if (config is null) throw new ArgumentNullException(nameof(config));
			if (logger is null) throw new ArgumentNullException(nameof(logger));

			_config = config;

			// General plugin settings
			_maxDropsPerLevel = _config.Bind(
				"General",
				nameof(MaxDropsPerLevel),
				200,
				new ConfigDescription(
					"Maximum number of items that can drop each level.",
					new AcceptableValueRange<int>(0, 1000)));

			// Build or rebuild the runtime matrix from config entries
			ItemDropTables.InitializeConfig(_config);

			// Persist any new defaults to disk (helps users discover the full set of keys)
			_config.Save();

			// Log current weights
			ItemDropTables.LogWeights(logger);
			logger.LogInfo($"EnemyDrops: Configuration initialized. MaxDropsPerLevel={MaxDropsPerLevel}");
		}

		/// Reloads configuration from disk and rebuilds the drop tables.
		internal static void Reload(ManualLogSource logger)
		{
			if (_config is null)
			{
				logger.LogWarning("EnemyDrops: Configuration reload requested before initialization.");
				return;
			}

			try
			{
				_config.Reload();
				ItemDropTables.InitializeConfig(_config);
				_config.Save();

				ItemDropTables.LogWeights(logger);
				logger.LogInfo($"EnemyDrops: Configuration reloaded. MaxDropsPerLevel={MaxDropsPerLevel}");
			}
			catch (Exception ex)
			{
				logger.LogError($"EnemyDrops: Failed to reload configuration: {ex}");
			}
		}
	}
}