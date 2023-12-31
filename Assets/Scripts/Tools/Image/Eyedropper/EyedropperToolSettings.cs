using System;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Images.Base;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public class EyedropperToolSettings : BasePaintToolSettings
    {
        [PaintToolSettings] public bool UseAllActiveLayers { get; set; } = true;
        [PaintToolSettings] public bool SampleAlpha { get; set; } = true;
        
        public EyedropperToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}