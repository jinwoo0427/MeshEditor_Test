using UnityEngine;

namespace GetampedPaint.Core.PaintObject.Base
{
    public class BasePaintObjectData
    {
        private float pressure = 1f;
        public float Pressure
        {
            get => Mathf.Clamp(pressure, 0.01f, 10f);
            set => pressure = value;
        }
        
        public readonly LineData LineData;
        public Vector2 PreviousPaintPosition;
        public Vector2? ScreenPosition { get; set; }
        public Vector3? LocalPosition { get; set; }
        public Vector2? PaintPosition { get; set; }
        public bool InBounds { get; set; }
        public bool IsPainting { get; set; }
        public bool IsPaintingDone { get; set; }

        public BasePaintObjectData(bool useExtraDataForLines)
        {
            var lineElements = useExtraDataForLines ? 3 : 1;
            LineData = new LineData(lineElements);
        }
    }
}