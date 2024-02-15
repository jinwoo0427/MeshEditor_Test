using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using GetampedPaint.Controllers;
using GetampedPaint.Core;
using GetampedPaint.Tools.Raycast.Data;
using GetampedPaint.Utils;
using Object = UnityEngine.Object;

namespace GetampedPaint.Tools.Raycast.Base
{
    public abstract class BaseRaycastMeshData : IRaycastMeshData
    {
        private readonly List<IPaintManager> paintManagers = new List<IPaintManager>();
        public IReadOnlyCollection<IPaintManager> PaintManagers => paintManagers;

        private Transform transform;
        public Transform Transform => transform;
        
        private List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> Vertices => vertices;

        private Mesh mesh;
        public Mesh Mesh => mesh;

        protected List<Vector2> UV = new List<Vector2>();
        protected Bounds MeshWorldBounds;
        protected int BakedFrame = -1;
        protected bool IsTrianglesDataUpdated;
        private DepthToWorldConverter depthToWorldConverter;
        private Dictionary<int, UVChannelData> uvChannelsData = new Dictionary<int, UVChannelData>();
        private Dictionary<int, SubMeshTrianglesData> trianglesSubMeshData = new Dictionary<int, SubMeshTrianglesData>();
        private Dictionary<int, List<RaycastData>> raycasts = new Dictionary<int, List<RaycastData>>(RaycastsCapacity);
        private Dictionary<KeyValuePair<IPaintManager, int>, RaycastData> raycastsDict = new Dictionary<KeyValuePair<IPaintManager, int>, RaycastData>(RaycastsDictCapacity);
        private List<Triangle> lineRaycastTriangles = new List<Triangle>(LineRaycastTrianglesCapacity);
        private Dictionary<IPaintManager, RaycastTriangleData[]> outputData = new Dictionary<IPaintManager, RaycastTriangleData[]>();

        private const int RaycastsCapacity = 64;
        private const int RaycastsDictCapacity = 32;
        private const int LineRaycastTrianglesCapacity = 64;

        public virtual void Init(Component paintComponent, Component rendererComponent)
        {
            mesh = new Mesh();
            transform = paintComponent.transform;
        }

        public void SetDepthToWorldConverter(DepthToWorldConverter depthConverter)
        {
            depthToWorldConverter = depthConverter;
        }

        public virtual void AddPaintManager(IPaintManager paintManager)
        {
            paintManagers.Add(paintManager);
            outputData.Add(paintManager, new RaycastTriangleData[paintManager.Triangles.Length]);
        }

        public virtual void RemovePaintManager(IPaintManager paintManager)
        {
            paintManagers.Remove(paintManager);
            outputData.Remove(paintManager);

            if (trianglesSubMeshData.ContainsKey(paintManager.SubMesh))
            {
                trianglesSubMeshData[paintManager.SubMesh].PaintManagers.Remove(paintManager);
                if (trianglesSubMeshData[paintManager.SubMesh].PaintManagers.Count == 0)
                {
                    trianglesSubMeshData.Remove(paintManager.SubMesh);
                }
            }
            
            if (uvChannelsData.ContainsKey(paintManager.UVChannel))
            {
                uvChannelsData[paintManager.UVChannel].PaintManagers.Remove(paintManager);
                if (uvChannelsData[paintManager.UVChannel].PaintManagers.Count == 0)
                {
                    uvChannelsData.Remove(paintManager.UVChannel);
                }
            }
        }
        
        public void DoDispose()
        {
            if (mesh != null)
            {
                Object.Destroy(mesh);
                mesh = null;
            }
        }
                        
        public Vector2 GetUV(int channel, int index)
        {
            return uvChannelsData[channel].UV[index];
        }

