using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Controllers.InputData;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject;
using XDPaint.Core.PaintObject.Base;
using XDPaint.States;
using XDPaint.Tools;
using XDPaint.Tools.Image.Base;
using XDPaint.Tools.Layers;
using XDPaint.Tools.Raycast;
using XDPaint.Tools.Triangles;
using XDPaint.Utils;

namespace XDPaint
{
    public class PaintBoardManager : MonoBehaviour, IPaintManager
    {
        [SerializeField] private PaintManager ModelPaintManager;

        #region Events

        public event Action<PaintManager> OnInitialized;
        public event Action OnDisposed;

        #endregion

        #region Properties and variables

        [FormerlySerializedAs("ObjectForPainting")][SerializeField] private GameObject objectForPainting;
        public GameObject ObjectForPainting
        {
            get => objectForPainting;
            set => objectForPainting = value;
        }

        [FormerlySerializedAs("Material")][SerializeField] private Paint material = new Paint();
        public Paint Material => material;

        [SerializeField] private LayersController layersController;
        public ILayersController LayersController => layersController;


        [SerializeField] private BasePaintObject paintObject;
        public BasePaintObject PaintObject
        {
            get => paintObject;
            private set => paintObject = value;
        }

        private StatesController statesController;
        public IStatesController StatesController => ModelPaintManager.StatesController;

