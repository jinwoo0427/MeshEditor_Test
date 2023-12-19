using System;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools.Raycast;

namespace XDPaint.Utils
{
    public static class MathHelper
    {
        /// <summary>
        /// UV 교차점 가져오기
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="ray"></param>
        /// <returns></returns>
        public static Vector2 GetIntersectionUV(Triangle triangle, Ray ray)
        {
            // 삼각형의 세 꼭지점과 UV 좌표를 가져옵니다.
            var p1 = triangle.Position0;
            var p2 = triangle.Position1;
            var p3 = triangle.Position2;
            // 삼각형의 두 변 벡터를 계산합니다.
            var e1 = p2 - p1;
            var e2 = p3 - p1;

            // 광선 방향과 외적을 계산합니다.
            var p = Vector3.Cross(ray.direction, e2);
            // 내적을 계산합니다.
            var det = Vector3.Dot(e1, p);

            // det의 역수를 계산합니다.
            var invDet = 1.0f / det;
            // t, u, v를 계산
            var t = ray.origin - p1;
            var u = Vector3.Dot(t, p) * invDet;
            var q = Vector3.Cross(t, e1);
            var v = Vector3.Dot(ray.direction, q) * invDet;
            var result = triangle.UV0 + (triangle.UV1 - triangle.UV0) * u + (triangle.UV2 - triangle.UV0) * v;
            return result;
        }

        private static readonly KeyValuePair<int, Vector3>[] IntersectionsEdges = new KeyValuePair<int, Vector3>[3];
        public static Vector3 GetExitPointFromTriangle(Camera camera, Triangle triangle, Vector3 firstHit, Vector3 lastHit, Vector3 normal)
        {
            var intersectionEdge3 = Vector3.zero;
            var isIntersectedEdge3 = false;
            var isIntersectedEdge1 = IsPlaneIntersectLine(normal, firstHit, triangle.Position0, triangle.Position1, out var intersectionEdge1);
            var isIntersectedEdge2 = IsPlaneIntersectLine(normal, firstHit, triangle.Position1, triangle.Position2, out var intersectionEdge2);
            if (!isIntersectedEdge1 || !isIntersectedEdge2)
            {
                isIntersectedEdge3 = IsPlaneIntersectLine(normal, firstHit, triangle.Position0, triangle.Position2, out intersectionEdge3);
            }

            var intersectionsCount = isIntersectedEdge1 ? 1 : 0;
            intersectionsCount += isIntersectedEdge2 ? 1 : 0;
            intersectionsCount += isIntersectedEdge3 ? 1 : 0;
            if (intersectionsCount == 0)
            {
#if XDP_DEBUG
                Debug.LogWarning("Can't find intersection. Zero intersections");
#endif
                return firstHit;
            }

            var filled = 0;
            if (isIntersectedEdge1)
            {
                IntersectionsEdges[filled] = new KeyValuePair<int, Vector3>(filled, intersectionEdge1);
                filled++;
            }
            
            if (isIntersectedEdge2)
            {
                IntersectionsEdges[filled] = new KeyValuePair<int, Vector3>(filled, intersectionEdge2);
                filled++;
            }
            
            if (isIntersectedEdge3)
            {
                IntersectionsEdges[filled] = new KeyValuePair<int, Vector3>(filled, intersectionEdge3);
            }

            var indexEdge = 0;
            if (intersectionsCount == 1)
            {
                indexEdge = IntersectionsEdges[0].Key;
            }
            else if (intersectionsCount == 2)
            {
                var p0 = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(IntersectionsEdges[0].Value));
                var p1 = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(IntersectionsEdges[1].Value));
                var pEnd = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(lastHit));
                var distFirst = Vector3.Distance(p0, pEnd);
                var distLast = Vector3.Distance(p1, pEnd);

                indexEdge = distFirst < distLast ? IntersectionsEdges[0].Key : IntersectionsEdges[1].Key;
            }
            else
            {
                var p0 = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(IntersectionsEdges[0].Value));
                var p1 = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(IntersectionsEdges[1].Value));
                var p2 = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(IntersectionsEdges[2].Value));
                var pEnd = camera.WorldToScreenPoint(triangle.Transform.TransformPoint(lastHit));
                var dist1 = Vector3.Distance(p0, pEnd);
                var dist2 = Vector3.Distance(p1, pEnd);
                var dist3 = Vector3.Distance(p2, pEnd);
                Vector3 resultVector;

                if (dist1 < dist2)
                {
                    indexEdge = IntersectionsEdges[0].Key;
                    resultVector = p0;
                }
                else
                {
                    indexEdge = IntersectionsEdges[1].Key;
                    resultVector = p1;
                }
                if (Vector3.Distance(resultVector, pEnd) > dist3)
                {
                    indexEdge = IntersectionsEdges[2].Key;
                }
            }
            return IntersectionsEdges[indexEdge].Value;
        }

        //Cubic Bezier 곡선의 수식 사용 ( 걍 공식임. 외우자...;;; )
        public static Vector2 Interpolate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            // t의 제곱 및 세제곱 값을 미리 계산
            var t2 = t * t;
            var t3 = t2 * t;

            // Cubic Bezier Curve의 x 및 y 좌표 계산
            //
            var x = 0.5f * (2 * p1.x + (-p0.x + p2.x) * t + (2 * p0.x - 5 * p1.x + 4 * p2.x - p3.x) * t2 +
                            (-p0.x + 3 * p1.x - 3 * p2.x + p3.x) * t3);
            var y = 0.5f * (2 * p1.y + (-p0.y + p2.y) * t + (2 * p0.y - 5 * p1.y + 4 * p2.y - p3.y) * t2 +
                            (-p0.y + 3 * p1.y - 3 * p2.y + p3.y) * t3);
            return new Vector2(x, y);
        }
        
        public static float CalculateAreaOfTriangle(Vector3 p0, Vector3 p1 , Vector3 p2)
        {
            var res = Mathf.Pow(p1.x * p0.y - p2.x * p0.y - p0.x * p1.y + p2.x * p1.y + p0.x * p2.y - p1.x * p2.y, 2.0f);
            res += Mathf.Pow(p1.x * p0.z - p2.x * p0.z - p0.x * p1.z + p2.x * p1.z + p0.x * p2.z - p1.x * p2.z, 2.0f);
            res += Mathf.Pow(p1.y * p0.z - p2.y * p0.z - p0.y * p1.z + p2.y * p1.z + p0.y * p2.z - p1.y * p2.z, 2.0f);
            return Mathf.Sqrt(res) * 0.5f;
        }

        private static bool IsPlaneIntersectLine(Vector3 n, Vector3 a, Vector3 w, Vector3 v, out Vector3 p)
        {
            p = Vector3.zero;
            var dotProduct = Vector3.Dot(n, w - v);
            if (Math.Abs(dotProduct) < float.Epsilon)
                return false;
            
            var dot1 = Vector3.Dot(n, a - v);
            var t = dot1 / dotProduct;
            if (t > 1f || t < 0f)
                return false;
            
            p = v + t * (w - v);
            return true;
        }
    }
}