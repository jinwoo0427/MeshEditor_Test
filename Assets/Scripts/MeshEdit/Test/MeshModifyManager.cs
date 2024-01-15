using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using GetampedPaint.Controllers.InputData.Base;
using GetampedPaint.Controllers;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core;
using GetampedPaint;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.States;
using GetampedPaint.Tools.Images.Base;
using GetampedPaint.Tools.Layers;
using GetampedPaint.Tools;
using GetampedPaint.Tools.Raycast;
using GetampedPaint.Controllers.InputData;
using GetampedPaint.Tools.Triangles;
using GetampedPaint.Core.PaintObject;
using GetampedPaint.Demo.UI;
using UnityEditor;

public class MeshModifyManager : MonoBehaviour, IMeshModifyManager
{
    #region Events

    public event Action<MeshModifyManager> OnInitialized;
    public event Action OnDisposed;

    #endregion

    #region Properties and variables

    [FormerlySerializedAs("ObjectForEditing")][SerializeField] private GameObject objectForModifying;
    public GameObject ObjectForModifying
    {
        get => objectForModifying;
        set => objectForModifying = value;
    }

    //[FormerlySerializedAs("Material")][SerializeField] private Paint material = new Paint();
    //public Paint Material => material;


    [SerializeField] private BaseMeshModifyObject meshModifyObject;
    public BaseMeshModifyObject ModifyObject
    {
        get => meshModifyObject;
        private set => meshModifyObject = value;
    }

    private StatesController statesController;
    public IStatesController StatesController => statesController;

    [SerializeField] private ElementMode modifyModeType;


    public Camera Camera => PaintController.Instance !=null ? PaintController.Instance.Camera : Camera.main;


    public bool HasTrianglesData => triangles != null && triangles.Length > 0;

    public bool Initialized => initialized;

    private Triangle[] triangles;
    public Triangle[] Triangles => triangles;

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
    [SerializeField]
    private ObjectComponentType componentType ;
    public ObjectComponentType ComponentType => componentType;

    private IModifyMode modifyMode;
    public IModifyMode ModifyMode
    {
        get
        {
            if (initialized && Application.isPlaying)
            {
                //editMode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
            }
            return modifyMode;
        }
    }

    private ElementCache selection;
    private IPaintData paintData;
    private BaseInputData inputData;
    private bool initialized;
    private void Start()
    {
        if (initialized)
            return;

        Init();
    }
    public void Init()
    {
        if (initialized)
            DoDispose();

        initialized = false;
        if (ObjectForModifying == null)
        {
            Debug.LogError("ObjectForPainting is null!");
            return;
        }


        if (componentType == ObjectComponentType.Unknown)
        {
            Debug.LogError("Unknown component type!");
            return;
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

        //    RaycastController.Instance.InitObject(this, paintComponent, renderComponent);
        //}


        //register PaintManager
        InitMeshObject();

        SubscribeInputEvents(componentType);
        initialized = true;
        Render();

        OnInitialized?.Invoke(this);
    }

    public void Render()
    {

    }

    public void DoDispose()
    {

    }

    private void InitMeshObject()
    {
        if (ModifyObject != null)
        {
            UnsubscribeInputEvents();
            ModifyObject.DoDispose();
        }

        if (ComponentType == ObjectComponentType.MeshFilter)
        {
            ModifyObject = new SkinnedMeshRendererModify();
        }

        ModifyObject.SetModifyMode(ModifyMode);
    }

    #endregion

    void SetElementMode(ElementMode mode)
    {
        modifyModeType = mode;

        if (selection.mesh.handlesRenderer.material != null)
            DestroyImmediate(selection.mesh.handlesRenderer.material);

        selection.mesh.handlesRenderer.material = new Material(
            Shader.Find(mode == ElementMode.Vertex ? "Hidden/QuickEdit/VertexShader" : "Hidden/QuickEdit/FaceShader"));

        if (modifyModeType == ElementMode.Vertex)
            selection.mesh.handlesRenderer.material.SetFloat("_Scale", 3f);

        selection.mesh.handlesRenderer.material.hideFlags = HideFlags.HideAndDontSave;

        CacheIndicesForGraphics();

        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        throw new NotImplementedException();
    }

    private void CacheIndicesForGraphics()
    {
        throw new NotImplementedException();
    }


    #region Setup Input Events

    private void SubscribeInputEvents(ObjectComponentType component)
    {
        if (inputData != null)
        {
            UnsubscribeInputEvents();
            inputData.DoDispose();
        }
        inputData = new InputDataResolver().Resolve(component, true);
        inputData.Init(this, Camera);
        UpdatePreviewInput(true);

        // 인풋컨트롤러에 액션 등록하고
        // 등록된 액션 데이터의 액션을 머할건지 등록한다
        InputController.Instance.OnUpdate += inputData.OnUpdate;
        InputController.Instance.OnMouseDown += inputData.OnDown;
        InputController.Instance.OnMouseButton += inputData.OnPress;
        InputController.Instance.OnMouseUp += inputData.OnUp;
        inputData.OnDownHandler += ModifyObject.OnMouseDown;
        inputData.OnDownFailedHandler += ModifyObject.OnMouseFailed;
        inputData.OnPressHandler += ModifyObject.OnMouseButton;
        inputData.OnPressFailedHandler += ModifyObject.OnMouseFailed;
        inputData.OnUpHandler += ModifyObject.OnMouseUp;
    }

    private void UnsubscribeInputEvents()
    {
        inputData.OnHoverSuccessHandler -= ModifyObject.OnMouseHover;
        inputData.OnHoverFailedHandler -= ModifyObject.OnMouseHoverFailed;
        inputData.OnDownHandler -= ModifyObject.OnMouseDown;
        inputData.OnDownFailedHandler -= ModifyObject.OnMouseFailed;
        inputData.OnPressHandler -= ModifyObject.OnMouseButton;
        inputData.OnPressFailedHandler -= ModifyObject.OnMouseFailed;
        inputData.OnUpHandler -= ModifyObject.OnMouseUp;
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

            inputData.OnHoverSuccessHandler -= ModifyObject.OnMouseHover;
            inputData.OnHoverSuccessHandler += ModifyObject.OnMouseHover;
            inputData.OnHoverFailedHandler -= ModifyObject.OnMouseHoverFailed;
            inputData.OnHoverFailedHandler += ModifyObject.OnMouseHoverFailed;
            InputController.Instance.OnMouseHover -= inputData.OnHover;
            InputController.Instance.OnMouseHover += inputData.OnHover;
        }
        else
        {
            inputData.OnHoverSuccessHandler -= ModifyObject.OnMouseHover;
            inputData.OnHoverFailedHandler -= ModifyObject.OnMouseHoverFailed;
            InputController.Instance.OnMouseHover -= inputData.OnHover;
        }
    }

    #endregion

}