        [SerializeField] private PaintMode paintModeType;
        [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;
        public FilterMode FilterMode
        {
            get => filterMode;
            set
            {
                filterMode = value;
                if (initialized)
                {
                    layersController.SetFilterMode(filterMode);
                }
            }
        }

        [NonSerialized] private IBrush currentBrush;
        [SerializeField] private Brush brush = new Brush();
        public IBrush Brush
        {
            get
            {
                if (Application.isPlaying)
                {
                    return currentBrush;
                }
                return brush;
            }
            set
            {
                if (Application.isPlaying)
                {
                    currentBrush = value;
                    currentBrush.Init(paintMode);
                    toolsManager.CurrentTool.OnBrushChanged(currentBrush);
                }
                else
                {
                    brush = (Brush)value;
                }

                if (initialized)
                {
                    PaintObject.Brush = currentBrush;
                    Material.SetPreviewTexture(currentBrush.RenderTexture);
                }
            }
        }

        [SerializeField] private ToolsManager toolsManager;
        public ToolsManager ToolsManager => toolsManager;

        [SerializeField] private PaintTool paintTool;
        public PaintTool Tool
        {
            get
            {
                if (toolsManager.CurrentTool != null)
                {
                    return toolsManager.CurrentTool.Type;
                }
                return PaintTool.Brush;
            }
            set
            {
                paintTool = value;
                if (initialized)
                {
                    Material.SetPreviewTexture(currentBrush.RenderTexture);
                    if (toolsManager != null)
                    {
                        toolsManager.SetTool(paintTool);
                        toolsManager.CurrentTool.SetPaintMode(paintMode);
                    }
                }
            }
        }

        public Camera Camera => PaintController.Instance.Camera;

        [SerializeField] private bool useSourceTextureAsBackground;
        public bool UseSourceTextureAsBackground
        {
            get => useSourceTextureAsBackground;
            set => useSourceTextureAsBackground = value;
        }


        public bool HasTrianglesData => triangles != null && triangles.Length > 0;

        public bool Initialized => initialized;

        private Triangle[] triangles;
        public Triangle[] Triangles => ModelPaintManager.Triangles;

        [SerializeField] private int subMesh;
        public int SubMesh
        {
            get => subMesh;
            set => subMesh = value;
        }

        [SerializeField] private int uvChannel;
        public int UVChannel
        {
            get => uvChannel;
            set => uvChannel = value;
        }

        private ObjectComponentType componentType;
        public ObjectComponentType ComponentType => componentType;

        private IPaintMode paintMode;
        public IPaintMode PaintMode
        {
            get
            {
                if (initialized && Application.isPlaying)
                {
                    paintMode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
                }
                return paintMode;
            }
        }
        public Transform DrawPanel;
        public RawImage PaintBoard;
        private LayersMergeController layersMergeController;
        private IRenderTextureHelper renderTextureHelper;
        private IRenderComponentsHelper renderComponentsHelper;
        private IPaintData paintData;
        private BaseInputData inputData;
        private LayersContainer loadedLayersContainer;
        private bool initialized = false;

        #endregion


        public void StartInit(PaintManager paintManager)
        {
            if (initialized)
                return;
            InitPaintBoard(paintManager);
            Init();
        }

        private void LateUpdate()
        {
            if (!initialized)
                return;

            if (PaintObject.IsPainting || currentBrush.Preview)
            {
                Render();
            }
        }

        private void OnDestroy()
        {
            //DoDispose();
        }
        public void InitPaintBoard(PaintManager paintManager)
        {
            ModelPaintManager = paintManager;
            //renderTextureHelper = ModelPaintManager.renderTextureHelper;
            //renderComponentsHelper = ModelPaintManager.renderComponentsHelper;
        }
        public void Init()
        {
            if (initialized)
                DoDispose();

            initialized = false;
            if (ObjectForPainting == null)
            {
                Debug.LogError("ObjectForPainting is null!");
                return;
            }

            RestoreSourceMaterialTexture();

            if (renderComponentsHelper == null)
            {
                renderComponentsHelper = new RenderComponentsHelper();
            }
            renderComponentsHelper.Init(ObjectForPainting, out componentType);
            if (componentType == ObjectComponentType.Unknown)
            {
                Debug.LogError("Unknown component type!");
                return;
            }

            if (ControllersContainer.Instance == null)
            {
                var containerGameObject = new GameObject(Settings.Instance.ContainerGameObjectName);
                containerGameObject.AddComponent<ControllersContainer>();
            }

            //if (renderComponentsHelper.IsMesh())
            //{
            //    var paintComponent = renderComponentsHelper.PaintComponent;
            //    var renderComponent = renderComponentsHelper.RendererComponent;
            //    var mesh = renderComponentsHelper.GetMesh(this);
            //    if (triangles == null || triangles.Length == 0)
            //    {
            //        if (mesh != null)
            //        {
            //            triangles = TrianglesData.GetData(mesh, subMesh, uvChannel);
            //        }
            //        else
            //        {
            //            Debug.LogError("Mesh is null!");
            //            return;
            //        }
            //    }

            //    RaycastController.Instance.InitObject(ModelPaintManager, paintComponent, renderComponent);
            //}

            InitRenderTexture();
            InitLayers();
            InitStates();
            InitMaterial();

            //register PaintManager
            //PaintController.Instance.RegisterPaintManager(ModelPaintManager);
            InitBrush();
            InitPaintObject();
            InitTools();

            SubscribeInputEvents(componentType);
            initialized = true;
            Render();

            //PaintBoard.texture = GetResultRenderTexture();
            OnInitialized?.Invoke(ModelPaintManager);
        }

        public void DoDispose()
        {
            if (!initialized)
                return;

            //unregister PaintManager
            //PaintController.Instance.UnRegisterPaintManager(ModelPaintManager);
            //restore source material and texture
            RestoreSourceMaterialTexture();
            //free tools resources
            toolsManager.OnToolChanged -= OnToolChanged;
            toolsManager.DoDispose();
            paintData.DoDispose();
            //free brush resources
            if (brush != null)
            {
                brush.OnTextureChanged -= Material.SetPreviewTexture;
                brush.OnPreviewChanged -= UpdatePreviewInput;
                brush.DoDispose();
            }
            //destroy created material
            Material.DoDispose();
            //free RenderTextures
            renderTextureHelper.DoDispose();
            if (statesController != null)
            {
                layersController.OnLayersCollectionChanged -= statesController.AddState;
                statesController.OnUndo -= TryRender;
                statesController.OnRedo -= TryRender;
            }
            else
            {
            }
            layersController.DoDispose();
            statesController?.DoDispose();
            //destroy raycast data
            //if (renderComponentsHelper.IsMesh())
            //{
            //    RaycastController.Instance.DestroyMeshData(ModelPaintManager);
            //}
            //unsubscribe input events
            UnsubscribeInputEvents();
            inputData.DoDispose();
            //free undo/redo RenderTextures and meshes
            PaintObject.DoDispose();
            initialized = false;
            OnDisposed?.Invoke();
        }

        public void Render()
        {
            if (initialized)
            {
                PaintObject.OnRender();
                PaintObject.Render();
            }
        }

        public void SetPaintMode(PaintMode paintMode2)
        {
            paintModeType = paintMode2;
            if (Application.isPlaying)
            {
                paintMode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
                toolsManager.CurrentTool.SetPaintMode(paintMode);
                PaintObject.SetPaintMode(paintMode);
                Material.SetPreviewTexture(currentBrush.RenderTexture);
            }
        }

        public RenderTexture GetPaintTexture()
        {
            return layersController.ActiveLayer.RenderTexture;
        }

        public RenderTexture GetPaintInputTexture()
        {
            return renderTextureHelper.GetTexture(RenderTarget.Input);
        }

        public RenderTexture GetResultRenderTexture()
        {
            return renderTextureHelper.GetTexture(RenderTarget.Combined);
        }

        /// <summary>
        /// 결과 텍스처를 반환
        /// </summary>
        /// <param name="hideBrushPreview">브러시 미리보기를 숨길지 여부</param>
        /// <returns></returns>
        public Texture2D GetResultTexture(bool hideBrushPreview = false)
        {
            // 브러시 미리보기를 숨기는 것이 필요한지 확인합니다.
            var needToHideBrushPreview = hideBrushPreview && currentBrush.Preview;
            // 브러시 미리보기를 숨기는 경우 currentBrush.Preview를 false로 설정하고 다시 렌더링합니다.
            if (needToHideBrushPreview)
            {
                currentBrush.Preview = false;
                Render();
            }

            RenderTexture temp = null;
            var renderTexture = renderTextureHelper.GetTexture(RenderTarget.Combined);
            // 객체가 SpriteRenderer를 사용하고 특정 쉐이더를 가지고 있는지 확인합니다.
            if (renderComponentsHelper.ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var spriteRenderer = renderComponentsHelper.RendererComponent as SpriteRenderer;
                if (spriteRenderer != null && spriteRenderer.material != null &&
                    spriteRenderer.material.shader == Settings.Instance.SpriteMaskShader)
                {
                    // 임시 RenderTexture를 생성하고 마스크를 적용합니다.
                    temp = RenderTextureFactory.CreateTemporaryRenderTexture(renderTexture, false);
                    var rti = new RenderTargetIdentifier(temp);
                    var commandBufferBuilder = new CommandBufferBuilder("ResultTexture");
                    commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(rti).ClearRenderTarget().Execute();
                    Graphics.Blit(spriteRenderer.material.mainTexture, temp, spriteRenderer.material);
                    commandBufferBuilder.Release();
                }
            }

            // 최종 결과 텍스처를 Texture2D로 생성합니다.
            var resultTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = temp != null ? temp : renderTexture;

            // 활성 RenderTexture에서 resultTexture로 픽셀을 읽어옵니다.
            resultTexture.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0, false);
            resultTexture.Apply();

            // 활성 RenderTexture를 복원합니다.
            RenderTexture.active = previousRenderTexture;
            // 생성한 임시 RenderTexture를 해제합니다.
            if (temp != null)
            {
                RenderTexture.ReleaseTemporary(temp);
            }
            // 브러시 미리보기가 숨겨진 경우 미리보기 설정을 복원합니다.
            if (needToHideBrushPreview)
            {
                currentBrush.Preview = true;
            }

            return resultTexture;
        }

