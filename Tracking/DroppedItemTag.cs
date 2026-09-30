using UnityEngine;

namespace EnemyDrops.Tracking;

/// <summary>
/// Marker component attached to items spawned by EnemyDrops (level drops).
/// Used to track instances across scenes for battery state cleanup.
/// </summary>
internal sealed class DroppedItemTag : MonoBehaviour
{
}