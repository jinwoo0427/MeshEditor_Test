using UnityEngine;
using GetampedPaint.Controllers;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.Tools.Raycast.Data;
using GetampedPaint.Utils;

namespace GetampedPaint.Core.PaintObject
{
    public sealed class SkinnedMeshRendererPaint : BasePaintObject
    {
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh mesh;
        private Bounds bounds;

        public override bool CanSmoothLines => false;

        protected override void Init()
        {
            if (ObjectTransform.TryGetComponent(out skinnedMeshRenderer))
            {
				mesh = RaycastController.Instance.GetMesh(PaintManager);
            }

            if (mesh == null)
            {
                Debug.LogError("Can't find SkinnedMeshRenderer component!");
            }
        }

        protected override bool IsInBounds(Vector3 position)
        {
            if (skinnedMeshRenderer != null)
            {
                bounds = mesh.GetSubMesh(PaintManager.SubMesh).bounds;
                bounds = bounds.TransformBounds(skinnedMeshRenderer.transform);
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

        protected override void CalculatePaintPosition(int fingerId, Vector3 position, Vector2? uv = null, bool usePostPaint = true, RaycastData raycast = null)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.InBounds = IsInBounds(position, raycast);
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
                PaintObjectData[fingerId].PaintPosition = new Vector2(PaintMaterial.SourceTexture.width * uv.Value.x, PaintMaterial.SourceTexture.height * uv.Value.y);
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