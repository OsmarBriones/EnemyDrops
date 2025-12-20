using HarmonyLib;
using Photon.Pun;

namespace EnemyDrops.Patches
{
	[HarmonyPatch(typeof(PunManager), "SetItemNameLOGIC")]
	internal static class PunManager_SetItemNameLOGIC_Patch
	{
		private static void Postfix(string name, int photonViewID, ItemAttributes _itemAttributes)
		{
			var itemAttributes = _itemAttributes;
			if (SemiFunc.IsMultiplayer())
			{
				var pv = PhotonView.Find(photonViewID);
				if (pv != null)
				{
					itemAttributes = pv.GetComponent<ItemAttributes>();
				}
			}

			if (!itemAttributes) return;
			if (!itemAttributes.GetComponent<DroppedItemTag>()) return;

			DroppedInstanceTracker.RegisterInstance(name);
		}
	}
}