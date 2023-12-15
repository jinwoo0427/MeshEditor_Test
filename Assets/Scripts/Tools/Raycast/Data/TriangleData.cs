using UnityEngine;

namespace XDPaint.Tools.Raycast.Data
{
    public struct TriangleData
    {
        public int Id;
        public Vector3 Position0;
        public Vector3 Position1;
        public Vector3 Position2;
        public Vector2 UV0;
        public Vector2 UV1;
        public Vector2 UV2;
        
        public Vector2 GetUV(Vector3 position)
        {
            var distance0 = Position0 - position;
            var distance1 = Position1 - position;
            var distance2 = Position2 - position;
            //calculate the areas
            var va = Vector3.Cross(Position0 - Position1, Position0 - Position2);
            var va1 = Vector3.Cross(distance1, distance2);
            var va2 = Vector3.Cross(distance2, distance0);
            var va3 = Vector3.Cross(distance0, distance1);
            var area = va.magnitude;
            //calculate barycentric with sign
            var a1 = va1.magnitude / area * Mathf.Sign(Vector3.Dot(va, va1));
            var a2 = va2.magnitude / area * Mathf.Sign(Vector3.Dot(va, va2));
            var a3 = va3.magnitude / area * Mathf.Sign(Vector3.Dot(va, va3));
            var uv = UV0 * a1 + UV1 * a2 + UV2 * a3;
            return uv;
        }
    }
}