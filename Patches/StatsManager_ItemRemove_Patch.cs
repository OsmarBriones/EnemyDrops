#nullable enable
using EnemyDrops.Tracking;
using HarmonyLib;
using System;

namespace EnemyDrops.Patches;

/// <summary>
/// Decrements the preserved enemy drops count when an item is consumed or destroyed.
/// </summary>
[HarmonyPatch(typeof(StatsManager), nameof(StatsManager.ItemRemove))]
internal static class StatsManager_ItemRemove_Patch
{
	private static void Postfix(string instanceName)
	{
		try
		{
			PreservedItemTracker.OnItemRemoved(instanceName);
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogError($"StatsManager_ItemRemove_Patch: Failed to update preserved count on ItemRemove: {ex}");
		}
	}
}
