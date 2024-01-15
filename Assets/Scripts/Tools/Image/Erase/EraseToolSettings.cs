using System;
using GetampedPaint.Tools.Images.Base;

namespace GetampedPaint.Tools.Images
{
    [Serializable]
    public class EraseToolSettings : BasePaintToolSettings
    {
        public EraseToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}
