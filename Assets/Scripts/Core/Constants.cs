using System.Collections.Generic;
using UnityEngine;

namespace GetampedPaint.Core
{
    public static class Constants
    {
        public static class Defines
        {
            public static readonly string[] VREnabled = { "XDPAINT_VR_ENABLE" };
        }
        
        public static class Color
        {
            public static readonly UnityEngine.Color ClearBlack = new UnityEngine.Color(0, 0, 0, 0);
            public static readonly UnityEngine.Color ClearWhite = new UnityEngine.Color(1, 1, 1, 0);

            public static UnityEngine.Color ProBuilderBlue = new UnityEngine.Color(0f, .682f, .937f, 1f);
            public static UnityEngine.Color FaceSelectionColor = new UnityEngine.Color(0f, .682f, .937f, .3f);
            public static UnityEngine.Color ProBuilderLightGray = new UnityEngine.Color(.35f, .35f, .35f, .4f);
            public static UnityEngine.Color ProBuilderDarkGray = new UnityEngine.Color(.1f, .1f, .1f, .3f);

        }
       
        public static class PaintShader
        {
            public const string PaintTexture = "_MainTex";
            public const string InputTexture = "_Input";
            public const string MaskTexture = "_Mask";
            public const string BrushTexture = "_Brush";
            public const string BrushOffset = "_BrushOffset";
            public const string Opacity = "_Opacity";
            public const string TileKeyword = "TILE_ON";
            public const string PatternTexture = "_PatternTex";
            public const string PatternScale = "_PatternScale";
            public const string PatternOffset = "_PatternOffset";
            public const string PatternAngle = "_PatternAngle";
            //public static readonly Dictionary<BlendingMode, string> LayerBlendFormat = new Dictionary<BlendingMode, string>()
            //{
            //    { BlendingMode.Normal, "XDPAINT_LAYER_BLEND_NORMAL"},
            //    { BlendingMode.Darken, "XDPAINT_LAYER_BLEND_DARKEN"},
            //    { BlendingMode.Multiply, "XDPAINT_LAYER_BLEND_MULTIPLY"},
            //    { BlendingMode.ColorBurn, "XDPAINT_LAYER_BLEND_COLORBURN"},
            //    { BlendingMode.LinearBurn, "XDPAINT_LAYER_BLEND_LINEARBURN"},
            //    { BlendingMode.DarkerColor, "XDPAINT_LAYER_BLEND_DARKERCOLOR"},
            //    { BlendingMode.Lighten, "XDPAINT_LAYER_BLEND_LIGHTEN"},
            //    { BlendingMode.Screen, "XDPAINT_LAYER_BLEND_SCREEN"},
            //    { BlendingMode.ColorDodge, "XDPAINT_LAYER_BLEND_COLORDODGE"},
            //    { BlendingMode.LinearDodge, "XDPAINT_LAYER_BLEND_LINEARDODGE"},
            //    { BlendingMode.LighterColor, "XDPAINT_LAYER_BLEND_LIGHTERCOLOR"},
            //    { BlendingMode.Overlay, "XDPAINT_LAYER_BLEND_OVERLAY"},
            //    { BlendingMode.SoftLight, "XDPAINT_LAYER_BLEND_SOFTLIGHT"},
            //    { BlendingMode.HardLight, "XDPAINT_LAYER_BLEND_HARDLIGHT"},
            //    { BlendingMode.VividLight, "XDPAINT_LAYER_BLEND_VIVIDLIGHT"},
            //    { BlendingMode.LinearLight, "XDPAINT_LAYER_BLEND_LINEARLIGHT"},
            //    { BlendingMode.PinLight, "XDPAINT_LAYER_BLEND_PINLIGHT"},
            //    { BlendingMode.HardMix, "XDPAINT_LAYER_BLEND_HARDMIX"},
            //    { BlendingMode.Difference, "XDPAINT_LAYER_BLEND_DIFFERENCE"},
            //    { BlendingMode.Exclusion, "XDPAINT_LAYER_BLEND_EXCLUSION"},
            //    { BlendingMode.Subtract, "XDPAINT_LAYER_BLEND_SUBTRACT"},
            //    { BlendingMode.Divide, "XDPAINT_LAYER_BLEND_DIVIDE"},
            //    { BlendingMode.Hue, "XDPAINT_LAYER_BLEND_HUE"},
            //    { BlendingMode.Saturation, "XDPAINT_LAYER_BLEND_SATURATION"},
            //    { BlendingMode.Color, "XDPAINT_LAYER_BLEND_COLOR"},
            //    { BlendingMode.Luminosity, "XDPAINT_LAYER_BLEND_LUMINOSITY"}
            //};
        }

        public static class BrushShader
        {
            public const string SrcColorBlend = "_SrcColorBlend";
            public const string DstColorBlend = "_DstColorBlend";
            public const string SrcAlphaBlend = "_SrcAlphaBlend";
            public const string DstAlphaBlend = "_DstAlphaBlend";
            public const string BlendOpColor = "_BlendOpColor";
            public const string BlendOpAlpha = "_BlendOpAlpha";
            public const string Hardness = "_Hardness";
            public const string TexelSize = "_TexelSize";
            public const string ScaleUV = "_ScaleUV";
            public const string Offset = "_Offset";
            public const int RenderTexturePadding = 2;
        }
        
        public static class EyedropperShader
        {
            public static readonly int BrushTexture = Shader.PropertyToID("_BrushTex");
            public static readonly int BrushOffset = Shader.PropertyToID("_BrushOffset");
        }

        public static class BrushSamplerShader
        {
            public static readonly int BrushTexture = Shader.PropertyToID("_BrushTex");
            public static readonly int BrushMaskTexture = Shader.PropertyToID("_BrushMaskTex");
            public static readonly int BrushOffset = Shader.PropertyToID("_BrushOffset");
        }
        
        public static class CloneShader
        {
            public static readonly int MaskTexture = Shader.PropertyToID("_MaskTex");
            public static readonly int Offset = Shader.PropertyToID("_Offset");
        }

        public static class BlurShader
        {
            public static readonly int MaskTexture = Shader.PropertyToID("_MaskTex");
            public static readonly int BlurSize = Shader.PropertyToID("_BlurSize");
        }
        
        public static class GaussianBlurShader
        {
            public static readonly int Size = Shader.PropertyToID("_KernelSize");
            public static readonly int Spread = Shader.PropertyToID("_Spread");
            public static readonly int MaskTexture = Shader.PropertyToID("_MaskTex");
        }
        
        public static class GrayscaleShader
        {
            public static readonly int MaskTexture = Shader.PropertyToID("_MaskTex");
        }

        public static class DepthToWorldPositionShader
        {
            public static readonly int ScreenVector = Shader.PropertyToID("_ScreenUV");
            public static readonly int InverseViewProjectionMatrix = Shader.PropertyToID("_Matrix_IVP");
        }
    }
}