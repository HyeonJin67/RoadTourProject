using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Demo.UI;
using XDPaint.Tools;
using XDPaint.Tools.Image;

namespace XDPaint.Demo
{
	public class Demo : MonoBehaviour
	{
		[Serializable]
		public class PaintManagersData
		{
			public PaintManager PaintManager;
			public string Text;
			public bool RequiresOrthographicCamera;
		}
		
		[Serializable]
		public class PaintItem
		{
			public Image Image;
			public Button Button;
		}
		
		public PaintManagersData[] PaintManagers;
		public CameraMover CameraMover;
		public Camera MainCamera;

		public GameObject TutorialObject;
		public EventTrigger Tutorial;
		
		[Header("Top panel")]
		public Button TutorialButton;
		public Toggle BrushTool;
		public ToggleDoubleClick BrushToolDoubleClick;
		public Toggle EraseTool;
		public ToggleDoubleClick EraseToolDoubleClick;
		public Toggle EyedropperTool;
		public Toggle BrushSamplerTool;
		public Toggle CloneTool;
		public Toggle BlurTool;
		public Toggle GaussianBlurTool;
		public ToggleDoubleClick BlurToolDoubleClick;
		public ToggleDoubleClick GaussianBlurToolDoubleClick;
		public Toggle RotateToggle;
		public Toggle PlayPauseToggle;
		public RawImage BrushPreview;
		public RectTransform BrushPreviewTransform;
		public Button BrushButton;
		public EventTrigger TopPanel;
		public EventTrigger ColorPalette;
		public EventTrigger BrushesPanel;
		public EventTrigger BlurPanel;
		public EventTrigger GaussianBlurPanel;
		public Slider BlurSlider;
		public Slider GaussianBlurSlider;
		public PaintItem[] Colors;
		public PaintItem[] Brushes;

		[Header("Right panel")]
		public Slider OpacitySlider;
		public Slider BrushSizeSlider;
		public Slider HardnessSlider;
		public Button UndoButton;
		public Button RedoButton;
		public EventTrigger RightPanel;

		[Header("Bottom panel")]
		public Button NextButton;
		public Button PreviousButton;
		public Text BottomPanelText;
		public EventTrigger BottomPanel;

		public EventTrigger AllArea;
		
		private PaintManager PaintManager { get { return PaintManagers[currentPaintManagerId].PaintManager; } }

		private Texture selectedTexture;
		private Animator paintManagerAnimator;
		private PaintTool previousTool;
		private int currentPaintManagerId;
		private const int TutorialShowCount = 3;
		
		void Awake()
		{
#if !UNITY_WEBGL
			Application.targetFrameRate = 60;
#endif
			if (MainCamera == null)
			{
				MainCamera = Camera.main;
			}

			selectedTexture = Settings.Instance.DefaultBrush;
			InitPaintManagers();
			UpdateButtons();

			foreach (var paintManager in PaintManagers)
			{
				paintManager.PaintManager.gameObject.SetActive(false);
			}
						
			PaintManager.OnInitialized += OnInitialized;
			
			//tutorial
			var tutorialShowsCount = PlayerPrefs.GetInt("XDPaintDemoTutorialShowsCount", 0);
			var tutorialClick = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
			tutorialClick.callback.AddListener(delegate
			{
				PlayerPrefs.SetInt("XDPaintDemoTutorialShowsCount", tutorialShowsCount + 1);
				OnTutorial(false);
			});				
			Tutorial.triggers.Add(tutorialClick);
			if (tutorialShowsCount < TutorialShowCount)
			{
				TutorialObject.gameObject.SetActive(true);
				InputController.Instance.enabled = false;
			}
			else
			{
				OnTutorial(false);
			}
			
			var hoverEnter = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
			hoverEnter.callback.AddListener(HoverEnter);
			var hoverExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
			hoverExit.callback.AddListener(HoverExit);
			
			//top panel
			TutorialButton.onClick.AddListener(delegate { OnTutorial(true); });
			BrushTool.onValueChanged.AddListener(SetBrushTool);
			BrushToolDoubleClick.OnDoubleClick.AddListener(OpenBrushPanel);
			EraseTool.onValueChanged.AddListener(SetEraseTool);
			EraseToolDoubleClick.OnDoubleClick.AddListener(OpenBrushPanel);
			EyedropperTool.onValueChanged.AddListener(SetEyedropperTool);
			BrushSamplerTool.onValueChanged.AddListener(SetBrushSamplerTool);
			CloneTool.onValueChanged.AddListener(SetCloneTool);
			BlurTool.onValueChanged.AddListener(SetBlurTool);
			BlurToolDoubleClick.OnDoubleClick.AddListener(OpenBlurPanel);
			GaussianBlurTool.onValueChanged.AddListener(SetGaussianBlurTool);
			GaussianBlurToolDoubleClick.OnDoubleClick.AddListener(OpenGaussianBlurPanel);
			RotateToggle.onValueChanged.AddListener(SetRotateMode);
			PlayPauseToggle.onValueChanged.AddListener(OnPlayPause);
			BrushButton.onClick.AddListener(OpenColorPalette);
			TopPanel.triggers.Add(hoverEnter);
			TopPanel.triggers.Add(hoverExit);
			ColorPalette.triggers.Add(hoverEnter);
			ColorPalette.triggers.Add(hoverExit);
			BrushesPanel.triggers.Add(hoverEnter);
			BrushesPanel.triggers.Add(hoverExit);
			BlurPanel.triggers.Add(hoverEnter);
			BlurPanel.triggers.Add(hoverExit);
			BlurSlider.onValueChanged.AddListener(OnBlurSlider);
			GaussianBlurPanel.triggers.Add(hoverEnter);
			GaussianBlurPanel.triggers.Add(hoverExit);
			GaussianBlurSlider.onValueChanged.AddListener(OnGaussianBlurSlider);
			
			BrushSizeSlider.value = PaintController.Instance.Brush.Size;
			HardnessSlider.value = PaintController.Instance.Brush.Hardness;
			OpacitySlider.value = PaintController.Instance.Brush.Color.a;

			//right panel
			OpacitySlider.onValueChanged.AddListener(OnOpacitySlider);
			BrushSizeSlider.onValueChanged.AddListener(OnBrushSizeSlider);
			HardnessSlider.onValueChanged.AddListener(OnHardnessSlider);
			UndoButton.onClick.AddListener(OnUndo);
			RedoButton.onClick.AddListener(OnRedo);
			RightPanel.triggers.Add(hoverEnter);
			RightPanel.triggers.Add(hoverExit);
			
			//bottom panel
			NextButton.onClick.AddListener(delegate { SwitchPaintManager(true); });
			PreviousButton.onClick.AddListener(delegate { SwitchPaintManager(false); });
			BottomPanel.triggers.Add(hoverEnter);
			BottomPanel.triggers.Add(hoverExit);	
			
			var onDown = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};
			onDown.callback.AddListener(ResetPlates);
			AllArea.triggers.Add(onDown);

