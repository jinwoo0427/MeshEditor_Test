using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GetampedPaint.Controllers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintObject.Data;
using GetampedPaint.States;
using GetampedPaint.Tools.Raycast.Data;
using GetampedPaint.Utils;
using Debug = UnityEngine.Debug;

namespace GetampedPaint.Core.PaintObject.Base
{
    [Serializable]
    public abstract class BasePaintObject : BasePaintObjectRenderer
    {
        #region Events

        /// <summary>
        /// Mouse hover event
        /// </summary>
        public event Action<PointerData> OnPointerHover;
        
        /// <summary>
        /// Mouse down event
        /// </summary>
        public event Action<PointerData> OnPointerDown;
        
        /// <summary>
        /// Mouse press event
        /// </summary>
        public event Action<PointerData> OnPointerPress;
        
        /// <summary>
        /// Mouse up event
        /// </summary>
        public event Action<PointerUpData> OnPointerUp;
        
        /// <summary>
        /// Draw point event, can be used by the developer to obtain data about painting
        /// </summary>
        public event Action<DrawPointData> OnDrawPoint;
        
        /// <summary>
        /// Draw line event, can be used by the developer to obtain data about painting
        /// </summary>
        public event Action<DrawLineData> OnDrawLine;
        
        /// <summary>
        /// Draw line event, can be used by the developer to obtain data about painting
        /// </summary>
        public event Action<DrawLineExtendedData> OnDrawLineExtended;
        
        #endregion

        #region Properties and variables

        public bool InBounds
        {
            get
            {
                foreach (var paintObjectData in PaintObjectData)
                {
                    if (paintObjectData.InBounds)
                        return true;
                }
                return false;
            }
        }

        public bool IsPainting
        {
            get
            {
                foreach (var x in PaintObjectData)
                {
                    if (x.IsPainting) 
                        return true;
                }
                return false;
            }
        }

        public bool IsPainted { get; private set; }
        public bool ProcessInput = true;

        private Camera thisCamera;
        public new Camera Camera
        {
            protected get => thisCamera;
            set
            {
                thisCamera = value;
                base.Camera = thisCamera;
            }
        }
        
        protected Transform ObjectTransform { get; private set; }  
        protected BasePaintObjectData[] PaintObjectData;
        protected IPaintManager PaintManager;

        private Vector3 RenderOffset
        {
            get
            {
                if (Brush == null)
                    return Vector3.zero;
                
                var renderOffset = Brush.RenderOffset;
                if (renderOffset.x > 0)
                {
                    renderOffset.x = PaintMaterial.SourceTexture.texelSize.x / 2f;
                }
                
                if (renderOffset.y > 0)
                {
                    renderOffset.y = PaintMaterial.SourceTexture.texelSize.y / 2f;
                }
                
                return renderOffset;
            }
        }
        
        private IStatesController statesController;
        private BasePaintObjectData[] paintObjectDataStorage;
        private bool shouldClearTexture = true;
        private bool writeClear;
        private const float HalfTextureRatio = 0.5f;
        
        #endregion

        #region Abstract methods

        public abstract bool CanSmoothLines { get; }
        protected abstract void Init();
        protected abstract void CalculatePaintPosition(int fingerId, Vector3 position, Vector2? uv = null, bool usePostPaint = true, RaycastData raycast = null);
        protected abstract bool IsInBounds(Vector3 position);
        protected abstract bool IsInBounds(Vector3 position, RaycastData raycast);

        #endregion
        
        public void Init(IPaintManager paintManagerInstance, Camera camera, Transform objectTransform, Paint paint, 
            IRenderTextureHelper renderTextureHelper, IStatesController states)
        {
            PaintManager = paintManagerInstance;
            thisCamera = camera;
            ObjectTransform = objectTransform;
            PaintMaterial = paint;
            RenderTextureHelper = renderTextureHelper;
            statesController = states;
            InitRenderer(PaintManager, Camera, PaintMaterial);
            PaintObjectData = new BasePaintObjectData[InputController.Instance.MaxTouchesCount + 1];
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                PaintObjectData[i] = new BasePaintObjectData(CanSmoothLines);
            }
            InitStatesController();
            Init();
        }

