using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Editor.Tools;
using XDPaint.Tools;
using XDPaint.Tools.Image;

namespace XDPaint.Editor
{
	[CustomEditor(typeof(PaintManager))]
	public class PaintManagerInspector : UnityEditor.Editor
	{
		public int SelectedPresetIndex;

		private SerializedProperty objectForPaintingProperty;
		private SerializedProperty paintMaterialProperty;
		private SerializedProperty shaderTextureNameProperty;
		private SerializedProperty overrideCameraProperty;
		private SerializedProperty paintModeProperty;
		private SerializedProperty filterModeProperty;
		private SerializedProperty defaultTextureWidth;
		private SerializedProperty defaultTextureHeight;
		private SerializedProperty cameraProperty;
		private SerializedProperty useNeighborsVerticesForRaycastsProperty;
		private SerializedProperty trianglesContainerProperty;
		private SerializedProperty copySourceTextureToPaintTextureProperty;
		private SerializedProperty useSourceTextureAsBackgroundProperty;
		private SerializedProperty paintToolProperty;
		private SerializedProperty brushProperty;

		private PaintManager paintManager;
		private BasePaintObject paintObject;
		private Component component;

		private int shaderTextureNameSelectedId;
		private bool isMeshObject;
		private bool objectForPaintChanged;
		private bool shouldCheckTexture = true;
		private bool sortPresetsByName = true;
		private bool hasTexture;
		private PaintTool tool = PaintTool.Brush;
		private EnumDrawer<PaintTool> paintTool;
		private EnumDrawer<PaintTool> paintToolDrawer
		{
			get
			{
				if (paintTool == null)
				{
					paintTool = new EnumDrawer<PaintTool>();
					paintTool.Init();
				}
				return paintTool;
			}
		}

		private EnumDrawer<PaintMode> paintMode;
		private EnumDrawer<PaintMode> paintModeDrawer
		{
			get
			{
				if (paintMode == null)
				{
					paintMode = new EnumDrawer<PaintMode>();
					paintMode.Init();
				}
				return paintMode;
			}
		}
		
		private EnumDrawer<FilterMode> filterMode;
		private EnumDrawer<FilterMode> filterModeDrawer
		{
			get
			{
				if (filterMode == null)
				{
					filterMode = new EnumDrawer<FilterMode>();
					filterMode.Init();
				}
				return filterMode;
			}
		}
		
		private bool showWarning;
		private bool allowSavePresetsInRuntime = false;
		private int blurIterationsCount;
		private float blurStrength;
		private int blurDownscaleRatio;
		private int blurGaussianKernelSize;
		private float blurGaussianSpread;
		private string savedName;
		private bool rename;
		private bool showDialogName;

		#region Menu Items
		
		[MenuItem("GameObject/2D\u22153D Paint", false, 32)]
		static void AddPaintManagerObject()
		{
			var clothObject = new GameObject("2D/3D Paint");
			clothObject.AddComponent<PaintManager>();
			Selection.activeObject = clothObject.gameObject;
		}

		[MenuItem("Component/2D\u22153D Paint")]
		static void AddPaintManagerComponent()
		{
			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<PaintManager>() == null)
			{
				Selection.activeGameObject.AddComponent<PaintManager>();
			}
		}

		[MenuItem("CONTEXT/PaintManager/Fill Triangles Data")]
		static void AddTrianglesDataWithNeighbors(MenuCommand command)
		{
			var paintManager = (PaintManager)command.context;
			paintManager.UseNeighborsVerticesForRaycasts = true;
			OpenTrianglesDataWindow(paintManager);
		}

		[MenuItem("CONTEXT/PaintManager/Fill Triangles Data", true)]
		static bool ValidationAddTrianglesDataWithNeighbors()
		{
			var paintManager = Selection.activeGameObject.GetComponent<PaintManager>();
			var supportedComponent = PaintManagerHelper.GetSupportedComponent(paintManager.ObjectForPainting);
			return supportedComponent != null && PaintManagerHelper.IsMeshObject(supportedComponent);
		}

