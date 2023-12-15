using UnityEngine;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Raycast.Data;

namespace XDPaint.Core.PaintObject
{
    public sealed class MeshRendererPaint : BasePaintObject
    {
        private Renderer renderer;
        private Mesh mesh;
        private Bounds bounds;

        public override bool CanSmoothLines => false;

        protected override void Init()
        {
            ObjectTransform.TryGetComponent(out renderer);
            
            if (ObjectTransform.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                mesh = meshFilter.sharedMesh;
            }
            
            if (mesh == null)
            {
                Debug.LogError("Can't find MeshFilter component!");
            }
        }

        protected override bool IsInBounds(Vector3 position)
        {
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            var ray = Camera.ScreenPointToRay(position);
            var inBounds = bounds.IntersectRay(ray);
            return inBounds;
        }

        protected override bool IsInBounds(Vector3 position, RaycastData raycast)
        {
            if (raycast == null)
                return false;
            
            return IsInBounds(position);
        }

        protected override void CalculatePaintPosition(int fingerId, Vector3 position, Vector2? uv = null, bool usePostPaint = true, RaycastData raycastData = default)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.InBounds = IsInBounds(position, raycastData);
            if (!paintObjectData.InBounds)
            {
                PaintObjectData[fingerId].PaintPosition = null;
                if (usePostPaint)
                {
                    OnPostPaint(fingerId);
                }
                else
                {
                    UpdateBrushPreview(fingerId);
                }
                return;
            }

            var hasRaycast = uv != null;
            if (hasRaycast)
            {
                paintObjectData.PaintPosition = new Vector2(PaintMaterial.SourceTexture.width * uv.Value.x, PaintMaterial.SourceTexture.height * uv.Value.y);
                paintObjectData.IsPaintingDone = true;
            }

            if (usePostPaint)
            {
                OnPostPaint(fingerId);
            }
            else
            {
                UpdateBrushPreview(fingerId);
            }
        }
    }
}