        public override void DoDispose()
        {
            if (statesController != null)
            {
                statesController.OnRenderTextureAction -= OnExtraDraw;
                statesController.OnClearTextureAction -= OnClearTexture;
                statesController.OnResetState -= OnResetState;
            }
            base.DoDispose();
        }

        private void InitStatesController()
        {
            if (statesController == null)
                return;
            
            statesController.OnRenderTextureAction += OnExtraDraw;
            statesController.OnClearTextureAction += OnClearTexture;
            statesController.OnResetState += OnResetState;
        }
        private void OnResetState()
        {
            shouldClearTexture = true;
        }

        #region Input

        public void OnMouseHover(int fingerId, Vector3 position, RaycastData raycast = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }

            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;

            if (!IsPainting)
            {
                var paintObjectData = PaintObjectData[fingerId];
                if (raycast != null)
                {
                    CalculatePaintPosition(fingerId, position, raycast.UVHit, false, raycast);
                    paintObjectData.ScreenPosition = position;
                    paintObjectData.LocalPosition = raycast.Hit;
                    if (OnPointerHover != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        var data = new PointerData(paintObjectData.LocalPosition.Value, position, raycast.UVHit, paintObjectData.PaintPosition.Value, 1f, fingerId);
                        OnPointerHover(data);
                    }
                }
                else 
                {
                    paintObjectData.ScreenPosition = position;
                    CalculatePaintPosition(fingerId, position, null, false);
                    if (OnPointerHover != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        var uv = new Vector2(
                            paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width, 
                            paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                        var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, 1f, fingerId);
                        OnPointerHover(data);
                    }
                }
            }
        }

        public void OnMouseHoverFailed(int fingerId, Vector3 position, RaycastData raycast = null)
        {
            PaintObjectData[fingerId].InBounds = false;
        }

        public void OnMouseDown(int fingerId, Vector3 position, float pressure = 1f, RaycastData raycast = null)
        {
            OnMouse(true, fingerId, position, pressure, raycast);
        }

        public void OnMouseButton(int fingerId, Vector3 position, float pressure = 1f, RaycastData raycast = null)
        {
            OnMouse(false, fingerId, position, pressure, raycast);
        }
        
        private void OnMouse(bool isDown, int fingerId, Vector3 position, float pressure = 1f, RaycastData raycast = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            var paintObjectData = PaintObjectData[fingerId];
            if (isDown)
            {
                paintObjectData.IsPaintingDone = false;
                paintObjectData.InBounds = false;
                paintObjectData.Pressure = pressure;
            }
            
            if (raycast == null)
            {
                paintObjectData.IsPainting = true;
                paintObjectData.ScreenPosition = position;
                paintObjectData.LineData.AddBrush(pressure * Brush.Size);
                paintObjectData.Pressure = pressure;
                CalculatePaintPosition(fingerId, position);
                if (paintObjectData.InBounds && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                {
                    var uv = new Vector2(paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width, paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                    if (isDown)
                    {
                        if (OnPointerDown != null)
                        {
                            var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                            OnPointerDown.Invoke(data);
                        }
                    }
                    else
                    {
                        if (OnPointerPress != null)
                        {
                            var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                            OnPointerPress.Invoke(data);
                        }
                    }
                }
            }
            else if (raycast.Triangle.Transform == ObjectTransform)
            {
                paintObjectData.IsPainting = true;
                paintObjectData.ScreenPosition = position;
                paintObjectData.LineData.AddBrush(pressure * Brush.Size);
                paintObjectData.LineData.AddRaycast(raycast);
                paintObjectData.Pressure = pressure;
                paintObjectData.LocalPosition = raycast.Hit;
                CalculatePaintPosition(fingerId, position, raycast.UVHit, true, raycast);
                if (paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                {
                    if (isDown)
                    {
                        if (OnPointerDown != null)
                        {
                            var data = new PointerData(paintObjectData.LocalPosition.Value, position, raycast.UVHit, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                            OnPointerDown.Invoke(data);
                        }
                    }
                    else
                    {
                        if (OnPointerPress != null)
                        {
                            var data = new PointerData(paintObjectData.LocalPosition.Value, position, raycast.UVHit, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                            OnPointerPress.Invoke(data);
                        }
                    }
                }
            }
            else
            {
                paintObjectData.ScreenPosition = null;
                paintObjectData.LocalPosition = null;
                paintObjectData.PaintPosition = null;
                paintObjectData.LineData.Clear();
            }
        }
        
        public void OnMouseFailed(int fingerId, Vector3 position, float pressure = 1f, RaycastData raycast = null)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.InBounds = false;
            paintObjectData.Pressure = pressure;
            paintObjectData.LineData.Clear();
        }

        public void OnMouseUp(int fingerId, Vector3 position)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            FinishPainting(fingerId);
            if (OnPointerUp != null)
            {
                var data = new PointerUpData(position, IsInBounds(position), fingerId);
                OnPointerUp.Invoke(data);
            }
        }

        public Vector2? GetPaintPosition(int fingerId, Vector3 position, RaycastData raycast = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return null;
            }

            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return null;
            
            if (raycast != null)
            {
                CalculatePaintPosition(fingerId, position, raycast.UVHit, false);
            }
            else
            {
                CalculatePaintPosition(fingerId, position, null, false);
            }

            var paintObjectData = PaintObjectData[fingerId];
            var paintPosition = paintObjectData.PaintPosition;
            if (paintObjectData.InBounds && paintPosition != null)
            {
                return paintPosition.Value;
            }
            return null;
        }

        #endregion
        
        #region DrawFromCode

        /// <summary>
        /// Save paint state of fingerId
        /// </summary>
        /// <param name="fingerId"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void SavePaintState(int fingerId = 0)
        {
            if (paintObjectDataStorage == null)
            {
                paintObjectDataStorage = new BasePaintObjectData[InputController.Instance.MaxTouchesCount + 1];
                for (var i = 0; i < paintObjectDataStorage.Length; i++)
                {
                    paintObjectDataStorage[i] = new BasePaintObjectData(CanSmoothLines);
                }
            }
            
            var paintObjectData = PaintObjectData[fingerId];
            var storageObjectData = paintObjectDataStorage[fingerId];
            storageObjectData.Pressure = paintObjectData.Pressure;
            storageObjectData.PreviousPaintPosition = paintObjectData.PreviousPaintPosition;
            storageObjectData.PaintPosition = paintObjectData.PaintPosition;
            storageObjectData.IsPainting = paintObjectData.IsPainting;
            storageObjectData.IsPaintingDone = paintObjectData.IsPaintingDone;
            storageObjectData.LineData.Clear();
            storageObjectData.LineData.Raycasts.AddRange(paintObjectData.LineData.Raycasts);
            storageObjectData.LineData.PaintPositions.AddRange(paintObjectData.LineData.PaintPositions);
            storageObjectData.LineData.BrushSizes.AddRange(paintObjectData.LineData.BrushSizes);
        }
        
        /// <summary>
        /// Restore paint state of fingerId
        /// </summary>
        /// <param name="fingerId"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void RestorePaintState(int fingerId = 0)
        {
            if (paintObjectDataStorage == null)
            {
                Debug.LogError("Can't find saved states!");
                return;
            }
            
            var paintObjectData = PaintObjectData[fingerId];
            var storageObjectData = paintObjectDataStorage[fingerId];
            paintObjectData.Pressure = storageObjectData.Pressure;
            paintObjectData.PreviousPaintPosition = storageObjectData.PreviousPaintPosition;
            paintObjectData.PaintPosition = storageObjectData.PaintPosition;
            paintObjectData.IsPainting = storageObjectData.IsPainting;
            paintObjectData.IsPaintingDone = storageObjectData.IsPaintingDone;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.Raycasts.AddRange(storageObjectData.LineData.Raycasts);
            paintObjectData.LineData.PaintPositions.AddRange(storageObjectData.LineData.PaintPositions);
            paintObjectData.LineData.BrushSizes.AddRange(storageObjectData.LineData.BrushSizes);
        }
        
        public void DrawPoint(DrawPointData drawPointData)
        {
            DrawPoint(drawPointData.TexturePosition, drawPointData.Pressure, drawPointData.FingerId);
        }

        /// <summary>
        /// Draws point with pressure
        /// </summary>
        /// <param name="position"></param>
        /// <param name="brushPressure"></param>
        /// <param name="fingerId"></param>
        public void DrawPoint(Vector2 position, float brushPressure = 1f, int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.Pressure = brushPressure;
            paintObjectData.PaintPosition = position;
            paintObjectData.IsPainting = true;
            paintObjectData.IsPaintingDone = true;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.AddBrush(brushPressure * Brush.Size);
            paintObjectData.LineData.AddPosition(position);
            OnRender();
            Render();
            // FinishPainting(fingerId, true);
        }
        
        /// <summary>
        /// Draws line using DrawLineData
        /// </summary>
        /// <param name="drawLineData"></param>
        public void DrawLine(DrawLineData drawLineData)
        {
            DrawLine(drawLineData.LineStartPosition, drawLineData.LineEndPosition, 
                drawLineData.StartPressure, drawLineData.EndPressure, drawLineData.FingerId);
        }

        /// <summary>
        /// Draws line with pressure
        /// </summary>
        /// <param name="positionStart"></param>
        /// <param name="positionEnd"></param>
        /// <param name="pressureStart"></param>
        /// <param name="pressureEnd"></param>
        /// <param name="fingerId"></param>
        public void DrawLine(Vector2 positionStart, Vector2 positionEnd, float pressureStart = 1f, float pressureEnd = 1f, int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.Pressure = pressureEnd;
            paintObjectData.PaintPosition = positionEnd;
            paintObjectData.IsPainting = true;
            paintObjectData.IsPaintingDone = true;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.AddBrush(pressureStart * Brush.Size);
            paintObjectData.LineData.AddBrush(paintObjectData.Pressure * Brush.Size);
            paintObjectData.LineData.AddPosition(positionStart);
            paintObjectData.LineData.AddPosition(positionEnd);

            if (Tool.Smoothing > 1)
            {
                paintObjectData.LineData.AddBrush(paintObjectData.Pressure * Brush.Size);
                paintObjectData.LineData.AddPosition(positionEnd);
            }
            
            OnRender();
            Render();
            // FinishPainting(fingerId, true);
        }

        /// <summary>
        /// Draws line extended with pressure (mostly used for MeshRenderer and SkinnedMeshRenderer)
        /// </summary>
        /// <param name="lineExtendedData"></param>
        public void DrawLineExtended(DrawLineExtendedData lineExtendedData)
        {
            var paintObjectData = PaintObjectData[lineExtendedData.FingerId];
            paintObjectData.Pressure = lineExtendedData.EndPressure;
            paintObjectData.PaintPosition = lineExtendedData.SegmentPositionEnd;
            paintObjectData.IsPainting = true;
            paintObjectData.IsPaintingDone = true;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.AddBrush(lineExtendedData.StartPressure * Brush.Size);
            paintObjectData.LineData.AddBrush(paintObjectData.Pressure * Brush.Size);
            paintObjectData.LineData.AddPosition(lineExtendedData.SegmentPositionStart);
            paintObjectData.LineData.AddPosition(lineExtendedData.SegmentPositionEnd);

            if (Tool.Smoothing > 1)
            {
                paintObjectData.LineData.AddBrush(paintObjectData.Pressure * Brush.Size);
                paintObjectData.LineData.AddPosition(lineExtendedData.SegmentPositionEnd);
            }
            
            RenderLine(lineExtendedData.LinesPositions, RenderOffset, Brush.RenderTexture, Brush.Size, new []{ lineExtendedData.StartPressure, lineExtendedData.EndPressure }, Tool.RandomizeLinesQuadsAngle);
            Render();
            // FinishPainting(fingerId, true);
        }

        
        /// <summary>
        /// Draws lines using array of DrawPointData
        /// </summary>
        /// <param name="drawPointsData"></param>
        /// <param name="fingerId"></param>
        public void DrawLines(DrawPointData[] drawPointsData, int fingerId = 0)
        {
            if (drawPointsData.Length < 3)
            {
                Debug.LogError("Incorrect length of the input array!");
                return;
            }
            
            var paintObjectData = PaintObjectData[fingerId];
            for (var i = 0; i < drawPointsData.Length; i++)
            {
                var pointData = drawPointsData[i];
                paintObjectData.Pressure = pointData.Pressure;
                paintObjectData.PaintPosition = pointData.TexturePosition;
                paintObjectData.IsPainting = true;
                paintObjectData.IsPaintingDone = true;
                paintObjectData.LineData.AddBrush(pointData.Pressure * Brush.Size);
                paintObjectData.LineData.AddPosition(pointData.TexturePosition);
                
                OnRender(i == drawPointsData.Length - 1);
                Render();
            }
            // FinishPainting(fingerId, true);
        }

        #endregion

        /// <summary>
        /// Resets all states, bake paint result into PaintTexture, save paint result to StatesController
        /// </summary>
        /// <param name="fingerId"></param>
        /// <param name="forceFinish"></param>
        public void FinishPainting(int fingerId = 0, bool forceFinish = false)
        {
            var shouldRender = false;
            if (/*PaintObjectData[fingerId].IsPaintingDone ||*/ forceFinish)
            {
                shouldRender = true;
                OnRender(true);
            }

            var paintObjectData = PaintObjectData[fingerId];
            if (IsPainting || forceFinish)
            {
                paintObjectData.Pressure = 1f;
                if (PaintMode.UsePaintInput)
                {
                    BakeInputToPaint();
                    ClearTexture(RenderTarget.Input);
                }

                paintObjectData.IsPainting = false;
                if ((paintObjectData.IsPaintingDone || forceFinish) && Tool.ProcessingFinished)
                {
                    SaveUndoTexture();
                }
                paintObjectData.LineData.Clear();
                if (!PaintMode.UsePaintInput)
                {
                    ClearTexture(RenderTarget.Input);
                    Render();
                    shouldRender = false;
                }
            }

            if (shouldRender)
            {
                Render();
            }
            
            PaintMaterial.SetPaintPreviewVector(Vector4.zero);
            paintObjectData.ScreenPosition = null;
            paintObjectData.LocalPosition = null;
            paintObjectData.PaintPosition = null;
            paintObjectData.IsPaintingDone = false;
            paintObjectData.InBounds = false;
            paintObjectData.PreviousPaintPosition = default;
        }

        /// <summary>
        /// 점과 선을 렌더링하고, Undo/Redo를 호출할 때 텍스처를 복원하는 기능
        /// </summary>
        /// <param name="finishPainting"></param>
        public void OnRender(bool finishPainting = false)
        {
            if (shouldClearTexture)
            {
                ClearTexture(RenderTarget.Input);
                shouldClearTexture = false;
                if (writeClear && Tool.RenderToTextures)
                {
                    SaveUndoTexture();
                    writeClear = false;
                }
            }

            var painted = false;
            IsPainted = false;
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                var paintObjectData = PaintObjectData[i];
                if (IsPainting && paintObjectData.PaintPosition != null && (!Tool.ConsiderPreviousPosition ||
                     paintObjectData.PreviousPaintPosition != paintObjectData.PaintPosition.Value) && Tool.AllowRender)
                {
                    painted = true;
                    IsPainted = true;
                    if (paintObjectData.LineData.HasOnePosition())
                    {
                        DrawPoint(i);
                        paintObjectData.PreviousPaintPosition = paintObjectData.PaintPosition.Value;
                    }
                    else if (Tool.BaseSettings.CanPaintLines)
                    {
                        DrawLine(i, finishPainting);
                        paintObjectData.PreviousPaintPosition = paintObjectData.PaintPosition.Value;
                    }
                }
                IsPainted = painted;
            }
        }

        /// <summary>
        /// Combines textures, render preview
        /// </summary>
        public void Render()
        {
            DrawPreProcess();
            ClearTexture(RenderTarget.Combined);
            DrawProcess();
        }

        public void RenderToTextureWithoutPreview(RenderTexture resultTexture)
        {
            DrawPreProcess();
            ClearTexture(RenderTarget.Combined);
            //disable preview
            var inBounds = PaintObjectData.Select(x => x.InBounds).ToArray();
            foreach (var paintObjectData in PaintObjectData)
            {
                paintObjectData.InBounds = false;
            }
            DrawProcess();
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                PaintObjectData[i].InBounds = inBounds[i];
            }
            Graphics.Blit(RenderTextureHelper.GetTexture(RenderTarget.Combined), resultTexture);
        }

        public void SaveUndoTexture()
        {
            if(Tool.Type != PaintTool.Eyedropper && Tool.Type != PaintTool.Selection)
                ActiveLayer().SaveState();
        }
        
        /// <summary>
        /// Restores texture when Undo/Redo invoking
        /// </summary>
        private void OnExtraDraw()
        {
            if (!PaintMode.UsePaintInput)
            {
                ClearTexture(RenderTarget.Input);
            }
            Render();
        }

        private void OnClearTexture(RenderTexture renderTexture)
        {
            ClearTexture(renderTexture, Color.clear);
            Render();
        }
        
        /// <summary>
        /// Renders quad (point)
        /// </summary>
        /// <param name="fingerId"></param>
        private void DrawPoint(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (OnDrawPoint != null)
            {
                var data = new DrawPointData(paintObjectData.PaintPosition.Value, Brush.Size * paintObjectData.Pressure, fingerId);
                OnDrawPoint.Invoke(data);
            }

            RenderQuad(paintObjectData.PaintPosition.Value, RenderOffset, Brush.Size * paintObjectData.Pressure, Tool.RandomizePointsQuadsAngle);
        }

        /// <summary>
        /// Renders a few quads (line)
        /// </summary>
        /// <param name="fingerId"></param>
        /// <param name="finishPainting"></param>
        private void DrawLine(int fingerId = 0, bool finishPainting = false)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (!CanSmoothLines || Tool.Smoothing == 1)
            {
                IList<Vector2> linePositions;
                IList<Vector2> segmentPositions;
                if (paintObjectData.LineData.HasDifferentTriangles())
                {
                    segmentPositions = paintObjectData.LineData.PaintPositions;
                    var triangles = paintObjectData.LineData.Raycasts;
                    linePositions = GetLinePositions(segmentPositions[0], segmentPositions[1], triangles[0], triangles[1], fingerId);
                }
                else
                {
                    segmentPositions = paintObjectData.LineData.PaintPositions;
                    linePositions = segmentPositions;
                }

                if (linePositions.Count > 0)
                {
                    var brushes = paintObjectData.LineData.BrushSizes;
                    if (brushes.Count < 2)
                    {
                        Debug.LogWarning("Incorrect length of the brushes array!");
                    }
                    else
                    {
                        if (OnDrawLine != null)
                        {
                            var data = new DrawLineData(segmentPositions[segmentPositions.Count - 2], segmentPositions[segmentPositions.Count - 1], 
                                brushes[brushes.Count - 2], brushes[brushes.Count - 1], fingerId);
                            OnDrawLine.Invoke(data);
                        }

                        if (OnDrawLineExtended != null)
                        {
                            var data = new DrawLineExtendedData(linePositions, segmentPositions[segmentPositions.Count - 2], segmentPositions[segmentPositions.Count - 1], 
                                brushes[brushes.Count - 2], brushes[brushes.Count - 1], fingerId);
                            OnDrawLineExtended.Invoke(data);
                        }

                        //Debug.Log("DrawLineTest");
                        Debug.Log("DrawLine");

                        RenderLine(linePositions, RenderOffset, Brush.RenderTexture, Brush.Size, brushes, Tool.RandomizeLinesQuadsAngle);
                    }
                }
            }
            else if (CanSmoothLines && paintObjectData.LineData.PaintPositions.Count >= 3)
            {
                Vector2? previous = null;
                var lineElements = paintObjectData.LineData.LineElements;
                var paintPositions = paintObjectData.LineData.PaintPositions;
                var pressures = paintObjectData.LineData.BrushSizes;
                var startPressure = paintPositions.Count <= lineElements ? pressures[0] : pressures[pressures.Count - 2];
                var endPressure = paintPositions.Count <= lineElements ? pressures[1] : pressures[pressures.Count - 1];
                var prevPressure = startPressure;
                float length;

                if (paintPositions.Count == lineElements)
                {
                    length = finishPainting ? Vector2.Distance(paintPositions[1], paintPositions[2]) : Vector2.Distance(paintPositions[0], paintPositions[1]);
                }
                else
                {
                    length = finishPainting ? Vector2.Distance(paintPositions[2], paintPositions[3]) : Vector2.Distance(paintPositions[1], paintPositions[2]);
                }
                var numSegments = (int)Mathf.Clamp(length / 10f, 1, Tool.Smoothing);

                for (var j = 0; j <= numSegments; j++)
                {
                    var t = (float)j / numSegments;
                    Vector2 interpolatedPoint;
                    if (paintPositions.Count == lineElements)
                    {
                        interpolatedPoint = finishPainting
                            ? MathHelper.Interpolate(paintPositions[0], paintPositions[1], paintPositions[2], paintPositions[2], t)
                            : MathHelper.Interpolate(paintPositions[0], paintPositions[0], paintPositions[1], paintPositions[2], t);
                    }
                    else
                    {
                        interpolatedPoint = finishPainting
                            ? MathHelper.Interpolate(paintPositions[1], paintPositions[2], paintPositions[3], paintPositions[3], t)
                            : MathHelper.Interpolate(paintPositions[0], paintPositions[1], paintPositions[2], paintPositions[3], t);
                    }

                    if (previous != null)
                    {
                        IList<Vector2> linePositions = new[] { previous.Value, interpolatedPoint };
                        var pressureStart = prevPressure;
                        var pressureEnd = pressureStart + (endPressure - pressureStart) * t;
                        if (linePositions.Count > 0)
                        {
                            
                            RenderLine(linePositions, RenderOffset, Brush.RenderTexture, Brush.Size, new[] { pressureStart, pressureEnd }, Tool.RandomizeLinesQuadsAngle);
                        }

                        prevPressure = pressureEnd;
                    }
                    
                    previous = interpolatedPoint;
                }
                                                         
                if (OnDrawLine != null)
                {
                    if (finishPainting)
                    {
                        var data = new DrawLineData(paintPositions[paintPositions.Count - 2], paintPositions[paintPositions.Count - 1], startPressure, endPressure, fingerId);
                        OnDrawLine.Invoke(data);
                    }
                    else
                    {
                        var data = paintPositions.Count == lineElements
                            ? new DrawLineData(paintPositions[0], paintPositions[1], startPressure, endPressure, fingerId)
                            : new DrawLineData(paintPositions[1], paintPositions[2], startPressure, endPressure, fingerId);
                        OnDrawLine.Invoke(data);
                    }
                }
            }
        }

        /// <summary>
        /// Post paint method, used by CalculatePaintPosition method
        /// </summary>
        /// <param name="fingerId"></param>
        protected void OnPostPaint(int fingerId)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (paintObjectData.PaintPosition != null && IsPainting)
            {
                paintObjectData.LineData.AddPosition(paintObjectData.PaintPosition.Value);
            }
            else if (paintObjectData.PaintPosition == null)
            {
                paintObjectData.LineData.Clear();
            }
        }

        protected void UpdateBrushPreview(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (Brush.Preview && paintObjectData.InBounds)
            {
                if (paintObjectData.PaintPosition != null)
                {
                    var previewVector = GetPreviewVector(fingerId);
                    //Debug.Log("preview : " + previewVector);
                    PaintMaterial.SetPaintPreviewVector(previewVector);
                }
                else
                {
                    PaintMaterial.SetPaintPreviewVector(Vector4.zero);
                }
            }
        }
        
        /// <summary>
        /// Returns Vector4 for brush preview
        /// </summary>
        /// <param name="fingerId"></param>
        /// <returns></returns>
        private Vector4 GetPreviewVector(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            //Debug.Log("PaintPosition : " + paintObjectData.PaintPosition.Value);
            var brushRatio = new Vector2(
                PaintMaterial.SourceTexture.width / (float)Brush.RenderTexture.width,
                PaintMaterial.SourceTexture.height / (float)Brush.RenderTexture.height) / Brush.Size / paintObjectData.Pressure;
            var brushOffset = new Vector4(
                paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width * brushRatio.x + Brush.RenderOffset.x,
                paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height * brushRatio.y + Brush.RenderOffset.y,
                brushRatio.x, brushRatio.y);
            return brushOffset;
        }
    }
}