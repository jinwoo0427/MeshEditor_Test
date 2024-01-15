using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GetampedPaint.Controllers.InputData;
using GetampedPaint.Core;
using GetampedPaint.Tools.Raycast;
using GetampedPaint.Tools.Raycast.Base;
using GetampedPaint.Tools.Raycast.Data;
using GetampedPaint.Utils;

namespace GetampedPaint.Controllers
{
    public class RaycastController : Singleton<RaycastController>
    {
        [SerializeField] private bool useDepthTexture = true;
        public bool UseDepthTexture
        {
            get => useDepthTexture;
            set
            {
                useDepthTexture = value;
                if (depthToWorldConverter != null)
                {
                    depthToWorldConverter.IsEnabled = useDepthTexture;
                }
            }
        }

        private DepthToWorldConverter depthToWorldConverter;
        private ulong requestID;
        // 메쉬 데이터리스트
        private readonly List<IRaycastMeshData> meshesData = new List<IRaycastMeshData>();
        private readonly Dictionary<KeyValuePair<IPaintManager, int>, List<RaycastData>> raycastResults = new Dictionary<KeyValuePair<IPaintManager, int>, List<RaycastData>>(RaycastsResultsCapacity);
        private readonly Dictionary<int, List<RaycastData>> raycasts = new Dictionary<int, List<RaycastData>>(RaycastsCapacity);
        private readonly Dictionary<KeyValuePair<IPaintManager, int>, Dictionary<int, List<Triangle>>> lineRaycastTriangles = new Dictionary<KeyValuePair<IPaintManager, int>, Dictionary<int, List<Triangle>>>(LineRaycastTrianglesCapacity);
        private readonly Dictionary<KeyValuePair<IPaintManager, int>, InputRequest> pendingRequests = new Dictionary<KeyValuePair<IPaintManager, int>, InputRequest>(PendingRequestsCapacity);

        private const int RaycastsResultsCapacity = 64;
        private const int RaycastsCapacity = 64;
        private const int LineRaycastTrianglesCapacity = 32;
        private const int PendingRequestsCapacity = 16;
        
        private void Start()
        {
            InitDepthToWorldConverter();
        }
        
        private void LateUpdate()
        {
            if (pendingRequests.Count > 0)
            {
                raycastResults.Clear();
            }

            var processRaycast = false;
            for (var i = pendingRequests.Keys.Count - 1; i >= 0; i--)
            {
                var key = pendingRequests.Keys.ElementAt(i);
                var inputAction = pendingRequests[key];
                if (inputAction != null)
                {
                    ProcessRaycast(inputAction.RequestContainer);
                    processRaycast = true;
                }
            }

            // 만약 어떤 요청이라도 처리하라고 하면 액션 실행
            if (processRaycast)
            {
                for (var i = pendingRequests.Keys.Count - 1; i >= 0; i--)
                {
                    var key = pendingRequests.Keys.ElementAt(i);
                    var inputAction = pendingRequests[key];
                    if (inputAction != null)
                    {
                        var requestContainer = inputAction.RequestContainer;
                        foreach (var callback in inputAction.Callbacks)
                        {
                            callback?.Invoke(requestContainer);
                        }
                        inputAction.RequestContainer = null;
                    }
                }
            }
            
            pendingRequests.Clear();
        }

        private void OnDestroy()
        {
            depthToWorldConverter?.DoDispose();
            DisposeRequests();
        }

        public void InitObject(IPaintManager paintManager, Component paintComponent, Component renderComponent)
        {
            DestroyMeshData(paintManager);
            
            var raycastMeshData = meshesData.FirstOrDefault(x => x.Transform == paintComponent.transform);
            if (raycastMeshData == null)
            {
                if (renderComponent is SkinnedMeshRenderer)
                {
                    raycastMeshData = new RaycastSkinnedMeshRendererData();
                }
                else if (renderComponent is MeshRenderer)
                {
                    raycastMeshData = new RaycastMeshRendererData();
                }

                if (raycastMeshData != null)
                {
                    raycastMeshData.Init(paintComponent, renderComponent);
                    raycastMeshData.SetDepthToWorldConverter(depthToWorldConverter);
                    meshesData.Add(raycastMeshData);
                }
                else
                {
                    return;
                }
            }
            
            if (paintManager.Triangles != null)
            {
                foreach (var triangle in paintManager.Triangles)
                {
                    triangle.SetRaycastMeshData(raycastMeshData, paintManager.UVChannel);
                }
            }
            
            raycastMeshData.AddPaintManager(paintManager);
        }
        
