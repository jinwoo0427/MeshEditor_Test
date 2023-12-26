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

        #region Geometry

        /**
		 * 레이캐스트가 삼각형과 교차하는 경우 true를 반환합니다.
		 */
        public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA, Vector3 InTriangleB, Vector3 InTriangleC, out float OutDistance, out Vector3 OutPoint)
        {
            OutDistance = 0f;
            OutPoint = Vector3.zero;

            Vector3 e1, e2;  //Edge1, Edge2
            Vector3 P, Q, T;
            float det, inv_det, u, v;
            float t;

            // V1을 공유하는 두 가지 엣지에 대한 벡터 찾기
            e1 = InTriangleB - InTriangleA;
            e2 = InTriangleC - InTriangleA;

            //행렬식 계산 시작 - 'u' 매개변수 계산에도 사용됨
            P = Vector3.Cross(InRay.direction, e2);

            //행렬식이 0에 가까우면 광선은 삼각형 평면에 놓이게 됩니다.
            det = Vector3.Dot(e1, P);

            // Culling을 수행하지 않는 브랜치
            // {
            if (det > -Mathf.Epsilon && det < Mathf.Epsilon)
                return false;

            inv_det = 1f / det;

            // V1에서 광선 원점까지의 거리 계산
            T = InRay.origin - InTriangleA;

            // 'u' 매개변수를 계산하고 경계를 테스트
            u = Vector3.Dot(T, P) * inv_det;

            // 교차점이 삼각형 외부에 있으면 false 반환
            if (u < 0f || u > 1f)
                return false;

            // 'v' 매개변수를 테스트하기 위해 준비
            Q = Vector3.Cross(T, e1);

            // 'v' 매개변수를 계산하고 경계를 테스트
            v = Vector3.Dot(InRay.direction, Q) * inv_det;

            // 교차점이 삼각형 외부에 있으면 false 반환
            if (v < 0f || u + v > 1f)
                return false;

            t = Vector3.Dot(e2, Q) * inv_det;
            // }

            if (t > Mathf.Epsilon)
            {
                // 광선 교차
                OutDistance = t;

                OutPoint.x = (u * InTriangleB.x + v * InTriangleC.x + (1 - (u + v)) * InTriangleA.x);
                OutPoint.y = (u * InTriangleB.y + v * InTriangleC.y + (1 - (u + v)) * InTriangleA.y);
                OutPoint.z = (u * InTriangleB.z + v * InTriangleC.z + (1 - (u + v)) * InTriangleA.z);

                return true;
            }

            return false;
        }
        #endregion

        #region Normal and Tangents

        /**
		 * 3개 점의 단위 벡터 법선 계산: B-A x C-A
		 */
        public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 cross = Vector3.Cross(p1 - p0, p2 - p0);
            if (cross.magnitude < Mathf.Epsilon)
                return new Vector3(0f, 0f, 0f); // bad triangle
            else
            {
                return cross.normalized;
            }
        }

        public static Vector3 Normal(this Mesh mesh, Triangle tri)
        {
            return Normal(mesh.vertices[tri.I0], mesh.vertices[tri.I1], mesh.vertices[tri.I2]);
        }

        /**
		 * 접선 및 이중 접선에 사용할 수 있는 첫 번째 삼각형을 사용하여 
		 * 이 면의 첫 번째 법선, 접선 및 이중 접선을 반환합니다.
         * 일반 또는 uv 정보를 위해 mesh.msh에 의존하지 않습니다.
         * mesh.vertices 및 mesh.uv를 사용합니다.
		 */
        public static void NormalTangentBitangent(Vector3[] vertices, Vector2[] uv, Triangle tri, out Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            normal = Normal(vertices[tri.I0], vertices[tri.I1], vertices[tri.I2]);

            Vector3 tan1 = Vector3.zero;
            Vector3 tan2 = Vector3.zero;
            Vector4 tan = new Vector4(0f, 0f, 0f, 1f);

            long i1 = tri.I0;
            long i2 = tri.I1;
            long i3 = tri.I2;

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1 += sdir;
            tan2 += tdir;

            Vector3 n = normal;

            Vector3.OrthoNormalize(ref n, ref tan1);

            tan.x = tan1.x;
            tan.y = tan1.y;
            tan.z = tan1.z;

            tan.w = (Vector3.Dot(Vector3.Cross(n, tan1), tan2) < 0.0f) ? -1.0f : 1.0f;

            tangent = ((Vector3)tan) * tan.w;
            bitangent = Vector3.Cross(normal, tangent);
        }

        /**
		 * p.Length % 3 == 0이면 면에 있는 각 삼각형의 법선을 찾아 평균을 반환합니다.
         * 그렇지 않으면 처음 세 점의 법선을 반환합니다.
		 */
        public static Vector3 Normal(Vector3[] p)
        {
            if (p.Length % 3 == 0)
            {
                Vector3 nrm = Vector3.zero;

                for (int i = 0; i < p.Length; i += 3)
                    nrm += Normal(p[i + 0],
                                    p[i + 1],
                                    p[i + 2]);

                return nrm / (p.Length / 3f);
            }
            else
            {
                Vector3 cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
                if (cross.magnitude < Mathf.Epsilon)
                    return new Vector3(0f, 0f, 0f); // bad triangle
                else
                {
                    return cross.normalized;
                }
            }
        }
        #endregion

        #region Compare (Max, Min, Average, etc)

        public static T Max<T>(T[] array) where T : System.IComparable<T>
        {
            if (array == null || array.Length < 1)
                return default(T);

            T max = array[0];
            for (int i = 1; i < array.Length; i++)
                if (array[i].CompareTo(max) >= 0)
                    max = array[i];
            return max;
        }

        public static T Min<T>(T[] array) where T : System.IComparable<T>
        {
            if (array == null || array.Length < 1)
                return default(T);

            T min = array[0];
            for (int i = 1; i < array.Length; i++)
                if (array[i].CompareTo(min) < 0)
                    min = array[i];
            return min;
        }

        /**
		 * Vector3에서 가장 큰 축을 반환합니다.
		 */
        public static float LargestValue(Vector3 v)
        {
            if (v.x > v.y && v.x > v.z) return v.x;
            if (v.y > v.x && v.y > v.z) return v.y;
            return v.z;
        }

        /**
		 * Vector2에서 가장 큰 축을 반환합니다.
		 */
        public static float LargestValue(Vector2 v)
        {
            return (v.x > v.y) ? v.x : v.y;
        }

        /**
		 * Vector2의 배열에서 발견된 가장 작은 X 및 Y 값입니다.
           동일한 Vector2에 속할 수도 있고 속하지 않을 수도 있습니다.
		 */
        public static Vector2 SmallestVector2(Vector2[] v)
        {
            Vector2 s = v[0];
            for (int i = 1; i < v.Length; i++)
            {
                if (v[i].x < s.x)
                    s.x = v[i].x;
                if (v[i].y < s.y)
                    s.y = v[i].y;
            }
            return s;
        }

        /**
		 * 배열에서 가장 큰 X 및 Y 값입니다. 
           동일한 Vector2에 속할 수도 있고 속하지 않을 수도 있습니다.
		 */
        public static Vector2 LargestVector2(Vector2[] v)
        {
            Vector2 l = v[0];
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i].x > l.x)
                    l.x = v[i].x;
                if (v[i].y > l.y)
                    l.y = v[i].y;
            }
            return l;
        }

        /**
		 * 정점이 있는 AABB를 생성하고 중심점을 반환합니다.
		 */
        public static Vector3 BoundsCenter(Vector3[] verts)
        {
            if (verts.Length < 1) return Vector3.zero;

            Vector3 min = verts[0];
            Vector3 max = min;

            for (int i = 1; i < verts.Length; i++)
            {
                min.x = Mathf.Min(verts[i].x, min.x);
                max.x = Mathf.Max(verts[i].x, max.x);

                min.y = Mathf.Min(verts[i].y, min.y);
                max.y = Mathf.Max(verts[i].y, max.y);

                min.z = Mathf.Min(verts[i].z, min.z);
                max.z = Mathf.Max(verts[i].z, max.z);
            }

            return (min + max) * .5f;
        }

        public static Rect ClampRect(Rect rect, Rect bounds)
        {
            if (rect.x + rect.width > bounds.x + bounds.width)
                rect.x = (bounds.x + bounds.width) - rect.width;
            else if (rect.x < bounds.x)
                rect.x = bounds.x;

            if (rect.y + rect.height > bounds.y + bounds.height)
                rect.y = (bounds.y + bounds.height) - rect.height;
            else if (rect.y < bounds.y)
                rect.y = bounds.y;

            return rect;
        }

        /**
		 * \brief 제공된 Vector3[] 배열의 중심점을 가져옵니다.
         * \returns 전달된 정점 배열의 평균 Vector3입니다.
		 */
        public static Vector3 Average(List<Vector3> v)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < v.Count; i++)
                sum += v[i];
            return sum / (float)v.Count;
        }

        public static Vector3 Average(Vector3[] v)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < v.Length; i++)
                sum += v[i];
            return sum / (float)v.Length;
        }

        public static Vector2 Average(List<Vector2> v)
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < v.Count; i++)
                sum += v[i];
            return sum / (float)v.Count;
        }

        public static Vector2 Average(Vector2[] v)
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < v.Length; i++)
                sum += v[i];
            return sum / (float)v.Length;
        }

        public static Vector4 Average(List<Vector4> v)
        {
            Vector4 sum = Vector4.zero;
            for (int i = 0; i < v.Count; i++)
                sum += v[i];
            return sum / (float)v.Count;
        }

        public static Vector4 Average(Vector4[] v)
        {
            Vector4 sum = Vector4.zero;
            for (int i = 0; i < v.Length; i++)
                sum += v[i];
            return sum / (float)v.Length;
        }

        public static Color Average(Color[] Array)
        {
            Color sum = Array[0];

            for (int i = 1; i < Array.Length; i++)
                sum += Array[i];

            return sum / (float)Array.Length;
        }

        /**
		 *	\brief 2개의 벡터3 객체를 비교하여 오차 범위를 허용합니다.
		 */
        public static bool Approx(this Vector3 v, Vector3 b, float delta)
        {
            return
                Mathf.Abs(v.x - b.x) < delta &&
                Mathf.Abs(v.y - b.y) < delta &&
                Mathf.Abs(v.z - b.z) < delta;
        }

        /**
		 *	\brief 2개의 색상 객체를 비교하여 오차 범위를 허용합니다.
		 */
        public static bool Approx(this Color a, Color b, float delta)
        {
            return Mathf.Abs(a.r - b.r) < delta &&
                    Mathf.Abs(a.g - b.g) < delta &&
                    Mathf.Abs(a.b - b.b) < delta &&
                    Mathf.Abs(a.a - b.a) < delta;
        }

        public static T[] ValuesWithIndices<T>(T[] arr, IList<int> indices)
        {
            T[] vals = new T[indices.Count];
            for (int i = 0; i < indices.Count; i++)
                vals[i] = arr[indices[i]];
            return vals;
        }

        public static T[] FilledArray<T>(T val, int length)
        {
            T[] arr = new T[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = val;
            }
            return arr;
        }
        #endregion
    }
}