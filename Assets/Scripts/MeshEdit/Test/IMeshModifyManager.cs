using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Core.PaintObject.Base;
using GetampedPaint.States;
using GetampedPaint.Tools.Raycast;

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
