using EnemyDrops.Configuration;
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
			if (!SemiFunc.RunIsLevel()) return;
			try
			{
				// Refresh config, then reset the per-level drop counter
				ConfigurationController.Reload(EnemyDrops.Logger);
				ItemDropper.ResetForNewLevel();
			}
			catch (System.Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDrops: Failed during level-start reload/reset: {ex}");
			}
		}
	}
}