        public IList<Triangle> GetLineTriangles(IPaintManager paintManager, int fingerId)
        {
            var key = new KeyValuePair<IPaintManager, int>(paintManager, fingerId);
            if (lineRaycastTriangles.TryGetValue(key, out var trianglesDictionary))
            {
                return trianglesDictionary.TryGetValue(fingerId, out var triangles) ? triangles : null;
            }
            return null;
        }

        public Mesh GetMesh(IPaintManager paintManager)
        {
            return meshesData.Find(x => x.PaintManagers.Contains(paintManager)).Mesh;
        }

        public void DestroyMeshData(IPaintManager paintManager)
        {
            for (var i = meshesData.Count - 1; i >= 0; i--)
            {
                if (meshesData[i].PaintManagers.Count == 1 && meshesData[i].PaintManagers.ElementAt(0) == paintManager)
                {
                    meshesData[i].DoDispose();
                    meshesData.RemoveAt(i);
                    break;
                }

                if (meshesData[i].PaintManagers.Count > 1 && meshesData[i].PaintManagers.Contains(paintManager))
                {
                    meshesData[i].RemovePaintManager(paintManager);
                    break;
                }
            }
            
            DisposeRequests(paintManager);
        }

        public RaycastData RaycastLocal(IPaintManager paintManager, Ray ray, int fingerId)
        {
            raycasts.Clear();
            foreach (var meshData in meshesData)
            {
                var raycast = meshData?.GetRaycast(paintManager, ray, fingerId, null, true, false);
                if (raycast != null)
                {
                    if (!raycasts.ContainsKey(fingerId))
                    {
                        raycasts.Add(fingerId, new List<RaycastData>());
                    }
                    raycasts[fingerId].Add(raycast);
                }
            }

            return raycasts.TryGetValue(fingerId, out var list) ? SortIntersects(list) : null;
        }
        
        public void AddCallbackToRequest(IPaintManager sender, int fingerId, Action callback = null)
        {
            var key = new KeyValuePair<IPaintManager, int>(sender, fingerId);
            if (!pendingRequests.ContainsKey(key))
            {
                var requestContainer = new RaycastRequestContainer
                {
                    Sender = sender,
                    FingerId = fingerId,
                    RequestID = requestID
                };
                requestID++;

                pendingRequests.Add(key, new InputRequest
                {
                    RequestContainer = requestContainer,
                    Callbacks = new List<Action<RaycastRequestContainer>>()
                });
            }

            pendingRequests[key].Callbacks.Add(_ => callback?.Invoke());
        }

        private bool TryAddRequest(IPaintManager sender, int fingerId, Action<RaycastRequestContainer> callback = null)
        {
            var isAdded = false;
            var key = new KeyValuePair<IPaintManager, int>(sender, fingerId);
            if (!pendingRequests.ContainsKey(key))
            {
                var requestContainer = new RaycastRequestContainer
                {
                    Sender = sender,
                    FingerId = fingerId,
                    RequestID = requestID
                };
                requestID++;

                pendingRequests.Add(key, new InputRequest
                {
                    RequestContainer = requestContainer,
                    Callbacks = new List<Action<RaycastRequestContainer>>()
                });
                isAdded = true;
            }
            pendingRequests[key].Callbacks.Add(callback);
            return isAdded;
        }

        public void RequestRaycast(IPaintManager sender, Ray ray, int fingerId, Vector3? prevPosition, Vector3 position, Action<RaycastRequestContainer> callback = null)
        {
            if (!TryAddRequest(sender, fingerId, callback))
                return;

            foreach (var meshData in meshesData)
            {
                if (meshData == null)
                    continue;
                
                if (!meshData.PaintManagers.Contains(sender))
                    continue;
                
                foreach (var paintManager in meshData.PaintManagers)
                {
                    if (paintManager == null)
                        continue;
                    
                    var paintManagerComponent = (PaintManager)paintManager;
                    if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled && paintManager == sender)
                    {
                        var result = meshData.RequestRaycast(requestID, paintManager, ray, fingerId, prevPosition, position);
                        if (result != null)
                        {
                            var key = new KeyValuePair<IPaintManager, int>(sender, fingerId);
                            pendingRequests[key].RequestContainer.RaycastRequests.Add(result);
                        }
                    }
                }
            }
        }

