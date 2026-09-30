#nullable enable
using EnemyDrops.Configuration;
using EnemyDrops.Reflection;
using EnemyDrops.Tracking;
using RepoAPI.Items;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EnemyDrops.Drops;

/// <summary>
/// Selects an item table based on enemy difficulty and delegates weighted spawn to RepoAPI ItemProvider.
/// </summary>
internal static class ItemDropper
{
	private static int dropsThisLevel;

	private static readonly string[] excludedEnemyNames =
	{
		"Gnome",
		"Banger"
	};

	private static FieldInfo? enemyParentField;
	private static FieldInfo? enemyNameField;

	/// <summary>
	/// Resets the per-level drop counter at the start of each level.
	/// </summary>
	internal static void ResetForNewLevel()
	{
		dropsThisLevel = 0;
		EnemyDropsPlugin.Logger.LogInfo($"ItemDropper: Drop counter reset for new level. MaxDropsPerLevel={ConfigurationController.MaxDropsPerLevel}");
	}

	/// <summary>
	/// Evaluates enemy difficulty and attempts to spawn a weighted item at the enemy's position.
	/// </summary>
	internal static bool TrySpawnForEnemy(Enemy enemy, out GameObject? spawned, float upwardOffset = 0.15f)
	{
		spawned = null;

		if (!SemiFunc.IsMasterClientOrSingleplayer()) return false;
		if (!enemy)
		{
			EnemyDropsPlugin.Logger.LogDebug("ItemDropper: Enemy null.");
			return false;
		}

		// Enforce per-level drop cap
		if (dropsThisLevel >= ConfigurationController.MaxDropsPerLevel)
		{
			EnemyDropsPlugin.Logger.LogInfo($"ItemDropper: Max drops per level reached ({ConfigurationController.MaxDropsPerLevel}).");
			return false;
		}

		Transform t = enemy.CustomValuableSpawnTransform
			? enemy.CustomValuableSpawnTransform
			: enemy.CenterTransform ? enemy.CenterTransform : enemy.transform;

		Vector3 pos = t.position + (Vector3.up * upwardOffset);
		Quaternion rot = t.rotation;

		int dangerLevel = EnemyDifficultyAccessor.GetDangerLevel(enemy);
		string enemyName = GetEnemyNameSafe(enemy);
		EnemyDropsPlugin.Logger.LogInfo($"ItemDropper: Enemy='{enemyName}' DangerLevel={dangerLevel}");

		if (IsExcludedEnemy(enemyName))
		{
			EnemyDropsPlugin.Logger.LogInfo($"ItemDropper: Excluding enemy '{enemyName}' from drops.");
			return false;
		}

		IReadOnlyList<WeightedKey> table = dangerLevel switch
		{
			1 => ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty1),
			2 => ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty2),
			3 => ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty3),
			_ => ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty1),
		};

		bool success = ItemProvider.TrySpawnWeightedItem(table, pos, rot, out spawned, 0f);
		if (success)
		{
			DroppedInstanceTracker.MarkDropped(spawned!);
			dropsThisLevel++;
		}
		else
		{
			EnemyDropsPlugin.Logger.LogDebug($"ItemDropper: TrySpawnWeightedItem returned false (dangerLevel={dangerLevel}).");
		}

		return success;
	}

	private static bool IsExcludedEnemy(string enemyName)
	{
		if (string.IsNullOrEmpty(enemyName)) return false;
		for (int i = 0; i < excludedEnemyNames.Length; i++)
		{
			if (string.Equals(enemyName, excludedEnemyNames[i], StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetEnemyNameSafe(Enemy enemy)
	{
		try
		{
			enemyParentField ??= typeof(Enemy).GetField("EnemyParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var enemyParentObj = enemyParentField?.GetValue(enemy);
			if (enemyParentObj != null)
			{
				enemyNameField ??= enemyParentObj.GetType().GetField("enemyName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (enemyNameField != null)
				{
					var name = enemyNameField.GetValue(enemyParentObj) as string;
					if (!string.IsNullOrEmpty(name))
					{
						return name!;
					}
				}
			}
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogDebug($"ItemDropper: enemy name reflection failed: {ex.Message}");
		}

		return enemy ? enemy.gameObject.name : "Unknown";
	}
}