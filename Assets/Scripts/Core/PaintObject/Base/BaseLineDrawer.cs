using System;
using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Controllers;
using GetampedPaint.Tools;
using GetampedPaint.Tools.Raycast.Data;
using GetampedPaint.Utils;
using Random = UnityEngine.Random;

namespace GetampedPaint.Core.PaintObject.Base
{
    /// <summary>
    /// Performs lines drawing
    /// </summary>
    public class BaseLineDrawer
    {
        public Camera Camera { set => camera = value; }

        private IPaintManager paintManager;
        private BasePaintObjectRenderer objectRenderer;
        private Camera camera;
        private Transform Transform => firstRaycast.Triangle.Transform;
        private Vector3 IntersectionOffset => (firstRaycast.Hit - lastRaycast.Hit) * OffsetValue;
        private Action<List<Vector3>, List<Vector2>, List<int>, List<Color>> drawLine;
        private List<Vector2> drawPositions = new List<Vector2>();
        private List<Vector3> positions = new List<Vector3>();
        private List<Color> colors = new List<Color>();
        private List<int> indices = new List<int>();
        private List<Vector2> uv = new List<Vector2>();
        private RaycastData firstRaycast, lastRaycast;
        private Vector3 normal, cameraLocalPosition;
        private Vector2 textureSize;

        private const int MaxIterationsCount = 512;
        private const float OffsetValue = 0.0001f;

        public BaseLineDrawer(IPaintManager paintManagerInstance)
        {
            paintManager = paintManagerInstance;
        }

        public void Init(Camera currentCamera, Vector2 sourceTextureSize, Action<List<Vector3>, List<Vector2>, List<int>, List<Color>> onDrawLine)
        {
            camera = currentCamera;
            textureSize = sourceTextureSize;
            drawLine = onDrawLine;
        }
        
        public Vector2[] GetLinePositions(Vector2 paintUV1, Vector2 paintUV2, RaycastData raycastA, RaycastData raycastB, int fingerId, bool canRetry = true)
        {
            if (raycastA == raycastB)
                return new[] { raycastA.UVHit, raycastB.UVHit };

            firstRaycast = raycastA;
            lastRaycast = raycastB;
            
            if (canRetry)
            {
                cameraLocalPosition = Transform.InverseTransformPoint(camera.transform.position);
                normal = Vector3.Cross(firstRaycast.Hit - cameraLocalPosition, lastRaycast.Hit - cameraLocalPosition);
            }

            var iterationsCount = 0;
            var currentRaycast = firstRaycast;
            var triangles = new List<int>();

            if (Mathf.Abs(IntersectionOffset.magnitude) < float.Epsilon)
                return drawPositions.ToArray();

            var uvFirst = GetIntersectionUV(firstRaycast, lastRaycast.Hit, out var intersection);
            drawPositions.Add(paintUV1);
            drawPositions.Add(uvFirst);
            var beginExit = intersection;
            while (iterationsCount < MaxIterationsCount && currentRaycast.Triangle.Id != lastRaycast.Triangle.Id)
            {
                iterationsCount++;
                intersection -= IntersectionOffset;
                var ray = GetRay(intersection);
                var raycastData = RaycastController.Instance.RaycastLocal(paintManager, ray, fingerId);
                if (raycastData == null)
                {
                    if (canRetry)
                    {
                        return GetLinePositions(paintUV2, paintUV1, lastRaycast, firstRaycast, fingerId, false);
                    }
                    break;
                }

                if (raycastData.Triangle.Id != lastRaycast.Triangle.Id && currentRaycast.Triangle.Id != raycastData.Triangle.Id)
                {
                    currentRaycast = raycastData;
                    if (triangles.Contains(currentRaycast.Triangle.Id))
                    {
                        break;
                    }
                    triangles.Add(currentRaycast.Triangle.Id);
                    intersection = MathHelper.GetExitPointFromTriangle(camera, currentRaycast.Triangle, beginExit, lastRaycast.Hit, normal);
                    beginExit = intersection;
                    ray = GetRay(intersection);
                    var uvCurrent = MathHelper.GetIntersectionUV(currentRaycast.Triangle, ray);
                    var uvRaycast = new Vector2(raycastData.UVHit.x * textureSize.x, raycastData.UVHit.y * textureSize.y);
                    uvCurrent = new Vector2(uvCurrent.x * textureSize.x, uvCurrent.y * textureSize.y);
                    drawPositions.Add(uvRaycast);
                    drawPositions.Add(uvCurrent);
                }
                else
                {
                    break;
                }
            }

            var uvLast = GetIntersectionUV(lastRaycast, beginExit, out intersection, false);
            drawPositions.Add(uvLast);
            drawPositions.Add(paintUV2);
            return drawPositions.ToArray();
        }

        private Vector2 GetIntersectionUV(RaycastData raycastData, Vector3 exit, out Vector3 exitPosition, bool isStart = true)
        {
            exitPosition = MathHelper.GetExitPointFromTriangle(camera, raycastData.Triangle, raycastData.Hit, exit, normal);
            var ray = GetRay(exitPosition);
            var intersectionUV = MathHelper.GetIntersectionUV(raycastData.Triangle, ray);
            return new Vector2(intersectionUV.x * textureSize.x, intersectionUV.y * textureSize.y);
        }

        private Ray GetRay(Vector3 point)
        {
            var direction = point - cameraLocalPosition;
            return new Ray(point + direction, -direction);
        }