        private void ProcessRaycast(RaycastRequestContainer requestContainer)
        {
            if (requestContainer == null)
                return;

            var key = requestContainer.Key;
            if (!lineRaycastTriangles.ContainsKey(key))
            {
                var dictionary = new Dictionary<int, List<Triangle>> { { requestContainer.FingerId, new List<Triangle>() } };
                lineRaycastTriangles.Add(key, dictionary);
            }
            else
            {
                if (lineRaycastTriangles[key].TryGetValue(requestContainer.FingerId, out var list))
                {
                    list.Clear();
                }
                else
                {
                    lineRaycastTriangles[key] = new Dictionary<int, List<Triangle>>{ { requestContainer.FingerId, new List<Triangle>() } };
                }
            }
            
            foreach (var meshData in meshesData)
            {
                if (meshData == null)
                    continue;
                
                if (!meshData.PaintManagers.Contains(requestContainer.Sender))
                    continue;
                
                foreach (var paintManager in meshData.PaintManagers)
                {
                    if (paintManager == null)
                        continue;
                    
                    var paintManagerComponent = (PaintManager)paintManager;
                    if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
                    {
                        var raycast = meshData.TryGetRaycastResponse(requestContainer, out var triangles);
                        if (raycast == null) 
                            continue;
                        
                        if (triangles != null)
                        {
                            lineRaycastTriangles[key][requestContainer.FingerId].AddRange(triangles);
                        }

                        if (!raycastResults.ContainsKey(key))
                        {
                            raycastResults.Add(key, new List<RaycastData>());
                        }
                        raycastResults[key].Add(raycast);
                    }
                }
            }
        }

        public RaycastData TryGetRaycast(RaycastRequestContainer requestContainer)
        {
            return SortIntersects(requestContainer.Sender, requestContainer.FingerId, raycastResults);
        }

        private RaycastData SortIntersects(IPaintManager sender, int fingerId, Dictionary<KeyValuePair<IPaintManager, int>, List<RaycastData>> raycastsData)
        {
            IPaintManager paintManager = null;
            RaycastData raycastData = null;
            var cameraPosition = PaintController.Instance.Camera.transform.position;
            var currentDistance = float.MaxValue;

            foreach (var pair in raycastsData)
            {
                var key = pair.Key;
                var data = raycastsData[key];

                if (data.Count == 0 || pair.Key.Value != fingerId)
                    continue;

                foreach (var raycast in data)
                {
                    var distance = Vector3.Distance(cameraPosition, raycast.WorldHit);
                    if (distance < currentDistance)
                    {
                        currentDistance = distance;
                        paintManager = key.Key;
                        raycastData = raycast;
                    }
                }
            }

            return paintManager == sender ? raycastData : null;
        }

        private RaycastData SortIntersects(IList<RaycastData> triangles)
        {
            if (triangles.Count == 0)
                return null;
            
            if (triangles.Count == 1)
                return triangles[0];
            
            var result = triangles[0];
            var cameraPosition = PaintController.Instance.Camera.transform.position;
            var currentDistance = Vector3.Distance(cameraPosition, result.WorldHit);
            for (var i = 1; i < triangles.Count; i++)
            {
                var distance = Vector3.Distance(cameraPosition, triangles[i].WorldHit);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    result = triangles[i];
                }
            }
            return result;
        }

        private void InitDepthToWorldConverter()
        {
            if (useDepthTexture)
            {
                if (PaintController.Instance.Camera.orthographic)
                {
                    Debug.LogWarning("Camera is orthographic, 'useDepthTexture' flag will be ignored.");
                    return;
                }
                
                // 현재 디바이스가 텍스쳐 형식을 지원하는지 쳌
                var textureFloatSupports = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat);
                var renderTextureFloatSupports = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
                if (textureFloatSupports && renderTextureFloatSupports)
                {
                    //& 연산자: 비트 단위 AND
                    if ((PaintController.Instance.Camera.depthTextureMode & DepthTextureMode.Depth) != 0)
                    {
                        //뎁스 설정 안되있다면 뎁스로 설정
                        PaintController.Instance.Camera.depthTextureMode |= DepthTextureMode.Depth;
                    }
                    depthToWorldConverter = new DepthToWorldConverter();
                    depthToWorldConverter.Init();
                }
                else
                {
                    Debug.LogWarning("Float 텍스처 형식은 지원되지 않습니다! UseDepthTexture를 false로 설정합니다.");
                    useDepthTexture = false;
                }
            }
        }

        //요청 해제 메소드
        private void DisposeRequests(IPaintManager paintManager = null)
        {
            foreach (var key in pendingRequests.Keys)
            {
                var request = pendingRequests[key];
                if (request != null && request.RequestContainer != null && request.RequestContainer.RaycastRequests != null)
                {
                    if (paintManager == null || request.RequestContainer.Sender == paintManager)
                    {
                        foreach (var raycastRequest in request.RequestContainer.RaycastRequests)
                        {
                            raycastRequest.DoDispose();
                        }
                        request.RequestContainer.RaycastRequests.Clear();
                    }
                }
            }
            pendingRequests.Clear();
        }
    }
}