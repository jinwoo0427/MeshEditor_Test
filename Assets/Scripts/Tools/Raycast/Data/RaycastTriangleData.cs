using UnityEngine;

namespace XDPaint.Tools.Raycast.Data
{
    public struct RaycastTriangleData
    {
        public int IntersectPlaneTriangleId;
        public int RaycastTriangleId;
        public Vector3 Hit;
        public Vector2 UVHit;
    }
}