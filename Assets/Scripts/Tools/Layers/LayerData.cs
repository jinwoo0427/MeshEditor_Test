using System;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Utils;

namespace GetampedPaint.Tools.Layers
{
    [Serializable]
    public class LayerData
    {
        public bool IsEnabled;
        public bool IsMaskEnabled;
        public string Name;
        public float Opacity;
        public Texture SourceTexture;
        //public BlendingMode BlendingMode;
        public Texture2D Texture;
        public Texture2D Mask;

        public LayerData(ILayer layer)
        {
            IsEnabled = layer.Enabled;
            IsMaskEnabled = layer.MaskEnabled;
            Name = layer.Name;
            Opacity = layer.Opacity;
            SourceTexture = layer.SourceTexture;
            //BlendingMode = layer.BlendingMode;
            Texture = layer.RenderTexture.GetTexture2D();
            if (layer.MaskRenderTexture != null)
            {
                Mask = layer.MaskRenderTexture.GetTexture2D();
            }
        }
    }
}
