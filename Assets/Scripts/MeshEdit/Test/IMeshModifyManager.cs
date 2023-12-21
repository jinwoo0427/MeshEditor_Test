using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject.Base;
using XDPaint.States;
using XDPaint.Tools.Raycast;

public interface IMeshModifyManager : IDisposable
{
    Camera Camera { get; }
    GameObject ObjectForModifying { get; }
    //Paint Material { get; }
    BaseMeshModifyObject ModifyObject { get; }
    IStatesController StatesController { get; }
    IModifyMode ModifyMode { get; }
    Triangle[] Triangles { get; }
    int SubMesh { get; }
    int UVChannel { get; }
    bool Initialized { get; }

    void Init();
    void Render();
}
