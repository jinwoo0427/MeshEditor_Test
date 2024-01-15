using UnityEngine;

namespace GetampedPaint.Utils
{
    public class MeshGenerator
    {
        /// <summary>
        /// 쿼드 생성
        /// </summary>
        public static Mesh GenerateQuad(Vector3 size, Vector3 offset)
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    Vector3.up * size.y - offset,
                    new Vector3(1f * size.x, 1f * size.y, 0) - offset,
                    Vector3.right * size.x - offset,
                    Vector3.zero - offset,
                },
                uv = new[]
                {
                    Vector2.up, 
                    Vector2.one, 
                    Vector2.right, 
                    Vector2.zero
                },
                triangles = new[]
                {
                    0, 1, 2,
                    2, 3, 0
                },
                colors = new[]
                {
                    Color.white,
                    Color.white,
                    Color.white,
                    Color.white
                }
            };
            return mesh;
        }
    }
}