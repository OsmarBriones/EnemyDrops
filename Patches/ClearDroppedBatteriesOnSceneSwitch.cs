using HarmonyLib;
using System;

namespace EnemyDrops.Patches
{
	// Clear battery entries for dropped (non-owned) items when the scene switches.
	[HarmonyPatch(typeof(SemiFunc), "OnSceneSwitch")]
	internal static class ClearDroppedBatteriesOnSceneSwitch
	{
		private static void Postfix(bool _gameOver, bool _leaveGame)
		{
			try
			{
				DroppedInstanceTracker.ClearBatteriesForDroppedInstances();
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDrops: Failed to clear dropped batteries on scene switch: {ex}");
			}
		}
	}
}