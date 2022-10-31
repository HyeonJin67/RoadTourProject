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
	public class GaussianBlurTool : BasePaintTool
    {
	    public override PaintTool Type { get { return PaintTool.BlurGaussian; } }
	    public override bool ShowPreview { get { return false; } }
	    public override bool RenderToPaintTexture { get { return false; } }
	    public override bool RenderToInputTexture { get { return true; } }
	    public override bool DrawPreProcess { get { return true; } }

	    #region Blur settings

        public int KernelSize = 3;
        public float Spread = 5f;

        #endregion

        private BlurData blurData;
		private bool initialized;
		private const string KernelSizeParam = "_KernelSize";
		private const string SpreadParam = "_Spread";

		public override void Enter()
		{
			base.Enter();
			PaintManager.Render();
			blurData = new BlurData();
			blurData.Enter(PaintManager);
			UpdateRenderTextures();
			initialized = true;
		}

		public override void Exit()
		{
			PaintManager.Material.Material.SetTexture(Paint.InputTextureShaderParam, PaintManager.GetPaintInputTexture());
			initialized = false;
			base.Exit();
			if (blurData != null)
			{
				blurData.Exit();
				blurData = null;
			}
		}

		#region Initialization

		private void UpdateRenderTextures()
		{
			if (PaintManager == null || !PaintManager.Initialized || blurData.BlurTexture != null)
				return;
			
			var renderTexture = PaintManager.GetPaintTexture();
			blurData.BlurTexture = RenderTextureFactory.CreateRenderTexture(renderTexture);
			blurData.BlurTarget = new RenderTargetIdentifier(blurData.BlurTexture);
			CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(blurData.BlurTarget).ClearRenderTarget().Execute();
			blurData.PreBlurTexture = RenderTextureFactory.CreateRenderTexture(renderTexture);
			blurData.PreBlurTarget = new RenderTargetIdentifier(blurData.PreBlurTexture);
			CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(blurData.PreBlurTarget).ClearRenderTarget().Execute();
			blurData.InitMaterials();
		}

		#endregion

		private void Blur(Material blurMaterial, RenderTexture source, RenderTexture destination)
		{
			if (blurMaterial != null)
			{
				blurMaterial.SetFloat(KernelSizeParam, KernelSize);
				blurMaterial.SetFloat(SpreadParam, Spread);
				Graphics.Blit(source, destination, blurMaterial, 0);
			}
			else
			{
				Graphics.Blit(source, destination);
			}
		}

		private void Render()
		{
			blurData.MaskMaterial.color = PaintManager.Brush.Color;
			//clear render texture
			CommandBufferBuilder.Clear().LoadOrtho().SetRenderTarget(blurData.PreBlurTarget).ClearRenderTarget().Execute();
			//blur
			Blur(blurData.BlurMaterial, PaintManager.GetResultRenderTexture(), blurData.PreBlurTexture);
			//render with mask
			CommandBufferBuilder.Clear().SetRenderTarget(blurData.BlurTarget).ClearRenderTarget().DrawMesh(QuadMesh, blurData.MaskMaterial).Execute();
		}

		public override void OnDrawPreProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			base.OnDrawPreProcess(sender, commandBuffer, rti, material);
			if (sender.IsPainted)
			{
				Render();
			}
		}

		public override void OnDrawProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			if (!initialized)
			{
				base.OnDrawProcess(sender, commandBuffer, rti, material);
				CommandBufferBuilder.Clear().SetRenderTarget(PaintManager.GetPaintInputTexture()).ClearRenderTarget().Execute();
				return;
			}
			
			material.SetTexture(Paint.InputTextureShaderParam, blurData.BlurTexture);
			base.OnDrawProcess(sender, commandBuffer, rti, material);
			OnBakeInputToPaint(sender, commandBuffer, PaintManager.GetPaintTexture(), material);
		}
		
		public override void OnBakeInputToPaint(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti, Material material)
		{
			base.OnBakeInputToPaint(sender, commandBuffer, rti, material);
			material.SetTexture(Paint.InputTextureShaderParam, PaintManager.GetPaintInputTexture());
			CommandBufferBuilder.Clear().SetRenderTarget(PaintManager.GetPaintInputTexture()).ClearRenderTarget().Execute();
		}

		[Serializable]
		private class BlurData
		{
			public Material BlurMaterial;
			public Material MaskMaterial;
			public RenderTexture BlurTexture;
			public RenderTargetIdentifier BlurTarget;
			public RenderTexture PreBlurTexture;
			public RenderTargetIdentifier PreBlurTarget;
			
			private PaintManager paintManager;
			private const string MaskTexParam = "_MaskTex";
		
			public void Enter(PaintManager paintManager)
			{
				this.paintManager = paintManager;
			}
		
			public void Exit()
			{
				if (PreBlurTexture != null)
				{
					PreBlurTexture.ReleaseTexture();
				}
				if (BlurTexture != null)
				{
					BlurTexture.ReleaseTexture();
				}
				if (BlurMaterial != null)
				{
					Object.Destroy(BlurMaterial);
				}
				BlurMaterial = null;
				if (MaskMaterial != null)
				{
					Object.Destroy(MaskMaterial);
				}
				MaskMaterial = null;
			}
		
			public void InitMaterials()
			{
				if (MaskMaterial == null)
				{
					MaskMaterial = new Material(Settings.Instance.BrushBlurShader);
				}
				MaskMaterial.mainTexture = PreBlurTexture;
				MaskMaterial.SetTexture(MaskTexParam, paintManager.GetPaintInputTexture());
				MaskMaterial.color = paintManager.Brush.Color;
				if (BlurMaterial == null)
				{
					BlurMaterial = new Material(Settings.Instance.GaussianBlurShader);
				}
				BlurMaterial.mainTexture = BlurTexture;
			}
		}
    }
}