        public IRaycastRequest RequestRaycast(ulong requestId, IPaintManager sender, Ray ray, int fingerId, 
            Vector3? prevScreenPosition, Vector3? screenPosition = null, bool useWorld = true, bool useCache = true, bool raycastAll = true)
        {
            // 레이캐스트 결과를 저장할 리스트들을 초기화
            raycasts.Clear();
            raycastsDict.Clear();
            lineRaycastTriangles.Clear();
            lineRaycastTriangles.Capacity = LineRaycastTrianglesCapacity;

            // 레이를 월드 좌표계로 변환
            var rayTransformed = new Ray(ray.origin, ray.direction);
            if (useWorld)
            {
                // sender의 MeshBounds를 업데이트하고, 레이가 메시의 경계와 교차하는지 확인
                UpdateMeshBounds(sender);
                MeshWorldBounds.Expand(0.0001f);
                var boundsIntersect = MeshWorldBounds.IntersectRay(ray);
                if (!boundsIntersect || !IsBoundsInDepth(MeshWorldBounds, screenPosition))
                    return null;

                // 화면 좌표를 월드 좌표로 변환
                var origin = Transform.InverseTransformPoint(ray.origin);
                var direction = Transform.InverseTransformVector(ray.direction);
                rayTransformed = new Ray(origin, direction);
            }

            // 레이캐스트 요청 객체를 초기화
            IRaycastRequest raycastRequest = null;
            var paintManager = sender;
            var paintManagerComponent = (PaintManager)sender;
            if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
            {
                // 레이캐스트에 사용할 삼각형 리스트를 가져옴
                var lineTriangles = RaycastController.Instance.GetLineTriangles(paintManagerComponent, fingerId);
                var raycastAllTriangles = raycastAll || lineTriangles == null;
                var triangles = raycastAllTriangles ? paintManagerComponent.Triangles : lineTriangles;

                // 레이캐스트에 사용할 평면들의 위치와 법선을 계산
                var cameraPos = PaintController.Instance.Camera.transform.position;
                var distance = Vector3.Distance(cameraPos, transform.position);
                var hit2 = PaintController.Instance.Camera.ScreenToWorldPoint(new Vector3(screenPosition.Value.x, screenPosition.Value.y, distance));
                var hit1 = prevScreenPosition.HasValue ? PaintController.Instance.Camera.ScreenToWorldPoint(new Vector3(prevScreenPosition.Value.x, prevScreenPosition.Value.y, distance)) : hit2;
                var plane2Position = transform.InverseTransformPoint(hit1);
                var plane3Position = transform.InverseTransformPoint(hit2);
                var nearPlanePoint3 = transform.InverseTransformPoint(cameraPos);
                var plane1Position = (plane2Position + plane3Position) / 2f;
                var plane1Normal = Vector3.Cross(plane3Position - plane2Position, nearPlanePoint3 - plane2Position);
                var plane2Normal = -Vector3.Cross(plane2Position + plane1Normal - nearPlanePoint3, plane2Position - plane1Normal - nearPlanePoint3);
                var plane3Normal = -Vector3.Cross(plane3Position + plane1Normal - nearPlanePoint3, plane3Position - plane1Normal - nearPlanePoint3);
                var skipPlaneIntersectsTriangle = plane2Position == plane3Position;

                if (Settings.Instance.RaycastsMethod == RaycastSystemType.JobSystem)
                {
                    // JobSystem을 사용하여 레이캐스트를 수행
                    var verticesData = trianglesSubMeshData[paintManager.SubMesh];
                    if (!IsTrianglesDataUpdated)
                    {
                        for (var i = 0; i < paintManager.Triangles.Length; i++)
                        {
                            var triangle = paintManager.Triangles[i];
                            var triangleData = new TriangleData
                            {
                                Id = triangle.Id,
                                Position0 = Vertices[triangle.I0],
                                Position1 = Vertices[triangle.I1],
                                Position2 = Vertices[triangle.I2],
                                UV0 = UV[triangle.I0],
                                UV1 = UV[triangle.I1],
                                UV2 = UV[triangle.I2]
                            };
                            verticesData.TrianglesData[i] = triangleData;
                        }
                    }

                    // NativeArray로 데이터를 복사
                    var trianglesData = new NativeArray<TriangleData>(verticesData.TrianglesData, Allocator.TempJob);
                    var data = new NativeArray<RaycastTriangleData>(verticesData.TrianglesData.Length, Allocator.TempJob);
                    var jobRay = raycastAll ? rayTransformed : ray;
                    var job = new RaycastJob
                    {
                        Triangles = trianglesData,
                        OutputData = data,
                        RayOrigin = jobRay.origin,
                        RayDirection = jobRay.direction,
                        Plane1Position = plane1Position,
                        Plane1Normal = plane1Normal,
                        Plane2Position = plane2Position,
                        Plane2Normal = plane2Normal,
                        Plane3Position = plane3Position,
                        Plane3Normal = plane3Normal,
                        SkipPlaneIntersectsTriangle = skipPlaneIntersectsTriangle
                    };

                    // Job을 스케쥴
                    var jobHandle = job.Schedule(verticesData.TrianglesData.Length, 32);
                    var request = new JobRaycastRequest
                    {
                        Sender = paintManager,
                        FingerId = fingerId,
                        RequestId = requestId,
                        JobHandle = jobHandle,
                        InputNativeArray = trianglesData,
                        OutputNativeArray = data
                    };
                    raycastRequest = request;
                }
                else
                {
                    //CPU
                    var raycastData = new List<RaycastTriangleData>();
                    foreach (var triangle in triangles)
                    {
                        var result = new RaycastTriangleData
                        {
                            IntersectPlaneTriangleId = -1,
                            RaycastTriangleId = -1
                        };

                        if (skipPlaneIntersectsTriangle)
                        {
                            var data = GetRaycastData(triangle, raycastAll ? rayTransformed : ray);
                            if (data != null)
                            {
                                result.RaycastTriangleId = data.Triangle.Id;
                                result.Hit = data.Hit;
                                result.UVHit = data.UVHit;
                            }

                            raycastData.Add(result);
                            continue;
                        }

                        if (IsPlane1IntersectsTriangle(plane1Position, plane1Normal, triangle.Position0, triangle.Position1, triangle.Position2) ||
                            IsPlane2IntersectsTriangle(plane2Position, plane2Normal, triangle.Position0, triangle.Position1, triangle.Position2) ||
                            IsPlane3IntersectsTriangle(plane3Position, plane3Normal, triangle.Position0, triangle.Position1, triangle.Position2))
                        {
                            result.IntersectPlaneTriangleId = -1;
                        }
                        else
                        {
                            result.IntersectPlaneTriangleId = triangle.Id;
                            var data = GetRaycastData(triangle, raycastAll ? rayTransformed : ray);
                            if (data != null)
                            {
                                result.RaycastTriangleId = data.Triangle.Id;
                                result.Hit = data.Hit;
                                result.UVHit = data.UVHit;
                            }
                        }

                        raycastData.Add(result);
                    }

                    var request = new CPURaycastRequest
                    {
                        Sender = paintManager,
                        FingerId = fingerId,
                        RequestId = requestId,
                        OutputList = raycastData
                    };
                    raycastRequest = request;
                }
            }
            return raycastRequest;
        }

