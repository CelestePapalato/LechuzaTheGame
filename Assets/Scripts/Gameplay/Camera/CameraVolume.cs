using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CameraRegion
{
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
    public Vector2 topRight;
    public Vector2 topLeft;

    public static CameraRegion CreateDefaultSquare(float halfSize = 5f)
    {
        return new CameraRegion
        {
            bottomLeft = new Vector2(-halfSize, -halfSize),
            bottomRight = new Vector2(halfSize, -halfSize),
            topRight = new Vector2(halfSize, halfSize),
            topLeft = new Vector2(-halfSize, halfSize)
        };
    }
}

public class CameraVolume : MonoBehaviour
{
    [SerializeField] private List<CameraRegion> regions = new List<CameraRegion>();

    public bool ContainsWorldPoint(Vector3 worldPosition)
    {
        return GetRegionIndex(worldPosition) >= 0;
    }

    public Vector3 ConstrainPosition(Vector3 worldPosition, Vector3 focusWorldPosition)
    {
        if (regions == null || regions.Count == 0)
            return worldPosition;

        Vector2 point = new Vector2(worldPosition.x, worldPosition.y);

        if (ContainsWorldPoint(worldPosition))
            return worldPosition;

        int focusRegion = GetRegionIndex(focusWorldPosition);
        if (focusRegion >= 0)
            return ClosestOnRegion(worldPosition, focusRegion);

        int cameraRegion = GetRegionIndex(worldPosition);
        if (cameraRegion >= 0)
            return ClosestOnRegion(worldPosition, cameraRegion);

        return ClosestOnUnion(worldPosition);
    }

    private int GetRegionIndex(Vector3 worldPosition)
    {
        if (regions == null || regions.Count == 0)
            return -1;

        Vector2 point = new Vector2(worldPosition.x, worldPosition.y);

        for (int i = 0; i < regions.Count; i++)
        {
            if (ContainsPoint(GetWorldQuad(regions[i]), point))
                return i;
        }

        return -1;
    }

    private Vector3 ClosestOnRegion(Vector3 worldPosition, int regionIndex)
    {
        Vector2 point = new Vector2(worldPosition.x, worldPosition.y);
        Vector2 closest = ClosestPointOnQuad(GetWorldQuad(regions[regionIndex]), point);
        return new Vector3(closest.x, closest.y, worldPosition.z);
    }

    private Vector3 ClosestOnUnion(Vector3 worldPosition)
    {
        Vector2 point = new Vector2(worldPosition.x, worldPosition.y);
        Vector2 best = point;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < regions.Count; i++)
        {
            Vector2 closest = ClosestPointOnQuad(GetWorldQuad(regions[i]), point);
            float distSq = (closest - point).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = closest;
            }
        }

        return new Vector3(best.x, best.y, worldPosition.z);
    }

    private Vector2[] GetWorldQuad(CameraRegion region)
    {
        return new[]
        {
            (Vector2)transform.TransformPoint(region.bottomLeft),
            (Vector2)transform.TransformPoint(region.bottomRight),
            (Vector2)transform.TransformPoint(region.topRight),
            (Vector2)transform.TransformPoint(region.topLeft)
        };
    }

    private static bool ContainsPoint(Vector2[] quad, Vector2 point)
    {
        bool hasPositive = false;
        bool hasNegative = false;

        for (int i = 0; i < quad.Length; i++)
        {
            Vector2 a = quad[i];
            Vector2 b = quad[(i + 1) % quad.Length];
            float cross = (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x);

            if (cross > 0f)
                hasPositive = true;
            if (cross < 0f)
                hasNegative = true;
        }

        return !(hasPositive && hasNegative);
    }

    private static Vector2 ClosestPointOnQuad(Vector2[] quad, Vector2 point)
    {
        if (ContainsPoint(quad, point))
            return point;

        Vector2 best = point;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < quad.Length; i++)
        {
            Vector2 closest = ClosestPointOnSegment(quad[i], quad[(i + 1) % quad.Length], point);
            float distSq = (closest - point).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = closest;
            }
        }

        return best;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        Vector2 ab = b - a;
        float lengthSq = ab.sqrMagnitude;
        if (lengthSq < Mathf.Epsilon)
            return a;

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
        return a + ab * t;
    }

    private void OnDrawGizmosSelected()
    {
        if (regions == null)
            return;

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        float z = transform.position.z;

        for (int i = 0; i < regions.Count; i++)
        {
            Vector2[] quad = GetWorldQuad(regions[i]);
            for (int e = 0; e < quad.Length; e++)
            {
                Vector3 a = new Vector3(quad[e].x, quad[e].y, z);
                Vector3 b = new Vector3(quad[(e + 1) % quad.Length].x, quad[(e + 1) % quad.Length].y, z);
                Gizmos.DrawLine(a, b);
            }
        }
    }
}
