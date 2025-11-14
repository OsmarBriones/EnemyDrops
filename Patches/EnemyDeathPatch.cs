using HarmonyLib;
using System;
using System.Runtime.CompilerServices;

namespace EnemyDrops.Patches
{
	internal static class EnemyDeathPatch
	{
		// Tracks which EnemyHealth instances we subscribed to (auto-releases when instance is GC'd)
		private static readonly ConditionalWeakTable<EnemyHealth, object> Subscribed = new();

		[HarmonyPatch(typeof(EnemyHealth), "Awake")]
		private static class EnemyHealth_Awake_Patch
		{
			private static void Postfix(EnemyHealth __instance)
			{
				if (Subscribed.TryGetValue(__instance, out _))
					return;

				Subscribed.Add(__instance, new object());
				__instance.onDeath.AddListener(() => OnEnemyDeath(__instance));
			}
		}

		private static void OnEnemyDeath(EnemyHealth health)
		{
			try
			{
				bool canSpawn = true;
				try { canSpawn = SemiFunc.IsMasterClientOrSingleplayer(); } catch { }
				if (!canSpawn) return;

				var enemy = health.GetComponent<Enemy>();
				if (enemy == null)
				{
					EnemyDrops.Logger.LogWarning("EnemyDeathPatch: Enemy component not found on dead object.");
					return;
				}

				// Delegate drop logic (difficulty + weighted selection) to ItemDropper
				if (!ItemDropper.TrySpawnForEnemy(enemy, out var spawned))
				{
					EnemyDrops.Logger.LogDebug("EnemyDeathPatch: ItemDropper failed or chose no item to spawn.");
				}
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDeathPatch.OnEnemyDeath failed: {ex}");
			}
		}
	}
}