using EnemyDrops.Configuration;
using EnemyDrops.Providers;
using EnemyDrops.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EnemyDrops
{
	/// Selects an item key based on enemy difficulty (weighted) and delegates spawn to ItemProvider.
	public static class ItemDropper
	{
		private static readonly System.Random s_rng = new System.Random();

		// Per-level drop counter
		private static int s_dropsThisLevel;

		// Centralized excluded enemy names (exact strings).
		private static readonly string[] s_excludedEnemyNames = {
			"Gnome",
			"Banger"
		};

		// Cached reflection for enemy name lookup when EnemyParent is not publicly accessible
		private static FieldInfo? s_enemyParentField;
		private static FieldInfo? s_enemyNameField;

		// Called by level-start patch
		internal static void ResetForNewLevel()
		{
			s_dropsThisLevel = 0;
			EnemyDrops.Logger.LogInfo($"ItemDropper: Drop counter reset for new level. MaxDropsPerLevel={ConfigurationController.MaxDropsPerLevel}");
		}

		public static bool TrySpawnForEnemy(Enemy enemy, out GameObject? spawned, float upwardOffset = 0.15f)
		{
			spawned = null;

			if (!SemiFunc.IsMasterClientOrSingleplayer()) return false;
			if (!enemy)
			{
				EnemyDrops.Logger.LogDebug("ItemDropper: Enemy null.");
				return false;
			}

			// Enforce per-level drop cap
			if (s_dropsThisLevel >= ConfigurationController.MaxDropsPerLevel)
			{
				EnemyDrops.Logger.LogInfo($"ItemDropper:  Max drops per level reached ({ConfigurationController.MaxDropsPerLevel}).");
				return false;
			}

			Transform t = enemy.CustomValuableSpawnTransform
				? enemy.CustomValuableSpawnTransform
				: enemy.CenterTransform ? enemy.CenterTransform : enemy.transform;

			Vector3 pos = t.position + (Vector3.up * upwardOffset);
			Quaternion rot = t.rotation;

			// Reflection-based danger level (1..3)
			int dangerLevel = EnemyDifficultyAccessor.GetDangerLevel(enemy);

			// Log enemy name and danger level
			string enemyName = GetEnemyNameSafe(enemy);
			EnemyDrops.Logger.LogInfo($"ItemDropper: Enemy='{enemyName}' DangerLevel={dangerLevel}");

			// Exclusions
			if (IsExcludedEnemy(enemyName))
			{
				EnemyDrops.Logger.LogInfo($"ItemDropper: Excluding enemy '{enemyName}' from drops.");
				return false;
			}

			// Map dangerLevel -> EnemyParent.Difficulty if available; if enum inaccessible just branch on int.
			IReadOnlyList<WeightedKey> table;
			switch (dangerLevel)
			{
				case 1: table = ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty1); break;
				case 2: table = ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty2); break;
				case 3: table = ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty3); break;
				default: table = ItemDropTables.GetWeightsFor(EnemyParent.Difficulty.Difficulty1); break;
			}

			string? key = PickWeightedKey(table);
			if (string.IsNullOrEmpty(key))
			{
				EnemyDrops.Logger.LogWarning($"ItemDropper: No key picked (dangerLevel={dangerLevel}).");
				return false;
			}

			bool success = ItemProvider.TrySpawnByKey(key, pos, rot, out spawned, 0f);
			if (success)
			{
				DroppedInstanceTracker.MarkDropped(spawned!);
				s_dropsThisLevel++;
			}
			return success;
		}

		// Centralized enemy exclusion rule(s)
		private static bool IsExcludedEnemy(string enemyName)
		{
			if (string.IsNullOrEmpty(enemyName)) return false;
			for (int i = 0; i < s_excludedEnemyNames.Length; i++)
			{
				if (string.Equals(enemyName, s_excludedEnemyNames[i], StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		private static string GetEnemyNameSafe(Enemy enemy)
		{
			try
			{
				s_enemyParentField ??= typeof(Enemy).GetField("EnemyParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var enemyParentObj = s_enemyParentField?.GetValue(enemy);
				if (enemyParentObj != null)
				{
					// Cache the name field once we know the exact runtime type
					s_enemyNameField ??= enemyParentObj.GetType().GetField("enemyName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (s_enemyNameField != null)
					{
						var name = s_enemyNameField.GetValue(enemyParentObj) as string;
						if (!string.IsNullOrEmpty(name))
							return name!;
					}
				}
			}
			catch (System.Exception ex)
			{
				EnemyDrops.Logger.LogDebug($"ItemDropper: enemy name reflection failed: {ex.Message}");
			}
			// Fallback to Unity object name
			return enemy ? enemy.gameObject.name : "Unknown";
		}

		private static string? PickWeightedKey(IReadOnlyList<WeightedKey> weights)
		{
			if (weights == null || weights.Count == 0) return null;

			float total = 0f;
			foreach (var w in weights)
			{
				if (w.Weight > 0f) total += w.Weight;
			}
			if (total <= 0f) return null;

			double roll = s_rng.NextDouble() * total;
			float accum = 0f;
			for (int i = 0; i < weights.Count; i++)
			{
				var w = weights[i];
				if (w.Weight <= 0f) continue;
				accum += w.Weight;
				if (roll <= accum)
				{
					return w.Key;
				}
			}
			for (int i = weights.Count - 1; i >= 0; i--)
			{
				if (weights[i].Weight > 0f) return weights[i].Key;
			}
			return null;
		}
	}
}