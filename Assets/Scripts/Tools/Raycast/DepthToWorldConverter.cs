using UnityEngine;
using UnityEngine.Rendering;
using GetampedPaint.Controllers;
using GetampedPaint.Core;
using GetampedPaint.Utils;

namespace GetampedPaint.Tools.Raycast
{
    public class DepthToWorldConverter : IDisposable
    {
        public bool IsEnabled = true;
        
        private CommandBufferBuilder commandBuffer;
        private RenderTexture renderTexture;
        private Mesh quadMesh;
        private Material material;
        private Texture2D texture;
        private int frameId;
        private bool supportsCopyTexture;

        public void Init()
        {
            DoDispose();
            // 시스템이 RenderTexture에서 텍스처로의 복사를 지원하는지 여부 확인
            supportsCopyTexture = (SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) == CopyTextureSupport.RTToTexture;
            commandBuffer = new CommandBufferBuilder();
            quadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
            material = new Material(Settings.Instance.DepthToWorldPositionShader);
            renderTexture = RenderTextureFactory.CreateRenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
            texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false, true);
        }
        
        public void DoDispose()
        {
            commandBuffer?.Release();
            if (quadMesh != null)
            {
                Object.Destroy(quadMesh);
            }
            if (material != null)
            {
                Object.Destroy(material);
            }
            if (renderTexture != null)
            {
                renderTexture.ReleaseTexture();
            }
            if (texture != null)
            {
                Object.Destroy(texture);
            }
        }
        
        public Vector4 GetPosition(Vector2 screenPosition)
        {
            if (frameId == Time.frameCount)
                return texture.GetPixel(Mathf.RoundToInt(screenPosition.x), Mathf.RoundToInt(screenPosition.y));
            
            var mainCamera = PaintController.Instance.Camera;
            //카메라의 투영 행렬을 GPU 투영 행렬로 가져옵니다.
            var projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false); 

            // 카메라의 역뷰프로젝션 행렬을 계산합니다.
            var inverseViewProjectionMatrix = (projectionMatrix * mainCamera.worldToCameraMatrix).inverse;
            //쉐이더에 역뷰프로젝션 행렬을 전달합니다.
            material.SetMatrix(Constants.DepthToWorldPositionShader.InverseViewProjectionMatrix, inverseViewProjectionMatrix);
            //CommandBuffer를 사용하여 특정 렌더 텍스처에 대한 렌더링 명령을 생성하고 실행합니다.
            commandBuffer.LoadOrtho().Clear().SetRenderTarget(renderTexture).DrawMesh(quadMesh, material).Execute();

            // 텍스처 변환을 지원하면 Graphics.ConvertTexture을 사용하여 텍스처를 복사합니다.
            if (supportsCopyTexture && Graphics.ConvertTexture(renderTexture, texture))
            {
                frameId = Time.frameCount;// 현재 프레임 ID를 저장
                return texture.GetPixel(Mathf.RoundToInt(screenPosition.x), Mathf.RoundToInt(screenPosition.y));
            }

            var prevRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = prevRenderTexture;
            frameId = Time.frameCount;
            return texture.GetPixel(Mathf.RoundToInt(screenPosition.x), Mathf.RoundToInt(screenPosition.y));
        }
    }
}