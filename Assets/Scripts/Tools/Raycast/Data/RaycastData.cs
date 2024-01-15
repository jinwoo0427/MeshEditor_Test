using UnityEngine;

namespace GetampedPaint.Tools.Raycast.Data
{
    public class RaycastData
    {
        private Barycentric barycentric;

        public Vector3 Hit
        {
            get => barycentric.Interpolate(triangle.Position0, triangle.Position1, triangle.Position2);
            set => barycentric = new Barycentric(triangle.Position0, triangle.Position1, triangle.Position2, value);
        }
        
        public Vector3 WorldHit
        {
            get
            {
                //로컬에서 월드로 변환
                var localHit = Hit;
                return triangle.Transform.localToWorldMatrix.MultiplyPoint(localHit);
            }
        }
                
        private Vector2 uvHit;
        public Vector2 UVHit
        {
            get => uvHit;
            set => uvHit = value;
        }
        
        private Triangle triangle;
        public Triangle Triangle => triangle;

        public RaycastData(Triangle triangle)
        {
            this.triangle = triangle;
            barycentric = new Barycentric();
        }
    }
}