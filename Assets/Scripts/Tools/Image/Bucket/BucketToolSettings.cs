using System;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.Tools.Images.Base;

namespace GetampedPaint.Tools.Images
{
    [Serializable]
    public class BucketToolSettings : BasePatternPaintToolSettings
    {
        [PaintToolSettings, PaintToolRange(0.0001f, 1f)] public float Tolerance { get; set; } = 0.01f;
        [PaintToolSettings] public bool CanFillAlpha { get; set; } = true;
        
        public BucketToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}
