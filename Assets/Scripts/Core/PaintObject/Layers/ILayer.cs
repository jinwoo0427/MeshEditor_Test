using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GetampedPaint.Core.Layers
{
    public interface ILayer : IDisposable
    {
        event Action<ILayer> OnLayerChanged;
        
        bool Enabled { get; set; }
        bool CanBeDisabled { get; }
        bool MaskEnabled { get; set; }
        string Name { get; set; }
        float Opacity { get; set; }
        Texture SourceTexture { get; set; }
        RenderTexture RenderTexture { get; }
        RenderTargetIdentifier RenderTarget { get; }
        RenderTexture MaskRenderTexture { get; }
        RenderTargetIdentifier MaskRenderTarget { get; }
        //BlendingMode BlendingMode { get; set; }
        void AddMask(RenderTextureFormat format);
        void AddMask(Texture maskTexture, RenderTextureFormat format);
        void RemoveMask();
        void SaveState();
    }
}