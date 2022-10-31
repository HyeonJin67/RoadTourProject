using System;
using System.Linq;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.PaintObject.Base;
#if XDP_DEBUG
using XDPaint.Tools.Image;
#endif
using XDPaint.Tools.Image.Base;
using IDisposable = XDPaint.Core.IDisposable;

namespace XDPaint.Tools
{
	[Serializable]
	public class ToolsManager : IDisposable
	{
		public IPaintTool CurrentTool { get { return currentTool; } }
		private IPaintTool[] allTools;
		private IPaintTool currentTool;
		private PaintManager paintManager;
		private bool initialized;
		
#if XDP_DEBUG
#pragma warning disable 414
		[SerializeField] private BasePaintTool basePaintTool;
		[SerializeField] private EraseTool eraseTool;
		[SerializeField] private EyedropperTool eyedropperTool;
		[SerializeField] private BrushSamplerTool brushSamplerTool;
		[SerializeField] private CloneTool cloneTool;
		[SerializeField] private BlurTool blurTool;
		[SerializeField] private GaussianBlurTool gaussianBlurTool;
#pragma warning restore 414
#endif

		public ToolsManager(PaintManager paintManager, PaintTool paintTool)
		{
			this.paintManager = paintManager;
			var type = typeof(IPaintTool);
			var tools = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
				s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass);
			var toolsArray = tools.ToArray();
			allTools = new IPaintTool[toolsArray.Length];
			for (var i = 0; i < toolsArray.Length; i++)
			{
				var tool = toolsArray[i];
				var toolInstance = Activator.CreateInstance(tool) as IPaintTool;
				allTools[i] = toolInstance;
				toolInstance.SetPaintManager(this.paintManager);
			}
			currentTool = allTools.First(x => x.Type == paintTool);
			currentTool.Enter();
#if XDP_DEBUG
			basePaintTool = allTools.First(x => x.Type == PaintTool.Brush) as BasePaintTool;
			eraseTool = allTools.First(x => x.Type == PaintTool.Erase) as EraseTool;
			eyedropperTool = allTools.First(x => x.Type == PaintTool.Eyedropper) as EyedropperTool;
			brushSamplerTool = allTools.First(x => x.Type == PaintTool.BrushSampler) as BrushSamplerTool;
			cloneTool = allTools.First(x => x.Type == PaintTool.Clone) as CloneTool;
			blurTool = allTools.First(x => x.Type == PaintTool.Blur) as BlurTool;
			gaussianBlurTool = allTools.First(x => x.Type == PaintTool.BlurGaussian) as GaussianBlurTool;
#endif
			initialized = true;
		}

		public void Init()
		{
			paintManager.PaintObject.OnPaintHandler -= Paint;
			paintManager.PaintObject.OnPaintHandler += Paint;
			paintManager.PaintObject.OnMouseHoverHandler -= OnMouseHover;
			paintManager.PaintObject.OnMouseHoverHandler += OnMouseHover;
			paintManager.PaintObject.OnMouseDownHandler -= OnMouseDown;
			paintManager.PaintObject.OnMouseDownHandler += OnMouseDown;
			paintManager.PaintObject.OnMouseHandler -= OnMouse;
			paintManager.PaintObject.OnMouseHandler += OnMouse;
			paintManager.PaintObject.OnMouseUpHandler -= OnMouseUp;
			paintManager.PaintObject.OnMouseUpHandler += OnMouseUp;
			paintManager.PaintObject.OnUndoHandler -= OnUndo;
			paintManager.PaintObject.OnUndoHandler += OnUndo;
			paintManager.PaintObject.OnRedoHandler -= OnRedo;
			paintManager.PaintObject.OnRedoHandler += OnRedo;
		}

		public void DoDispose()
		{
			if (!initialized)
				return;
			
			foreach (var tool in allTools)
			{
				if (currentTool == tool)
				{
					tool.Exit();
				}
				tool.DoDispose();
			}
			allTools = null;
			initialized = false;
		}

		public void SetTool(PaintTool newTool)
		{
			foreach (var tool in allTools)
			{
				if (tool.Type == newTool)
				{
					currentTool.Exit();
					currentTool = tool;
					currentTool.Enter();
					break;
				}
			}
		}

		private void Paint(BasePaintObject sender, Vector2 paintPosition, float pressure)
		{			
			currentTool.OnPaint(sender, paintPosition, pressure);
		}
		
		private void OnMouseHover(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			currentTool.UpdateHover(sender, uv, paintPosition, pressure);
		}

		private void OnMouseDown(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			currentTool.UpdateDown(sender, uv, paintPosition, pressure);
		}
		
		private void OnMouse(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure)
		{
			currentTool.UpdatePress(sender, uv, paintPosition, pressure);
		}
		
		private void OnMouseUp(BasePaintObject sender, bool inBounds)
		{
			currentTool.UpdateUp(sender, inBounds);
		}
		
		private void OnUndo(BasePaintObject sender)
		{
			currentTool.OnUndo(sender);
		}
		
		private void OnRedo(BasePaintObject sender)
		{
			currentTool.OnRedo(sender);
		}
	}
}