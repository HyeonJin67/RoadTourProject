using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Tools;

namespace XDPaint.Controllers
{
	public class PaintController : Singleton<PaintController>
	{
		[Obsolete("ToolsManager was removed from PaintController, please use PaintManager.ToolsManager instead", true)]
		public ToolsManager ToolsManager
		{
			get { return null; }
		}
		
		private List<IPaintMode> paintModes;
		[SerializeField] private PaintMode paintModeType;
		[SerializeField] private bool useSharedSettings = true;
		public bool UseSharedSettings
		{
			get { return useSharedSettings; }
			set
			{
				useSharedSettings = value;
				if (!initialized)
					return;
				
				if (useSharedSettings)
				{
					foreach (var paintManager in allPaintManagers)
					{
						if (paintManager == null)
							continue;
						paintManager.Brush = brush;
						paintManager.Tool = paintTool;
						paintManager.UpdatePreviewInput();
						paintManager.SetPaintMode(paintModeType);
					}
				}
				else
				{
					foreach (var paintManager in allPaintManagers)
					{
						if (paintManager == null)
							continue;
						paintManager.InitBrush();
						paintManager.Tool = paintTool;
						paintManager.UpdatePreviewInput();
					}
				}
			}
		}
		
		public PaintMode PaintMode
		{
			get { return paintModeType; }
			set
			{
				var previousModeType = paintModeType;
				paintModeType = value;
				mode = GetPaintMode(paintModeType);
				if (Application.isPlaying && paintModeType != previousModeType && useSharedSettings)
				{
					foreach (var paintManager in allPaintManagers)
					{
						if (paintManager == null)
							continue;
						paintManager.SetPaintMode(paintModeType);
					}
				}
			}
		}

		[SerializeField] private PaintTool paintTool;
		public PaintTool Tool
		{
			get
			{
				return paintTool;
			}
			set
			{
				paintTool = value;
				if (initialized && useSharedSettings)
				{
					foreach (var paintManager in allPaintManagers)
					{
						if (paintManager == null)
							continue;
						paintManager.Tool = paintTool;
					}
				}
			}
		}

		[SerializeField] private Brush brush = new Brush();
		public Brush Brush
		{
			get { return brush; }
			set
			{
				// brush = value;
				brush.SetValues(value);
			}
		}

		private List<PaintManager> allPaintManagers;
		private IPaintMode mode;
		private bool initialized;

		private new void Awake()
		{
			base.Awake();
			allPaintManagers = new List<PaintManager>();
			CreatePaintModes();
			Init();
		}

		private void CreatePaintModes()
		{
			if (paintModes == null)
			{
				paintModes = new List<IPaintMode>();
				var type = typeof(IPaintMode);
				var types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => type.IsAssignableFrom(p) && p.IsClass);
				foreach (var modeType in types)
				{
					var paintMode = Activator.CreateInstance(modeType) as IPaintMode;
					paintModes.Add(paintMode);
				}
			}
		}

		private void Init()
		{
			if (Application.isPlaying && !initialized)
			{
				mode = GetPaintMode(paintModeType);
				if (brush.SourceTexture == null)
				{
					brush.SourceTexture = Settings.Instance.DefaultBrush;
				}
				brush.Init(mode);
				brush.SetPaintMode(mode);
				brush.SetPaintTool(paintTool);
				initialized = true;
			}
		}

		[Obsolete("This method was obsolete, please use RegisterPaintManager instead", true)]
		public void Init(PaintManager paintManager)
		{
		}

		public IPaintMode GetPaintMode(PaintMode paintMode)
		{
			if (paintModes == null)
			{
				CreatePaintModes();
			}
			return paintModes.FirstOrDefault(x => x.PaintMode == paintMode);
		}

		public void RegisterPaintManager(PaintManager paintManager)
		{
			UnRegisterPaintManager(paintManager);
			allPaintManagers.Add(paintManager);
		}

		public void UnRegisterPaintManager(PaintManager paintManager)
		{
			if (allPaintManagers.Contains(paintManager))
			{
				allPaintManagers.Remove(paintManager);
			}
		}

		private void OnDestroy()
		{
			brush.DoDispose();
		}

		public PaintManager[] ActivePaintManagers()
		{
			return allPaintManagers.Where(paintManager => paintManager != null && paintManager.gameObject.activeInHierarchy && paintManager.enabled && paintManager.Initialized).ToArray();
		}

		public PaintManager[] AllPaintManagers()
		{
			return allPaintManagers.Where(paintManager => paintManager != null).ToArray();
		}
	}
}