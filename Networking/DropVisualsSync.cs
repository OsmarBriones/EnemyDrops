#nullable enable
using System.Collections;
using EnemyDrops.Tracking;
using EnemyDrops.Visuals;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace EnemyDrops.Networking;

/// <summary>
/// Lets clients that have the mod installed render the drop visuals (tint and aura) on items the host spawned.
/// The host raises a lightweight Photon event with the item's view id; clients without the mod ignore it.
/// </summary>
internal static class DropVisualsSync
{
	// Photon custom event codes must be below 200. Chosen to stay clear of the game's and PUN's own codes.
	private const byte DropVisualsEventCode = 142;
	private const float ResolveTimeoutSeconds = 5f;

	private static bool registered;

	internal static void Register()
	{
		if (registered) return;
		PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
		registered = true;
	}

	internal static void Unregister()
	{
		if (!registered) return;
		PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
		registered = false;
	}

	/// <summary>
	/// Host side: tells the other players which spawned item should get the drop visuals.
	/// </summary>
	internal static void Broadcast(GameObject spawned)
	{
		if (!SemiFunc.IsMultiplayer() || !SemiFunc.IsMasterClientOrSingleplayer()) return;

		var view = spawned.GetComponent<PhotonView>() ?? spawned.GetComponentInParent<PhotonView>();
		if (!view || view.ViewID == 0) return;

		var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
		PhotonNetwork.RaiseEvent(DropVisualsEventCode, view.ViewID, options, SendOptions.SendReliable);
	}

	private static void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code != DropVisualsEventCode) return;
		if (PhotonNetwork.IsMasterClient) return;
		if (photonEvent.CustomData is not int viewId) return;

		EnemyDropsPlugin.Instance.StartCoroutine(ApplyWhenSpawned(viewId));
	}

	// The event can arrive before the item has been instantiated on this client, so wait for it.
	private static IEnumerator ApplyWhenSpawned(int viewId)
	{
		float deadline = Time.unscaledTime + ResolveTimeoutSeconds;
		PhotonView? view = PhotonView.Find(viewId);
		while (!view && Time.unscaledTime < deadline)
		{
			yield return null;
			view = PhotonView.Find(viewId);
		}

		if (!view)
		{
			EnemyDropsPlugin.Logger.LogDebug($"DropVisualsSync: View {viewId} never appeared; skipping visuals.");
			yield break;
		}

		if (!view.GetComponent<EnemyDropVisuals>())
		{
			view.gameObject.AddComponent<EnemyDropVisuals>();
		}
	}
}
