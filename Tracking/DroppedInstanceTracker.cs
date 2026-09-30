#nullable enable
using EnemyDrops.Configuration;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnemyDrops.Tracking;

/// <summary>
/// Maintains in-memory tracking of dropped items during a level to clean up unpersisted instances
/// or preserve secured items into StatsManager on scene transitions.
/// </summary>
internal static class DroppedInstanceTracker
{
	private static readonly HashSet<string> droppedInstances = new();

	internal static void MarkDropped(GameObject go)
	{
		if (!go) return;
		if (!go.GetComponent<DroppedItemTag>())
		{
			go.AddComponent<DroppedItemTag>();
		}
	}

	internal static void RegisterInstance(string instanceName)
	{
		if (string.IsNullOrEmpty(instanceName)) return;
		if (droppedInstances.Add(instanceName))
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Registered dropped instance '{instanceName}'.");
		}
	}

	internal static bool IsDropped(string instanceName)
	{
		return !string.IsNullOrEmpty(instanceName) && droppedInstances.Contains(instanceName);
	}

	/// <summary>
	/// Determines whether an item has been secured inside the truck or in a player's inventory.
	/// </summary>
	internal static bool IsItemSecured(ItemAttributes itemAttr)
	{
		if (!itemAttr) return false;

		// 1. In truck room volume
		var roomCheck = itemAttr.GetComponent<RoomVolumeCheck>();
		if (roomCheck != null && roomCheck.inTruck) return true;

		// 2. Equipped or held by a player
		var equippable = itemAttr.GetComponent<ItemEquippable>();
		if (equippable != null && (equippable.IsEquipped() || equippable.isEquipped)) return true;

		var grab = itemAttr.GetComponent<PhysGrabObject>();
		if (grab != null && (grab.grabbed || grab.grabbedLocal || (grab.playerGrabbing != null && grab.playerGrabbing.Count > 0)))
		{
			return true;
		}

		// 3. Registered in player inventory spots (StatsManager)
		if (!string.IsNullOrEmpty(itemAttr.instanceName))
		{
			int hash = itemAttr.instanceName.GetHashCode();
			var stats = StatsManager.instance;
			if (stats != null)
			{
				if (stats.playerInventorySpot1.ContainsValue(hash) ||
					stats.playerInventorySpot2.ContainsValue(hash) ||
					stats.playerInventorySpot3.ContainsValue(hash))
				{
					return true;
				}
			}
		}

		// 4. Proximity to truck spawn point (safety fallback if colliders slightly miss boundary)
		if (TruckSafetySpawnPoint.instance != null)
		{
			float dist = Vector3.Distance(itemAttr.transform.position, TruckSafetySpawnPoint.instance.transform.position);
			if (dist <= 8f) return true;
		}

		return false;
	}

	/// <summary>
	/// Processes scene switches: preserves secured enemy-drop items up to the configured limit,
	/// and cleans up unpersisted dropped instances from StatsManager.
	/// </summary>
	internal static void ProcessSceneSwitch(bool gameOver, bool leaveGame)
	{
		if (gameOver || leaveGame)
		{
			if (gameOver)
			{
				PreservedItemTracker.Reset();
			}
			ClearForNewLevel();
			return;
		}

		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			ClearForNewLevel();
			return;
		}

		var stats = StatsManager.instance;
		if (stats == null || stats.item == null || stats.itemStatBattery == null || stats.itemsPurchased == null)
		{
			EnemyDropsPlugin.Logger.LogWarning("EnemyDrops: StatsManager tables are null; cannot process scene switch.");
			ClearForNewLevel();
			return;
		}

		bool preserveEnabled = ConfigurationController.PreserveItemsBetweenLevels;
		int maxPreserved = ConfigurationController.MaxPreservedItems;
		int currentPreserved = PreservedItemTracker.GetTotalPreservedCount();

		var droppedTags = Object.FindObjectsOfType<DroppedItemTag>();
		var preservedInstances = new HashSet<string>();

		if (preserveEnabled && droppedTags != null)
		{
			for (int i = 0; i < droppedTags.Length; i++)
			{
				var tag = droppedTags[i];
				if (!tag) continue;

				var itemAttr = tag.GetComponent<ItemAttributes>();
				if (!itemAttr) continue;

				if (IsItemSecured(itemAttr))
				{
					string instanceName = itemAttr.instanceName;
					string baseItemName = itemAttr.item != null ? itemAttr.item.name : (instanceName.Contains("/") ? instanceName.Split('/')[0] : instanceName);

					if (currentPreserved >= maxPreserved)
					{
						EnemyDropsPlugin.Logger.LogInfo($"EnemyDrops: Item '{baseItemName}' ({instanceName}) is secured, but MaxPreservedItems limit ({maxPreserved}) has been reached. Item will not persist.");
						continue;
					}

					if (!string.IsNullOrEmpty(baseItemName) && stats.itemDictionary.ContainsKey(baseItemName))
					{
						int currentPurchased = stats.itemsPurchased.TryGetValue(baseItemName, out int cp) ? cp : 0;
						stats.itemsPurchased[baseItemName] = currentPurchased + 1;

						int currentTotal = stats.itemsPurchasedTotal.TryGetValue(baseItemName, out int ct) ? ct : 0;
						stats.itemsPurchasedTotal[baseItemName] = currentTotal + 1;

						PreservedItemTracker.RecordPreservedItem(baseItemName);
						currentPreserved++;

						if (!string.IsNullOrEmpty(instanceName))
						{
							preservedInstances.Add(instanceName);
						}

						EnemyDropsPlugin.Logger.LogInfo($"EnemyDrops: Preserved item '{baseItemName}' ({instanceName}) into truck/next level. Total preserved: {currentPreserved}/{maxPreserved}");
					}
				}
			}
		}

		// Snapshot tracked instances to clean up unpersisted drops
		var toRemove = new List<string>(droppedInstances);
		var removed = new List<string>();

		for (int i = 0; i < toRemove.Count; i++)
		{
			var instanceName = toRemove[i];
			if (string.IsNullOrEmpty(instanceName)) continue;

			if (preservedInstances.Contains(instanceName))
			{
				// Successfully preserved! Keep in stats.item and stats.itemStatBattery so battery and identity persist.
				continue;
			}

			if (stats.item.ContainsKey(instanceName))
			{
				stats.item.Remove(instanceName);
			}

			if (stats.itemStatBattery.ContainsKey(instanceName))
			{
				stats.itemStatBattery.Remove(instanceName);
			}

			removed.Add(instanceName);
		}

		if (removed.Count > 0)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"EnemyDrops: Cleaned up {removed.Count} non-preserved dropped instance(s) from item + itemStatBattery:");
			for (int i = 0; i < removed.Count; i++)
			{
				sb.AppendLine($"  {removed[i]}");
			}
			EnemyDropsPlugin.Logger.LogDebug(sb.ToString());
		}

		ClearForNewLevel();
	}

	/// <summary>
	/// Backward-compatible alias for scene switch cleanup.
	/// </summary>
	internal static void ClearBatteriesForDroppedInstances()
	{
		ProcessSceneSwitch(false, false);
	}

	internal static void ClearForNewLevel()
	{
		if (droppedInstances.Count > 0)
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Clearing dropped-instance tracker ({droppedInstances.Count} entries).");
		}
		droppedInstances.Clear();
	}
}
