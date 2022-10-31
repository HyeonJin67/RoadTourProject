using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core
{
	public interface IRenderTextureHelper
	{
		void Init(int width, int height, FilterMode filterMode);
		void ReleaseTextures();
		RenderTargetIdentifier GetTarget(RenderTarget target);
		RenderTexture GetTexture(RenderTarget target);
	}
}