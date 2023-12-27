using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.Data;
using XDPaint.Tools.Raycast;
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
        hovering.mode = elementMode;
        int hash = hovering.hashCode;
        bool wasValid = hovering.valid;
        hovering.valid = false;

        switch (elementMode)
        {
            case ElementMode.Vertex:
                {
                    int tri = -1;

                    if (MeshRayCastUtils.VertexRaycast(position, 32, selection, out tri))
                    {
                        hovering.valid = true;
                        hovering.hashCode = tri.GetHashCode();
                        hovering.vertices[0] = selection.verticesInWorldSpace[tri];
                        //CreateDot(hovering.vertices[0]);
                    }
                }
                break;

            case ElementMode.Face:
                {
                    MeshRaycastHit hit;

                    if (MeshRayCastUtils.MeshRaycast(HandleUtility.GUIPointToWorldRay(position), selection.mesh, out hit))
                    {
                        Triangle face = selection.mesh.faces[hit.FaceIndex];
                        hovering.valid = true;

                        if (hash != face.GetHashCode())
                        {
                            hovering.hashCode = face.GetHashCode();

                            hovering.vertices[0] = selection.verticesInWorldSpace[face.I0];
                            hovering.vertices[1] = selection.verticesInWorldSpace[face.I1];
                            hovering.vertices[2] = selection.verticesInWorldSpace[face.I2];

                            hovering.vertices[3] = selection.verticesInWorldSpace[face.I2];
                        }
                    }
                }
                break;

            case ElementMode.Edge:
                {
                    Edge edge;

                    if (MeshRayCastUtils.EdgeRaycast(position, selection, out edge))
                    {
                        hovering.valid = true;

                        if (hash != edge.GetHashCode())
                        {
                            hovering.hashCode = edge.GetHashCode();

                            hovering.vertices[0] = selection.verticesInWorldSpace[edge.x];
                            hovering.vertices[1] = selection.verticesInWorldSpace[edge.y];
                        }
                    }
                }
                break;
        }

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