		[MenuItem("CONTEXT/PaintManager/Clear Triangles Data")]
		static void ClearTrianglesData(MenuCommand command)
		{
			var paintManager = (PaintManager)command.context;
			paintManager.ClearTrianglesData();
			paintManager.UseNeighborsVerticesForRaycasts = false;
			if (!Application.isPlaying)
			{
				EditorUtility.SetDirty(paintManager);
				EditorSceneManager.MarkSceneDirty(paintManager.gameObject.scene);
			}
		}
		
		[MenuItem("CONTEXT/PaintManager/Clear Triangles Data", true)]
		static bool ValidationClearTrianglesData()
		{
			var paintManager = Selection.activeGameObject.GetComponent<PaintManager>();
			var supportedComponent = PaintManagerHelper.GetSupportedComponent(paintManager.ObjectForPainting);
			return supportedComponent != null && PaintManagerHelper.IsMeshObject(supportedComponent);
		}
		
		#endregion
		
		private static void OpenTrianglesDataWindow(PaintManager paintManager)
		{
			var progressBar = (TrianglesDataWindow)EditorWindow.GetWindow(typeof(TrianglesDataWindow), false, PaintManagerHelper.TrianglesDataWindowTitle);
			progressBar.maxSize = new Vector2(PaintManagerHelper.TrianglesDataWindowSize.x, PaintManagerHelper.TrianglesDataWindowSize.y);
			progressBar.SetPaintManager(paintManager);
			progressBar.Show(false);
		}

		void OnEnable()
		{
			paintManager = (PaintManager)target;
			paintObject = paintManager.PaintObject;

			objectForPaintingProperty = serializedObject.FindProperty("ObjectForPainting");
			paintMaterialProperty = serializedObject.FindProperty("Material.SourceMaterial");
			shaderTextureNameProperty = serializedObject.FindProperty("Material.shaderTextureName");
			overrideCameraProperty = serializedObject.FindProperty("ShouldOverrideCamera");
			paintModeProperty = serializedObject.FindProperty("paintModeType");
			filterModeProperty = serializedObject.FindProperty("filterMode");
			defaultTextureWidth = serializedObject.FindProperty("Material.defaultTextureWidth");
			defaultTextureHeight = serializedObject.FindProperty("Material.defaultTextureHeight");
			cameraProperty = serializedObject.FindProperty("overrideCamera");
			useNeighborsVerticesForRaycastsProperty = serializedObject.FindProperty("useNeighborsVerticesForRaycasts");
			trianglesContainerProperty = serializedObject.FindProperty("trianglesContainer");
			copySourceTextureToPaintTextureProperty = serializedObject.FindProperty("CopySourceTextureToPaintTexture");
			useSourceTextureAsBackgroundProperty = serializedObject.FindProperty("useSourceTextureAsBackground");
			paintToolProperty = serializedObject.FindProperty("paintTool");
			brushProperty = serializedObject.FindProperty("brush");

			UpdateTexturesList();
		}

