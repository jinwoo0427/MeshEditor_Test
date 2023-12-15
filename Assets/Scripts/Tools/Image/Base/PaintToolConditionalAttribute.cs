using System;

namespace XDPaint.Core.PaintObject.Base
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PaintToolConditionalAttribute : Attribute
    {
        public string Condition;

        public PaintToolConditionalAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