        /// <summary>
        /// Set Layers data from LayersContainer
        /// </summary>
        /// <param name="container"></param>
        public void SetLayersData(LayersContainer container)
        {
            foreach (var layerData in container.LayersData)
            {
                ILayer layer;
                if (layerData.SourceTexture == null)
                {
                    layer = layersController.AddNewLayer(layerData.Name);
                }
                else
                {
                    layer = layersController.AddNewLayer(layerData.Name, layerData.SourceTexture);
                }
                layer.Enabled = layerData.IsEnabled;
                layer.MaskEnabled = layerData.IsMaskEnabled;
                layer.Opacity = layerData.Opacity;
                //layer.BlendingMode = layerData.BlendingMode;
                if (layerData.Mask != null)
                {
                    layersController.AddLayerMask(layer);
                    Graphics.Blit(layerData.Mask, layer.MaskRenderTexture);
                }
            }
            layersController.SetActiveLayer(container.ActiveLayerIndex);
        }

        /// <summary>
        /// Returns Layers data
        /// </summary>
        /// <returns></returns>
        public LayerData[] GetLayersData()
        {
            var layersData = new LayerData[layersController.Layers.Count];
            for (var i = 0; i < layersController.Layers.Count; i++)
            {
                var layer = layersController.Layers[i];
                layersData[i] = new LayerData(layer);
            }
            return layersData;
        }

        

