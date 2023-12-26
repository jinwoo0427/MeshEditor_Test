using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HoveringPreview
{
    public Vector3[] vertices = new Vector3[4];

    public ElementMode mode;
    public bool valid = false;
    public int hashCode;

    readonly Color FACE_HIGHLIGHT_COLOR = new Color(.8f, .2f, .2f, .4f);
    readonly Color EDGE_HIGHLIGHT_COLOR = new Color(.8f, .2f, .2f, .8f);
    readonly Color VERTEX_HIGHLIGHT_COLOR = new Color(.8f, .2f, .2f, .8f);

    public void DrawHandles()
    {
        if (!valid)
            return;

        switch (mode)
        {
            case ElementMode.Vertex:
                {
                    Handles.color = VERTEX_HIGHLIGHT_COLOR;

                    //Handles.DotCap(-1, vertices[0], Quaternion.identity, HandleUtility.GetHandleSize(vertices[0]) * .06f);
                    Handles.DotHandleCap(-1, vertices[0], Quaternion.identity, HandleUtility.GetHandleSize(vertices[0]) * .06f, EventType.Repaint);


                }
                break;

            case ElementMode.Face:
                {
                    Handles.DrawSolidRectangleWithOutline(vertices,
                                                            FACE_HIGHLIGHT_COLOR,
                                                            FACE_HIGHLIGHT_COLOR);
                }
                break;

            case ElementMode.Edge:
                {
                    Handles.color = EDGE_HIGHLIGHT_COLOR;
                    Handles.DrawLine(vertices[0], vertices[1]);
                }
                break;
        }

        Handles.color = Color.white;
    }
}
