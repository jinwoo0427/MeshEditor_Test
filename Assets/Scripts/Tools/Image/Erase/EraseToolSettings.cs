using System;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    [Serializable]
    public class EraseToolSettings : BasePaintToolSettings
    {
        public EraseToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}
