using System;
using XDPaint.Tools.Images.Base;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public class EraseToolSettings : BasePaintToolSettings
    {
        public EraseToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}
