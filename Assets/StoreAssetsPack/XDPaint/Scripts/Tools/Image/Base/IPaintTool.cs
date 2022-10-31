using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Tools.Image.Base
{
    public interface IPaintTool : IDisposable
    {
        PaintTool Type { get; }
        bool AllowRender { get; }
        bool ShowPreview { get; }
        bool DrawPreview { get; }
        bool RenderToTextures { get; }
        bool RenderToPaintTexture { get; }
        bool RenderToInputTexture { get; }
        bool DrawPreProcess { get; }
        bool DrawProcess { get; }
        bool BakeInputToPaint { get; }

        void SetPaintManager(PaintManager paintManager);
        void SetPaintMode(IPaintMode mode);
        void OnDrawPreProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material);
        void OnDrawProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material);
        void OnBakeInputToPaint(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material);
        void Enter();
        void Exit();
        void UpdateHover(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure);
        void UpdateDown(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure);
        void UpdatePress(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure);
        void UpdateUp(BasePaintObject sender, bool inBounds);
        void OnPaint(BasePaintObject sender, Vector2 paintPosition, float pressure);
        void OnUndo(BasePaintObject sender);
        void OnRedo(BasePaintObject sender);
    }
}