        public RaycastData TryGetRaycastResponse(RaycastRequestContainer request, out IList<Triangle> triangles)
        {
            if (request.IsDisposed)
            {
                triangles = null;
                return null;
            }

            foreach (var raycastRequest in request.RaycastRequests)
            {
                if (raycastRequest.IsDisposed || request.Sender != raycastRequest.Sender || request.FingerId != raycastRequest.FingerId)
                    continue;

                if (raycastRequest is JobRaycastRequest jobRaycastRequest)
                {
                    jobRaycastRequest.JobHandle.Complete();
                    var data = outputData[request.Sender];
                    jobRaycastRequest.OutputNativeArray.CopyTo(data);
                    
                    foreach (var item in data)
                    {
                        if (item.IntersectPlaneTriangleId >= 0)
                        {
                            var triangle = request.Sender.Triangles[item.IntersectPlaneTriangleId];
                            if (!lineRaycastTriangles.Contains(triangle))
                            {
                                lineRaycastTriangles.Add(triangle);
                            }
                        }
                    
                        if (item.RaycastTriangleId >= 0)
                        {
                            var raycastData = new RaycastData(request.Sender.Triangles[item.RaycastTriangleId])
                            {
                                Hit = item.Hit,
                                UVHit = item.UVHit
                            };
                            
                            if (!raycasts.ContainsKey(raycastRequest.FingerId))
                            {
                                raycasts.Add(raycastRequest.FingerId, new List<RaycastData>());
                            }
                            raycasts[raycastRequest.FingerId].Add(raycastData);
                        }
                    }

                    if (raycasts.Count > 0)
                    {
                        if (raycasts.ContainsKey(raycastRequest.FingerId))
                        {
                            var sortedIntersect = SortRaycasts(raycasts[raycastRequest.FingerId]);
                            raycastsDict[request.Key] = sortedIntersect;
                        }
                    }

                    jobRaycastRequest.DoDispose();
                }
                else if (raycastRequest is CPURaycastRequest cpuRaycastRequest)
                {
                    foreach (var item in cpuRaycastRequest.OutputList)
                    {
                        if (item.IntersectPlaneTriangleId >= 0)
                        {
                            var triangle = request.Sender.Triangles[item.IntersectPlaneTriangleId];
                            if (!lineRaycastTriangles.Contains(triangle))
                            {
                                lineRaycastTriangles.Add(triangle);
                            }
                        }
                    
                        if (item.RaycastTriangleId >= 0)
                        {
                            var raycastData = new RaycastData(request.Sender.Triangles[item.RaycastTriangleId])
                            {
                                Hit = item.Hit,
                                UVHit = item.UVHit
                            };
                            
                            if (!raycasts.ContainsKey(raycastRequest.FingerId))
                            {
                                raycasts.Add(raycastRequest.FingerId, new List<RaycastData>());
                            }
                            raycasts[raycastRequest.FingerId].Add(raycastData);
                        }
                    }
                    
                    if (raycasts.Count > 0)
                    {
                        if (raycasts.ContainsKey(raycastRequest.FingerId))
                        {
                            var sortedIntersect = SortRaycasts(raycasts[raycastRequest.FingerId]);
                            raycastsDict[request.Key] = sortedIntersect;
                        }
                    }
                    
                    cpuRaycastRequest.DoDispose();
                }
            }

            request.RaycastRequests.RemoveAll(x => x.IsDisposed);
            if (request.RaycastRequests.Count == 0)
            {
                request.DoDispose();
            }
            
            triangles = lineRaycastTriangles;

            if (!IsTrianglesDataUpdated)
            {
                IsTrianglesDataUpdated = true;
            }

            if (raycastsDict.Count == 0)
                return null;
            
            KeyValuePair<KeyValuePair<IPaintManager, int>, RaycastData> closestRaycast = default;
            var minDistance = float.MaxValue;
            foreach (var raycast in raycastsDict)
            {
                if (raycast.Key.Value != request.FingerId)
                    continue;
                
                var triangle = raycast.Value;
                var distance = Vector3.Distance(PaintController.Instance.Camera.transform.position, triangle.WorldHit);
                if (distance < minDistance)
                {
                    closestRaycast = raycast;
                    minDistance = distance;
                }
            }

            if (closestRaycast.Key.Key != request.Sender)
                return null;
            
            return closestRaycast.Value;
        }

