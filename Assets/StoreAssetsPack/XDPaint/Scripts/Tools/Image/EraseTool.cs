using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
	[Serializable]
	public class EraseTool : BasePaintTool
	{
		public override PaintTool Type { get { return PaintTool.Erase; } }
		public override bool DrawPreview { get { return true; } }
		public override bool DrawPreProcess { get { return drawPreProcess;} }
		public override bool RenderToPaintTexture { get { return !paintMode.UsePaintInput; } }

		private readonly int[] inputToPaintPasses = { Paint.ErasePass };
		protected override int[] InputToPaintPasses { get { return inputToPaintPasses; } }

		private RenderTexture renderTexture;
		private RenderTargetIdentifier renderTarget;
		private IPaintMode paintMode;
		private bool drawPreProcess;

		public override void Enter()
		{
			base.Enter();
			drawPreProcess = true;
			PaintManager.Render();
			if (PaintManager.GetPaintMode().UsePaintInput)
			{
				InitRenderTexture();
			}
		}

		public override void Exit()
		{
			base.Exit();
			drawPreProcess = false;
			PaintManager.Render();
			PaintManager.Material.Material.SetTexture(Paint.PaintTextureShaderParam, PaintManager.GetPaintTexture());
			ReleaseRenderTexture();
		}

		public override void SetPaintMode(IPaintMode mode)
		{
			base.SetPaintMode(mode);
			paintMode = mode;
			if (paintMode.UsePaintInput)
			{
				RenderToInputTexture = true;
				if (renderTexture == null)
				{
					InitRenderTexture();
				}
				SetZeroBlending();
				PaintManager.Material.Material.SetTexture(Paint.PaintTextureShaderParam, renderTexture);
			}
			else
			{
				RenderToInputTexture = false;
				ReleaseRenderTexture();
				PaintManager.Material.Material.SetTexture(Paint.PaintTextureShaderParam, PaintManager.GetPaintTexture());
			}
		}

		public override void OnDrawPreProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			base.OnDrawPreProcess(sender, commandBuffer, rti, material);
			if (PaintManager.GetPaintMode().UsePaintInput && PaintManager.PaintObject.IsPainted)
			{
				CopyPainTextureToRenderTexture();
			}
		}

		public override void OnDrawProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			SetDefaultBlending();
			var hasSourceTexture = PaintManager.Material.SourceTexture != null && PaintManager.UseSourceTextureAsBackground;
			if (hasSourceTexture)
			{
				CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(rti).DrawMesh(QuadMesh, material, Paint.BackgroundPass, Paint.PaintPass);
			}
			else
			{
				CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(rti).DrawMesh(QuadMesh, material, Paint.PaintPass);
			}
			RenderPreview(CommandBufferBuilder);
			CommandBufferBuilder.Execute();
		}

		public override void OnBakeInputToPaint(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			base.OnBakeInputToPaint(sender, commandBuffer, rti, material);
			PaintManager.Material.Material.SetTexture(Paint.PaintTextureShaderParam, PaintManager.GetPaintTexture());
		}

		private void InitRenderTexture()
		{
			var result = PaintManager.GetResultRenderTexture();
			renderTexture = RenderTextureFactory.CreateRenderTexture(result);
			renderTarget = new RenderTargetIdentifier(renderTexture);
			CopyPainTextureToRenderTexture();
		}

		private void CopyPainTextureToRenderTexture()
		{
			var material = PaintManager.Material.Material;
			material.SetTexture(Paint.PaintTextureShaderParam, PaintManager.GetPaintTexture());
			SetDefaultBlending();
			// SetZeroBlending(); 
			CommandBufferBuilder.Clear().SetRenderTarget(renderTarget).ClearRenderTarget().DrawMesh(QuadMesh, material, Paint.PaintPass, Paint.ErasePass).Execute();
			material.SetTexture(Paint.PaintTextureShaderParam, renderTexture);
		}

		private void ReleaseRenderTexture()
		{
			if (renderTexture != null)
			{
				renderTexture.ReleaseTexture();
			}
		}
	}
}