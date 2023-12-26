using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.PaintModes;

public class BaseMeshModifyObjectRenderer : IDisposable
{
    protected IModifyMode ModifyMode;

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
