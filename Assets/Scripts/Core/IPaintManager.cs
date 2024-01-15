using UnityEngine;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.States;
using GetampedPaint.Tools.Raycast;

namespace GetampedPaint.Core
{
    public interface IPaintManager : IDisposable
    {
        Camera Camera { get; }
        GameObject ObjectForPainting { get; }
        Paint Material { get; }
        BasePaintObject PaintObject { get; }
        ILayersController LayersController { get; }
        IStatesController StatesController { get; }
        IBrush Brush { get; }
        IPaintMode PaintMode { get; }
        Triangle[] Triangles { get; }
        int SubMesh { get; }
        int UVChannel { get; }
        bool Initialized { get; }
   
        void Init();
        void Render();
    }
}