using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools.Raycast.Data;

namespace XDPaint.Core.PaintObject.Base
{
    public class LineData
    {
        public readonly List<RaycastData> Raycasts = new List<RaycastData>();
        public readonly List<Vector2> PaintPositions = new List<Vector2>();
        public readonly List<float> BrushSizes = new List<float>();
        public readonly int LineElements;

        public LineData(int maxElementsCount)
        {
            LineElements = maxElementsCount;
        }
        
        public void AddBrush(float brushSize)
        {
            if (BrushSizes.Count > LineElements)
            {
                BrushSizes.RemoveAt(0);
            }
            BrushSizes.Add(brushSize);
        }

        public void AddPosition(Vector2 texturePosition)
        {
            if (PaintPositions.Count > LineElements)
            {
                PaintPositions.RemoveAt(0);
            }
            PaintPositions.Add(texturePosition);
        }

        public void AddRaycast(RaycastData raycast)
        {
            if (Raycasts.Count > 1)
            {
                Raycasts.RemoveAt(0);
            }
            Raycasts.Add(raycast);
        }

        public bool HasOnePosition()
        {
            return PaintPositions.Count == 1;
        }

        public bool HasDifferentTriangles()
        {
            return Raycasts.Count == 2 && Raycasts[0].Triangle.Id != Raycasts[1].Triangle.Id;
        }

        public void Clear()
        {
            Raycasts.Clear();
            PaintPositions.Clear();
            BrushSizes.Clear();
        }
    }
}