        /// <summary>
        /// Restore source material and texture
        /// </summary>
        private void RestoreSourceMaterialTexture()
        {
            if (initialized && Material.SourceMaterial != null)
            {
                if (Material.SourceMaterial.GetTexture(Material.ShaderTextureName) == null)
                {
                    Material.SourceMaterial.SetTexture(Material.ShaderTextureName, Material.SourceTexture);
                }
                renderComponentsHelper.SetSourceMaterial(Material.SourceMaterial, Material.MaterialIndex);
            }
        }

        public void InitBrush()
        {
            // 보통은 true일 예정
            if (PaintController.Instance.UseSharedSettings)
            {
                currentBrush = PaintController.Instance.Brush;
            }
            else
            {
                if (currentBrush != null)
                {
                    currentBrush.OnTextureChanged -= Material.SetPreviewTexture;
                }
                currentBrush = brush;
                currentBrush.Init(paintMode);
                if (PaintObject != null)
                {
                    PaintObject.Brush = currentBrush;
                }
            }
            Material.SetPreviewTexture(currentBrush.RenderTexture);
            currentBrush.OnTextureChanged -= Material.SetPreviewTexture;
            currentBrush.OnTextureChanged += Material.SetPreviewTexture;
            currentBrush.OnPreviewChanged -= UpdatePreviewInput;
            currentBrush.OnPreviewChanged += UpdatePreviewInput;
        }

        private void InitRenderTexture()
        {
            paintMode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
            if (renderTextureHelper == null)
            {
             //   renderTextureHelper = ModelPaintManager.renderTextureHelper;
                renderTextureHelper = new RenderTextureHelper();
            }
            Material.Init(renderComponentsHelper, null);
            renderTextureHelper.Init(Material.SourceTexture.width, Material.SourceTexture.height, filterMode);
        }

        private void InitLayers()
        {
            layersMergeController = ModelPaintManager.LayersMergeController;
            layersController = ModelPaintManager.LayersController as LayersController;
        }

        private void InitMaterial()
        {
            if (Material.SourceTexture != null)
            {
                Graphics.Blit(Material.SourceTexture, renderTextureHelper.GetTexture(RenderTarget.Combined));
            }
            Material.SetObjectMaterialTexture(renderTextureHelper.GetTexture(RenderTarget.Combined));
            Material.SetPaintTexture(layersController.ActiveLayer.RenderTexture);
            Material.SetInputTexture(renderTextureHelper.GetTexture(RenderTarget.Input));
        }

        private void InitStates()
        {
            if (StatesSettings.Instance.UndoRedoEnabled)
            {
                statesController = ModelPaintManager.StatesController as StatesController;
                statesController.OnUndo -= TryRender;
                statesController.OnUndo += TryRender;
                statesController.OnRedo -= TryRender;
                statesController.OnRedo += TryRender;
                statesController.Enable();
            }
        }

        private void TryRender()
        {
            var isLayersProcessing = (statesController != null && (statesController.IsRedoProcessing || statesController.IsUndoProcessing)) ||
                                     statesController == null ||
                                     layersController.IsMerging;
            if (isLayersProcessing)
                return;

            if (currentBrush != null && !currentBrush.Preview)
            {
                Render();
            }
        }

        private void InitPaintObject()
        {
            if (PaintObject != null)
            {
                UnsubscribeInputEvents();
                PaintObject.DoDispose();
            }

            if (renderComponentsHelper.ComponentType == ObjectComponentType.RawImage)
            {
                PaintObject = new CanvasRendererPaint();
            }
            else if (renderComponentsHelper.ComponentType == ObjectComponentType.SpriteRenderer)
            {
                PaintObject = new SpriteRendererPaint();
            }
            else if (renderComponentsHelper.ComponentType == ObjectComponentType.MeshFilter)
            {
                PaintObject = new MeshRendererPaint();
            }
            else if (renderComponentsHelper.ComponentType == ObjectComponentType.SkinnedMeshRenderer)
            {
                PaintObject = new SkinnedMeshRendererPaint();
            }

            PaintObject.Init(this, Camera, ObjectForPainting.transform, Material, renderTextureHelper, statesController);
            PaintObject.Brush = currentBrush;
            PaintObject.SetActiveLayer(layersController.GetActiveLayer);
            PaintObject.SetPaintMode(paintMode);
            layersMergeController.OnLayersMerge = PaintObject.RenderToTextureWithoutPreview;
        }

