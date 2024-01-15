using System.Collections;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.States;
using GetampedPaint.Utils;

namespace GetampedPaint.Tools.Images.Base
{
    public class BasePaintData : IPaintData
    {
        public IStatesController StatesController => paintManager.StatesController;
        public ILayersController LayersController => paintManager.LayersController;
        public IRenderTextureHelper TexturesHelper { get; }
        public IRenderComponentsHelper RenderComponents { get; }
        public IBrush Brush => paintManager.Brush;
        public IPaintMode PaintMode => paintManager.PaintMode;
        public Camera Camera => paintManager.Camera;
        public Material Material => paintManager.Material.Material;
        public CommandBufferBuilder CommandBuilder => commandBufferBuilder;
        public Mesh QuadMesh => quadMesh;
        public bool IsPainted => paintObject.IsPainted;
        public bool IsPainting => paintObject.IsPainting;
        public bool InBounds => paintObject.InBounds;
        public bool CanSmoothLines => paintObject.CanSmoothLines;

        private readonly IPaintManager paintManager;
        private readonly BasePaintObject paintObject;
        private CommandBufferBuilder commandBufferBuilder;
        private Mesh quadMesh;

        public BasePaintData(IPaintManager currentPaintManager, IRenderTextureHelper currentRenderTextureHelper, IRenderComponentsHelper componentsHelper)
        {
            paintManager = currentPaintManager;
            paintObject = paintManager.PaintObject;
            TexturesHelper = currentRenderTextureHelper;
            RenderComponents = componentsHelper;
            commandBufferBuilder = new CommandBufferBuilder();
            quadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
        }

        public virtual void Render()
        {
            paintManager.Render();
        }

        public virtual void SaveState()
        {
            paintObject.SaveUndoTexture();
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            if(paintManager is PaintManager)
                ((PaintManager)paintManager).StartCoroutine(coroutine);
            else
                ((PaintBoardManager)paintManager).StartCoroutine(coroutine);

        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            if (paintManager is PaintManager)
                ((PaintManager)paintManager).StopCoroutine(coroutine);
            else
                ((PaintBoardManager)paintManager).StopCoroutine(coroutine);
        }

        public void DoDispose()
        {
            if (commandBufferBuilder != null)
            {
                commandBufferBuilder.Release();
                commandBufferBuilder = null;
            }
            if (quadMesh != null)
            {
                Object.Destroy(quadMesh);
                quadMesh = null;
            }
        }
    }
}