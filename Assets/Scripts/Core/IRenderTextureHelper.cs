using UnityEngine;
using UnityEngine.Rendering;

namespace XDPaint.Core
{
    public interface IRenderTextureHelper : IDisposable
    {
        void Init(int width, int height, FilterMode filterMode);
        RenderTargetIdentifier GetTarget(RenderTarget target);
        RenderTexture GetTexture(RenderTarget target);
        void CreateActiveLayerTempRenderTexture();
        void ReleaseActiveLayerTempRenderTexture();
        void CreateCombinedTempRenderTexture();
        void ReleaseCombinedTempRenderTexture();
    }
}