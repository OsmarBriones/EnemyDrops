using System;
using System.Reflection;

namespace EnemyDrops.Reflection
{
	/// <summary>
	/// Reflection helper to safely extract an enemy's difficulty (danger level) even if EnemyParent or its members are internal.
	/// </summary>
	internal static class EnemyDifficultyAccessor
	{
		private static FieldInfo? _enemyParentField;          // Enemy.EnemyParent
		private static FieldInfo? _difficultyField;           // EnemyParent.difficulty enum field
		private static Type? _enemyType;
		private static Type? _enemyParentType;
		private static bool _initialized;

		private static void Init(Enemy enemy)
		{
			if (_initialized || !enemy) return;
			try
			{
				_enemyType = enemy.GetType();
				_enemyParentField = _enemyType.GetField("EnemyParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var enemyParentObj = _enemyParentField?.GetValue(enemy);
				if (enemyParentObj != null)
				{
					_enemyParentType = enemyParentObj.GetType();
					_difficultyField = _enemyParentType.GetField("difficulty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogDebug($"EnemyDifficultyAccessor.Init reflection failed: {ex.Message}");
			}
			finally
			{
				_initialized = true;
			}
		}

		/// <summary>
		/// Returns difficulty enum boxed (if available) or null.
		/// </summary>
		private static object? GetDifficultyEnum(Enemy enemy)
		{
			Init(enemy);
			if (!enemy || _enemyParentField == null || _difficultyField == null) return null;
			try
			{
				var enemyParentObj = _enemyParentField.GetValue(enemy);
				return enemyParentObj != null ? _difficultyField.GetValue(enemyParentObj) : null;
			}
			catch (Exception ex)
			{
				EnemyDrops.Logger.LogDebug($"EnemyDifficultyAccessor.GetDifficultyEnum failed: {ex.Message}");
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
				// Enum underlying int (Difficulty1=0, Difficulty2=1, Difficulty3=2)
				int raw = (int)Convert.ChangeType(diffEnum, typeof(int));
				return raw + 1; // map to 1..3
			}
			catch
			{
				return 1;
			}
		}
	}
}