		private void UpdateTexturesList()
		{
			var material = paintMaterialProperty.objectReferenceValue as Material;
			if (material != null)
			{
				var shaderTextureNames = PaintManagerHelper.GetTexturesListFromShader(material);
				shaderTextureNameSelectedId = Array.IndexOf(shaderTextureNames, shaderTextureNameProperty.stringValue);
			}
			paintManager.Material.InitMaterial(material);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(objectForPaintingProperty, new GUIContent("Object For Painting", PaintManagerHelper.ObjectForPaintingTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				objectForPaintChanged = true;
				shouldCheckTexture = true;
			}
			
			if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(objectForPaintingProperty.objectReferenceValue != null)))
			{
				paintManager = (PaintManager)target;
				paintObject = paintManager.PaintObject;
				component = PaintManagerHelper.GetSupportedComponent(objectForPaintingProperty.objectReferenceValue as GameObject);
				isMeshObject = PaintManagerHelper.IsMeshObject(component);
				if (isMeshObject && objectForPaintChanged)
				{
					objectForPaintChanged = false;
					paintManager.ClearTrianglesData();
					useNeighborsVerticesForRaycastsProperty.boolValue = false;
					MarkAsDirty();
				}
				DrawMaterialBlock();
				
				if (paintManager != null && paintManager.ToolsManager != null)
				{
					var blurTool = paintManager.ToolsManager.CurrentTool as BlurTool;
					if (blurTool != null)
					{
						blurIterationsCount = blurTool.Iterations;
						blurStrength = blurTool.BlurStrength;
						blurDownscaleRatio = blurTool.DownscaleRatio;
					}
					
					var gaussianBlurTool = paintManager.ToolsManager.CurrentTool as GaussianBlurTool;
					if (gaussianBlurTool != null)
					{
						blurGaussianKernelSize = gaussianBlurTool.KernelSize;
						blurGaussianSpread = gaussianBlurTool.Spread;
					}
				}
				
				DrawToolBlock();
				// DrawBrushBlock();
				if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(paintMaterialProperty.objectReferenceValue != null)))
				{
					DrawCheckboxesBlock();
					if (Settings.Instance != null)
					{
						DrawPresetsBlock();
					}
					DrawButtonsBlock();
				}
				EditorGUILayout.EndFadeGroup();
			}
			EditorGUILayout.EndFadeGroup();
			DrawAutoFillButton();
#if XDP_DEBUG
			if (Application.isPlaying && paintManager.Initialized)
			{
				var rect = GUILayoutUtility.GetRect(320, 240, GUILayout.ExpandWidth(true));
				GUI.DrawTexture(rect, paintManager.GetPaintInputTexture(), ScaleMode.ScaleToFit);
			}
