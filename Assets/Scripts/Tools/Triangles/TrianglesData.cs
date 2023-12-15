using System;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools.Raycast;

namespace XDPaint.Tools.Triangles
{
    public static class TrianglesData
    {
        /// <summary>
        /// 이 함수는 Mesh의 특정 서브메시에서 삼각형 데이터와 UV 좌표 데이터를 추출하여 
        /// 해당 데이터를 가진 Triangle 객체 배열을 반환합니다
        /// </summary>
        public static Triangle[] GetData(Mesh mesh, int subMeshIndex, int uvChannel)
        {
            //지정된 서브메시의 인덱스 배열을 가져옵니다. 이 배열은 삼각형을 이루는 정점의 인덱스를 나타냅니다.
            var indices = mesh.GetTriangles(subMeshIndex);
            if (indices.Length == 0)
            {
                Debug.LogError("메시에 인덱스가 없습니다!(유효한 삼각형이 없는 경우)");
                return Array.Empty<Triangle>(); // 빈 배열 반환
            }

            var uvData = new List<Vector2>();
            //지정된 UV 채널에서 UV 좌표 데이터를 가져옵니다.
            mesh.GetUVs(uvChannel, uvData);
            if (uvData.Count == 0)
            {
                Debug.LogError("선택한 채널에 메쉬의 UV가 없습니다!");
                return Array.Empty<Triangle>();
            }

            var indexesCount = indices.Length;

            // 인덱스 배열을 기반으로 Triangle 배열을 초기화합니다.
            // 각 삼각형은 3개의 인덱스로 이루어져 있으므로
            // 배열의 크기는 인덱스 배열 길이의 1/3이 됩니다.
            var triangles = new Triangle[indexesCount / 3];
            for (var i = 0; i < indexesCount; i += 3)
            {
                // Triangle 객체를 생성하고 배열에 할당합니다.
                var index = i / 3;
                var index0 = indices[i + 0];
                var index1 = indices[i + 1];
                var index2 = indices[i + 2];
                triangles[index] = new Triangle(index, index0, index1, index2);
            }

            return triangles;
        }
    }
}