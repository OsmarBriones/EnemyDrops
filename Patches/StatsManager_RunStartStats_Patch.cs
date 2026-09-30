#nullable enable
using EnemyDrops.Tracking;
using HarmonyLib;
using System;

namespace EnemyDrops.Patches;

/// <summary>
/// Resets the preserved enemy drops tracker when a new run starts.
/// </summary>
[HarmonyPatch(typeof(StatsManager), "RunStartStats")]
internal static class StatsManager_RunStartStats_Patch
{
	private static void Postfix()
	{
		try
		{
			PreservedItemTracker.Reset();
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogError($"StatsManager_RunStartStats_Patch: Failed to reset preserved tracker on RunStartStats: {ex}");
		}
	}
}
