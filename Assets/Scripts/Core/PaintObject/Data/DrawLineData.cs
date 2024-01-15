using UnityEngine;
using GetampedPaint.Core.PaintObject.Base;

namespace GetampedPaint.Core.PaintObject.Data
{
    public class DrawLineData : BasePointerData
    {
        public Vector2 LineStartPosition;
        public Vector2 LineEndPosition;
        public float StartPressure;
        public float EndPressure;

        public DrawLineData(Vector2 lineStartPosition, Vector2 lineEndPosition, float startPressure, float endPressure,
            int fingerId = 0) : base(fingerId)
        {
            LineStartPosition = lineStartPosition;
            LineEndPosition = lineEndPosition;
            StartPressure = startPressure;
            EndPressure = endPressure;
        }
    }
}