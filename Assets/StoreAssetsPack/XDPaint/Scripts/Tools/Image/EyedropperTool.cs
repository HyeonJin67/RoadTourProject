using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
	[Serializable]
	public class EyedropperTool : BasePaintTool
	{
		public override PaintTool Type { get { return PaintTool.Eyedropper; } }
		public override bool RenderToInputTexture { get { return false; } }
		public override bool ShowPreview { get { return false; } }
		
		private Material material;
		private RenderTexture brushTexture;
		private CommandBufferBuilder commandBufferBuilder;
		private RenderTargetIdentifier brushRti;
		private const string MainTexParam = "_MainTex";
		private const string BrushTexParam = "_BrushTex";
		private const string BrushOffsetShaderParam = "_BrushOffset";

		public override void Enter()
		{
			base.Enter();
			RenderToPaintTexture = false;
			commandBufferBuilder = new CommandBufferBuilder("EyedropperToolBuffer");
			InitMaterial();
		}

		public override void Exit()
		{
			base.Exit();
			if (commandBufferBuilder != null)
			{
				commandBufferBuilder.Release();
			}
		}

		public override void UpdatePress(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdatePress(sender, uv, paintPosition, pressure);
			var brushOffset = GetPreviewVector(PaintManager, paintPosition, pressure);
			material.SetTexture(MainTexParam, PaintManager.GetResultRenderTexture());
			material.SetVector(BrushOffsetShaderParam, brushOffset);
			UpdateRenderTexture();
			Render(PaintManager);
		}

		private Vector4 GetPreviewVector(PaintManager paintManager, Vector2 paintPosition, float pressure)
		{
			var brushRatio = new Vector2(
				paintManager.Material.SourceTexture.width,
				paintManager.Material.SourceTexture.height) / paintManager.Brush.Size / pressure;
			var brushOffset = new Vector4(
				paintPosition.x / paintManager.Material.SourceTexture.width * brushRatio.x,
				paintPosition.y / paintManager.Material.SourceTexture.height * brushRatio.y,
				1f / brushRatio.x, 1f / brushRatio.y);
			return brushOffset;
		}

		private void InitMaterial()
		{
			if (material == null)
			{
				material = new Material(Settings.Instance.EyedropperShader);
			}
		}

		/// <summary>
		/// Renders pixel to RenderTexture and set a new brush color
		/// </summary>
		/// <param name="paintManager"></param>
		private void Render(PaintManager paintManager)
		{
			commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(brushRti).ClearRenderTarget(Constants.Color.ClearBlack).DrawMesh(QuadMesh, material).Execute();
			
			var previousRenderTexture = RenderTexture.active;
			var texture2D = new Texture2D(brushTexture.width, brushTexture.height, TextureFormat.ARGB32, false);
			RenderTexture.active = brushTexture;
			texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
			texture2D.Apply();
			RenderTexture.active = previousRenderTexture;

			var pixelColor = texture2D.GetPixel(0, 0);
			paintManager.Brush.SetColor(pixelColor);
		}

		/// <summary>
		/// Creates 1x1 render texture
		/// </summary>
		private void UpdateRenderTexture()
		{
			if (brushTexture != null)
				return;
			brushTexture = RenderTextureFactory.CreateRenderTexture(1, 1);
			material.SetTexture(BrushTexParam, brushTexture);
			brushRti = new RenderTargetIdentifier(brushTexture);
		}
	}
}