			//colors
			foreach (var colorItem in Colors)
			{
				colorItem.Button.onClick.AddListener(delegate { ColorClick(colorItem.Image.color); });
			}
			
			//brushes
			for (var i = 0; i < Brushes.Length; i++)
			{
				var brushItem = Brushes[i];
				var brushId = i;
				brushItem.Button.onClick.AddListener(delegate { BrushClick(brushItem.Image.mainTexture, brushId); });
			}

			LoadPrefs();
		}

		private void OnInitialized(PaintManager paintManager)
		{
			//undo/redo status
			paintManager.PaintObject.TextureKeeper.OnUndoStatusChanged += OnUndoStatusChanged;
			paintManager.PaintObject.TextureKeeper.OnRedoStatusChanged += OnRedoStatusChanged;

			BrushPreview.texture = PaintController.Instance.UseSharedSettings 
				? PaintController.Instance.Brush.RenderTexture 
				: PaintManager.Brush.RenderTexture;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			PaintController.Instance.Brush.Preview = false;
#endif
		}

		private void LoadPrefs()
		{
			//brush id
			var brushId = PlayerPrefs.GetInt("XDPaintDemoBrushId");
			PaintController.Instance.Brush.SetTexture(Brushes[brushId].Image.mainTexture, true, false);
			selectedTexture = Brushes[brushId].Image.mainTexture;
			//opacity
			OpacitySlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushOpacity", 1f);
			//size
			BrushSizeSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushSize", 0.4f);
			//hardness
			HardnessSlider.value = PlayerPrefs.GetFloat("XDPaintDemoBrushHardness", 0.99f);
			//color
			Color color;
			ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("XDPaintDemoBrushColor", "#FFFFFF"), out color);
			ColorClick(color);
		}

		private void OnTutorial(bool show)
		{
			TutorialObject.gameObject.SetActive(show);
			PaintManagers[currentPaintManagerId].PaintManager.gameObject.SetActive(!show);
			PaintManagers[currentPaintManagerId].PaintManager.Init();
			InputController.Instance.enabled = !show;
		}

		private void InitPaintManagers()
		{
			for (var i = 0; i < PaintManagers.Length; i++)
			{
				PaintManagers[i].PaintManager.gameObject.SetActive(i == currentPaintManagerId);
				if (paintManagerAnimator == null)
				{
					var skinnedMeshRenderer = PaintManagers[i].PaintManager.ObjectForPainting.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMeshRenderer != null)
					{
						var animator = PaintManagers[i].PaintManager.GetComponentInChildren<Animator>(true);
						if (animator != null)
						{
							paintManagerAnimator = animator;
						}
					}
				}
			}
		}

		private void SetBrushTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.Brush;
				}
				else
				{
					PaintManager.Tool = PaintTool.Brush;
				}
			}
		}

		private void SetEraseTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.Erase;
				}
				else
				{
					PaintManager.Tool = PaintTool.Erase;
				}
			}
		}

		private void SetEyedropperTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.Eyedropper;
				}
				else
				{
					PaintManager.Tool = PaintTool.Eyedropper;
				}
			}
		}

		private void SetBrushSamplerTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.BrushSampler;
				}
				else
				{
					PaintManager.Tool = PaintTool.BrushSampler;
				}
			}
		}
		
		private void SetCloneTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.Clone;
				}
				else
				{
					PaintManager.Tool = PaintTool.Clone;
				}
			}
		}

		private void SetBlurTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.Blur;
				}
				else
				{
					PaintManager.Tool = PaintTool.Blur;
				}
			}
		}
		
		private void SetGaussianBlurTool(bool isOn)
		{
			if (isOn)
			{
				if (PaintController.Instance.UseSharedSettings)
				{
					PaintController.Instance.Tool = PaintTool.BlurGaussian;
				}
				else
				{
					PaintManager.Tool = PaintTool.BlurGaussian;
				}
			}
		}
		
		private void OpenColorPalette()
		{
			BrushesPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			ColorPalette.gameObject.SetActive(!ColorPalette.gameObject.activeInHierarchy);
		}

		private void OpenBrushPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(true);
			var brushesPanelTransform = BrushesPanel.transform;
			brushesPanelTransform.position = new Vector3(xPosition, brushesPanelTransform.position.y, brushesPanelTransform.position.z);
		}
		
		private void OpenBlurPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(true);
			var blurPanelTransform = BlurPanel.transform;
			blurPanelTransform.position = new Vector3(xPosition, blurPanelTransform.position.y, blurPanelTransform.position.z);
		}
		
		private void OpenGaussianBlurPanel(float xPosition)
		{
			ColorPalette.gameObject.SetActive(false);
			BrushesPanel.gameObject.SetActive(false);
			BlurPanel.gameObject.SetActive(false);
			GaussianBlurPanel.gameObject.SetActive(true);
			var blurPanelTransform = GaussianBlurPanel.transform;
			blurPanelTransform.position = new Vector3(xPosition, blurPanelTransform.position.y, blurPanelTransform.position.z);
		}

		private void SetRotateMode(bool isOn)
		{
			CameraMover.enabled = isOn;
			if (isOn)
			{
				PaintManager.PaintObject.FinishPainting();
			}
			InputController.Instance.enabled = !isOn;
		}

		private void OnPlayPause(bool isOn)
		{
			if (paintManagerAnimator != null)
			{
				paintManagerAnimator.enabled = !isOn;
			}
		}

		private void OnOpacitySlider(float value)
		{
			var color = PaintController.Instance.Brush.Color;
			color.a = value;
			PaintController.Instance.Brush.SetColor(color);
			PlayerPrefs.SetFloat("XDPaintDemoBrushOpacity", value);
		}
		
		private void OnBrushSizeSlider(float value)
		{
			PaintController.Instance.Brush.Size = value;
			BrushPreviewTransform.localScale = Vector3.one * value;
			PlayerPrefs.SetFloat("XDPaintDemoBrushSize", value);
		}

		private void OnHardnessSlider(float value)
		{
			PaintController.Instance.Brush.Hardness = value;
			PlayerPrefs.SetFloat("XDPaintDemoBrushHardness", value);
		}
		
		private void OnBlurSlider(float value)
		{
			var blurTool = PaintManager.ToolsManager.CurrentTool as BlurTool;
			if (blurTool != null)
			{
				if (value < 0.1f)
				{
					blurTool.Iterations = 1;
					blurTool.BlurStrength = 0.5f;
				}
				else if (value < 0.5f)
				{
					blurTool.Iterations = 2;
					blurTool.BlurStrength = 1f;
				}
				else
				{
					blurTool.Iterations = 3;
					blurTool.BlurStrength = 3f;
				}
				blurTool.BlurStrength = value;
			}
		}
		
		private void OnGaussianBlurSlider(float value)
		{
			var blurTool = PaintManager.ToolsManager.CurrentTool as GaussianBlurTool;
			if (blurTool != null)
			{
				if (value < 0.1f)
				{
					blurTool.KernelSize = 3;
					blurTool.Spread = 1.0f;
				}
				else if (value < 0.5f)
				{
					blurTool.KernelSize = 4;
					blurTool.Spread = 2.0f;
				}
				else
				{
					blurTool.KernelSize = 5;
					blurTool.Spread = 3.0f;
				}
			}
		}
		
		private void OnUndo()
		{
			if (PaintManager.PaintObject.TextureKeeper.CanUndo())
			{
				PaintManager.PaintObject.TextureKeeper.Undo();
				PaintManager.Render();
			}
		}
		
		private void OnRedo()
		{
			if (PaintManager.PaintObject.TextureKeeper.CanRedo())
			{
				PaintManager.PaintObject.TextureKeeper.Redo();
				PaintManager.Render();
			}
		}

		private void SwitchPaintManager(bool switchToNext)
		{
			PaintManager.gameObject.SetActive(false);
			PaintManager.PaintObject.TextureKeeper.OnUndoStatusChanged -= OnUndoStatusChanged;
			PaintManager.PaintObject.TextureKeeper.OnRedoStatusChanged -= OnRedoStatusChanged;
			PaintManager.DoDispose();
			if (switchToNext)
			{
				currentPaintManagerId = (currentPaintManagerId + 1) % PaintManagers.Length;
			}
			else
			{
				currentPaintManagerId--;
				if (currentPaintManagerId < 0)
				{
					currentPaintManagerId = PaintManagers.Length - 1;
				}
			}
			MainCamera.orthographic = PaintManagers[currentPaintManagerId].RequiresOrthographicCamera;
			BrushTool.isOn = true;
			PaintManager.gameObject.SetActive(true);
			PaintManager.OnInitialized -= OnInitialized;
			PaintManager.OnInitialized += OnInitialized;
			PaintManager.Init();
			PaintManager.Tool = PaintTool.Brush;
			PaintManager.Brush.SetTexture(selectedTexture);
			CameraMover.Reset();
			UpdateButtons();
			
			//clear PaintManager states
			PaintManager.PaintObject.TextureKeeper.Reset();
			PaintManager.PaintObject.ClearTexture();
			PaintManager.Render();
		}

		private void OnRedoStatusChanged(bool canRedo)
		{
			RedoButton.interactable = canRedo;
		}

		private void OnUndoStatusChanged(bool canUndo)
		{
			UndoButton.interactable = canUndo;
		}

		private void UpdateButtons()
		{
			MainCamera.orthographic = PaintManagers[currentPaintManagerId].RequiresOrthographicCamera;
			RotateToggle.interactable = !PaintManagers[currentPaintManagerId].RequiresOrthographicCamera;

			var skinnedMeshRenderer = PaintManager.ObjectForPainting.GetComponent<SkinnedMeshRenderer>();
			var hasSkinnedMeshRenderer = skinnedMeshRenderer != null;
			if (!hasSkinnedMeshRenderer)
			{
				PlayPauseToggle.isOn = false;
			}
			PlayPauseToggle.interactable = hasSkinnedMeshRenderer;
			if (paintManagerAnimator != null)
			{
				paintManagerAnimator.enabled = hasSkinnedMeshRenderer;
			}
			BottomPanelText.text = PaintManagers[currentPaintManagerId].Text;
		}
		
		private void HoverEnter(BaseEventData data)
		{
			if (!PaintManager.Initialized)
				return;
			
			if (Input.mousePresent)
			{
				PaintManager.PaintObject.ProcessInput = false;
			}
			PaintManager.PaintObject.FinishPainting();
		}
		
		private void HoverExit(BaseEventData data)
		{
			if (!PaintManager.Initialized)
				return;
			
			if (Input.mousePresent)
			{
				PaintManager.PaintObject.ProcessInput = true;
			}
		}
		
		private void ColorClick(Color color)
		{
			var brushColor = PaintController.Instance.Brush.Color;
			brushColor = new Color(color.r, color.g, color.b, brushColor.a);
			PaintController.Instance.Brush.SetColor(brushColor);
			BrushTool.isOn = true;

			var colorString = ColorUtility.ToHtmlStringRGB(brushColor);
			PlayerPrefs.SetString("XDPaintDemoBrushColor", colorString);
		}

		private void BrushClick(Texture texture, int brushId)
		{
			PaintController.Instance.Brush.SetTexture(texture, true, false);
			selectedTexture = texture;
			BrushesPanel.gameObject.SetActive(false);
			PlayerPrefs.SetInt("XDPaintDemoBrushId", brushId);
		}

		private void ResetPlates(BaseEventData data)
		{
			if (ColorPalette.gameObject.activeInHierarchy || BrushesPanel.gameObject.activeInHierarchy || BlurPanel.gameObject.activeInHierarchy || GaussianBlurPanel.gameObject.activeInHierarchy)
			{
				ColorPalette.gameObject.SetActive(false);
				BrushesPanel.gameObject.SetActive(false);
				BlurPanel.gameObject.SetActive(false);
				GaussianBlurPanel.gameObject.SetActive(false);
			}
			HoverExit(null);
		}
	}
}