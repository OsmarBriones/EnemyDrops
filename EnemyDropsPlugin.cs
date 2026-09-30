#nullable enable
using BepInEx;
using BepInEx.Logging;
using EnemyDrops.Configuration;
using HarmonyLib;
using UnityEngine;

namespace EnemyDrops;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class EnemyDropsPlugin : BaseUnityPlugin
{
	public const string PluginGuid = "osmarbriones.EnemyDrops";
	public const string PluginName = "EnemyDrops";
	public const string PluginVersion = "1.2.0";

	internal static EnemyDropsPlugin Instance { get; private set; } = null!;
	internal static new ManualLogSource Logger { get; private set; } = null!;
	internal Harmony? Harmony { get; private set; }

	private void Awake()
	{
		Instance = this;
		Logger = base.Logger;

		// Prevent the plugin from being deleted across scene transitions
		gameObject.transform.parent = null;
		gameObject.hideFlags = HideFlags.HideAndDontSave;

		// Centralized configuration initialization
		ConfigurationController.Initialize(Config, Logger);

		Patch();

		Logger.LogInfo($"{PluginName} v{PluginVersion} loaded!");
	}

	internal void Patch()
	{
		Harmony ??= new Harmony(PluginGuid);
		Harmony.PatchAll();
	}

	internal void Unpatch()
	{
		Harmony?.UnpatchSelf();
	}
}