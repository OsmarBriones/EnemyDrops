using System.Collections.Generic;
using System.Text;

namespace EnemyDrops
{
	internal static class DroppedInstanceTracker
	{
		private static readonly HashSet<string> s_droppedInstances = new HashSet<string>();

		public static void MarkDropped(UnityEngine.GameObject go)
		{
			if (!go) return;
			if (!go.GetComponent<DroppedItemTag>())
			{
				go.AddComponent<DroppedItemTag>();
			}
		}

		public static void RegisterInstance(string instanceName)
		{
			if (string.IsNullOrEmpty(instanceName)) return;
			if (s_droppedInstances.Add(instanceName))
			{
				EnemyDrops.Logger.LogDebug($"EnemyDrops: Registered dropped instance '{instanceName}'.");
			}
		}

		public static bool IsDropped(string instanceName)
		{
			return !string.IsNullOrEmpty(instanceName) && s_droppedInstances.Contains(instanceName);
		}

		public static void ClearBatteriesForDroppedInstances()
		{
			var stats = StatsManager.instance;
			if (stats == null)
			{
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager.instance is null; cannot clear dropped instances.");
				return;
			}

			if (stats.item == null || stats.itemStatBattery == null)
			{
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager item tables are null; cannot clear dropped instances.");
				ClearForNewLevel();
				return;
			}

			// Snapshot tracked instances so we can safely modify the dictionaries.
			var toRemove = new List<string>(s_droppedInstances);
			var removed = new List<string>();

			for (int i = 0; i < toRemove.Count; i++)
			{
				var instanceName = toRemove[i];
				if (string.IsNullOrEmpty(instanceName)) continue;

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
				sb.AppendLine("EnemyDrops: Removed dropped instances from item + itemStatBattery:");
				for (int i = 0; i < removed.Count; i++)
				{
					sb.AppendLine($"  {removed[i]}");
				}
				EnemyDrops.Logger.LogInfo(sb.ToString());
			}
			else
			{
				EnemyDrops.Logger.LogDebug("EnemyDrops: No dropped instances found to remove.");
			}

			ClearForNewLevel();
		}

		public static void ClearForNewLevel()
		{
			if (s_droppedInstances.Count > 0)
			{
				EnemyDrops.Logger.LogDebug($"EnemyDrops: Clearing dropped-instance tracker ({s_droppedInstances.Count} entries).");
			}
			s_droppedInstances.Clear();
		}
	}
}