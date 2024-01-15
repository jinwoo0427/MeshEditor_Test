using UnityEngine;
using GetampedPaint.Core.PaintObject.Base;

namespace GetampedPaint.Core.PaintObject.Data
{
    public class PointerData : BasePointerData
    {
        public Vector3 LocalPosition;
        public Vector2 ScreenPosition;
        public Vector2 UV;
        public Vector2 TexturePosition;
        public readonly float Pressure;

        public PointerData(Vector3 localPosition, Vector2 screenPosition, Vector2 uv, Vector2 texturePosition,
            float pressure, int fingerId = 0) : base(fingerId)
        {
            LocalPosition = localPosition;
            ScreenPosition = screenPosition;
            UV = uv;
            TexturePosition = texturePosition;
            Pressure = pressure;
        }
    }
}