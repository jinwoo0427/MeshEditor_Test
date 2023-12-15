using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Tools.Image.Base
{
    public abstract class BasePatternPaintToolSettings : BasePaintToolSettings
    {
        // ReSharper disable once InconsistentNaming
        internal bool usePattern;
        [PaintToolSettings] public bool UsePattern
        {
            get => usePattern;
            set
            {
                usePattern = value;
                if (usePattern)
                {
                    Data.Material.EnableKeyword(Constants.PaintShader.TileKeyword);
                }
                else
                {
                    Data.Material.DisableKeyword(Constants.PaintShader.TileKeyword);
                }
                OnPropertyChanged();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once Unity.RedundantSerializeFieldAttribute
        // ReSharper disable once InconsistentNaming
        [PaintToolSettings(Group = 1), PaintToolConditional("UsePattern"), SerializeField] internal Texture patternTexture;
        [PaintToolSettings(Group = 1), PaintToolConditional("UsePattern")] public Texture PatternTexture
        {
            get => patternTexture;
            set
            {
                patternTexture = value;
                if (patternTexture.wrapMode != TextureWrapMode.Repeat)
                {
                    Debug.LogWarning("Pattern texture does not have Wrap Mode = Repeat");
                }
                PatternScale = patternScale;
                Data.Material.SetTexture(Constants.PaintShader.PatternTexture, patternTexture);
            }
        }

        private Vector2 patternScale = Vector2.one;
        // ReSharper disable once MemberCanBePrivate.Global
        [PaintToolSettings, PaintToolConditional("UsePattern")] public Vector2 PatternScale
        {
            get => patternScale;
            set
            {
                patternScale = value;
                var layerTexture = Data.LayersController.ActiveLayer.RenderTexture;
                var scale = Vector2.one;
                if (patternTexture != null)
                {
                    scale = new Vector2(layerTexture.width / (float)patternTexture.width, layerTexture.height / (float)patternTexture.height);
                }
                Data.Material.SetVector(Constants.PaintShader.PatternScale, Vector2.one / patternScale * scale);
            }
        }

        private float patternAngle;
        [PaintToolSettings, PaintToolConditional("UsePattern")] public float PatternAngle
        {
            get => patternAngle;
            set
            {
                patternAngle = value;
                Data.Material.SetFloat(Constants.PaintShader.PatternAngle, patternAngle);
            }
        }
        
        private Vector2 patternOffset = Vector2.zero;
        [PaintToolSettings, PaintToolConditional("UsePattern")] public Vector2 PatternOffset
        {
            get => patternOffset;
            set
            {
                patternOffset = value;
                Data.Material.SetVector(Constants.PaintShader.PatternOffset, patternOffset);
            }
        }
        
        protected BasePatternPaintToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}