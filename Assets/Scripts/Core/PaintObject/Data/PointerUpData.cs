using UnityEngine;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintObject.Data
{
    public class PointerUpData : BasePointerData
    {
        public Vector2 ScreenPosition;
        public readonly bool IsInBounds;

        public PointerUpData(Vector2 screenPosition, bool isInBounds, int fingerId = 0) : base(fingerId)
        {
            ScreenPosition = screenPosition;
            IsInBounds = isInBounds;
        }
    }
}