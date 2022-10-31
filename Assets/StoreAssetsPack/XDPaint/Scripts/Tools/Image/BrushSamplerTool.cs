using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;
using Object = UnityEngine.Object;

namespace XDPaint.Tools.Image
{
	[Serializable]
	public class BrushSamplerTool : BasePaintTool
	{
		public override PaintTool Type { get { return PaintTool.BrushSampler; } }
		public override bool ShowPreview { get { return preview; } }
		public override bool RenderToPaintTexture { get { return false; } }
		public override bool RenderToInputTexture { get { return false; } }

		private Material brushMaterial;
		private RenderTexture brushTexture;
		private RenderTexture previewTexture;
		private RenderTargetIdentifier brushTarget;
		private bool preview;
		private bool shouldSetBrushTextureParam;
		private const string BrushTexParam = "_BrushTex";
		private const string BrushMaskTexParam = "_MaskTex";
		private const string BrushOffsetShaderParam = "_BrushOffset";

		public override void Enter()
		{
			preview = PaintManager.Brush.Preview;
			base.Enter();
			InitMaterial();
			PaintManager.Brush.SetColor(new Color(1, 1, 1, PaintManager.Brush.Color.a), false, false);
			UpdateBrushPreview();
		}

		public override void Exit()
		{
			base.Exit();
			// if (brushTexture != null)
			// {
			// 	brushTexture.ReleaseTexture();
			// }
			if (previewTexture != null)
			{
				previewTexture.ReleaseTexture();
			}
			PaintManager.Material.Material.SetTexture(Paint.BrushTextureShaderParam, PaintManager.Brush.RenderTexture);
			if (brushMaterial != null)
			{
				Object.Destroy(brushMaterial);
				brushMaterial = null;
			}
		}

		public override void DoDispose()
		{
			base.DoDispose();
			if (brushTexture != null && brushTexture.IsCreated())
			{
				brushTexture.Release();
				Object.Destroy(brushTexture);
			}
		}

		public override void UpdatePress(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdatePress(sender, uv, paintPosition, pressure);
			var brushOffset = GetPreviewVector(paintPosition, pressure);
			brushMaterial.SetVector(BrushOffsetShaderParam, brushOffset);
			RenderBrush();
		}

		public override void UpdateDown(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdateDown(sender, uv, paintPosition, pressure);
			UpdateRenderTexture();
			if (shouldSetBrushTextureParam)
			{
				brushMaterial.SetTexture(BrushTexParam, brushTexture);
				shouldSetBrushTextureParam = false;
			}
		}

		private void UpdateBrushPreview()
		{
			previewTexture = RenderTextureFactory.CreateRenderTexture(PaintManager.Brush.RenderTexture);
			PaintManager.Material.Material.SetTexture(Paint.BrushTextureShaderParam, previewTexture);
			var brushSourceTexture = PaintManager.Brush.SourceTexture;
			var previousColor = PaintManager.Brush.Color;
			var brushHardness = PaintManager.Brush.Hardness;
			PaintManager.Brush.Hardness = 1.0f;
			PaintManager.Brush.SetColor(Color.white, false, false);
			PaintManager.Brush.SetTexture(Settings.Instance.DefaultCircleBrush, true, false, false);
			Graphics.Blit(PaintManager.Brush.RenderTexture, previewTexture);
			PaintManager.Brush.SetColor(previousColor, true, false);
			PaintManager.Brush.SetTexture(brushSourceTexture, true, false);
			PaintManager.Brush.Hardness = brushHardness;
		}

		private Vector4 GetPreviewVector(Vector2 paintPosition, float pressure)
		{
			var brushRatio = new Vector2(
				PaintManager.Material.SourceTexture.width / PaintManager.Brush.SourceTextureSize.x,
				PaintManager.Material.SourceTexture.height / PaintManager.Brush.SourceTextureSize.y) / PaintManager.Brush.Size / pressure;
			var brushOffset = new Vector4(
				paintPosition.x / PaintManager.Material.SourceTexture.width * brushRatio.x,
				paintPosition.y / PaintManager.Material.SourceTexture.height * brushRatio.y,
				1f / brushRatio.x, 1f / brushRatio.y);
			return brushOffset;
		}

		private void InitMaterial()
		{
			if (brushMaterial == null)
			{
				brushMaterial = new Material(Settings.Instance.BrushSamplerShader);
				shouldSetBrushTextureParam = true;
				brushMaterial.SetTexture(BrushMaskTexParam, PaintManager.Brush.SourceTexture);
			}
		}

		/// <summary>
		/// Renders part of Result texture into RenderTexture, set new brush
		/// </summary>
		private void RenderBrush()
		{
			//set preview to false
			preview = false;
			PaintManager.Render();
			brushMaterial.mainTexture = PaintManager.GetResultRenderTexture();
			CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(brushTarget).ClearRenderTarget(Constants.Color.ClearBlack).DrawMesh(QuadMesh, brushMaterial).Execute();

			var brushSourceTexture = PaintManager.Brush.SourceTexture;
			var previousColor = PaintManager.Brush.Color;
			var brushHardness = PaintManager.Brush.Hardness;
			PaintManager.Brush.Hardness = 1.0f;
			PaintManager.Brush.SetColor(Color.white, false, false);
			PaintManager.Brush.SetTexture(Settings.Instance.DefaultCircleBrush, true, false, false);
			Graphics.Blit(PaintManager.Brush.RenderTexture, previewTexture);
			PaintManager.Brush.SetColor(previousColor, false, false);
			PaintManager.Brush.SetTexture(brushTexture, false, false, false);
			PaintManager.Brush.SourceTexture = brushSourceTexture;
			PaintManager.Brush.Hardness = brushHardness;

			//restore preview
			preview = true;
		}

		/// <summary>
		/// Creates new brush texture
		/// </summary>
		private void UpdateRenderTexture()
		{
			if (brushTexture != null && 
			    brushTexture.width == PaintManager.Brush.SourceTexture.width && 
			    brushTexture.height == PaintManager.Brush.SourceTexture.height)
				return;

			if (brushTexture != null && brushTexture.IsCreated())
			{
				brushTexture.Release();
				brushTexture.width = PaintManager.Brush.SourceTexture.width;
				brushTexture.height = PaintManager.Brush.SourceTexture.height;
				brushTexture.Create();
			}
			else
			{
				brushTexture = RenderTextureFactory.CreateRenderTexture(PaintManager.Brush.SourceTexture);
				brushTarget = new RenderTargetIdentifier(brushTexture);
			}
		}
	}
}