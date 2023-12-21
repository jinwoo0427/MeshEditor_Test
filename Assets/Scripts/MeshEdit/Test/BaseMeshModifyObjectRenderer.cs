using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.PaintModes;

public class BaseMeshModifyObjectRenderer : IDisposable
{
    protected IModifyMode ModifyMode;

    public void SetModifyMode(IModifyMode modifyMode)
    {
        ModifyMode = modifyMode;
    }

    public void InitRenderer()
    {

    }

    public void DoDispose()
    {

    }


}
