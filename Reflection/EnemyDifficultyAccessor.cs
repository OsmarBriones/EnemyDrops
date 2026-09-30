#nullable enable
using System;
using System.Reflection;

namespace EnemyDrops.Reflection;

/// <summary>
/// Reflection helper to safely extract an enemy's danger level even if EnemyParent or its members are internal.
/// </summary>
internal static class EnemyDifficultyAccessor
{
	private static FieldInfo? enemyParentField;
	private static FieldInfo? difficultyField;
	private static Type? enemyType;
	private static Type? enemyParentType;
	private static bool initialized;

	private static void Init(Enemy enemy)
	{
		if (initialized || !enemy) return;
		try
		{
			enemyType = enemy.GetType();
			enemyParentField = enemyType.GetField("EnemyParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var enemyParentObj = enemyParentField?.GetValue(enemy);
			if (enemyParentObj != null)
			{
				enemyParentType = enemyParentObj.GetType();
				difficultyField = enemyParentType.GetField("difficulty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDifficultyAccessor.Init reflection failed: {ex.Message}");
		}
		finally
		{
			initialized = true;
		}
	}

	/// <summary>
	/// Returns difficulty enum boxed (if available) or null.
	/// </summary>
	private static object? GetDifficultyEnum(Enemy enemy)
	{
		Init(enemy);
		if (!enemy || enemyParentField == null || difficultyField == null) return null;
		try
		{
			var enemyParentObj = enemyParentField.GetValue(enemy);
			return enemyParentObj != null ? difficultyField.GetValue(enemyParentObj) : null;
		}
		catch (Exception ex)
		{
			EnemyDropsPlugin.Logger.LogDebug($"EnemyDifficultyAccessor.GetDifficultyEnum failed: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Returns numeric danger level 1..3 derived from difficulty enum (Difficulty1=0 => 1, etc.). Falls back to 1.
	/// </summary>
	public static int GetDangerLevel(Enemy enemy)
	{
		try
		{
			var diffEnum = GetDifficultyEnum(enemy);
			if (diffEnum == null) return 1;
			int raw = (int)Convert.ChangeType(diffEnum, typeof(int));
			return raw + 1; // map to 1..3 
		}
		catch
		{
			return 1;
		}
	}
}