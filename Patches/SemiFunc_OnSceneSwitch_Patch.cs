#nullable enable
using EnemyDrops.Tracking;
using HarmonyLib;
using System;

namespace EnemyDrops.Patches;

/// <summary>
/// Intercepts scene transitions before the game saves to preserve secured dropped items
/// and clean up unpersisted dropped instances.
/// </summary>
[HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.OnSceneSwitch))]
internal static class SemiFunc_OnSceneSwitch_Patch
{
	private static void Prefix(bool _gameOver, bool _leaveGame)
	{
		try
		{
			DroppedInstanceTracker.ProcessSceneSwitch(_gameOver, _leaveGame);
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogError($"SemiFunc_OnSceneSwitch_Patch: Failed to process dropped instances on scene switch: {ex}");
		}
	}
}
