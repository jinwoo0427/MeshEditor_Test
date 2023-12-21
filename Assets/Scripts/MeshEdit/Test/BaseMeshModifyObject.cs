using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.Data;
using XDPaint.Tools.Raycast.Data;

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



    #region Input

    public void OnMouseHover(int fingerId, Vector3 position, RaycastData raycast = null)
    {
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
