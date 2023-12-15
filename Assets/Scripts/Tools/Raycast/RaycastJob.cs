using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using XDPaint.Tools.Raycast.Data;
using XDPaint.Utils;
#if BURST
using Unity.Burst;
#endif

namespace XDPaint.Tools.Raycast
{
#if BURST
    [BurstCompile]
#endif
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<TriangleData> Triangles;
        [WriteOnly] public NativeArray<RaycastTriangleData> OutputData;
        public Vector3 RayOrigin, RayDirection;
        public Vector3 Plane1Position, Plane1Normal, Plane2Position, Plane2Normal, Plane3Position, Plane3Normal;
        public bool SkipPlaneIntersectsTriangle;

        private RaycastTriangleData IsIntersected(int i)
        {
            var result = new RaycastTriangleData
            {
                IntersectPlaneTriangleId = -1,
                RaycastTriangleId = -1
            };
            
            var eps = float.Epsilon;
            var triangle = Triangles[i];
            var p1 = triangle.Position0;
            var p2 = triangle.Position1;
            var p3 = triangle.Position2;
            var e1 = p2 - p1;
            var e2 = p3 - p1;

            var p = Vector3.Cross(RayDirection, e2);
            var det = Vector3.Dot(e1, p);
            if (det.IsNaNOrInfinity() || det > eps && det < -eps)
                return result;

            var invDet = 1.0f / det;
            var t = RayOrigin - p1;
            var u = Vector3.Dot(t, p) * invDet;
            if (u.IsNaNOrInfinity() || u < 0f || u > 1f)
                return result;

            var q = Vector3.Cross(t, e1);
            var v = Vector3.Dot(RayDirection, q) * invDet;
            if (v.IsNaNOrInfinity() || v < 0f || u + v > 1f)
                return result;

            if (Vector3.Dot(e2, q) * invDet > eps)
            {
                result.RaycastTriangleId = triangle.Id;
                result.Hit = p1 + u * e1 + v * e2;
                result.UVHit = triangle.UV0 + (triangle.UV1 - triangle.UV0) * u + (triangle.UV2 - triangle.UV0) * v;
            }
            return result;
        }

        private bool IsPlane1IntersectsTriangle(Vector3 planePosition, Vector3 planeNormal, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var a = Vector3.Dot(v0 - planePosition, planeNormal) >= 0f;
            if (a != Vector3.Dot(v1 - planePosition, planeNormal) >= 0f)
                return false;
            
            return a == Vector3.Dot(v2 - planePosition, planeNormal) >= 0f;
        }
        
        private bool IsPlane2IntersectsTriangle(Vector3 planePosition, Vector3 planeNormal, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v0 - planePosition, planeNormal) >= 0f && 
                   Vector3.Dot(v1 - planePosition, planeNormal) >= 0f && 
                   Vector3.Dot(v2 - planePosition, planeNormal) >= 0f;
        }
        
        private bool IsPlane3IntersectsTriangle(Vector3 planePosition, Vector3 planeNormal, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Dot(v0 - planePosition, planeNormal) < 0 && 
                   Vector3.Dot(v1 - planePosition, planeNormal) < 0 && 
                   Vector3.Dot(v2 - planePosition, planeNormal) < 0;
        }
        
        public void Execute(int index)
        {
            var result = new RaycastTriangleData
            {
                IntersectPlaneTriangleId = -1,
                RaycastTriangleId = -1
            };

            if (SkipPlaneIntersectsTriangle)
            {
                var data = IsIntersected(index);
                result.RaycastTriangleId = data.RaycastTriangleId;
                result.Hit = data.Hit;
                result.UVHit = data.UVHit;
                if (result.RaycastTriangleId >= 0)
                {
                    result.IntersectPlaneTriangleId = result.RaycastTriangleId;
                }
                OutputData[index] = result;
                return;
            }
            
            if (IsPlane1IntersectsTriangle(Plane1Position, Plane1Normal, Triangles[index].Position0, Triangles[index].Position1, Triangles[index].Position2) || 
                IsPlane2IntersectsTriangle(Plane2Position, Plane2Normal, Triangles[index].Position0, Triangles[index].Position1, Triangles[index].Position2) || 
                IsPlane3IntersectsTriangle(Plane3Position, Plane3Normal, Triangles[index].Position0, Triangles[index].Position1, Triangles[index].Position2))
            {
                result.IntersectPlaneTriangleId = -1;
            }
            else
            {
                result.IntersectPlaneTriangleId = index;
                
                var data = IsIntersected(index);
                result.RaycastTriangleId = data.RaycastTriangleId;
                result.Hit = data.Hit;
                result.UVHit = data.UVHit;
            }

            OutputData[index] = result;
        }
    }
}