        public RaycastData GetRaycast(IPaintManager sender, Ray ray, int fingerId, Vector3? screenPosition = null, bool useWorld = true, bool raycastAll = true)
        {
            var rayTransformed = new Ray(ray.origin, ray.direction);
            if (useWorld)
            {
                UpdateMeshBounds(sender);
                MeshWorldBounds.Expand(0.0001f);
                var boundsIntersect = MeshWorldBounds.IntersectRay(ray);
                if (!boundsIntersect && !IsBoundsInDepth(MeshWorldBounds, screenPosition))
                    return null;
                
                var origin = Transform.InverseTransformPoint(ray.origin);
                var direction = Transform.InverseTransformVector(ray.direction);
                rayTransformed = new Ray(origin, direction);
            }
            
            raycasts.Clear();
            raycastsDict.Clear();
            foreach (var paintManager in PaintManagers)
            {
                var paintManagerComponent = (PaintManager)paintManager;
                if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
                {
                    var lineTriangles = RaycastController.Instance.GetLineTriangles(paintManager, fingerId);
                    var raycastAllTriangles = raycastAll || lineTriangles == null;
                    var triangles = raycastAllTriangles ? paintManager.Triangles : lineTriangles;
                    foreach (var triangle in triangles)
                    {
                        var raycastData = GetRaycastData(triangle, raycastAll ? rayTransformed : ray);
                        if (raycastData != null)
                        {
                            if (!raycasts.ContainsKey(fingerId))
                            {
                                raycasts.Add(fingerId, new List<RaycastData>());
                            }
                            raycasts[fingerId].Add(raycastData);
                        }
                    }

                    if (raycasts.Count > 0)
                    {
                        var sortedIntersect = SortRaycasts(raycasts[fingerId]);
                        var key = new KeyValuePair<IPaintManager, int>(sender, fingerId);
                        raycastsDict[key] = sortedIntersect;
                    }
                }
            }
            
            if (!IsTrianglesDataUpdated)
            {
                IsTrianglesDataUpdated = true;
            }
            
            if (raycastsDict.Count == 0)
                return null;
            
            KeyValuePair<KeyValuePair<IPaintManager, int>, RaycastData> closestRaycast = default;
            var minDistance = float.MaxValue;
            foreach (var raycast in raycastsDict)
            {
                var triangle = raycast.Value;
                var distance = Vector3.Distance(PaintController.Instance.Camera.transform.position, triangle.WorldHit);
                if (distance < minDistance)
                {
                    closestRaycast = raycast;
                    minDistance = distance;
                }
            }

            if (closestRaycast.Key.Key != sender)
                return null;
            
            return closestRaycast.Value;
        }

        protected abstract void UpdateMeshBounds(IPaintManager paintManager);

