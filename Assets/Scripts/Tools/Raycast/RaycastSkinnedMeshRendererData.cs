using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools.Raycast.Base;
using XDPaint.Tools.Raycast.Data;
using XDPaint.Utils;

namespace XDPaint.Tools.Raycast
{
    public class RaycastSkinnedMeshRendererData : BaseRaycastMeshData
    {
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Transform transform;
        private TransformData transformData;
        private Transform[] bones;
        private TransformData[] bonesData;

        public override void Init(Component paintComponent, Component rendererComponent)
        {
            base.Init(paintComponent, rendererComponent);
            skinnedMeshRenderer = rendererComponent as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                transform = skinnedMeshRenderer.transform;
                transformData = new TransformData
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    LossyScale = transform.lossyScale
                };
                
                bones = skinnedMeshRenderer.bones;
                var bonesCount = bones.Length;
                bonesData = new TransformData[bonesCount];
                for (var i = 0; i < bonesCount; i++)
                {
                    var boneTransform = bones[i];
                    bonesData[i].Position = boneTransform.position;
                    bonesData[i].Rotation = boneTransform.rotation;
                    bonesData[i].LossyScale = boneTransform.lossyScale;
                }
            }
            else
            {
                Debug.LogError("Can't find SkinnedMeshRenderer component!");
            }
        }

        public override void AddPaintManager(IPaintManager paintManager)
        {
            base.AddPaintManager(paintManager);
            BakeMesh(paintManager);
            InitUVs(paintManager, Mesh);
            InitTriangles(paintManager, Mesh);
        }

        protected override void UpdateMeshBounds(IPaintManager paintManager)
        {
            TryBakeMesh(paintManager);
            MeshWorldBounds = Mesh.bounds.TransformBounds(transform);
        }

        private void TryBakeMesh(IPaintManager sender, bool bakeForced = false)
        {
            if (bakeForced || BakedFrame != Time.frameCount)
            {
                // if (IsBonesTransformsChanged())
                {
                    BakeMesh(sender);
                }
                BakedFrame = Time.frameCount;
            }
        }

        private void BakeMesh(IPaintManager sender)
        {
#if UNITY_2020_2_OR_NEWER
            skinnedMeshRenderer.BakeMesh(Mesh, true);
#else
            skinnedMeshRenderer.BakeMesh(Mesh);
#endif
            Mesh.GetVertices(Vertices);
            Mesh.GetUVs(sender.UVChannel, UV);
            Mesh.RecalculateBounds();
            IsTrianglesDataUpdated = false;
        }

        private bool IsBonesTransformsChanged()
        {
            var isChanged = false;
            
            if (transformData.Position != transform.position || 
                transformData.Rotation != transform.rotation || 
                transformData.LossyScale != transform.lossyScale)
            {
                transformData.Position = transform.position;
                transformData.Rotation = transform.rotation;
                transformData.LossyScale = transform.lossyScale;
                isChanged = true;
            }
            
            for (var i = 0; i < bones.Length; i++)
            {
                var boneTransform = bones[i];
                if (boneTransform.position != bonesData[i].Position || 
                    boneTransform.rotation != bonesData[i].Rotation || 
                    boneTransform.lossyScale != bonesData[i].LossyScale)
                {
                    bonesData[i].Position = boneTransform.position;
                    bonesData[i].Rotation = boneTransform.rotation;
                    bonesData[i].LossyScale = boneTransform.lossyScale;
                    isChanged = true;
                }
            }
            return isChanged;
        }
    }
}