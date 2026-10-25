using UnityEngine;

/// <summary>
/// Marks one or more Collider2D components as the walkable floor boundary of a station.
/// Floor colliders should normally be triggers and must not be treated as obstacles.
/// </summary>
public sealed class StationFloorSurface : MonoBehaviour
{
    [SerializeField]
    private Collider2D[] colliders;

    public Collider2D[] Colliders
    {
        get
        {
            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponents<Collider2D>();
            }

            return colliders;
        }
    }

    public bool Contains(Vector2 worldPoint)
    {
        Collider2D[] sources = Colliders;
        for (int i = 0; i < sources.Length; i++)
        {
            Collider2D source = sources[i];
            if (source && source.enabled && source.OverlapPoint(worldPoint))
            {
                return true;
            }
        }

        return false;
    }
}
