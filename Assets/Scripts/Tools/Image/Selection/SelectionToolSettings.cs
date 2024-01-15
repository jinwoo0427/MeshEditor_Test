using System;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.Tools.Images.Base;

namespace GetampedPaint.Tools.Images
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