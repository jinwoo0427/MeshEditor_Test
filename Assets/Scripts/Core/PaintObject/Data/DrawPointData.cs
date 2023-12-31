using UnityEngine;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintObject.Data
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