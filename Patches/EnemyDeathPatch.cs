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

		// ItemDictionary keys for the upgrade pickups
		private static readonly string[] UpgradeItemKeys =
		{
			"Item Upgrade Death Head Battery",
			"Item Upgrade Map Player Count",
			"Item Upgrade Player Crouch Rest",
			"Item Upgrade Player Energy",
			"Item Upgrade Player Extra Jump",
			"Item Upgrade Player Grab Range",
			"Item Upgrade Player Grab Strength",
			"Item Upgrade Player Health",
			"Item Upgrade Player Sprint Speed",
			"Item Upgrade Player Tumble Climb",
			"Item Upgrade Player Tumble Launch",
			"Item Upgrade Player Tumble Wings",
		};

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
				var enemy = health.GetComponent<Enemy>();
				if (enemy == null)
				{
					EnemyDrops.Logger.LogWarning("EnemyDeathPatch: Enemy component not found on dead object.");
					return;
				}

				// Avoid duplicate spawns in multiplayer
				bool canSpawn = true;
				try { canSpawn = SemiFunc.IsMasterClientOrSingleplayer(); } catch { /* ignore if not available */ }
				if (!canSpawn) return;

				SpawnUpgradeDrop(enemy);
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"EnemyDeathPatch.OnEnemyDeath failed: {ex}");
			}
		}

		private static void SpawnUpgradeDrop(Enemy enemy)
		{
			Transform t = enemy.CustomValuableSpawnTransform
						   ? enemy.CustomValuableSpawnTransform
						   : (enemy.CenterTransform ? enemy.CenterTransform : enemy.transform);

			Vector3 pos = t.position + Vector3.up * 0.15f;
			Quaternion rot = t.rotation;

			var item = TryGetRandomUpgradeItemFromDictionary();
			if (item == null)
			{
				EnemyDrops.Logger.LogWarning("Failed to resolve any upgrade Item from StatsManager.itemDictionary.");
				return;
			}

			GameObject spawned = null;
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
				EnemyDrops.Logger.LogError($"Failed to spawn upgrade '{item.name}' at {pos}: {ex.Message}");
				return;
			}

			// Nudge it for a nicer feel if it has a rigidbody
			if (spawned && spawned.TryGetComponent<Rigidbody>(out var rb))
			{
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
				rb.AddForce(UnityEngine.Random.insideUnitSphere * 1.25f + Vector3.up * 2f, ForceMode.Impulse);
			}

			EnemyDrops.Logger.LogInfo($"Spawned upgrade '{(spawned ? spawned.name : item.name)}' at {pos}");
		}

		private static Item TryGetRandomUpgradeItemFromDictionary()
		{
			try
			{
				var dict = StatsManager.instance?.itemDictionary;
				if (dict == null || dict.Count == 0)
					return null;

				// Start from a random offset to vary selection
				int start = Rng.Next(UpgradeItemKeys.Length);
				for (int i = 0; i < UpgradeItemKeys.Length; i++)
				{
					string key = UpgradeItemKeys[(start + i) % UpgradeItemKeys.Length];
					if (dict.TryGetValue(key, out var item) && item != null && item.prefab != null)
					{
						return item;
					}
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