using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Tools;
using XDPaint.Tools.Image.Base;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
	public class BasePaintObjectRenderer : IDisposable
	{
		public bool UseNeighborsVertices { set { lineDrawer.UseNeighborsVertices = value; } }
		public Brush Brush { get; set; }
		public IPaintTool Tool { get; set; }
		public bool InBounds { get; protected set; }
		protected Camera Camera { set { lineDrawer.Camera = value; } }

		protected Paint PaintMaterial;
		protected bool IsPaintingDone;
		protected IPaintMode PaintMode;
		protected IRenderTextureHelper RenderTextureHelper;
		private bool copySourceTextureToPaint;
		private BaseLineDrawer lineDrawer;
		private Mesh mesh;
		private Mesh quadMesh;
		private RenderTexture paintTexture;
		private CommandBufferBuilder commandBufferBuilder;
		
		public void SetPaintMode(IPaintMode paintMode)
		{
			PaintMode = paintMode;
		}
		
		protected void InitRenderer(Camera camera, Paint paint, bool copySourceTextureToPaintTexture)
		{
			mesh = new Mesh();
			PaintMaterial = paint;
			copySourceTextureToPaint = copySourceTextureToPaintTexture;
			lineDrawer = new BaseLineDrawer();
			var sourceTextureSize = new Vector2(paint.SourceTexture.width, paint.SourceTexture.height);
			lineDrawer.Init(camera, sourceTextureSize, RenderLine);
			paintTexture = RenderTextureHelper.GetTexture(RenderTarget.Paint);
			commandBufferBuilder = new CommandBufferBuilder("XDPaintObject");
			InitQuadMesh();
		}

		private void InitQuadMesh()
		{
			if (quadMesh == null)
			{
				quadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
			}
		}

		[Obsolete("Method was obsolete, please use DoDispose instead")]
		public void Destroy()
		{
			DoDispose();
		}
		
		public virtual void DoDispose()
		{
			if (commandBufferBuilder != null)
			{
				commandBufferBuilder.Release();
			}
			if (mesh != null)
			{
				UnityEngine.Object.Destroy(mesh);
			}
			if (quadMesh != null)
			{
				UnityEngine.Object.Destroy(quadMesh);
			}
		}

		protected void ClearTexture(RenderTarget target)
		{
			commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).ClearRenderTarget().Execute();
		}

		private void ClearTextureAndRender(RenderTarget target, Mesh drawMesh)
		{
			commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).ClearRenderTarget().
				DrawMesh(drawMesh, Brush.Material).Execute();
		}

		private void RenderToTexture(RenderTarget target, Mesh drawMesh)
		{
			if (!Tool.RenderToPaintTexture && target == RenderTarget.Paint)
				return;
			
			if (!Tool.RenderToInputTexture && target == RenderTarget.PaintInput)
				return;

			commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).DrawMesh(drawMesh, Brush.Material).Execute();

			//Colorize PaintInput texture
			if (target == RenderTarget.PaintInput)
			{
				commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(RenderTarget.PaintInput)).DrawMesh(drawMesh, Brush.Material, Brush.Material.passCount - 1).Execute();
			}
		}

		protected void BlitTexture()
		{
			if (PaintMaterial.SourceTexture != null && copySourceTextureToPaint)
			{
				Graphics.Blit(PaintMaterial.SourceTexture, paintTexture);
			}
		}
		
		protected void DrawPreProcess(BasePaintObject sender)
		{
			if (Tool.DrawPreProcess)
			{
				Tool.OnDrawPreProcess(sender, commandBufferBuilder.CommandBuffer, RenderTextureHelper.GetTarget(RenderTarget.Paint), PaintMaterial.Material);
			}
		}

		protected void DrawProcess(BasePaintObject sender)
		{
			if (Tool.DrawProcess)
			{
				Tool.OnDrawProcess(sender, commandBufferBuilder.CommandBuffer, RenderTextureHelper.GetTarget(RenderTarget.Combined), PaintMaterial.Material);
			}
		}

		protected void BakeInputToPaint(BasePaintObject sender)
		{
			if (Tool.BakeInputToPaint)
			{
				Tool.OnBakeInputToPaint(sender, commandBufferBuilder.CommandBuffer, RenderTextureHelper.GetTarget(RenderTarget.Paint), PaintMaterial.Material);
			}
		}

		protected void UpdateQuad(Action<Vector2> onDraw, Rect positionRect, bool isUndo = false)
		{
			quadMesh.vertices = new[]
			{
				new Vector3(positionRect.xMin, positionRect.yMax, 0),
				new Vector3(positionRect.xMax, positionRect.yMax, 0),
				new Vector3(positionRect.xMax, positionRect.yMin, 0),
				new Vector3(positionRect.xMin, positionRect.yMin, 0)
			};
			quadMesh.uv = new[] {Vector2.up, Vector2.one, Vector2.right, Vector2.zero};
			GL.LoadOrtho();
			if (Tool.RenderToPaintTexture)
			{
				RenderToTexture(PaintMode.RenderTarget, quadMesh);
			}
			if (Tool.RenderToInputTexture)
			{
				RenderToLineTexture(quadMesh);
			}
			if (!isUndo)
			{
				if (onDraw != null)
				{
					onDraw(Vector2.zero);
				}
			}
		}

		protected Vector2[] GetLinePositions(Vector2 fistPaintPos, Vector2 lastPaintPos, Triangle firstTriangle, Triangle lastTriangle)
		{
			return lineDrawer.GetLinePositions(fistPaintPos, lastPaintPos, firstTriangle, lastTriangle);
		}

		protected void RenderLine(Action<Vector2> onDraw, Vector2[] drawLine, Texture brushTexture, float brushSizeActual, float[] brushSizes, bool isUndo = false)
		{
			lineDrawer.RenderLine(onDraw, drawLine, brushTexture, brushSizeActual, brushSizes, isUndo);
		}

		private void RenderToLineTexture(Mesh renderMesh)
		{
			if (Tool.RenderToInputTexture)
			{
				if (PaintMode.UsePaintInput)
				{
					RenderToTexture(RenderTarget.PaintInput, renderMesh);
				}
				else
				{
					ClearTextureAndRender(RenderTarget.PaintInput, renderMesh);
				}
			}
		}
		
		private void RenderLine(Vector3[] positions, Vector2[] uv, int[] indices, Color[] colors)
		{
			if (mesh != null)
			{
				mesh.Clear(false);
			}
			mesh.vertices = positions;
			mesh.uv = uv;
			mesh.triangles = indices;
			mesh.colors = colors;

			if (PaintMode.UsePaintInput)
			{
				Brush.Material.SetInt(Brush.DstAlphaBlend, (int)BlendMode.One);
			}
			
			GL.LoadOrtho();
			RenderToTexture(PaintMode.RenderTarget, mesh);
			RenderToLineTexture(mesh);
		}
	}
}