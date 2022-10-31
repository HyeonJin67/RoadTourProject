using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintModes
{
    public class AdditivePaintMode : IPaintMode
    {
        public PaintMode PaintMode { get { return PaintMode.Additive; } }
        public RenderTarget RenderTarget { get { return RenderTarget.PaintInput; } }
        public bool UsePaintInput { get { return true; } }
    }
}