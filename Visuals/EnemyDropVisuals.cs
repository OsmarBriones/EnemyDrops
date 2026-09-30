#nullable enable
using EnemyDrops.Configuration;
using System.Collections;
using UnityEngine;

namespace EnemyDrops.Visuals;

/// <summary>
/// Manages the visual styling for items dropped by enemies.
/// Applies a color modification (multiply and additive tint) preserving textures and details,
/// and attaches a centered reddish light aura to the item.
/// </summary>
internal sealed class EnemyDropVisuals : MonoBehaviour
{
	private static readonly Color FaintRedLow = new(0.9f, 0.12f, 0.12f);
	private static readonly Color FaintRedHigh = new(1.0f, 0.28f, 0.28f);

	private GameObject? auraLightObject;
	private Light? auraLight;

	private PhysGrabObject? physGrabObject;
	private ItemEquippable? itemEquippable;

	private bool visualsApplied;

	private IEnumerator Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>() ?? GetComponentInParent<PhysGrabObject>();
		itemEquippable = GetComponent<ItemEquippable>() ?? GetComponentInParent<ItemEquippable>();

		// Apply immediately on spawn
		ApplyVisuals();

		// Wait one frame to ensure any late-initializing item scripts (e.g. ItemDrone, ItemOrb) have executed,
		// then re-apply to guarantee all parts remain modified
		yield return null;
		ApplyVisuals();
	}

	internal void ApplyVisuals()
	{
		if (ConfigurationController.EnableColorModification)
		{
			ApplyMaterialColorModification();
		}

		if (ConfigurationController.EnableDropAura)
		{
			SetupAuraLight();
		}
		else if (auraLight != null && auraLight.enabled)
		{
			auraLight.enabled = false;
		}

		visualsApplied = true;
	}

	private void ApplyMaterialColorModification()
	{
		float multR = ConfigurationController.ColorMultiplierR;
		float multG = ConfigurationController.ColorMultiplierG;
		float multB = ConfigurationController.ColorMultiplierB;
		float add = ConfigurationController.ColorAdd;

		var renderers = GetComponentsInChildren<Renderer>(true);
		int modifiedRenderers = 0;

		for (int r = 0; r < renderers.Length; r++)
		{
			var renderer = renderers[r];
			if (!renderer) continue;

			// Only modify actual 3D model mesh renderers
			if (renderer is not MeshRenderer && renderer is not SkinnedMeshRenderer)
			{
				continue;
			}

			var mats = renderer.materials;
			bool modified = false;

			for (int m = 0; m < mats.Length; m++)
			{
				var mat = mats[m];
				if (!mat) continue;

				// Read original color (preserving textures so labels, screens, and patterns stay intact)
				Color originalColor = mat.HasProperty("_Color") ? mat.GetColor("_Color") : mat.color;

				Color newColor = new(
					Mathf.Clamp01((originalColor.r * multR) + add),
					Mathf.Clamp01((originalColor.g * multG) + add),
					Mathf.Clamp01((originalColor.b * multB) + add),
					originalColor.a
				);

				mat.color = newColor;

				if (mat.HasProperty("_Color")) mat.SetColor("_Color", newColor);
				if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", newColor);

				// Scale emission color proportionally if present
				if (mat.HasProperty("_EmissionColor"))
				{
					Color origEmission = mat.GetColor("_EmissionColor");
					Color newEmission = new(
						Mathf.Clamp01(origEmission.r * multR),
						Mathf.Clamp01(origEmission.g * multG),
						Mathf.Clamp01(origEmission.b * multB),
						origEmission.a
					);
					mat.SetColor("_EmissionColor", newEmission);
				}

				modified = true;
			}

			if (modified)
			{
				renderer.materials = mats;
				modifiedRenderers++;
			}
		}

		if (!visualsApplied)
		{
			EnemyDropsPlugin.Logger.LogInfo($"EnemyDropVisuals: Applied material color modification (mult=[{multR:F2},{multG:F2},{multB:F2}], add={add:F2}) to {modifiedRenderers} renderer(s) on '{gameObject.name}'.");
		}
	}

	private void SetupAuraLight()
	{
		if (auraLight != null) return;

		// Disable native white ItemLight if present so it doesn't conflict
		var vanillaItemLight = GetComponentInChildren<ItemLight>();
		if (vanillaItemLight != null && vanillaItemLight.itemLight != null)
		{
			vanillaItemLight.itemLight.enabled = false;
		}

		auraLightObject = new GameObject("EnemyDrop_AuraLight");
		auraLightObject.transform.SetParent(transform, false);

		// Compute the exact geometric center of all model renderers so the light is perfectly centered in the object
		Vector3 localCenter = Vector3.zero;
		var renderers = GetComponentsInChildren<Renderer>(true);
		bool boundsFound = false;
		Bounds combinedBounds = default;

		for (int i = 0; i < renderers.Length; i++)
		{
			var r = renderers[i];
			if (r is not MeshRenderer && r is not SkinnedMeshRenderer) continue;

			if (!boundsFound)
			{
				combinedBounds = r.bounds;
				boundsFound = true;
			}
			else
			{
				combinedBounds.Encapsulate(r.bounds);
			}
		}

		if (boundsFound)
		{
			localCenter = transform.InverseTransformPoint(combinedBounds.center);
		}

		auraLightObject.transform.localPosition = localCenter;

		auraLight = auraLightObject.AddComponent<Light>();
		auraLight.type = LightType.Point;
		auraLight.color = FaintRedHigh;
		auraLight.range = ConfigurationController.AuraRange;
		auraLight.intensity = ConfigurationController.AuraIntensity;
		auraLight.shadows = LightShadows.None;
		auraLight.renderMode = LightRenderMode.ForcePixel;
		auraLight.cullingMask = ~0;
		auraLight.enabled = true;

		EnemyDropsPlugin.Logger.LogInfo($"EnemyDropVisuals: Attached centered aura light at local offset {localCenter} on '{gameObject.name}'.");
	}

	private void Update()
	{
		if (auraLight == null || !ConfigurationController.EnableDropAura) return;

		bool isGrabbed = physGrabObject != null && (physGrabObject.grabbed || physGrabObject.grabbedLocal);
		bool isEquipped = itemEquippable != null && itemEquippable.IsEquipped();
		bool shouldShow = !isGrabbed && !isEquipped;

		if (auraLight.enabled != shouldShow)
		{
			auraLight.enabled = shouldShow;
		}

		if (shouldShow)
		{
			float pulse = (Mathf.Sin(Time.time * 2.5f) + 1f) * 0.5f;
			auraLight.color = Color.Lerp(FaintRedLow, FaintRedHigh, pulse);
			auraLight.intensity = ConfigurationController.AuraIntensity * (0.85f + (0.3f * pulse));
			auraLight.range = ConfigurationController.AuraRange;
		}
	}

	private void OnDestroy()
	{
		if (auraLightObject != null)
		{
			Destroy(auraLightObject);
		}
	}
}
