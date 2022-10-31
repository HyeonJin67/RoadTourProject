using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintModes
{
    public interface IPaintMode
    {
        PaintMode PaintMode { get; }
        RenderTarget RenderTarget { get; }
        bool UsePaintInput { get; }
    }
}