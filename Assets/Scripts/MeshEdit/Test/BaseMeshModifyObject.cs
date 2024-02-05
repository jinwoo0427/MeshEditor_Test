using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintObject.Data;
using GetampedPaint.Tools.Raycast;
using GetampedPaint.Tools.Raycast.Data;

[Serializable]
public abstract class BaseMeshModifyObject : BaseMeshModifyObjectRenderer
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

    private Camera thisCamera;
    public Camera ThisCamera
    {
        get => thisCamera;
        set
        {
            thisCamera = value;
        }
    }
    protected Transform ObjectTransform { get; private set; }
    protected IMeshModifyManager ModifyManager;




    #region Input

    public void OnMouseHover(int fingerId, Vector3 position, RaycastData raycast = null)
    {
        Debug.Log("OnMouseHover");
        hovering.mode = elementMode;
        int hash = hovering.hashCode;
        bool wasValid = hovering.valid;
        hovering.valid = false;

        //if (hash != hovering.hashCode || hovering.valid != wasValid)
        //    SceneView.RepaintAll();
    }

    public void OnMouseHoverFailed(int fingerId, Vector3 position, RaycastData raycast = null)
    {
        
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

    }

    public void OnMouseFailed(int fingerId, Vector3 position, float pressure = 1f, RaycastData raycast = null)
    {
       
    }

    public void OnMouseUp(int fingerId, Vector3 position)
    {

    }

    public Vector3? GetPaintPosition(int fingerId, Vector3 position, RaycastData raycast = null)
    {
        
        return null;
    }

    #endregion



}
