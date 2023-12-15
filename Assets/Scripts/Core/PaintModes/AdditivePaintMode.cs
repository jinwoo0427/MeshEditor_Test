using UnityEngine.Scripting;

namespace XDPaint.Core.PaintModes
{
    [Preserve]
    public class AdditivePaintMode : IPaintMode
    {
        public PaintMode PaintMode => PaintMode.Additive;
        public RenderTarget RenderTarget => RenderTarget.Input;
        public bool UsePaintInput => true;
    }
}