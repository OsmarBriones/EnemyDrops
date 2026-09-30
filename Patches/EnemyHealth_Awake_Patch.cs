#nullable enable
using EnemyDrops.Drops;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;

namespace EnemyDrops.Patches;

/// <summary>
/// Harmony patch on <see cref="EnemyHealth.Awake"/> to subscribe to enemy death events once per instance.
/// </summary>
[HarmonyPatch(typeof(EnemyHealth), "Awake")]
internal static class EnemyHealth_Awake_Patch
{
	// Tracks which EnemyHealth instances we subscribed to (auto-releases when instance is GC'd)
	private static readonly ConditionalWeakTable<EnemyHealth, object> subscribed = new();

	private static void Postfix(EnemyHealth __instance)
	{
		if (subscribed.TryGetValue(__instance, out _))
		{
			return;
		}

		subscribed.Add(__instance, new object());
		__instance.onDeath.AddListener(() => OnEnemyDeath(__instance));
	}

	private static void OnEnemyDeath(EnemyHealth health)
	{
		try
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

			var enemy = health.GetComponent<Enemy>();
			if (enemy == null)
			{
				EnemyDropsPlugin.Logger.LogWarning("EnemyHealth_Awake_Patch: Enemy component not found on dead object.");
				return;
			}

			// Delegate drop logic (difficulty + weighted selection + spawn) to ItemDropper
			if (!ItemDropper.TrySpawnForEnemy(enemy, out _))
			{
				EnemyDropsPlugin.Logger.LogDebug("EnemyHealth_Awake_Patch: ItemDropper did not spawn an item for enemy.");
			}
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogError($"EnemyHealth_Awake_Patch.OnEnemyDeath failed: {ex}");
		}
	}
}