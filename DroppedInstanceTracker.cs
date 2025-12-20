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
				EnemyDrops.Logger.LogWarning("EnemyDrops: StatsManager.instance is null; cannot clear dropped batteries.");
				return;
			}

			var map = stats.itemStatBattery;
			if (map == null || map.Count == 0)
			{
				EnemyDrops.Logger.LogDebug("EnemyDrops: itemStatBattery empty; nothing to clear.");
				ClearForNewLevel();
				return;
			}

			var removed = new List<string>();
			var keys = new List<string>(map.Keys); // snapshot to avoid modifying during enumeration
			for (int i = 0; i < keys.Count; i++)
			{
				var key = keys[i];
				if (IsDropped(key))
				{
					map.Remove(key);
					removed.Add(key);
				}
			}

			if (removed.Count > 0)
			{
				var sb = new StringBuilder();
				sb.AppendLine("EnemyDrops: Cleared batteries for dropped instances:");
				for (int i = 0; i < removed.Count; i++)
				{
					sb.AppendLine($"  {removed[i]}");
				}
				EnemyDrops.Logger.LogInfo(sb.ToString());
			}
			else
			{
				EnemyDrops.Logger.LogDebug("EnemyDrops: No dropped instances found to clear.");
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