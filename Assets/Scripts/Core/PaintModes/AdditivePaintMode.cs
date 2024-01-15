using UnityEngine.Scripting;

namespace GetampedPaint.Core.PaintModes
{
    [Preserve]
    public class AdditivePaintMode : IPaintMode
    {
        public PaintMode PaintMode => PaintMode.Additive;
        public RenderTarget RenderTarget => RenderTarget.Input;
        public bool UsePaintInput => true;
    }
}