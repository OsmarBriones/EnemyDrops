#nullable enable
using EnemyDrops.Configuration;
using EnemyDrops.Drops;
using EnemyDrops.Tracking;
using HarmonyLib;
using System;

namespace EnemyDrops.Patches;

/// <summary>
/// Reloads drop tables from the config file and resets in-memory counters at level start.
/// </summary>
[HarmonyPatch(typeof(EnemyDirector), "Start")]
internal static class EnemyDirector_Start_Patch
{
	private static void Postfix()
	{
		if (!SemiFunc.RunIsLevel()) return;

		try
		{
			// Refresh config, then reset per-level drop counter and instance tracker
			ConfigurationController.Reload(EnemyDropsPlugin.Logger);
			ItemDropper.ResetForNewLevel();
			DroppedInstanceTracker.ClearForNewLevel();
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogError($"EnemyDirector_Start_Patch: Failed during level-start reload/reset: {ex}");
		}
	}
}