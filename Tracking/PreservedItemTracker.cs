#nullable enable
using System.Collections.Generic;

namespace EnemyDrops.Tracking;

/// <summary>
/// Tracks the quantity of currently preserved enemy-dropped items in the player's run.
/// Integrates with StatsManager.dictionaryOfDictionaries so counts serialize and persist with save files.
/// </summary>
internal static class PreservedItemTracker
{
	private const string PreservedDictionaryKey = "enemyDropsPreserved";

	/// <summary>
	/// Gets or creates the dictionary tracking preserved drop counts in StatsManager.
	/// </summary>
	internal static Dictionary<string, int>? GetDictionary()
	{
		var stats = StatsManager.instance;
		if (stats == null) return null;

		if (!stats.dictionaryOfDictionaries.TryGetValue(PreservedDictionaryKey, out var dict))
		{
			dict = new Dictionary<string, int>();
			stats.dictionaryOfDictionaries[PreservedDictionaryKey] = dict;
		}

		return dict;
	}

	/// <summary>
	/// Calculates the total number of preserved enemy-drop items currently active.
	/// </summary>
	internal static int GetTotalPreservedCount()
	{
		var dict = GetDictionary();
		if (dict == null) return 0;

		int total = 0;
		foreach (var kvp in dict)
		{
			if (kvp.Value > 0)
			{
				total += kvp.Value;
			}
		}
		return total;
	}

	/// <summary>
	/// Increments the preserved count for a specific base item name.
	/// </summary>
	internal static void RecordPreservedItem(string baseItemName)
	{
		if (string.IsNullOrEmpty(baseItemName)) return;

		var dict = GetDictionary();
		if (dict == null) return;

		dict[baseItemName] = dict.TryGetValue(baseItemName, out int current) ? current + 1 : 1;
	}

	/// <summary>
	/// Decrements the preserved count when an item instance is destroyed, consumed, or removed.
	/// </summary>
	internal static void OnItemRemoved(string instanceName)
	{
		if (string.IsNullOrEmpty(instanceName)) return;
		string baseItemName = instanceName.Contains("/") ? instanceName.Split('/')[0] : instanceName;

		var dict = GetDictionary();
		if (dict == null) return;

		if (dict.TryGetValue(baseItemName, out int current) && current > 0)
		{
			dict[baseItemName] = current - 1;
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Decremented preserved count for '{baseItemName}' ({current - 1} remaining).");
		}
	}

	/// <summary>
	/// Resets the preserved tracker when a run starts or game is over.
	/// </summary>
	internal static void Reset()
	{
		var dict = GetDictionary();
		if (dict != null && dict.Count > 0)
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Resetting preserved drop count (was {GetTotalPreservedCount()}).");
			dict.Clear();
		}
	}
}
