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
    public class CloneTool : BasePaintTool
    {
	    public override PaintTool Type { get { return PaintTool.Clone;} }
	    public override bool ShowPreview { get { return preview; } }
	    public override bool RenderToPaintTexture { get { return false;} }
	    public override bool RenderToInputTexture { get { return renderToLine;} }

	    public bool CopyTextureOnPressDown = true;

	    private CloneData cloneData;
	    private Material brushMaterial;
	    private RenderTexture brushTexture;
	    private RenderTexture previewTexture;
		private RenderTargetIdentifier brushTarget;
		private RenderTexture inputTextureCopy;
		private bool renderToLine;
		private bool preview;
		private bool initialized;

		private const int PaddingPixels = 0;
		private const string BrushTexParam = "_BrushTex";
		private const string BrushMaskTexParam = "_MaskTex";
		private const string BrushOffsetShaderParam = "_BrushOffset";
		private const string BrushHardnessParam = "_Hardness";

		public override void Enter()
		{
			RenderToInputTexture = false;
			preview = PaintManager.Brush.Preview;
			base.Enter();
			PaintManager.Render();
			cloneData = new CloneData();
			cloneData.Enter(PaintManager);
			InitBrushMaterial();
			UpdateBrushPreview();
			PaintManager.Material.Material.SetTexture(Paint.InputTextureShaderParam, PaintManager.GetPaintInputTexture());
			if (inputTextureCopy == null)
			{
				inputTextureCopy = RenderTextureFactory.CreateRenderTexture(PaintManager.GetPaintInputTexture());
			}
			cloneData.CloneMaterial.SetTexture(CloneData.MaskTexParam, inputTextureCopy);
			initialized = true;
		}

		public override void Exit()
		{
			initialized = false;
			base.Exit();
			if (cloneData != null)
			{
				cloneData.Exit();
				cloneData = null;
			}
			if (brushTexture != null)
			{
				brushTexture.ReleaseTexture();
			}
			if (previewTexture != null)
			{
				previewTexture.ReleaseTexture();
			}
			PaintManager.Brush.Render();
			PaintManager.Material.Material.SetTexture(Paint.BrushTextureShaderParam, PaintManager.Brush.RenderTexture);
			if (inputTextureCopy != null)
			{
				inputTextureCopy.ReleaseTexture();
				inputTextureCopy = null;
			}
		}

		public override void UpdateHover(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdateHover(sender, uv, paintPosition, pressure);
			if (cloneData.ClickCount > 1 && PaintManager.Brush.Preview && (cloneData.PrevPaintPosition != paintPosition || cloneData.PrevUV != uv || cloneData.PrevPressure != pressure))
			{
				//render new brush
				var paintOffset = paintPosition - cloneData.PaintPosition;
				var brushOffset = GetPreviewVector(cloneData.PaintManager, paintOffset, pressure);
				brushMaterial.SetVector(BrushOffsetShaderParam, brushOffset);
				UpdateBrushRenderTexture();
				preview = false;
				RenderBrush();
				preview = true;
				cloneData.PrevUV = uv;
				cloneData.PrevPaintPosition = paintPosition;
				cloneData.PrevPressure = pressure;
			}
		}

		public override void UpdateDown(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdateDown(sender, uv, paintPosition, pressure);
			preview = true;
			renderToLine = false;
		}

		public override void UpdatePress(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			base.UpdatePress(sender, uv, paintPosition, pressure);
			if (cloneData.IsUp)
			{
				if (CopyTextureOnPressDown)
				{
					preview = false;
					cloneData.PaintManager.Render();
					preview = true;
					Graphics.Blit(cloneData.PaintManager.GetResultRenderTexture(), cloneData.CopiedTexture);
				}
				if (cloneData.ClickCount == 1)
				{
					cloneData.UVSecond = uv;
					cloneData.PaintPositionSecond = paintPosition;
					cloneData.UV = cloneData.UVFirst - cloneData.UVSecond;
					cloneData.PaintPosition = cloneData.PaintPositionSecond - cloneData.PaintPositionFirst;
				}
				cloneData.IsUp = false;
			}

			if (cloneData.IsFirstClick)
			{
				//render new brush
				var brushOffset = GetPreviewVector(cloneData.PaintManager, paintPosition, pressure);
				brushMaterial.SetVector(BrushOffsetShaderParam, brushOffset);
				UpdateBrushRenderTexture();
				RenderBrush();
				cloneData.PaintPositionFirst = paintPosition;
				cloneData.UVFirst = uv;
				UpdateRenderTexture();
				return;
			}

			renderToLine = true;
			cloneData.UpdateMaterial(cloneData.UV);
		}

		public override void UpdateUp(BasePaintObject sender, bool inBounds)
		{
			base.UpdateUp(sender, inBounds);
			if (inBounds)
			{
				renderToLine = true;
				cloneData.IsUp = true;
				preview = false;
				cloneData.IsFirstClick = false;
				cloneData.ClickCount++;
			}
		}

		#region Initialization

		private void InitBrushMaterial()
		{
			if (brushMaterial == null)
			{
				brushMaterial = new Material(Settings.Instance.BrushSamplerShader);
			}
			brushMaterial.mainTexture = PaintManager.GetResultRenderTexture();
			brushMaterial.SetFloat(BrushHardnessParam, PaintManager.Brush.Hardness);
		}
		
		private void UpdateBrushRenderTexture()
		{
			if (brushTexture != null && brushTexture.width == PaintManager.Brush.SourceTexture.width && brushTexture.height == PaintManager.Brush.SourceTexture.height)
				return;

			brushTexture = RenderTextureFactory.CreateRenderTexture(PaintManager.Brush.SourceTexture);
			brushTarget = new RenderTargetIdentifier(brushTexture);
			brushMaterial.SetTexture(BrushTexParam, brushTexture);
			brushMaterial.SetTexture(BrushMaskTexParam, PaintManager.Brush.SourceTexture);
			
		}
		private void UpdateRenderTexture()
		{
			var renderTexture = cloneData.PaintManager.GetResultRenderTexture();
			if (cloneData.CopiedTexture != null && cloneData.CopiedTexture.width - PaddingPixels == renderTexture.width && cloneData.CopiedTexture.height - PaddingPixels == renderTexture.height) 
				return;

			preview = false;
			cloneData.PaintManager.Render();
			preview = true;
			cloneData.CopiedTexture = RenderTextureFactory.CreateTemporaryRenderTexture(renderTexture.width + PaddingPixels, 
				renderTexture.height + PaddingPixels, 0, renderTexture.format, renderTexture.filterMode, 
				renderTexture.wrapMode, renderTexture.autoGenerateMips, renderTexture.useMipMap, renderTexture.anisoLevel);
			Graphics.Blit(renderTexture, cloneData.CopiedTexture);
		}

		#endregion

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
			PaintManager.Brush.SetColor(previousColor, false, false);
			PaintManager.Brush.SetTexture(brushSourceTexture, false, false);
			PaintManager.Brush.Hardness = brushHardness;
		}

		private Vector4 GetPreviewVector(PaintManager paintManager, Vector2 paintPosition, float pressure)
		{
			var brushRatio = new Vector2(
				paintManager.Material.SourceTexture.width / PaintManager.Brush.SourceTextureSize.x + PaddingPixels,
				paintManager.Material.SourceTexture.height / PaintManager.Brush.SourceTextureSize.y + PaddingPixels) / PaintManager.Brush.Size / pressure;
			var brushOffset = new Vector4(
				paintPosition.x / paintManager.Material.SourceTexture.width * brushRatio.x,
				paintPosition.y / paintManager.Material.SourceTexture.height * brushRatio.y,
				1f / brushRatio.x, 1f / brushRatio.y);
			return brushOffset;
		}
		
		private void RenderBrush()
		{
			PaintManager.Render();
			brushMaterial.mainTexture = PaintManager.GetResultRenderTexture();
			CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(brushTarget).ClearRenderTarget().DrawMesh(QuadMesh, brushMaterial).Execute();
			
			var brushSourceTexture = PaintManager.Brush.SourceTexture;
			PaintManager.Brush.SetTexture(brushTexture, true, false, false);
			PaintManager.Brush.SetTexture(brushSourceTexture, false, false, false);
		}

		private void Render()
		{
			Graphics.Blit(PaintManager.GetPaintInputTexture(), inputTextureCopy);
			CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(PaintManager.GetPaintInputTexture()).DrawMesh(QuadMesh, cloneData.CloneMaterial).Execute();
		}
		
		public override void OnDrawProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			if (!initialized)
			{
				base.OnDrawProcess(sender, commandBuffer, rti, material);
				return;
			}
			
			Render();
			base.OnDrawProcess(sender, commandBuffer, rti, material);
			if (!PaintManager.GetPaintMode().UsePaintInput)
			{
				OnBakeInputToPaint(sender, commandBuffer, PaintManager.GetPaintTexture(), material);
			}
		}

		[Serializable]
	    private class CloneData
	    {
		    public PaintManager PaintManager;
		    public RenderTexture CopiedTexture;
		    public Material CloneMaterial;
		    public Vector2 UVFirst;
		    public Vector2 UVSecond;
		    public Vector2 UV;
		    public Vector2 PaintPositionFirst;
		    public Vector2 PaintPositionSecond;
		    public Vector2 PaintPosition;
		    public Vector2 PrevUV = -Vector2.one;
		    public Vector2 PrevPaintPosition = -Vector2.one;
		    public float PrevPressure = -1f;
		    public int ClickCount;
		    public bool IsUp;
		    public bool IsFirstClick = true;
		
		    public const string MaskTexParam = "_MaskTex";
		    private const string OffsetParam = "_Offset";

		    public void Enter(PaintManager paintManager)
		    {
			    PaintManager = paintManager;
			    InitMaterial();
		    }
		
		    public void Exit()
		    {
			    ClickCount = 0;
			    IsFirstClick = true;
			    IsUp = false;
			    if (CopiedTexture != null)
			    {
				    RenderTexture.ReleaseTemporary(CopiedTexture);
			    }
			    if (CloneMaterial != null)
			    {
				    Object.Destroy(CloneMaterial);
			    }
		    }
		
		    private void InitMaterial()
		    {
			    if (CloneMaterial == null)
			    {
				    CloneMaterial = new Material(Settings.Instance.BrushCloneShader);
			    }
			    CloneMaterial.mainTexture = PaintManager.GetResultRenderTexture();
			    CloneMaterial.SetTexture(MaskTexParam, PaintManager.GetPaintInputTexture());
		    }

		    public void UpdateMaterial(Vector2 uv)
		    {
			    CloneMaterial.mainTexture = CopiedTexture;
			    CloneMaterial.SetVector(OffsetParam, uv);
		    }
	    }
    }
}