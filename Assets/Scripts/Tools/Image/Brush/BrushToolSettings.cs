using System;
using GetampedPaint.Tools.Images.Base;

namespace GetampedPaint.Tools.Images
{
    [Serializable]
    public class BrushToolSettings : BasePatternPaintToolSettings
    {
        public BrushToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}