        protected void InitUVs(IPaintManager paintManager, Mesh meshData)
        {
            //Cache UVs
            if (uvChannelsData.ContainsKey(paintManager.UVChannel))
            {
                uvChannelsData[paintManager.UVChannel].PaintManagers.Add(paintManager);
            }
            else
            {
                var uvs = new List<Vector2>();
                meshData.GetUVs(paintManager.UVChannel, uvs);
                
                if (UV == null || UV.Count == 0)
                {
                    UV = uvs;
                }
                
                uvChannelsData.Add(paintManager.UVChannel, new UVChannelData
                {
                    PaintManagers = new List<IPaintManager> { paintManager }, UV = uvs
                });
            }
        }
        
        protected void InitTriangles(IPaintManager paintManager, Mesh meshData)
        {
            if (trianglesSubMeshData.ContainsKey(paintManager.SubMesh))
            {
                trianglesSubMeshData[paintManager.SubMesh].PaintManagers.Add(paintManager);
            }
            else
            {
                trianglesSubMeshData.Add(paintManager.SubMesh, new SubMeshTrianglesData
                {
                    PaintManagers = new List<IPaintManager> { paintManager },
                    TrianglesData = new TriangleData[paintManager.Triangles.Length]
                });
            }
            
            if (Vertices == null || Vertices.Count == 0)
            {
                var verticesList = new List<Vector3>();
                meshData.GetVertices(verticesList);
                vertices = verticesList;
            }
            
            var verticesData = trianglesSubMeshData[paintManager.SubMesh];
            for (var i = 0; i < paintManager.Triangles.Length; i++)
            {
                var triangle = paintManager.Triangles[i];
                var triangleData = new TriangleData
                {
                    Id = triangle.Id,
                    Position0 = Vertices[triangle.I0],
                    Position1 = Vertices[triangle.I1],
                    Position2 = Vertices[triangle.I2],
                    UV0 = UV[triangle.I0],
                    UV1 = UV[triangle.I1],
                    UV2 = UV[triangle.I2]
                };
                verticesData.TrianglesData[i] = triangleData;
            }
        }

        private RaycastData SortRaycasts(List<RaycastData> raycastsList)
        {
            if (raycastsList.Count == 0)
                return null;
            
            if (raycastsList.Count == 1)
                return raycastsList[0];
            
            var result = raycastsList[0];
            var cameraPosition = PaintController.Instance.Camera.transform.position;
            var currentDistance = Vector3.Distance(cameraPosition, result.WorldHit);
            for (var i = 1; i < raycastsList.Count; i++)
            {
                var distance = Vector3.Distance(cameraPosition, raycastsList[i].WorldHit);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    result = raycastsList[i];
                }
            }
            return result;
        }

        private bool IsBoundsInDepth(Bounds worldBounds, Vector3? screenPosition)
        {
            if (depthToWorldConverter != null && depthToWorldConverter.IsEnabled && screenPosition != null)
            {
                var mainCamera = PaintController.Instance.Camera;
                if (!mainCamera.orthographic)
                {
                    var position = depthToWorldConverter.GetPosition(screenPosition.Value);
                    if (position.w > 0 && position.w > mainCamera.nearClipPlane && position.w < mainCamera.farClipPlane)
                    {
                        return worldBounds.Contains(position);
                    }
                }
            }
            return true;
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

        private RaycastData GetRaycastData(Triangle triangle, Ray ray)
        {
            var p1 = triangle.Position0;
            var p2 = triangle.Position1;
            var p3 = triangle.Position2;
            var e1 = p2 - p1;
            var e2 = p3 - p1;
            var eps = float.Epsilon;
            var p = Vector3.Cross(ray.direction, e2);
            var det = Vector3.Dot(e1, p);
            if (det.IsNaNOrInfinity() || det > eps && det < -eps)
            {
                return null;
            }
            var invDet = 1.0f / det;
            var t = ray.origin - p1;
            var u = Vector3.Dot(t, p) * invDet;
            if (u.IsNaNOrInfinity() || u < 0f || u > 1f)
            {
                return null;
            }
            var q = Vector3.Cross(t, e1);
            var v = Vector3.Dot(ray.direction, q) * invDet;
            if (v.IsNaNOrInfinity() || v < 0f || u + v > 1f)
            {
                return null;
            }
            if (Vector3.Dot(e2, q) * invDet > eps)
            {
                var raycastData = new RaycastData(triangle)
                {
                    Hit = p1 + u * e1 + v * e2,
                    UVHit = triangle.UV0 + (triangle.UV1 - triangle.UV0) * u + (triangle.UV2 - triangle.UV0) * v
                };
                return raycastData;
            }
            return null;
        }
    }
}