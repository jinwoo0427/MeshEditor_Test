using System;

namespace XDPaint.Core.PaintObject.Base
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PaintToolSettingsAttribute : Attribute
    {
        public int Group = 0;
    }
}