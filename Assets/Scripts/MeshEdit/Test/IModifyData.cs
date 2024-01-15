using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Core.Materials;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.States;
using GetampedPaint.Utils;

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
