using System;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine;
using XDPaint.Core.Layers;
using IDisposable = XDPaint.Core.IDisposable;

namespace XDPaint.States
{
    public interface IStatesController : IDisposable
    {
        event Action<RenderTexture> OnClearTextureAction;
        event Action OnRenderTextureAction;
        event Action OnChangeState;
        event Action OnResetState;
        event Action OnUndo;
        event Action OnRedo;
        event Action<bool> OnUndoStatusChanged;
        event Action<bool> OnRedoStatusChanged;
        bool IsUndoProcessing { get; }
        bool IsRedoProcessing { get; }
        
        IStatesController GetStatesController();
        void Init(ILayersController layersController);
        void Enable();
        void Disable();
        void AddState(Action action);
        void AddState(object entity, string property, RenderTexture oldValue, RenderTexture newValue, Texture source);
        void AddState(object entity, string property, object oldValue, object newValue);
        void AddState(IList collection, NotifyCollectionChangedEventArgs rawEventArg);
        void Undo();
        void Redo();
        int GetUndoActionsCount();
        int GetRedoActionsCount();
        bool CanUndo();
        bool CanRedo();
        void EnableGrouping();
        void DisableGrouping();
    }
}