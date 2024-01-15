using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GetampedPaint.Core;
using GetampedPaint.Core.PaintObject.Data;
using GetampedPaint.Tools.Images;
using GetampedPaint.Tools.Images.Base;
using IDisposable = GetampedPaint.Core.IDisposable;

namespace GetampedPaint.Tools
{
    [Serializable]
    public class ToolsManager : IDisposable
    {
        public event Action<IPaintTool> OnToolChanged;

        private IPaintTool currentTool;
        public IPaintTool CurrentTool => currentTool;

        [SerializeField] private BrushTool brushTool;
        [SerializeField] private EraseTool eraseTool;
        [SerializeField] private BucketTool bucketTool;
        [SerializeField] private EyedropperTool eyedropperTool;
        [SerializeField] private SelectionTool selectionTool;
        
        private IPaintTool[] allTools;
        private IPaintManager paintManager;
        private bool initialized;

        public ToolsManager(PaintTool paintTool, IPaintData paintData)
        {
            brushTool = new BrushTool(paintData);
            eraseTool = new EraseTool(paintData);
            bucketTool = new BucketTool(paintData);
            eyedropperTool = new EyedropperTool(paintData);
            selectionTool = new SelectionTool(paintData);
            allTools = new IPaintTool[]
            {
                brushTool, eraseTool, bucketTool, eyedropperTool, selectionTool
            };
            currentTool = allTools.First(x => x.Type == paintTool);
            currentTool.Enter();
        }
        public void Init(IPaintManager thisPaintManager)
        {
            paintManager = thisPaintManager;
            paintManager.PaintObject.OnPointerHover += OnPointerHover;
            paintManager.PaintObject.OnPointerDown += OnPointerDown;
            paintManager.PaintObject.OnPointerPress += OnPointerMouse;
            paintManager.PaintObject.OnPointerUp += OnPointerUp;
            initialized = true;
        }

        public void DoDispose()
        {
            if (!initialized)
                return;
            
            paintManager.PaintObject.OnPointerHover -= OnPointerHover;
            paintManager.PaintObject.OnPointerDown -= OnPointerDown;
            paintManager.PaintObject.OnPointerPress -= OnPointerMouse;
            paintManager.PaintObject.OnPointerUp -= OnPointerUp;
            
            foreach (var tool in allTools)
            {
                if (currentTool == tool)
                {
                    tool.Exit();
                }
                tool.DoDispose();
            }
            allTools = null;
            initialized = false;
        }

        public void SetTool(PaintTool newTool)
        {
            foreach (var tool in allTools)
            {
                if (tool.Type == newTool)
                {
                    currentTool.Exit();
                    currentTool = tool;
                    OnToolChanged?.Invoke(currentTool);
                    currentTool.Enter();
                    break;
                }
            }
        }

        public IPaintTool GetTool(PaintTool tool)
        {
            return allTools.First(x => x.Type == tool);
        }

        private void OnPointerHover(PointerData pointerData)
        {
            currentTool.UpdateHover(pointerData);
        }

        private void OnPointerDown(PointerData pointerData)
        {
            currentTool.UpdateDown(pointerData);
        }
        
        private void OnPointerMouse(PointerData pointerData)
        {
            currentTool.UpdatePress(pointerData);
        }
        
        private void OnPointerUp(PointerUpData pointerUpData)
        {
            currentTool.UpdateUp(pointerUpData);
        }
    }
}