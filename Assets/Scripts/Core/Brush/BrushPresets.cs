using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core.Materials;
using GetampedPaint.Utils;

namespace GetampedPaint.Tools
{
    [CreateAssetMenu(fileName = "BrushPresets", menuName = "XDPaint/Brush Presets", order = 100)]
    public class BrushPresets : SingletonScriptableObject<BrushPresets>
    {
        public List<Brush> Presets = new List<Brush>();
    }
}