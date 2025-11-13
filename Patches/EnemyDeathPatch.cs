using EnemyDrops.Configuration;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EnemyDrops.Patches
{
	internal static class EnemyDeathPatch
	{
		// Tracks which EnemyHealth instances we subscribed to (auto-releases when instance is GC'd)
		private static readonly ConditionalWeakTable<EnemyHealth, object> Subscribed = new();

		private static readonly System.Random Rng = new System.Random();

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
				// Avoid duplicate spawns in multiplayer
				bool canSpawn = true;
				try { canSpawn = SemiFunc.IsMasterClientOrSingleplayer(); } catch { /* ignore if not available */ }
				if (!canSpawn) return;

				// Avoid spawning if the enemy component is missing (it means that this is not a real enemy)
				var enemy = health.GetComponent<Enemy>();
				if (enemy == null)
				{
					EnemyDrops.Logger.LogWarning("EnemyDeathPatch: Enemy component not found on dead object.");
					return;
				}

				SpawnItemDrop(enemy);
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDeathPatch.OnEnemyDeath failed: {ex}");
			}
		}

		private static void SpawnItemDrop(Enemy enemy)
		{
			Transform t = enemy.CustomValuableSpawnTransform
						   ? enemy.CustomValuableSpawnTransform
						   : (enemy.CenterTransform ? enemy.CenterTransform : enemy.transform);

			Vector3 pos = t.position + Vector3.up;
			Quaternion rot = t.rotation;

			Item? item = TryGetRandomItemFromDictionary();
			if (item == null)
			{
				EnemyDrops.Logger.LogWarning("Failed to resolve any Item from StatsManager.itemDictionary.");
				return;
			}

			GameObject? spawned = null;
			try
			{
				// Use the same pattern the game uses for spawning Items
				if (SemiFunc.IsMultiplayer())
				{
					spawned = PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, pos, rot, 0);
				}
				else
				{
					spawned = UnityEngine.Object.Instantiate(item.prefab.Prefab, pos, rot);
				}
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"Failed to spawn Item '{item.name}' at {pos}: {ex.Message}");
				return;
			}

			// Nudge it for a nicer feel if it has a rigidbody
			if (spawned && spawned.TryGetComponent<Rigidbody>(out var rb))
			{
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
				rb.AddForce(UnityEngine.Random.insideUnitSphere * 1.25f + Vector3.up * 2f, ForceMode.Impulse);
			}

			EnemyDrops.Logger.LogInfo($"Spawned Item '{(spawned ? spawned.name : item.name)}' at {pos}");
		}

		private static Item? TryGetRandomItemFromDictionary()
		{
			try
			{
				var dict = StatsManager.instance?.itemDictionary;
				if (dict == null || dict.Count == 0)
					return null;

				// Start from a random offset to vary selection
				string? key = UpgradeItemKeysProvider.GetRandomKey();
				if (key == null)
				{
					EnemyDrops.Logger.LogWarning("UpgradeItemKeysProvider returned null key.");
					return null;
				}
				if (dict.TryGetValue(key, out var item) && item != null && item.prefab != null)
				{
					return item;
				}
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"Error while fetching Item from itemDictionary: {ex}");
			}
			return null;
		}
	}
}