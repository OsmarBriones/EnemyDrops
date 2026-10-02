#nullable enable
using RepoAPI.Items;
using System.Collections.Generic;

namespace EnemyDrops.Tracking;

/// <summary>
/// Tracks the quantity of currently preserved enemy-dropped items in the player's run.
/// Integrates with StatsManager.dictionaryOfDictionaries via RepoAPI.Items.ItemPreservation.
/// </summary>
internal static class PreservedItemTracker
{
	private const string PreservedDictionaryKey = "enemyDropsPreserved";

	/// <summary>
	/// Gets or creates the dictionary tracking preserved drop counts in StatsManager.
	/// </summary>
	internal static Dictionary<string, int>? GetDictionary()
	{
		return ItemPreservation.GetOrCreateStatsDictionary(PreservedDictionaryKey);
	}

	/// <summary>
	/// Calculates the total number of preserved enemy-drop items currently active.
	/// </summary>
	internal static int GetTotalPreservedCount()
	{
		return ItemPreservation.GetTotalCount(PreservedDictionaryKey);
	}

	/// <summary>
	/// Increments the preserved count for a specific base item name.
	/// </summary>
	internal static void RecordPreservedItem(string baseItemName)
	{
		ItemPreservation.RecordCount(PreservedDictionaryKey, baseItemName);
	}

	/// <summary>
	/// Decrements the preserved count when an item instance is destroyed, consumed, or removed.
	/// </summary>
	internal static void OnItemRemoved(string instanceName)
	{
		if (ItemPreservation.DecrementCount(PreservedDictionaryKey, instanceName, out string baseItemName, out int remaining))
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Decremented preserved count for '{baseItemName}' ({remaining} remaining).");
		}
	}

	/// <summary>
	/// Resets the preserved tracker when a run starts or game is over.
	/// </summary>
	internal static void Reset()
	{
		EnemyDropsPlugin.Logger.LogDebug($"EnemyDrops: Resetting preserved drop count (was {GetTotalPreservedCount()}).");
		ItemPreservation.ResetCounts(PreservedDictionaryKey);
	}
}
