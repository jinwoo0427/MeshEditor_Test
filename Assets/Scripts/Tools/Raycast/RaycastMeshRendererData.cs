using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools.Raycast.Base;

namespace XDPaint.Tools.Raycast
{
    public class RaycastMeshRendererData : BaseRaycastMeshData
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        
        public override void Init(Component paintComponent, Component rendererComponent)
        {
            base.Init(paintComponent, rendererComponent);
            meshRenderer = rendererComponent as MeshRenderer;
            meshFilter = paintComponent as MeshFilter;
        }

        public override void AddPaintManager(IPaintManager paintManager)
        {
            base.AddPaintManager(paintManager);
            var mesh = meshFilter.sharedMesh;
            InitUVs(paintManager, mesh);
            InitTriangles(paintManager, mesh);
        }

        protected override void UpdateMeshBounds(IPaintManager paintManager)
        {
            MeshWorldBounds = meshRenderer.bounds;
        }
    }
}