using EnemyDrops.Configuration;
using HarmonyLib;
using System.Text;

namespace EnemyDrops.Patches
{
	// Reload drop tables from the config file at the beginning of each level.
	// EnemyDirector.Start is called once per level load.
	[HarmonyPatch(typeof(EnemyDirector), "Start")]
	internal static class ReloadDropTablesOnLevelStart
	{
		// Temporary hard-coded list of base item names whose batteries should be reset
		// when ResetItemsEnergyOnLevelStart is enabled. You can extend this list later.
		private static readonly string[] s_energyResetItems =
		{
			"Item Cart Laser",
			"Item Cart Medium",
			"Item Drone Battery",
			"Item Drone Feather",
			"Item Drone Indestructible",
			"Item Drone Torque",
			"Item Drone Zero Gravity",
			"Item Extraction Tracker",
			"Item Gun Handgun",
			"Item Gun Laser",
			"Item Gun Shockwave",
			"Item Gun Shotgun",
			"Item Gun Stun",
			"Item Gun Tranq",
			"Item Melee Baseball Bat",
			"Item Melee Frying Pan",
			"Item Melee Inflatable Hammer",
			"Item Melee Sledge Hammer",
			"Item Melee Stun Baton",
			"Item Melee Sword",
			"Item Orb Zero Gravity",
			"Item Phase Bridge",
			"Item Rubber Duck",
			"Item Valuable Tracker"
		};

		private static void Postfix()
		{
			if (!SemiFunc.RunIsLevel()) return;
			try
			{
				// Refresh config, then reset the per-level drop counter
				ConfigurationController.Reload(EnemyDrops.Logger);
				ItemDropper.ResetForNewLevel();
				DroppedInstanceTracker.ClearForNewLevel();

				if (ConfigurationController.ResetItemsEnergyOnLevelStart)
				{
					RestoreEnergyForKnownItems();
				}

				//LogItemTable();
				//LogBatteryTable();
			}
			catch (System.Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDrops: Failed during level-start reload/reset: {ex}");
			}
		}

		/// <summary>
		/// Resets battery entries to 100 for all instances whose base item name
		/// is in s_energyResetItems. This does not create new keys; it only fixes
		/// existing entries in itemStatBattery.
		/// </summary>
		private static void RestoreEnergyForKnownItems()
		{
			var stats = StatsManager.instance;
			if (stats == null)
			{
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager.instance null; cannot restore energy.");
				return;
			}

			var map = stats.itemStatBattery;
			if (map == null || map.Count == 0)
			{
				EnemyDrops.Logger.LogDebug("EnemyDrops: itemStatBattery empty; nothing to restore.");
				return;
			}

			// Snapshot keys to avoid collection modification while iterating.
			var keys = new System.Collections.Generic.List<string>(map.Keys);
			var sb = new StringBuilder();
			int restoredCount = 0;

			for (int i = 0; i < keys.Count; i++)
			{
				var key = keys[i];
				if (string.IsNullOrEmpty(key))
				{
					continue;
				}

				// Base name is everything before the first '/' (or the whole key if no '/')
				string baseName;
				int slashIndex = key.IndexOf('/');
				if (slashIndex > 0)
				{
					baseName = key.Substring(0, slashIndex);
				}
				else
				{
					baseName = key;
				}

				// Check if this base name is in the reset list
				bool match = false;
				for (int j = 0; j < s_energyResetItems.Length; j++)
				{
					if (string.Equals(baseName, s_energyResetItems[j], System.StringComparison.Ordinal))
					{
						match = true;
						break;
					}
				}

				if (!match)
				{
					continue;
				}

				// Only touch entries that already exist
				map[key] = 100;
				sb.AppendLine($"  {key} -> 100");
				restoredCount++;
			}

			if (restoredCount > 0)
			{
				EnemyDrops.Logger.LogInfo($"EnemyDrops: ResetEnergyOnLevelStart fixed {restoredCount} entries for known energy items:\n{sb}");
			}
			else
			{
				EnemyDrops.Logger.LogInfo("EnemyDrops: ResetEnergyOnLevelStart enabled; no matching entries found for known energy items.");
			}
		}

		private static void LogItemTable()
		{
			var stats = StatsManager.instance;
			if (stats == null)
			{
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager.instance is null; cannot dump item table.");
				return;
			}

			var map = stats.item;
			if (map == null || map.Count == 0)
			{
				EnemyDrops.Logger.LogInfo("EnemyDrops: item dictionary is empty.");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine("EnemyDrops: item dump (instanceName = value):");
			foreach (var kvp in map)
			{
				sb.AppendLine($"  {kvp.Key} = {kvp.Value}");
			}
			EnemyDrops.Logger.LogInfo(sb.ToString());
		}

		private static void LogBatteryTable()
		{
			var stats = StatsManager.instance;
			if (stats == null)
			{
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager.instance is null; cannot dump battery table.");
				return;
			}

			var map = stats.itemStatBattery;
			if (map == null || map.Count == 0)
			{
				EnemyDrops.Logger.LogInfo("EnemyDrops: itemStatBattery is empty.");
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine("EnemyDrops: itemStatBattery dump (instanceName = battery):");
			foreach (var kvp in map)
			{
				sb.AppendLine($"  {kvp.Key} = {kvp.Value}");
			}
			EnemyDrops.Logger.LogInfo(sb.ToString());
		}
	}
}