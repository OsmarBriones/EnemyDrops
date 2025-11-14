using System;
using System.Collections.Generic;

namespace EnemyDrops.Providers
{
	public static class ItemKeysProvider
	{
		// Source now delegated to ItemKeys.All (single authority for literals).
		private static readonly string[] s_defaultKeys = ItemKeys.All;

		private static readonly IReadOnlyList<string> s_readOnlyKeys = Array.AsReadOnly(s_defaultKeys);
		private static readonly Random s_rng = new Random();

		public static IReadOnlyList<string> Keys => s_readOnlyKeys;

		public static string? GetRandomKey()
		{
			if (s_defaultKeys.Length == 0) return null;
			return s_defaultKeys[s_rng.Next(s_defaultKeys.Length)];
		}
	}
}