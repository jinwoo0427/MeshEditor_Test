using UnityEngine;

namespace GetampedPaint.Tools.Raycast
{
    //무게 중심
    public class Barycentric
    {
        private float u;
        private float v;
        private float w;

        public Barycentric() { }

        //삼각형 내의 한 점 나타내기
        public Barycentric(Vector3 aV1, Vector3 aV2, Vector3 aV3, Vector3 aP)
        {
            //세 변의 벡터를 계산
            Vector3 a = aV2 - aV3, b = aV1 - aV3, c = aP - aV3;

            //각 벡터의 길이의 제곱을 계산
            var aLen = a.x * a.x + a.y * a.y + a.z * a.z;
            var bLen = b.x * b.x + b.y * b.y + b.z * b.z;

            //각 벡터 사이의 내적을 계산
            var ab = a.x * b.x + a.y * b.y + a.z * b.z;
            var ac = a.x * c.x + a.y * c.y + a.z * c.z;
            var bc = b.x * c.x + b.y * c.y + b.z * c.z;

            // 여긴 그냥 공식인거 같다... 걍 외우자 ( 이해하기 귀찮... )
            var d = aLen * bLen - ab * ab;
            u = (aLen * bc - ab * ac) / d;
            v = (bLen * ac - ab * bc) / d;
            w = 1.0f - u - v;
        }
        // u, v, w의 가중치에 따라 선형 보간
        public Vector3 Interpolate(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return v1 * u + v2 * v + v3 * w;
        }
    }
}