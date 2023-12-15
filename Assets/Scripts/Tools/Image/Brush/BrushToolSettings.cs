using System;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    [Serializable]
    public class BrushToolSettings : BasePatternPaintToolSettings
    {
        public BrushToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}