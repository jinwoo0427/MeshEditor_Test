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
            // �ý����� RenderTexture���� �ؽ�ó���� ���縦 �����ϴ��� ���� Ȯ��
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
            //ī�޶��� ���� ����� GPU ���� ��ķ� �����ɴϴ�.
            var projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false); 

            // ī�޶��� ������������ ����� ����մϴ�.
            var inverseViewProjectionMatrix = (projectionMatrix * mainCamera.worldToCameraMatrix).inverse;
            //���̴��� ������������ ����� �����մϴ�.
            material.SetMatrix(Constants.DepthToWorldPositionShader.InverseViewProjectionMatrix, inverseViewProjectionMatrix);
            //CommandBuffer�� ����Ͽ� Ư�� ���� �ؽ�ó�� ���� ������ ����� �����ϰ� �����մϴ�.
            commandBuffer.LoadOrtho().Clear().SetRenderTarget(renderTexture).DrawMesh(quadMesh, material).Execute();

            // �ؽ�ó ��ȯ�� �����ϸ� Graphics.ConvertTexture�� ����Ͽ� �ؽ�ó�� �����մϴ�.
            if (supportsCopyTexture && Graphics.ConvertTexture(renderTexture, texture))
            {
                frameId = Time.frameCount;// ���� ������ ID�� ����
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