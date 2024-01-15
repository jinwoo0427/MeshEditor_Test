using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Tools.Raycast.Data;

namespace GetampedPaint.Tools.Raycast.Base
{
    public interface IRaycastMeshData : IDisposable
    {
        Transform Transform { get; }
        List<Vector3> Vertices { get; }
        Mesh Mesh { get; }
        IReadOnlyCollection<IPaintManager> PaintManagers { get; }

        void AddPaintManager(IPaintManager paintManager);
        void RemovePaintManager(IPaintManager paintManager);

        void Init(Component paintComponent, Component rendererComponent);
        void SetDepthToWorldConverter(DepthToWorldConverter depthConverter);
        Vector2 GetUV(int channel, int index);
        IRaycastRequest RequestRaycast(ulong requestId, IPaintManager sender, Ray ray, int fingerId, Vector3? prevScreenPos, Vector3? screenPosition = null, bool useWorld = true, bool useCache = true, bool raycastAll = true);
        RaycastData TryGetRaycastResponse(RaycastRequestContainer request, out IList<Triangle> triangles);
        RaycastData GetRaycast(IPaintManager sender, Ray ray, int fingerId, Vector3? screenPosition = null, bool useWorld = true, bool raycastAll = true);
    }
}