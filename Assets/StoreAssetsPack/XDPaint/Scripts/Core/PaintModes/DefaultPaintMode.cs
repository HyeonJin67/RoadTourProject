using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintModes
{
    public class DefaultPaintMode : IPaintMode
    {
        public PaintMode PaintMode { get { return PaintMode.Default; } }
        public RenderTarget RenderTarget { get { return RenderTarget.Paint; } }
        public bool UsePaintInput { get { return false; } }
    }
}