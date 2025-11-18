using BepInEx;
using BepInEx.Logging;
using EnemyDrops.Configuration;
using HarmonyLib;
using UnityEngine;

namespace EnemyDrops
{
	[BepInPlugin("osmarbriones.EnemyDrops", "EnemyDrops", "1.0.0")]
	public class EnemyDrops : BaseUnityPlugin
	{
		internal static EnemyDrops Instance { get; private set; } = null!;
		internal new static ManualLogSource Logger => Instance._logger;
		private ManualLogSource _logger => base.Logger;
		internal Harmony? Harmony { get; set; }

		private void Awake()
		{
			Instance = this;

			// Prevent the plugin from being deleted
			this.gameObject.transform.parent = null;
			this.gameObject.hideFlags = HideFlags.HideAndDontSave;

			// Centralized configuration initialization
			ConfigurationController.Initialize(this.Config, Logger);

			Patch();

			Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
		}

		internal void Patch()
		{
			Harmony ??= new Harmony(Info.Metadata.GUID);
			Harmony.PatchAll();
		}

		internal void Unpatch()
		{
			Harmony?.UnpatchSelf();
		}

		private void Update()
		{
			// Code that runs every frame goes here
		}
	}
}