#endif
			serializedObject.ApplyModifiedProperties();
		}
		
		public override bool RequiresConstantRepaint()
		{
			return Application.isPlaying && paintManager.Initialized;
		}
		
		private void DrawAutoFillButton()
		{
			var disabled = objectForPaintingProperty.objectReferenceValue == null || paintMaterialProperty.objectReferenceValue == null;
			EditorGUI.BeginDisabledGroup(!disabled);
			if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(disabled)))
			{
				if (GUILayout.Button(new GUIContent("Auto fill", PaintManagerHelper.AutoFillButtonTooltip), GUILayout.ExpandWidth(true)))
				{
					var objectForPaintingFillResult = FindObjectForPainting();
					var findMaterialResult = FindMaterial();
					if (!objectForPaintingFillResult && !findMaterialResult)
					{
						Debug.Log("Can't find ObjectForPainting and Material.");
					}
					else if (!objectForPaintingFillResult)
					{
						Debug.Log("Can't find ObjectForPainting.");
					}
					else if (!findMaterialResult)
					{
						Debug.Log("Can't find Material.");
					}
					else
					{
						MarkAsDirty();
					}
				}
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUI.EndDisabledGroup();
		}

		private bool FindObjectForPainting()
		{
			if (objectForPaintingProperty.objectReferenceValue == null)
			{
				var supportedComponent = PaintManagerHelper.GetSupportedComponent(paintManager.gameObject);
				if (supportedComponent != null)
				{
					objectForPaintingProperty.objectReferenceValue = supportedComponent.gameObject;
					return true;
				}
				if (paintManager.gameObject.transform.childCount > 0)
				{
					var compatibleComponents = new List<Component>();
					var allComponents = paintManager.gameObject.transform.GetComponentsInChildren<Component>();
					foreach (var component in allComponents)
					{
						var childComponent = PaintManagerHelper.GetSupportedComponent(component.gameObject);
						if (childComponent != null)
						{
							compatibleComponents.Add(childComponent);
							break;
						}
					}
					if (compatibleComponents.Count > 0)
					{
						objectForPaintingProperty.objectReferenceValue = compatibleComponents[0].gameObject;
						return true;
					}
				}
				return false;
			}
			return true;
		}

		private bool FindMaterial()
		{
			var result = false;
			component = PaintManagerHelper.GetSupportedComponent(objectForPaintingProperty.objectReferenceValue as GameObject);
			if (component != null)
			{
				var renderer = component as Renderer;
				if (renderer != null && renderer.sharedMaterial != null)
				{
					paintMaterialProperty.objectReferenceValue = renderer.sharedMaterial;
					result = true;
				}
				var maskableGraphic = component as RawImage;
				if (maskableGraphic != null && maskableGraphic.material != null)
				{
					paintMaterialProperty.objectReferenceValue = maskableGraphic.material;
					result = true;
				}
			}
			UpdateTexturesList();
			return result;
		}

		private void DrawMaterialBlock()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(paintMaterialProperty, new GUIContent("Material", PaintManagerHelper.MaterialTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				UpdateTexturesList();
				shouldCheckTexture = true;
			}
			if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(paintMaterialProperty.objectReferenceValue != null)))
			{
				var shaderTextureNames = PaintManagerHelper.GetTexturesListFromShader(paintMaterialProperty.objectReferenceValue as Material);
				var shaderTextureNamesContent = new GUIContent[shaderTextureNames.Length];
				for (var i = 0; i < shaderTextureNamesContent.Length; i++)
				{
					shaderTextureNamesContent[i] = new GUIContent(shaderTextureNames[i]);
				}

				var shaderTextureName = paintManager.Material.ShaderTextureName;
				if (shaderTextureNames.Contains(shaderTextureName))
				{
					for (var i = 0; i < shaderTextureNames.Length; i++)
					{
						if (shaderTextureNames[i] == shaderTextureName)
						{
							shaderTextureNameSelectedId = i;
							break;
						}
					}
				}
				
				shaderTextureNameSelectedId = Mathf.Clamp(shaderTextureNameSelectedId, 0, int.MaxValue);
				EditorGUI.BeginChangeCheck();
				shaderTextureNameSelectedId = EditorGUILayout.Popup(new GUIContent("Shader Texture Name", PaintManagerHelper.ShaderTextureNameTooltip), shaderTextureNameSelectedId, shaderTextureNamesContent);
				if (EditorGUI.EndChangeCheck())
				{
					shouldCheckTexture = true;
				}

				if (shaderTextureNames.Length > 0 && shaderTextureNames.Length > shaderTextureNameSelectedId && shaderTextureNames[shaderTextureNameSelectedId] != shaderTextureName)
				{
					shaderTextureNameProperty.stringValue = shaderTextureNames[shaderTextureNameSelectedId];
					paintManager.Material.ShaderTextureName = shaderTextureNameProperty.stringValue;
					MarkAsDirty();
				}
				
				if (!hasTexture || shouldCheckTexture)
				{
					if (shouldCheckTexture)
					{
						serializedObject.ApplyModifiedProperties();
						hasTexture = PaintManagerHelper.HasTexture(paintManager);
						shouldCheckTexture = false;
					}
					if (!hasTexture)
					{
						EditorGUILayout.HelpBox("Object does not have source texture, transparent texture will be created. Please specify the texture size", MessageType.Warning);
						EditorGUILayout.PropertyField(defaultTextureWidth, new GUIContent("Texture Width", PaintManagerHelper.TextureSizeTip));
						EditorGUILayout.PropertyField(defaultTextureHeight, new GUIContent("Texture Height", PaintManagerHelper.TextureSizeTip));
					}
				}
			}
			EditorGUILayout.EndFadeGroup();
		}

		private void DrawPresetsBlock()
		{
			EditorHelper.DrawHorizontalLine();
			//getting all presets
			var additionalPresetsCount = 2;
			var options = new string[BrushPresets.Instance.Presets.Count + additionalPresetsCount];
			options[0] = BrushDrawerHelper.CustomPresetName;
			options[1] = BrushDrawerHelper.DefaultPresetName;
			var unnamedPresetsCount = 0;
			for (var i = additionalPresetsCount; i < options.Length; i++)
			{
				var preset = BrushPresets.Instance.Presets[i - additionalPresetsCount];
				if (preset != null)
				{
					if (string.IsNullOrEmpty(preset.Name))
					{
						unnamedPresetsCount++;
					}
					options[i] = string.IsNullOrEmpty(preset.Name) ? "Unnamed preset " + unnamedPresetsCount : " [" + (i - 1) + "] " + preset.Name;
				}
			}
			
			//update selected index
			for (var i = 0; i < BrushPresets.Instance.Presets.Count; i++)
			{
				if (paintManager.Brush != null && string.IsNullOrEmpty(paintManager.Brush.Name))
				{
					paintManager.Brush.Name = Guid.NewGuid().ToString("N");
				}
				if (paintManager.Brush != null &&
				    !string.IsNullOrEmpty(paintManager.Brush.Name) && 
				    paintManager.Brush.Name != BrushDrawerHelper.CustomPresetName && 
				    BrushPresets.Instance.Presets[i].Name == paintManager.Brush.Name)
				{
					SelectedPresetIndex = i + additionalPresetsCount;
					break;
				}
			}
			
			//preset popup
			EditorGUI.BeginDisabledGroup(showDialogName);
			EditorGUI.BeginChangeCheck();
			SelectedPresetIndex = EditorGUILayout.Popup("Brush", SelectedPresetIndex, options);
			var presetChanged = EditorGUI.EndChangeCheck();
			EditorGUI.EndDisabledGroup();

			if (SelectedPresetIndex == 0 && paintManager.Initialized)
			{
				paintManager.Brush.Name = BrushDrawerHelper.CustomPresetName;
			}

			if (presetChanged)
			{
				Undo.RecordObjects(targets, "Brush Preset Update");
				foreach (var script in targets)
				{
					var targetPaintManager = script as PaintManager;
					if (targetPaintManager != null)
					{
						if (SelectedPresetIndex == 0)
						{
							if (Application.isPlaying)
							{
								PaintController.Instance.UseSharedSettings = false;
								targetPaintManager.InitBrush();
							}
						}
						else if (SelectedPresetIndex == 1)
						{
							if (Application.isPlaying)
							{
								targetPaintManager.Brush = PaintController.Instance.Brush;
							}
							else
							{
								targetPaintManager.Brush = new Brush(targetPaintManager.Brush)
								{
									Name = savedName
								};
							}
						}
						else
						{
							if (Application.isPlaying)
							{
								targetPaintManager.InitBrush();
								targetPaintManager.Brush.SetValues(BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount]);
							}
							{
								targetPaintManager.Brush = BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount].Clone();
							}
						}
						
						EditorHelper.MarkComponentAsDirty(targetPaintManager);
						serializedObject.Update();
					}
				}
			}
			
			EditorGUILayout.PropertyField(brushProperty, new GUIContent("Brush", PaintManagerHelper.BrushTooltip));
			brushProperty.serializedObject.ApplyModifiedProperties();

			if (!showDialogName)
			{
				//save and remove buttons
				var enableButtons = !Application.isPlaying || Application.isPlaying && allowSavePresetsInRuntime;
				GUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(!enableButtons);
				if (GUILayout.Button("Save As", GUILayout.ExpandWidth(true)))
				{
					showDialogName = true;
					showWarning = true;
					if (SelectedPresetIndex == 0 || SelectedPresetIndex == 1)
					{
						savedName = "Brush " + (BrushPresets.Instance.Presets.Count + 1);
					}
					else
					{
						savedName = BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount].Name;
					}
				}
				EditorGUI.BeginDisabledGroup(SelectedPresetIndex == 0 || SelectedPresetIndex == 1);
				if (GUILayout.Button("Rename", GUILayout.ExpandWidth(true)))
				{
					rename = true;
					showDialogName = true;
					showWarning = false;
					savedName = BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount].Name;
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(SelectedPresetIndex == 0 || SelectedPresetIndex == 1 || !enableButtons);
				if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
				{
					var result = EditorUtility.DisplayDialog("Remove selected brush?", 
						"Are you sure that you want to remove " + BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount].Name + " brush?", "Remove", "Cancel");
					if (result)
					{
						BrushPresets.Instance.Presets.RemoveAt(SelectedPresetIndex - additionalPresetsCount);
						SelectedPresetIndex = 0;
						foreach (var script in targets)
						{
							var targetPaintManager = script as PaintManager;
							if (targetPaintManager != null)
							{
								targetPaintManager.Brush.Name = string.Empty;
								EditorHelper.MarkAsDirty(targetPaintManager);
								serializedObject.Update();
							}
						}
					}
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();
			}
			else
			{
				//enter name for a new preset
				savedName = GUILayout.TextArea(savedName, GUILayout.ExpandWidth(true));
				var savedNameTrimmed = savedName.Trim();
				var hasSavedPresetWithSameName = false;
				foreach (var preset in BrushPresets.Instance.Presets)
				{
					if (preset.Name == savedNameTrimmed)
					{
						hasSavedPresetWithSameName = true;
						break;
					}
				}

				var hasPresetWithSameName = showWarning = 
					savedNameTrimmed == BrushDrawerHelper.DefaultPresetName || hasSavedPresetWithSameName || string.IsNullOrEmpty(savedNameTrimmed);
				if (showWarning)
				{
					EditorGUILayout.HelpBox("Please, enter unique name for brush", MessageType.Warning);
				}
				GUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(hasPresetWithSameName);
				if (GUILayout.Button("Save", GUILayout.ExpandWidth(true)))
				{
					if (!hasPresetWithSameName)
					{
						var selectedBrush = SelectedPresetIndex == 0 ? paintManager.Brush : BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount];
						var preset = new Brush(selectedBrush)
						{
							Name = savedNameTrimmed
						};
						if (rename)
						{
							BrushPresets.Instance.Presets[SelectedPresetIndex - additionalPresetsCount] = preset;
						}
						else
						{
							BrushPresets.Instance.Presets.Insert(SelectedPresetIndex - additionalPresetsCount + 1, preset);
							if (sortPresetsByName)
							{
								BrushPresets.Instance.Presets.Sort((brush1, brush2) => string.Compare(brush1.Name, brush2.Name, StringComparison.Ordinal));
								SelectedPresetIndex = BrushPresets.Instance.Presets.IndexOf(preset) + additionalPresetsCount;
							}
							else
							{
								SelectedPresetIndex++;
							}
						}
						foreach (var script in targets)
						{
							var targetPaintManager = script as PaintManager;
							if (targetPaintManager != null)
							{
								targetPaintManager.Brush = preset.Clone();
								EditorHelper.MarkAsDirty(targetPaintManager);
								serializedObject.Update();
							}
						}
						showDialogName = false;
						showWarning = false;
						rename = false;
					}
				}
				EditorGUI.EndDisabledGroup();
				if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
				{
					showDialogName = false;
					showWarning = false;
					rename = false;
				}
				GUILayout.EndHorizontal();
			}
		}

		private void DrawToolBlock()
		{
			var paintToolChanged = paintToolDrawer.Draw(paintToolProperty, "Paint Tool", PaintManagerHelper.PaintingToolTooltip, ref tool);
			if (paintToolChanged)
			{
				paintManager.Tool = tool;
				MarkAsDirty();
			}

			tool = (PaintTool)paintToolDrawer.ModeId;
			if (Application.isPlaying)
			{
				if (tool == PaintTool.Blur)
				{
					blurIterationsCount = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Iterations", blurIterationsCount, 1, 5);
					blurStrength = EditorGUI.Slider(EditorGUILayout.GetControlRect(), "Blur Strength", blurStrength, 0.01f, 5f);
					blurDownscaleRatio = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Downscale Ratio", blurDownscaleRatio, 1, 16);
					if (paintManager != null && paintManager.ToolsManager != null)
					{
						var blurTool = paintManager.ToolsManager.CurrentTool as BlurTool;
						if (blurTool != null)
						{
							blurTool.Iterations = blurIterationsCount;
							blurTool.BlurStrength = blurStrength;
							blurTool.DownscaleRatio = blurDownscaleRatio;
						}
					}
				}
				else if (tool == PaintTool.BlurGaussian)
				{
					blurGaussianKernelSize = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Kernel Size", blurGaussianKernelSize, 3, 7);
					blurGaussianSpread = EditorGUI.Slider(EditorGUILayout.GetControlRect(), "Blur Spread", blurGaussianSpread, 0.01f, 5f);
					if (paintManager != null && paintManager.ToolsManager != null)
					{
						var blurTool = paintManager.ToolsManager.CurrentTool as GaussianBlurTool;
						if (blurTool != null)
						{
							blurTool.KernelSize = blurGaussianKernelSize;
							blurTool.Spread = blurGaussianSpread;
						}
					}
				}
			}

			var mode = PaintMode.Default;
			var paintModeChanged = paintModeDrawer.Draw(paintModeProperty, "Paint Mode", PaintManagerHelper.PaintingModeTooltip, ref mode);
			if (paintModeChanged)
			{
				paintManager.SetPaintMode(mode);
				MarkAsDirty();
			}

			var filter = FilterMode.Point;
			var filterModeChanged = filterModeDrawer.Draw(filterModeProperty, "Filter Mode", PaintManagerHelper.FilteringModeTooltip, ref filter);
			if (filterModeChanged)
			{
				paintManager.FilterMode = filter;
				MarkAsDirty();
			}
		}

		private void DrawCheckboxesBlock()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(overrideCameraProperty, new GUIContent("Override Camera", PaintManagerHelper.OverrideCameraTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				paintManager.ShouldOverrideCamera = overrideCameraProperty.boolValue;
			}
			using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(overrideCameraProperty.boolValue)))
			{
				if (group.visible)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(cameraProperty, new GUIContent("Camera", PaintManagerHelper.CameraTooltip));
					if (EditorGUI.EndChangeCheck())
					{
						paintManager.Camera = cameraProperty.objectReferenceValue as Camera;
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(copySourceTextureToPaintTextureProperty, new GUIContent("Copy Source Texture To Paint Texture", PaintManagerHelper.CopySourceTextureToPaintTextureTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				paintManager.CopySourceTextureToPaintTexture = copySourceTextureToPaintTextureProperty.boolValue;
				MarkAsDirty();
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(useSourceTextureAsBackgroundProperty, new GUIContent("Use Source Texture as Background", PaintManagerHelper.UseSourceTextureAsBackgroundTooltip));
			if (EditorGUI.EndChangeCheck())
			{
				paintManager.UseSourceTextureAsBackground = useSourceTextureAsBackgroundProperty.boolValue;
				MarkAsDirty();
			}
			
			if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(isMeshObject)))
			{
				if (!useNeighborsVerticesForRaycastsProperty.boolValue || !paintManager.UseNeighborsVerticesForRaycasts || paintManager.UseNeighborsVerticesForRaycasts && !paintManager.HasTrianglesData && trianglesContainerProperty.objectReferenceValue == null)
				{
					EditorGUILayout.HelpBox("Object does not have neighbors triangles data, to fix it please check 'Use Neighbors Vertices For Raycasts'", MessageType.Warning);
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(useNeighborsVerticesForRaycastsProperty, new GUIContent("Use Neighbors Vertices For Raycasts", PaintManagerHelper.UseNeighborsVerticesForRaycastTooltip));
				if (EditorGUI.EndChangeCheck())
				{
					paintManager.UseNeighborsVerticesForRaycasts = useNeighborsVerticesForRaycastsProperty.boolValue;
					if (useNeighborsVerticesForRaycastsProperty.boolValue)
					{
						OpenTrianglesDataWindow(paintManager);
					}
					MarkAsDirty();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(trianglesContainerProperty, new GUIContent("Triangles Container"));
				if (EditorGUI.EndChangeCheck())
				{
					if (trianglesContainerProperty.objectReferenceValue != null)
					{
						useNeighborsVerticesForRaycastsProperty.boolValue = true;
						serializedObject.ApplyModifiedProperties();
						paintManager.ClearTrianglesData();
						serializedObject.Update();
					}
					else if (trianglesContainerProperty.objectReferenceValue == null && !paintManager.HasTrianglesData)
					{
						useNeighborsVerticesForRaycastsProperty.boolValue = false;
						serializedObject.ApplyModifiedProperties();
					}
					MarkAsDirty();
				}
			}
			EditorGUILayout.EndFadeGroup();
		}

		private void DrawButtonsBlock()
		{
			if (EditorGUILayout.BeginFadeGroup(Convert.ToSingle(isMeshObject)))
			{
				EditorGUI.BeginDisabledGroup(!paintManager.HasTrianglesData);
				if (GUILayout.Button(new GUIContent("Save Triangles Data to Asset", PaintManagerHelper.CloneMaterialTooltip), GUILayout.ExpandWidth(true)))
				{
					var path = EditorUtility.SaveFilePanelInProject("Save Triangles Data to TrianglesContainer", "TrianglesContainer", "asset", "TrianglesContainer asset saving");
					if (!string.IsNullOrEmpty(path))
					{
						var asset = CreateInstance<TrianglesContainer>();
						AssetDatabase.CreateAsset(asset, path);
						asset.Data = paintManager.GetTriangles();
						EditorUtility.SetDirty(asset);
						paintManager.ClearTrianglesData();
						serializedObject.Update();
						trianglesContainerProperty.objectReferenceValue = asset;
						MarkAsDirty();
					}
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndFadeGroup();
						
			var disablePlaying = Application.isPlaying;
			EditorGUI.BeginDisabledGroup(!disablePlaying);
			if (GUILayout.Button(new GUIContent("Initialize", PaintManagerHelper.SaveToFileTooltip), GUILayout.ExpandWidth(true)))
			{
				paintManager.Init();
			}
			EditorGUI.EndDisabledGroup();
			
			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(disablePlaying);
			if (GUILayout.Button(new GUIContent("Clone Material", PaintManagerHelper.CloneMaterialTooltip), GUILayout.ExpandWidth(true)))
			{
				var clonedMaterial = PaintManagerHelper.CloneMaterial(paintMaterialProperty.objectReferenceValue as Material);
				if (clonedMaterial != null)
				{
					paintMaterialProperty.objectReferenceValue = clonedMaterial;
				}
			}
			if (GUILayout.Button(new GUIContent("Clone Texture", PaintManagerHelper.CloneTextureTooltip), GUILayout.ExpandWidth(true)))
			{
				PaintManagerHelper.CloneTexture(paintMaterialProperty.objectReferenceValue as Material, shaderTextureNameProperty.stringValue);
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
				
			GUILayout.BeginHorizontal();
			var disableUndo = !(Application.isPlaying && paintObject != null && paintObject.TextureKeeper.CanUndo());
			EditorGUI.BeginDisabledGroup(disableUndo);
			if (GUILayout.Button(new GUIContent("Undo", PaintManagerHelper.UndoTooltip), GUILayout.ExpandWidth(true)))
			{
				paintObject.TextureKeeper.Undo();
				paintManager.Render();
			}
			EditorGUI.EndDisabledGroup();
			var disableRedo = !(Application.isPlaying && paintObject != null && paintObject.TextureKeeper.CanRedo());
			EditorGUI.BeginDisabledGroup(disableRedo);
			if (GUILayout.Button(new GUIContent("Redo", PaintManagerHelper.RedoTooltip), GUILayout.ExpandWidth(true)))
			{
				paintObject.TextureKeeper.Redo();
				paintManager.Render();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			var disableBake = !(Application.isPlaying && isMeshObject);
			EditorGUI.BeginDisabledGroup(disableBake);
			if (GUILayout.Button(new GUIContent("Bake", PaintManagerHelper.BakeTooltip), GUILayout.ExpandWidth(true)))
			{
				PaintManagerHelper.Bake(paintManager.Material.SourceTexture, paintManager.Bake);
			}
			EditorGUI.EndDisabledGroup();
				
			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			if (GUILayout.Button(new GUIContent("Save", PaintManagerHelper.SaveToFileTooltip), GUILayout.ExpandWidth(true)))
			{
				PaintManagerHelper.SaveToFile(paintManager);
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}

		private void MarkAsDirty()
		{
			if (!Application.isPlaying)
			{
				EditorUtility.SetDirty(paintManager);
				EditorSceneManager.MarkSceneDirty(paintManager.gameObject.scene);
			}
		}
	}
}