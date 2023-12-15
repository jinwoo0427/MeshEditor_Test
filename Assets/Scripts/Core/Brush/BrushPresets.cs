using System.Collections.Generic;
using UnityEngine;
using XDPaint.Core.Materials;
using XDPaint.Utils;

namespace XDPaint.Tools
{
    [CreateAssetMenu(fileName = "BrushPresets", menuName = "XDPaint/Brush Presets", order = 100)]
    public class BrushPresets : SingletonScriptableObject<BrushPresets>
    {
        public List<Brush> Presets = new List<Brush>();
    }
}