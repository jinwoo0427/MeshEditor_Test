using System;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Images.Base;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public class SelectionToolSettings : BasePaintToolSettings
    {
        [PaintToolSettings] public bool UseAllActiveLayers { get; set; } = true;
        [PaintToolSettings] public bool SampleAlpha { get; set; } = true;

        public SelectionToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}