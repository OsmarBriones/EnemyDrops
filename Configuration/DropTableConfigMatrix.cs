#nullable enable
using BepInEx.Configuration;
using EnemyDrops.Drops;
using RepoAPI.Items;
using System;
using System.Collections.Generic;

namespace EnemyDrops.Configuration;

internal sealed class DropTableConfigMatrix
{
	// Edit these constants to change the allowed range for all drop table weights.
	internal const int DropWeightMin = 0;
	internal const int DropWeightMax = 12;

	private readonly Dictionary<string, ConfigEntry<int>> entriesDifficulty1 = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, ConfigEntry<int>> entriesDifficulty2 = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, ConfigEntry<int>> entriesDifficulty3 = new(StringComparer.OrdinalIgnoreCase);

	private readonly IReadOnlyList<WeightedKey> weightsDifficulty1;
	private readonly IReadOnlyList<WeightedKey> weightsDifficulty2;
	private readonly IReadOnlyList<WeightedKey> weightsDifficulty3;

	public DropTableConfigMatrix(
		ConfigFile config,
		IReadOnlyList<WeightedKey> defaultsDifficulty1,
		IReadOnlyList<WeightedKey> defaultsDifficulty2,
		IReadOnlyList<WeightedKey> defaultsDifficulty3)
	{
		if (config is null) throw new ArgumentNullException(nameof(config));
		if (defaultsDifficulty1 is null) throw new ArgumentNullException(nameof(defaultsDifficulty1));
		if (defaultsDifficulty2 is null) throw new ArgumentNullException(nameof(defaultsDifficulty2));
		if (defaultsDifficulty3 is null) throw new ArgumentNullException(nameof(defaultsDifficulty3));

		// Build default lookup (missing => 0)
		var map1 = ToMap(defaultsDifficulty1);
		var map2 = ToMap(defaultsDifficulty2);
		var map3 = ToMap(defaultsDifficulty3);

		// Enforce range [DropWeightMin..DropWeightMax] for all weights via ConfigDescription (int)
		var range = new AcceptableValueRange<int>(DropWeightMin, DropWeightMax);

		foreach (var key in ItemKeys.All)
		{
			// Round float defaults to int and clamp
			int d1 = Clamp((int)Math.Round(map1.TryGetValue(key, out var v1) ? v1 : 0f), DropWeightMin, DropWeightMax);
			int d2 = Clamp((int)Math.Round(map2.TryGetValue(key, out var v2) ? v2 : 0f), DropWeightMin, DropWeightMax);
			int d3 = Clamp((int)Math.Round(map3.TryGetValue(key, out var v3) ? v3 : 0f), DropWeightMin, DropWeightMax);

			entriesDifficulty1[key] = config.Bind(
				"Easy Monsters (Elsa for example)",
				key,
				d1,
				new ConfigDescription($"Weight for \"{key}\" on easy monsters. (range {DropWeightMin}..{DropWeightMax})", range));

			entriesDifficulty2[key] = config.Bind(
				"Med. Monsters (Chef for example)",
				key,
				d2,
				new ConfigDescription($"Weight for \"{key}\" on medium monsters. (range {DropWeightMin}..{DropWeightMax})", range));

			entriesDifficulty3[key] = config.Bind(
				"Hard Monsters (Robe for example)",
				key,
				d3,
				new ConfigDescription($"Weight for \"{key}\" on hard monsters. (range {DropWeightMin}..{DropWeightMax})", range));
		}

		// Read once (runtime reload handled by re-creating this matrix)
		weightsDifficulty1 = BuildWeights(entriesDifficulty1);
		weightsDifficulty2 = BuildWeights(entriesDifficulty2);
		weightsDifficulty3 = BuildWeights(entriesDifficulty3);
	}

	public IReadOnlyList<WeightedKey> Get(EnemyParent.Difficulty difficulty)
	{
		return difficulty switch
		{
			EnemyParent.Difficulty.Difficulty1 => weightsDifficulty1,
			EnemyParent.Difficulty.Difficulty2 => weightsDifficulty2,
			_ => weightsDifficulty3,
		};
	}

	private static Dictionary<string, float> ToMap(IReadOnlyList<WeightedKey> list)
	{
		var map = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < list.Count; i++)
		{
			map[list[i].Key] = list[i].Weight;
		}
		return map;
	}

	private static IReadOnlyList<WeightedKey> BuildWeights(Dictionary<string, ConfigEntry<int>> byKey)
	{
		// Maintain ItemKeys.All order
		var arr = new WeightedKey[ItemKeys.All.Length];
		for (int i = 0; i < ItemKeys.All.Length; i++)
		{
			var key = ItemKeys.All[i];
			// Defensive clamp in case of external edits before validation applies
			int iv = Clamp(byKey[key].Value, DropWeightMin, DropWeightMax);
			arr[i] = new WeightedKey(key, iv);
		}
		return arr;
	}

	private static int Clamp(int value, int min, int max) =>
		value < min ? min : (value > max ? max : value);
}