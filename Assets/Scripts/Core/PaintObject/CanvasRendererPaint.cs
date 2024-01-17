using UnityEngine;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.Tools.Raycast.Data;
using UnityEngine.UI;

namespace GetampedPaint.Core.PaintObject
{
    public sealed class CanvasRendererPaint : BasePaintObject
    {
        private Canvas canvas;
        private RectTransform rectTransform;
        private Vector2 objectBoundsSize;
        private RenderMode renderMode;
        private RawImage rawImage;

        public override bool CanSmoothLines => true;

        protected override void Init()
        {
#if UNITY_2020_3_OR_NEWER
            canvas = ObjectTransform.transform.GetComponentInParent<Canvas>(true);
#else
            canvas = ObjectTransform.transform.GetComponentInParent<Canvas>();
#endif
            ObjectTransform.TryGetComponent(out rectTransform);
            ObjectTransform.TryGetComponent(out rawImage);
            UpdateObjectBounds();
        }

        protected override bool IsInBounds(Vector3 position)
        {
            Vector2 clickPosition = position;
            var bounds = new Bounds(rectTransform.position, Vector2.Scale(rectTransform.rect.size, ObjectTransform.lossyScale));
            bounds.center = new Vector3(bounds.center.x, bounds.center.y, 0);
            if (renderMode == RenderMode.ScreenSpaceOverlay)
            {
                bounds.size += new Vector3(Brush.RenderTexture.width, Brush.RenderTexture.height);
            }
            else
            {
                var offset = new Vector3(
                    Brush.RenderTexture.width * Brush.Size / PaintMaterial.SourceTexture.width * bounds.size.x,
                    Brush.RenderTexture.height * Brush.Size/ PaintMaterial.SourceTexture.height * bounds.size.y);
                bounds.center = rectTransform.position;
                bounds.size += offset;
                var ray = Camera.ScreenPointToRay(clickPosition);
                return bounds.IntersectRay(ray);
            }
            return bounds.Contains(clickPosition);
        }

        protected override bool IsInBounds(Vector3 position, RaycastData raycast)
        {
            return IsInBounds(position);
        }

        private void UpdateObjectBounds()
        {
            if (rectTransform != null)
            {
                var rect = rectTransform.rect;
                var lossyScale = rectTransform.lossyScale;
                objectBoundsSize = new Vector2(rect.size.x * lossyScale.x, rect.size.y * lossyScale.y);
                if (canvas != null)
                {
                    renderMode = canvas.renderMode;
                }
                else
                {
                    Debug.LogWarning("Can't find Canvas component in parent GameObjects!");
                }
            }
        }

        protected override void CalculatePaintPosition(int fingerId, Vector3 position, Vector2? uv = null, bool usePostPaint = true, RaycastData raycast = null)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.InBounds = IsInBounds(position);
            if (paintObjectData.InBounds)
            {
                paintObjectData.IsPaintingDone = true;
            }
            
            Vector3 clickPosition;
            if (renderMode == RenderMode.ScreenSpaceOverlay)
            {
                clickPosition = position;
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, position, Camera, out clickPosition);
            }

            var surfaceLocalClickPosition = ObjectTransform.InverseTransformPoint(clickPosition);
            var lossyScale = ObjectTransform.lossyScale;
            var clickLocalPosition = new Vector2(surfaceLocalClickPosition.x * lossyScale.x, surfaceLocalClickPosition.y * lossyScale.y);
            paintObjectData.LocalPosition = clickLocalPosition / lossyScale;
            UpdateObjectBounds();
            clickLocalPosition += objectBoundsSize / 2f;
            var rectWidth = (int)(PaintMaterial.SourceTexture.width * rawImage.uvRect.width);
            var rectHeight = (int)(PaintMaterial.SourceTexture.height * rawImage.uvRect.height);

            // Adjust UV coordinates based on rawImage.uvRect
            var uvX = rawImage.uvRect.x;
            var uvY = rawImage.uvRect.y;
            var uvWidth = rawImage.uvRect.width;
            var uvHeight = rawImage.uvRect.height;

            int dragX = (int)((PaintMaterial.SourceTexture.width * uvWidth) * (clickLocalPosition.x / PaintMaterial.SourceTexture.width) + (uvX * PaintMaterial.SourceTexture.width));
            int dragY = (int)((PaintMaterial.SourceTexture.height * uvHeight) * (clickLocalPosition.y / PaintMaterial.SourceTexture.height) + (uvY * PaintMaterial.SourceTexture.height));

            var ppi = new Vector2(
                rectWidth / objectBoundsSize.x / lossyScale.x,
                rectHeight / objectBoundsSize.y / lossyScale.y);
            paintObjectData.PaintPosition = new Vector2(
                (dragX),
                (dragY));

            //var ppi = new Vector2(
            //    rectWidth / objectBoundsSize.x / lossyScale.x,
            //    rectHeight / objectBoundsSize.y / lossyScale.y);
            //paintObjectData.PaintPosition = new Vector2(
            //    clickLocalPosition.x * lossyScale.x * ppi.x,
            //    clickLocalPosition.y * lossyScale.y * ppi.y);
            //Debug.Log(ppi + " : " + paintObjectData.PaintPosition);

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