        private float GetRatio(float totalDistanceInPixels, float brushSize, IList<float> brushSizes)
        {
            var brushPressureStart = brushSizes[0];
            var brushPressureEnd = brushSizes[1];
            var pressureDifference = Mathf.Abs(brushPressureStart - brushPressureEnd);
            var brushCenterPartWidth = Mathf.Clamp(Settings.Instance.BrushDuplicatePartWidth * brushSize, 1f, 100f);
            var ratioBrush = totalDistanceInPixels * pressureDifference / brushCenterPartWidth;
            var ratioSource = totalDistanceInPixels / brushCenterPartWidth;
            var ratio = (ratioSource + ratioBrush) / totalDistanceInPixels;
            return ratio;
        }

        /// <summary>
        /// Creates line mesh
        /// </summary>
        public void RenderLine(IList<Vector2> linePositions, Vector2 renderOffset, Texture brushTexture, float brushSizeActual, IList<float> brushSizes, bool randomizeAngle = false)
        {
            // 렌더링에 필요한 초기 변수 설정
            var pressureStart = brushSizes[0];
            var pressureEnd = brushSizes[1];
            var brushWidth = brushTexture.width;
            var brushHeight = brushTexture.height;
            var maxBrushPressure = Mathf.Max(pressureStart, pressureEnd);
            var brushOffset = new Vector2(brushWidth, brushHeight) * maxBrushPressure;
            var distances = new float[linePositions.Count / 2];
            var totalDistance = 0f;

            // 각 선분의 길이 및 전체 길이 계산
            for (var i = 0; i < linePositions.Count - 1; i += 2)
            {
                var from = linePositions[i + 0];
                from = from.Clamp(Vector2.zero - brushOffset, textureSize + brushOffset);
                var to = linePositions[i + 1];
                to = to.Clamp(Vector2.zero - brushOffset, textureSize + brushOffset);
                linePositions[i + 0] = from;
                linePositions[i + 1] = to;
                distances[i / 2] = Vector2.Distance(from, to);
                totalDistance += distances[i / 2];
            }
            // 길이에 따른 선분 비율 계산
            var ratio = GetRatio(totalDistance, brushSizeActual, brushSizes) * 2f;
            var quadsCount = 0;
            // 선분의 갯수 계산
            for (var i = 0; i < linePositions.Count - 1; i += 2)
            {
                quadsCount += (int)(distances[i / 2] * ratio + 1);
            }

            quadsCount = Mathf.Clamp(quadsCount, linePositions.Count / 2, 16384);

            // 메시 데이터 초기화
            ClearMeshData();
            var count = 0;
            var scale = new Vector2(textureSize.x, textureSize.y);

            // 각 선분에 대해 메시 데이터 생성
            for (var i = 0; i < linePositions.Count - 1; i += 2)
            {
                var from = linePositions[i + 0];
                var to = linePositions[i + 1];
                var currentDistance = Mathf.Max(1, (int)(distances[i / 2] * ratio));

                // 선분을 나누어 각 부분에 대한 메시 생성
                for (var j = 0; j < currentDistance; j++)
                {
                    // t 값 계산
                    var minDistance = Mathf.Max(1, (float) (quadsCount - 1));
                    var t = Mathf.Clamp(count / minDistance, 0, 1);

                    // quadScale 계산
                    var quadScale = Mathf.Lerp(pressureStart, pressureEnd, t);
                    var center = (Vector3)(from + (to - from) / currentDistance * j);
                    var v1 = center + new Vector3(-brushWidth, brushHeight) * quadScale / 2f;
                    var v2 = center + new Vector3(brushWidth, brushHeight) * quadScale / 2f;
                    var v3 = center + new Vector3(brushWidth, -brushHeight) * quadScale / 2f;
                    var v4 = center + new Vector3(-brushWidth, -brushHeight) * quadScale / 2f;

                    // randomizeAngle이 true인 경우 회전 적용
                    if (randomizeAngle)
                    {
                        var quaternion = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
                        v1 = quaternion * (v1 - center) + center;
                        v2 = quaternion * (v2 - center) + center;
                        v3 = quaternion * (v3 - center) + center;
                        v4 = quaternion * (v4 - center) + center;
                    }

                    // 화면 상의 좌표로 변환
                    v1 = v1 / scale + renderOffset;
                    v2 = v2 / scale + renderOffset;
                    v3 = v3 / scale + renderOffset;
                    v4 = v4 / scale + renderOffset;

                    // 메시 데이터에 추가
                    positions.Add(v1);
                    positions.Add(v2);
                    positions.Add(v3);
                    positions.Add(v4);
                    
                    colors.Add(Color.white);
                    colors.Add(Color.white);
                    colors.Add(Color.white);
                    colors.Add(Color.white);

                    uv.Add(Vector2.up);
                    uv.Add(Vector2.one);
                    uv.Add(Vector2.right);
                    uv.Add(Vector2.zero);

                    indices.Add(0 + count * 4);
                    indices.Add(1 + count * 4);
                    indices.Add(2 + count * 4);
                    indices.Add(2 + count * 4);
                    indices.Add(3 + count * 4);
                    indices.Add(0 + count * 4);

                    count++;
                }
            }

            // 렌더링할 메시 데이터가 있으면 drawLine 함수 호출
            if (positions.Count > 0)
            {
                //BasePaintObjectRenderer.RenderLine
                drawLine(positions, uv, indices, colors);
            }

            // 메시 데이터 초기화
            ClearMeshData();
            drawPositions.Clear();
        }

        private void ClearMeshData()
        {
            positions.Clear();
            colors.Clear();
            indices.Clear();
            uv.Clear();
        }
    }
}