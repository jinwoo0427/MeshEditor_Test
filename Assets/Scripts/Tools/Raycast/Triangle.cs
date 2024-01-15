using System;
using UnityEngine;
using GetampedPaint.Tools.Raycast.Base;

namespace GetampedPaint.Tools.Raycast
{
    [Serializable]
    public class Triangle
    {
        public int[] indices = new int[3];
        public int Id;
        public int I0;
        public int I1;
        public int I2;

        private IRaycastMeshData meshData;
        private int uvChannel;

        public Transform Transform => meshData.Transform;
        public Vector3 Position0 => meshData.Vertices[I0];
        public Vector3 Position1 => meshData.Vertices[I1];
        public Vector3 Position2 => meshData.Vertices[I2];

        public Vector2 UV0 => meshData.GetUV(uvChannel, I0);
        public Vector2 UV1 => meshData.GetUV(uvChannel, I1);
        public Vector2 UV2 => meshData.GetUV(uvChannel, I2);

        public Triangle(int id, int index0, int index1, int index2)
        {
            Id = id;
            I0 = index0;
            I1 = index1;
            I2 = index2;
        }

        public void SetRaycastMeshData(IRaycastMeshData raycastMeshData, int uvChannelIndex = 0)
        {
            meshData = raycastMeshData;
            uvChannel = uvChannelIndex;
        }

        private static int MIN(int lhs, int rhs)
        {
            return lhs < rhs ? lhs : rhs;
        }

        private static int MAX(int lhs, int rhs)
        {
            return lhs > rhs ? lhs : rhs;
        }

        public override int GetHashCode()
        {
            int min = MIN(MIN(I0, I1), I2);
            int max = MAX(MAX(I0, I1), I2);
            int mid = (I0 != min && I0 != max) ? I0 : (I1 != min && I1 != max) ? I1 : I2;

            // Calculate the hash code for the product. 
            return min ^ mid ^ max;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", I0, I1, I2);
        }

        public Edge[] GetEdges()
        {
            return new Edge[3] {
                    new Edge(I0, I1),
                    new Edge(I1, I2),
                    new Edge(I2, I0)
                };
        }

        public int[] GetIndices()
        {
            return indices;
        }
    }
}