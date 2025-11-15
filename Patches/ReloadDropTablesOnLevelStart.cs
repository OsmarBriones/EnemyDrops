using BepInEx.Configuration;
using HarmonyLib;

namespace EnemyDrops.Patches
{
	// Reload drop tables from the config file at the beginning of each level.
	// EnemyDirector.Start is called once per level load.
	[HarmonyPatch(typeof(EnemyDirector), "Start")]
	internal static class ReloadDropTablesOnLevelStart
	{
		static void Postfix()
		{
			try
			{
				// Re-read config from disk (in case user edited between levels)
				ConfigFile cfg = EnemyDrops.Instance.Config;
				cfg.Reload();

				// Rebuild the runtime matrix from config entries
				ItemDropTables.InitializeConfig(cfg);

				// Persist any defaulted/missing entries (optional but helpful)
				cfg.Save();

				// Optional: log the active weights to verify reload occurred
				ItemDropTables.LogWeights(EnemyDrops.Logger);
				EnemyDrops.Logger.LogInfo("EnemyDrops: Drop tables reloaded from config at level start.");
			}
			catch (System.Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDrops: Failed to reload drop tables at level start: {ex}");
			}
		}
	}
}