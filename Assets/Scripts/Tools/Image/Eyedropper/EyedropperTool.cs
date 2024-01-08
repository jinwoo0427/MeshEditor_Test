using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.PaintObject.Data;
using XDPaint.Tools.Images.Base;
using XDPaint.Utils;
using Object = UnityEngine.Object;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public sealed class EyedropperTool : BasePaintTool<EyedropperToolSettings>
    {
        [Preserve]
        public EyedropperTool(IPaintData paintData) : base(paintData)
        {
            Settings = new EyedropperToolSettings(paintData);
        }

        public override PaintTool Type => PaintTool.Eyedropper;
        public override bool RenderToLayer => false;
        public override bool RenderToInput => false;
        public override bool ShowPreview => false;
        private RenderTexture SourceTexture => Settings.UseAllActiveLayers ? 
            Data.TexturesHelper.GetTexture(RenderTarget.Combined) : 
            Data.LayersController.ActiveLayer.RenderTexture;
        
        
        
        private Material eyedropperMaterial;
        private RenderTexture brushTexture;
        private RenderTargetIdentifier rti;
        private Texture2D texture;

        public override void Enter()
        {
            base.Enter();
            InitMaterial();
            Data.LayersController.OnActiveLayerSwitched += OnActiveLayerSwitched;
        }

        public override void Exit()
        {
            Data.LayersController.OnActiveLayerSwitched -= OnActiveLayerSwitched;
            base.Exit();
            if (texture != null)
            {
                Object.Destroy(texture);
                texture = null;
            }
            if (eyedropperMaterial != null)
            {
                Object.Destroy(eyedropperMaterial);
                eyedropperMaterial = null;
            }
            if (brushTexture != null)
            {
                brushTexture.ReleaseTexture();
                brushTexture = null;
            }
        }

        private void OnActiveLayerSwitched(ILayer layer)
        {
            eyedropperMaterial.mainTexture = SourceTexture;
        }
        
        public override void UpdatePress(PointerData pointerData)
        {
            base.UpdatePress(pointerData);
            var brushOffset = GetPreviewVector(GetTexture(RenderTarget.ActiveLayer), pointerData.TexturePosition, pointerData.Pressure);
            eyedropperMaterial.SetVector(Constants.EyedropperShader.BrushOffset, brushOffset);
            UpdateRenderTexture();
            Render();
        }
        
        protected override Vector4 GetPreviewVector(Texture combinedTexture, Vector2 paintPosition, float pressure)
        {
            return new Vector2(paintPosition.x / combinedTexture.width, paintPosition.y / combinedTexture.height);
        }

        private void InitMaterial()
        {
            if (eyedropperMaterial == null)
            {
                eyedropperMaterial = new Material(Tools.Settings.Instance.EyedropperShader);
            }
            eyedropperMaterial.mainTexture = SourceTexture;
            texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        }

        /// <summary>
        /// Renders pixel to RenderTexture and set a new brush color
        /// </summary>
        private void Render()
        {
            Data.CommandBuilder.LoadOrtho().Clear().SetRenderTarget(rti).ClearRenderTarget(Constants.Color.ClearBlack).DrawMesh(Data.QuadMesh, eyedropperMaterial).Execute();
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = brushTexture;
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
            texture.Apply();
            RenderTexture.active = previousRenderTexture;
            var pixelColor = texture.GetPixel(0, 0);
            if (!Settings.SampleAlpha)
            {
                pixelColor.a = Data.Brush.Color.a;
            }
            Data.Brush.SetColor(pixelColor);
        }

        /// <summary>
        /// Creates 1x1 render texture
        /// </summary>
        private void UpdateRenderTexture()
        {
            if (brushTexture != null)
                return;
            brushTexture = RenderTextureFactory.CreateRenderTexture(1, 1);
            eyedropperMaterial.SetTexture(Constants.EyedropperShader.BrushTexture, brushTexture);
            rti = new RenderTargetIdentifier(brushTexture);
        }
    }
}