using System;
using UnityEngine;
using XDPaint.Tools.Raycast.Base;

namespace XDPaint.Tools.Raycast
{
    [Serializable]
    public class Triangle
    {
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
    }
}