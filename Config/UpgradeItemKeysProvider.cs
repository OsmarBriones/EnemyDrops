using System;
using System.Collections.Generic;

namespace EnemyDrops.Configuration
{
	public static class UpgradeItemKeysProvider
	{
		private static readonly string[] s_defaultKeys =
		{
			"Item Upgrade Death Head Battery",
			"Item Upgrade Map Player Count",
			"Item Upgrade Player Crouch Rest",
			"Item Upgrade Player Energy",
			"Item Upgrade Player Extra Jump",
			"Item Upgrade Player Grab Range",
			"Item Upgrade Player Grab Strength",
			"Item Upgrade Player Health",
			"Item Upgrade Player Sprint Speed",
			"Item Upgrade Player Tumble Climb",
			"Item Upgrade Player Tumble Launch",
			"Item Upgrade Player Tumble Wings",
		};

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