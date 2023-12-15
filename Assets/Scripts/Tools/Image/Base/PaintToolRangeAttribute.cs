using System;
using UnityEngine;

namespace XDPaint.Core.PaintObject.Base
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PaintToolRangeAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;

        public PaintToolRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
