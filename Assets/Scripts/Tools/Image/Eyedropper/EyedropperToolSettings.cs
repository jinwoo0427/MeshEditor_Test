using System;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
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