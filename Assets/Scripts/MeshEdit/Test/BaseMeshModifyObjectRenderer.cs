using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.PaintModes;

public class BaseMeshModifyObjectRenderer : IDisposable
{
    protected IModifyMode ModifyMode;
    protected HoveringPreview hovering;


    const int CLICK_RECT = 32;

    readonly Color MOUSE_DRAG_RECT_COLOR = new Color(.313f, .8f, 1f, 1f);

    [SerializeField] protected ElementMode elementMode = ElementMode.Face;

    [SerializeField] protected ElementCache selection;

    public void SetModifyMode(IModifyMode modifyMode)
    {
        ModifyMode = modifyMode;

       

        CacheIndicesForGraphics();

        UpdateGraphics();
    }
    private void DestroyImmediate(object material)
    {
        throw new System.NotImplementedException();
    }

    private void UpdateGraphics()
    {
        
    }

    private void CacheIndicesForGraphics()
    {

    }

    public void InitRenderer()
    {

    }

    public void DoDispose()
    {

    }


}
