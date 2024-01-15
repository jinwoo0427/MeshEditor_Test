using UnityEngine.Scripting;

namespace GetampedPaint.Core.PaintModes
{
    [Preserve]
    public class DefaultPaintMode : IPaintMode
    {
        public PaintMode PaintMode => PaintMode.Default;
        public RenderTarget RenderTarget => RenderTarget.ActiveLayer;
        public bool UsePaintInput => false;
    }
}