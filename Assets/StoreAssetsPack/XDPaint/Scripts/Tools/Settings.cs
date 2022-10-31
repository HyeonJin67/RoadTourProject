using UnityEngine;

namespace XDPaint.Tools
{
    [CreateAssetMenu(fileName = "XDPaintSettings", menuName = "XDPaint Settings", order = 100)]
    public class Settings : SingletonScriptableObject<Settings>
    {
        [HideInInspector] public Shader BrushShader;
        [HideInInspector] public Shader BrushRenderShader;
        [HideInInspector] public Shader EyedropperShader;
        [HideInInspector] public Shader BrushSamplerShader;
        [HideInInspector] public Shader BrushCloneShader;
        [HideInInspector] public Shader BlurShader;
        [HideInInspector] public Shader GaussianBlurShader;
        [HideInInspector] public Shader BrushBlurShader;
        [HideInInspector] public Shader PaintShader;
        [HideInInspector] public Shader AverageColorShader;
        [HideInInspector] public Shader AverageColorCutOffShader;
        [HideInInspector] public Shader SpriteMaskShader;
        public Texture DefaultBrush;
        public Texture DefaultCircleBrush;
        public bool UndoRedoEnabled = true;
        public uint UndoRedoMaxActionsCount = 20;
        public bool PressureEnabled = true;
        public bool CheckCanvasRaycasts = true;
        public uint BrushDuplicatePartWidth = 4;
        public float PixelPerUnit = 100f;
        public string ContainerGameObjectName = "[XDPaintContainer]";
    }
}