        private void InitTools()
        {
            if (toolsManager != null)
            {
                toolsManager.OnToolChanged -= OnToolChanged;
            }
            toolsManager?.DoDispose();
            if (PaintController.Instance.UseSharedSettings)
            {
                paintTool = PaintController.Instance.Tool;
            }
            paintData = new BasePaintData(this, renderTextureHelper, renderComponentsHelper);
            toolsManager = new ToolsManager(paintTool, paintData);
            toolsManager.OnToolChanged += OnToolChanged;
            toolsManager.Init(this);
            toolsManager.CurrentTool.SetPaintMode(paintMode);
            PaintObject.Tool = toolsManager.CurrentTool;
        }

        private void OnToolChanged(IPaintTool tool)
        {
            PaintObject.Tool = tool;
        }

        #region Setup Input Events

        private void SubscribeInputEvents(ObjectComponentType component)
        {
            if (inputData != null)
            {
                UnsubscribeInputEvents();
                inputData.DoDispose();
            }
            inputData = new InputDataResolver().Resolve(component);
            inputData.Init(this, Camera);
            UpdatePreviewInput(currentBrush.Preview);

            // 인풋컨트롤러에 액션 등록하고
            // 등록된 액션 데이터의 액션을 머할건지 등록한다
            InputController.Instance.OnUpdate += inputData.OnUpdate;
            InputController.Instance.OnMouseDown += inputData.OnDown;
            InputController.Instance.OnMouseButton += inputData.OnPress;
            InputController.Instance.OnMouseUp += inputData.OnUp;
            inputData.OnDownHandler += PaintObject.OnMouseDown;
            inputData.OnDownFailedHandler += PaintObject.OnMouseFailed;
            inputData.OnPressHandler += PaintObject.OnMouseButton;
            inputData.OnPressFailedHandler += PaintObject.OnMouseFailed;
            inputData.OnUpHandler += PaintObject.OnMouseUp;
        }

        private void UnsubscribeInputEvents()
        {
            inputData.OnHoverSuccessHandler -= PaintObject.OnMouseHover;
            inputData.OnHoverFailedHandler -= PaintObject.OnMouseHoverFailed;
            inputData.OnDownHandler -= PaintObject.OnMouseDown;
            inputData.OnDownFailedHandler -= PaintObject.OnMouseFailed;
            inputData.OnPressHandler -= PaintObject.OnMouseButton;
            inputData.OnPressFailedHandler -= PaintObject.OnMouseFailed;
            inputData.OnUpHandler -= PaintObject.OnMouseUp;
            InputController.Instance.OnUpdate -= inputData.OnUpdate;
            InputController.Instance.OnMouseHover -= inputData.OnHover;
            InputController.Instance.OnMouseDown -= inputData.OnDown;
            InputController.Instance.OnMouseButton -= inputData.OnPress;
            InputController.Instance.OnMouseUp -= inputData.OnUp;
        }

        private void UpdatePreviewInput(bool preview)
        {
            if (preview)
            {
                inputData.OnHoverSuccessHandler -= PaintObject.OnMouseHover;
                inputData.OnHoverSuccessHandler += PaintObject.OnMouseHover;
                inputData.OnHoverFailedHandler -= PaintObject.OnMouseHoverFailed;
                inputData.OnHoverFailedHandler += PaintObject.OnMouseHoverFailed;
                InputController.Instance.OnMouseHover -= inputData.OnHover;
                InputController.Instance.OnMouseHover += inputData.OnHover;
            }
            else
            {
                inputData.OnHoverSuccessHandler -= PaintObject.OnMouseHover;
                inputData.OnHoverFailedHandler -= PaintObject.OnMouseHoverFailed;
                InputController.Instance.OnMouseHover -= inputData.OnHover;
            }
        }

        #endregion

    }
}