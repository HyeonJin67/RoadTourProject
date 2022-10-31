using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools;

namespace XDPaint.Core
{
	public class RenderTextureHelper : IRenderTextureHelper
	{
		private Dictionary<RenderTarget, KeyValuePair<RenderTexture, RenderTargetIdentifier>> renderTexturesData;

		/// <summary>
		/// Creates 3 RenderTextures:
		/// Paint - for painting;
		/// PaintInput - for paint between using Input down and up events (AdditivePaintMode) or for current frame paint result storing (DefaultPaintMode);
		/// CombinedTexture - for combining source texture with paint texture and for brush preview.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="filterMode"></param>
		public void Init(int width, int height, FilterMode filterMode)
		{
			ReleaseTextures();
			renderTexturesData = new Dictionary<RenderTarget, KeyValuePair<RenderTexture, RenderTargetIdentifier>>();
			if (!renderTexturesData.ContainsKey(RenderTarget.Paint))
			{
				var paint = RenderTextureFactory.CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGB32, filterMode);
				renderTexturesData.Add(RenderTarget.Paint, new KeyValuePair<RenderTexture, RenderTargetIdentifier>(paint, new RenderTargetIdentifier(paint)));
			}
			if (!renderTexturesData.ContainsKey(RenderTarget.PaintInput))
			{
				var paintInput = RenderTextureFactory.CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGB32, filterMode);
				renderTexturesData.Add(RenderTarget.PaintInput, new KeyValuePair<RenderTexture, RenderTargetIdentifier>(paintInput, new RenderTargetIdentifier(paintInput)));
			}
			if (!renderTexturesData.ContainsKey(RenderTarget.Combined))
			{
				var combined = RenderTextureFactory.CreateRenderTexture(width, height, 0, RenderTextureFormat.ARGB32, filterMode);
				renderTexturesData.Add(RenderTarget.Combined, new KeyValuePair<RenderTexture, RenderTargetIdentifier>(combined, new RenderTargetIdentifier(combined)));
			}
		}

		/// <summary>
		/// Releases RenderTextures
		/// </summary>
		public void ReleaseTextures()
		{
			ReleaseRT(RenderTarget.Paint);
			ReleaseRT(RenderTarget.PaintInput);
			ReleaseRT(RenderTarget.Combined);
		}

		public RenderTargetIdentifier GetTarget(RenderTarget target)
		{
			return renderTexturesData[target].Value;
		}

		public RenderTexture GetTexture(RenderTarget target)
		{
			return renderTexturesData[target].Key;
		}

		private void ReleaseRT(RenderTarget target)
		{
			if (renderTexturesData != null && renderTexturesData.ContainsKey(target))
			{
				renderTexturesData[target].Key.ReleaseTexture();
				renderTexturesData.Remove(target);
			}
		}
	}
}