using Photon.Pun;
using System;
using UnityEngine;

namespace EnemyDrops.Providers
{
	/// <summary>
	/// Provides spawning of upgrade items using keys from ItemKeysProvider and StatsManager.itemDictionary.
	/// </summary>
	public static class ItemProvider
	{
		/// <summary>
		/// Attempts to spawn a random item at the given position (delegates to TrySpawnByKey).
		/// </summary>
		public static bool TrySpawnRandomItem(Vector3 position, Quaternion rotation, out GameObject? spawned, float upwardOffset = 0.15f)
		{
			spawned = null;

			string? key = ItemKeysProvider.GetRandomKey();
			if (string.IsNullOrEmpty(key))
			{
				EnemyDrops.Logger.LogWarning("ItemProvider: Random key provider returned null/empty.");
				return false;
			}

			return TrySpawnByKey(key, position, rotation, out spawned, upwardOffset);
		}

		/// <summary>
		/// Attempts to spawn a specific item by dictionary key.
		/// </summary>
		public static bool TrySpawnByKey(string key, Vector3 position, Quaternion rotation, out GameObject? spawned, float upwardOffset = 0.15f)
		{
			spawned = null;

			bool canSpawn = true;
			try { canSpawn = SemiFunc.IsMasterClientOrSingleplayer(); } catch { }
			if (!canSpawn)
			{
				return false;
			}

			var dict = StatsManager.instance?.itemDictionary;
			if (dict == null)
			{
				EnemyDrops.Logger.LogWarning("ItemProvider: itemDictionary unavailable.");
				return false;
			}

			if (!dict.TryGetValue(key, out var item) || item == null || item.prefab == null)
			{
				EnemyDrops.Logger.LogWarning($"ItemProvider: Key '{key}' not found or invalid.");
				return false;
			}

			Vector3 spawnPos = position + Vector3.up * upwardOffset;

			try
			{
				if (SemiFunc.IsMultiplayer())
				{
					spawned = PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, spawnPos, rotation, 0);
				}
				else
				{
					spawned = UnityEngine.Object.Instantiate(item.prefab.Prefab, spawnPos, rotation);
				}
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogError($"ItemProvider: Failed to spawn '{item.name}' at {spawnPos}: {ex.Message}");
				return false;
			}

			ApplySpawnImpulse(spawned);
			EnemyDrops.Logger.LogInfo($"ItemProvider: Spawned '{spawned!.name}' at {spawnPos} (key = {key})");
			return true;
		}

		private static void ApplySpawnImpulse(GameObject? go)
		{
			if (!go) return;
			if (go.TryGetComponent<Rigidbody>(out var rb))
			{
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
				rb.AddForce(UnityEngine.Random.insideUnitSphere * 1.25f + Vector3.up * 2f, ForceMode.Impulse);
			}
		}
	}
}