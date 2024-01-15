using System.Collections;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.States;
using GetampedPaint.Utils;

namespace GetampedPaint.Tools.Images.Base
{
    public interface IPaintData : IDisposable
    {
        IStatesController StatesController { get; }
        ILayersController LayersController { get; }
        IRenderTextureHelper TexturesHelper { get; }
        IRenderComponentsHelper RenderComponents { get; }
        IBrush Brush { get; }
        IPaintMode PaintMode { get; }
        Camera Camera { get; }
        Material Material { get; }
        CommandBufferBuilder CommandBuilder { get; }
        Mesh QuadMesh { get; }
        bool IsPainted { get; }
        bool IsPainting { get; }
        bool InBounds { get; }
        bool CanSmoothLines { get; }

        void Render();
        void SaveState();
        void StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(IEnumerator coroutine);
    }
}