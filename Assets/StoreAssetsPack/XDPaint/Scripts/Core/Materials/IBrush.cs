using UnityEngine;

namespace XDPaint.Core.Materials
{
    public interface IBrush
    {
        string Name { get; set; } 
        Material Material { get; }
        bool Preview { get; set; }
        float Hardness { get; }
        float Size { get; }
        FilterMode FilterMode { get; }
        Color Color { get; }
        Texture SourceTexture { get; }
        RenderTexture RenderTexture { get; }
        Vector2 SourceTextureSize { get; }
        
        void SetColor(Color color, bool render = true, bool sendToEvent = true);
        void SetTexture(Texture texture, bool render = true, bool sendToEvent = true, bool canUpdateRenderTexture = true);
    }
}