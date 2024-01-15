using UnityEngine;
using UnityEngine.Rendering;
using GetampedPaint.Core;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Core.PaintObject.Data;
using IDisposable = GetampedPaint.Core.IDisposable;

namespace GetampedPaint.Tools.Images.Base
{
    public interface IPaintTool : IDisposable
    {
        PaintTool Type { get; }
        bool AllowRender { get; }
        bool CanDrawLines { get; }
        bool ConsiderPreviousPosition { get; }
        bool ShowPreview { get; }
        int Smoothing { get; }
        bool RandomizePointsQuadsAngle { get; }
        bool RandomizeLinesQuadsAngle { get; }
        bool RenderToLayer { get; }
        bool RenderToInput { get; }
        bool RenderToTextures { get; }
        bool DrawPreProcess { get; }
        bool DrawProcess { get; }
        bool BakeInputToPaint { get; }
        bool ProcessingFinished { get; }
        BasePaintToolSettings BaseSettings { get; }

        void FillWithColor(Color color);
        void SetPaintMode(IPaintMode mode);
        void OnBrushChanged(IBrush brush);
        void OnDrawPreProcess(RenderTargetIdentifier combined);
        void OnDrawProcess(RenderTargetIdentifier combined);
        void OnBakeInputToLayer(RenderTargetIdentifier activeLayer);
        void Enter();
        void Exit();
        void UpdateHover(PointerData pointerData);
        void UpdateDown(PointerData pointerData);
        void UpdatePress(PointerData pointerData);
        void UpdateUp(PointerUpData pointerUpData);
    }
}