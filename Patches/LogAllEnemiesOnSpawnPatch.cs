using HarmonyLib;
using System;
using System.Linq;

namespace EnemyDrops.Patches
{
	// Logs the names of all currently spawned enemies right after any enemy finishes SpawnRPC.
	[HarmonyPatch(typeof(EnemyParent), "SpawnRPC")]
	internal static class LogAllEnemiesOnSpawnPatch
	{
		private static void Postfix(EnemyParent __instance)
		{
			try
			{
				// Avoid duplicate logs on clients
				bool canLog = true;
				try { canLog = SemiFunc.IsMasterClientOrSingleplayer(); } catch { }
				if (!canLog) return;

				var dir = EnemyDirector.instance;
				if (dir == null || dir.enemiesSpawned == null) return;

				var names = dir.enemiesSpawned
					.Where(e => e != null)
					.Select(e => string.IsNullOrEmpty(e.enemyName) ? e.gameObject.name : e.enemyName)
					.ToList();

				if (names.Count == 0) return;

				EnemyDrops.Logger.LogInfo($"Enemies spawned: {string.Join(", ", names)}");
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"LogAllEnemiesOnSpawnPatch failed: {ex}");
			}
		}
	}
}