namespace XDPaint.Core
{
	public enum ObjectComponentType
	{
		Unknown,
		RawImage,
		MeshFilter,
		SkinnedMeshRenderer,
		SpriteRenderer
	}
	
	public enum PaintTool
	{
		Brush,
		Erase,
		Eyedropper,
		BrushSampler,
		Clone,
		Blur,
		BlurGaussian
	}

	public enum PaintRenderTexture
	{
		PaintTexture,
		CombinedTexture
	}
	
	public enum PaintMode
	{
		Default = 0x0,
		Additive = 0x100
    }
    public enum ImageMode
    { 

	}
}