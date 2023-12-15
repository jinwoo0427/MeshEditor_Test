using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

namespace XDPaint.Core.Layers
{
    public interface ILayersController : IDisposable
    {
        event Action<ObservableCollection<ILayer>, NotifyCollectionChangedEventArgs> OnLayersCollectionChanged;
        event Action<ILayer> OnLayerChanged;
        event Action<ILayer> OnActiveLayerSwitched;
        event Action<bool> OnCanRemoveLayer;
        
        int ActiveLayerIndex { get; }
        bool CanDisableLayer { get; }
        bool CanRemoveLayer { get; }
        bool CanMergeLayers { get; }
        bool CanMergeAllLayers { get; }
        bool IsMerging { get; }
        ReadOnlyCollection<ILayer> Layers { get; }
        ILayer ActiveLayer { get; }
        
        void Init(int width, int height);
        void CreateBaseLayers(Texture sourceTexture, bool copySourceTextureToLayer, bool useSourceTextureAsBackground);
        void SetFilterMode(FilterMode filterMode);
        ILayer AddNewLayer();
        ILayer AddNewLayer(string name);
        ILayer AddNewLayer(string name, Texture source);
        void AddLayerMask(ILayer layer, Texture source);
        void AddLayerMask(ILayer layer);
        void AddLayerMask(Texture source);
        void AddLayerMask();
        void RemoveActiveLayerMask();
        void RemoveLayer(int index);
        void RemoveLayer(ILayer layer);
        void RemoveActiveLayer();
        ILayer GetActiveLayer();
        void SetActiveLayer(ILayer layer);
        void SetActiveLayer(int index);
        void SetLayerOrder(ILayer layer, int index);
        void MergeLayers();
        void MergeAllLayers();
        void SetLayerTexture(int index, Texture texture);
        Texture2D GetActiveLayerTexture();
        Texture2D GetLayerTexture(int layerIndex);
    }
}