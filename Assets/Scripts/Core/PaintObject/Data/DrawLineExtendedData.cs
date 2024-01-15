using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core.PaintObject.Base;

namespace GetampedPaint.Core.PaintObject.Data
{
    public class DrawLineExtendedData : BasePointerData
    {
        public IList<Vector2> LinesPositions;
        public Vector2 SegmentPositionStart;
        public Vector2 SegmentPositionEnd;
        public float StartPressure;
        public float EndPressure;

        public DrawLineExtendedData(IList<Vector2> linesPositions, Vector2 segmentPositionStart, Vector2 segmentPositionEnd, 
            float startPressure, float endPressure, int fingerId = 0) : base(fingerId)
        {
            LinesPositions = linesPositions;
            SegmentPositionStart = segmentPositionStart;
            SegmentPositionEnd = segmentPositionEnd;
            StartPressure = startPressure;
            EndPressure = endPressure;
        }
    }
}