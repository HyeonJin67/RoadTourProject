using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Tools.Image.Base
{
    [Serializable]
    public class BasePaintTool : IPaintTool
    {
        /// <summary>
        /// Type of the tool
        /// </summary>
        public virtual PaintTool Type { get { return PaintTool.Brush; } }
        protected CommandBufferBuilder CommandBufferBuilder;
        protected Mesh QuadMesh;
        protected IPaintMode PaintMode;
        private PaintManager thisPaintManager;
        private RenderTexture paintTextureCopy;

        protected PaintManager PaintManager
        {
            get { return thisPaintManager; }
            private set { thisPaintManager = value; }
        }

        public virtual bool ShowPreview { get { return thisPaintManager.Brush.Preview; } }
        public virtual bool DrawPreview { get { return false; } }
        public virtual bool RenderToPaintTexture { get; protected set; }
        public virtual bool RenderToInputTexture { get; protected set; }
        public virtual bool AllowRender { get { return RenderToPaintTexture || RenderToInputTexture || ShowPreview || DrawPreview; } }
        public virtual bool RenderToTextures { get { return RenderToPaintTexture || RenderToInputTexture; } }
        public virtual bool DrawPreProcess { get; protected set; }
        public virtual bool DrawProcess { get { return true;  } }
        public virtual bool BakeInputToPaint { get { return true; } }
        
        private readonly int[] inputToPaintPasses = { Paint.BlendPass };
        protected virtual int[] InputToPaintPasses { get { return inputToPaintPasses; } }
        
        private readonly Dictionary<string, int> defaultBlendOptions = new Dictionary<string, int>
        {
            { Paint.SrcColorBlend, (int)BlendMode.SrcAlpha }, { Paint.DstColorBlend, (int)BlendMode.OneMinusSrcAlpha },
            { Paint.SrcAlphaBlend, (int)BlendMode.SrcAlpha }, { Paint.DstAlphaBlend, (int)BlendMode.One }
        };
        protected Dictionary<string, int> DefaultBlendOptions { get { return defaultBlendOptions; } }
        
        private readonly Dictionary<string, int> zeroBlendOptions = new Dictionary<string, int>
        {
            { Paint.SrcColorBlend, (int)BlendMode.One }, { Paint.DstColorBlend, (int)BlendMode.Zero },
            { Paint.SrcAlphaBlend, (int)BlendMode.One }, { Paint.DstAlphaBlend, (int)BlendMode.Zero }
        };
        protected Dictionary<string, int> ZeroBlendOptions { get { return zeroBlendOptions; } }

        /// <summary>
        /// Enter the tool
        /// </summary>
        public virtual void Enter()
        {
            RenderToPaintTexture = true;
            RenderToInputTexture = true;
            if (CommandBufferBuilder == null)
            {
                CommandBufferBuilder = new CommandBufferBuilder("BasePaintTool");
            }
            if (QuadMesh == null)
            {
                InitQuadMesh();
            }
            if (paintTextureCopy == null)
            {
                paintTextureCopy = RenderTextureFactory.CreateRenderTexture(PaintManager.GetPaintTexture());
            }
            PaintManager.Material.Material.SetInt(Paint.SrcColorBlend, (int)BlendMode.SrcAlpha);
            PaintManager.Material.Material.SetInt(Paint.DstColorBlend, (int)BlendMode.OneMinusSrcAlpha);
            PaintManager.Material.Material.SetInt(Paint.SrcAlphaBlend, (int)BlendMode.SrcAlpha);
            PaintManager.Material.Material.SetInt(Paint.DstAlphaBlend, (int)BlendMode.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Exit from the tool
        /// </summary>
        public virtual void Exit()
        {
        }

        public virtual void DoDispose()
        {
            if (CommandBufferBuilder != null)
            {
                CommandBufferBuilder.Release();
                CommandBufferBuilder = null;
            }
            if (QuadMesh != null)
            {
                UnityEngine.Object.Destroy(QuadMesh);
                QuadMesh = null;
            }
            if (paintTextureCopy != null)
            {
                paintTextureCopy.ReleaseTexture();
            }
        }

        /// <summary>
        /// On Mouse Hover handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdateHover(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }

        /// <summary>
        /// On Mouse Down handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdateDown(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }

        /// <summary>
        /// On Mouse Press handler
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void UpdatePress(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
        {
        }

        /// <summary>
        /// On Paint Line handler (BasePaintObject.OnPaintLineHandler)
        /// </summary>
        /// <param name="paintPosition"></param>
        /// <param name="pressure"></param>
        public virtual void OnPaint(BasePaintObject sender, Vector2 paintPosition, float pressure)
        {
        }

        /// <summary>
        /// On Mouse Up handler
        /// </summary>
        public virtual void UpdateUp(BasePaintObject sender, bool inBounds)
        {
        }

        public void SetPaintManager(PaintManager paintManager)
        {
            thisPaintManager = paintManager;
        }

        public virtual void SetPaintMode(IPaintMode mode)
        {
            PaintMode = mode;
        }

        /// <summary>
        /// Pre Draw Process handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="commandBuffer"></param>
        /// <param name="rti"></param>
        /// <param name="material"></param>
        public virtual void OnDrawPreProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti,
            Material material)
        {
            DrawPreProcess = false;
        }

        /// <summary>
        /// Draw Process handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="commandBuffer"></param>
        /// <param name="rti"></param>
        /// <param name="material"></param>
        public virtual void OnDrawProcess(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti,
            Material material)
        {
            var hasSourceTexture = PaintManager.Material.SourceTexture != null;
            var useBackground = hasSourceTexture && PaintManager.UseSourceTextureAsBackground;
            var firstPass = useBackground ? 0 : 1;
            SetDefaultBlending();
            CommandBufferBuilder.Clear().LoadOrtho().SetRenderTarget(rti);
            if (sender.IsPainting)
            {
                if (useBackground)
                {
                    CommandBufferBuilder.DrawMesh(QuadMesh, material, Paint.BackgroundPass, Paint.BlendPass);
                }
                else
                {
                    CommandBufferBuilder.DrawMesh(QuadMesh, material, Paint.BlendPass);
                }
            }
            else
            {
                for (var i = firstPass; i < Paint.BlendPass; i++)
                {
                    CommandBufferBuilder.DrawMesh(QuadMesh, material, i);
                }
                RenderPreview(CommandBufferBuilder);
            }
            CommandBufferBuilder.Execute();
        }

        protected void SetDefaultBlending()
        {
            var material = PaintManager.Material.Material;
            foreach (var blendOption in DefaultBlendOptions)
            {
                material.SetInt(blendOption.Key, blendOption.Value);
            }
        }

        protected void SetZeroBlending()
        {
            var material = PaintManager.Material.Material;
            foreach (var blendOption in ZeroBlendOptions)
            {
                material.SetInt(blendOption.Key, blendOption.Value);
            }
        }

        protected void RenderPreview(CommandBufferBuilder cbb)
        {
            if (ShowPreview && PaintManager.PaintObject.InBounds)
            {
                cbb.DrawMesh(QuadMesh, PaintManager.Material.Material, PaintManager.Material.Material.passCount - 1);
            }
        }

        public virtual void OnBakeInputToPaint(BasePaintObject sender, CommandBuffer commandBuffer, RenderTargetIdentifier rti,
            Material material)
        {
            Graphics.Blit(PaintManager.GetPaintTexture(), paintTextureCopy);
            material.SetTexture(Paint.PaintTextureShaderParam, paintTextureCopy);
            
            var sourceColor = material.GetInt(Paint.SrcColorBlend);
            var destColor = material.GetInt(Paint.DstColorBlend);
            var sourceAlpha = material.GetInt(Paint.SrcAlphaBlend);
            var destAlpha = material.GetInt(Paint.DstAlphaBlend);
            SetZeroBlending();
            
            CommandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(rti).DrawMesh(QuadMesh, material, InputToPaintPasses).Execute();

            material.SetTexture(Paint.PaintTextureShaderParam, PaintManager.GetPaintTexture());

            material.SetInt(Paint.SrcColorBlend, sourceColor);
            material.SetInt(Paint.DstColorBlend, destColor);
            material.SetInt(Paint.SrcAlphaBlend, sourceAlpha);
            material.SetInt(Paint.DstAlphaBlend, destAlpha);
        }
        
        /// <summary>
        /// On Undo handler
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnUndo(BasePaintObject sender)
        {
            RenderPaintObject(sender);
        }

        /// <summary>
        /// On Redo handler
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnRedo(BasePaintObject sender)
        {
            RenderPaintObject(sender);
        }

        private void RenderPaintObject(BasePaintObject sender)
        {
            if (sender == null)
                return;
            var previousRenderToPaintTexture = RenderToPaintTexture;
            RenderToPaintTexture = true;
            sender.Render();
            RenderToPaintTexture = previousRenderToPaintTexture;
        }
        
        private void InitQuadMesh()
        {
            if (QuadMesh == null)
            {
                QuadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
            }
        }
    }
}