using UnityEngine;
using GetampedPaint.Core.PaintObject.Base;

namespace GetampedPaint.Core.PaintObject.Data
{
    public class DrawPointData : BasePointerData
    {
        public Vector2 TexturePosition;
        public float Pressure;

        public DrawPointData(Vector2 texturePosition, float pressure = 1f, int fingerId = 0) : base(fingerId)
        {
            TexturePosition = texturePosition;
            Pressure = pressure;
        }
    }
}