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
		private static void Postfix()
		{
			if (!SemiFunc.RunIsLevel()) return;
			try
			{
				// Refresh config, then reset the per-level drop counter
				ConfigurationController.Reload(EnemyDrops.Logger);
				ItemDropper.ResetForNewLevel();
				DroppedInstanceTracker.ClearForNewLevel();

				// Used for debugging, letting them here in case needed again in the future
				//LogItemTable();
				//LogBatteryTable();
			}
			catch (System.Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDrops: Failed during level-start reload/reset: {ex}");
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