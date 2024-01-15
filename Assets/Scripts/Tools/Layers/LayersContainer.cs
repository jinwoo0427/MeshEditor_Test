using UnityEngine;

namespace GetampedPaint.Tools.Layers
{
    [CreateAssetMenu(fileName = "LayersContainer", menuName = "XDPaint/LayersContainer", order = 101)]
    public class LayersContainer : ScriptableObject
    {
        public LayerData[] LayersData;
        public int ActiveLayerIndex;
    }
}