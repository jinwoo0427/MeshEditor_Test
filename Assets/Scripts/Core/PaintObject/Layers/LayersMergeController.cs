using System;
using UnityEngine;

namespace XDPaint.Core.Layers
{
    [Serializable]
    public class LayersMergeController
    {
        public Action<RenderTexture> OnLayersMerge;

        public void MergeLayers(RenderTexture resultTexture)
        {
            OnLayersMerge(resultTexture);
        }
    }
}