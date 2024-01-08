using System;
using XDPaint.Tools.Images.Base;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public class BrushToolSettings : BasePatternPaintToolSettings
    {
        public BrushToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}