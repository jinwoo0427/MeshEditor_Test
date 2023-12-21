using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.States;
using XDPaint.Utils;

public interface IModifyData : IDisposable
{
    IStatesController StatesController { get; }
    //IBrush Brush { get; }
    IModifyMode ModifyMode { get; }
    Camera Camera { get; }
    Material Material { get; }
    CommandBufferBuilder CommandBuilder { get; }
    Mesh QuadMesh { get; }
    bool IsModifying { get; }
    bool InBounds { get; }

    void Render();
    void SaveState();
    void StartCoroutine(IEnumerator coroutine);
    void StopCoroutine(IEnumerator coroutine);
}
