using System;
using UnityEditor;
using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Editor.Tools;
using XDPaint.Tools;
using XDPaint.Tools.Image;

namespace XDPaint.Editor
{
	[CustomEditor(typeof(PaintController))]
	public class PaintControllerInspector : UnityEditor.Editor
	{
		private SerializedProperty useSharedSettingsProperty;
		private SerializedProperty brushProperty;
		private SerializedProperty paintToolProperty;
		private SerializedProperty paintModeProperty;
		
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

		private PaintController paintController;
		private int blurIterationsCount;
		private float blurStrength;
		private int blurDownscaleRatio;
		private int blurGaussianKernelSize;
		private float blurGaussianSpread;
		private int selectedPresetIndex;
		private string savedName;
		private bool rename;
		private bool showDialogName;
		private bool showWarning;
		private bool allowSavePresetsInRuntime = false;
		private bool sortPresetsByName = true;
		private bool showDebugInfo;

		private const string DefaultPresetName = "Common brush";

		void OnEnable()
		{
			useSharedSettingsProperty = serializedObject.FindProperty("useSharedSettings");
			brushProperty = serializedObject.FindProperty("brush");
			paintToolProperty = serializedObject.FindProperty("paintTool");
			paintModeProperty = serializedObject.FindProperty("paintModeType");
		}
				
		public override bool RequiresConstantRepaint()
		{
			return Application.isPlaying;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(useSharedSettingsProperty);
			if (EditorGUI.EndChangeCheck())
			{
				paintController.UseSharedSettings = useSharedSettingsProperty.boolValue;
			}

			DrawToolBlock();
			
			if (Settings.Instance != null)
			{
				DrawPresetsBlock();
				EditorHelper.DrawHorizontalLine();
			}
			DrawBrushBlock();
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawToolBlock()
		{
			var paintToolChanged = paintToolDrawer.Draw(paintToolProperty, "Paint Tool", PaintManagerHelper.PaintingToolTooltip, ref tool);
			if (paintToolChanged)
			{
				paintController.Tool = tool;
				EditorHelper.MarkComponentAsDirty(paintController);
			}

			tool = (PaintTool)paintToolDrawer.ModeId;
			if (Application.isPlaying)
			{
				if (tool == PaintTool.Blur)
				{
					blurIterationsCount = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Iterations", blurIterationsCount, 1, 5);
					blurStrength = EditorGUI.Slider(EditorGUILayout.GetControlRect(), "Blur Strength", blurStrength, 0.01f, 5f);
					blurDownscaleRatio = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Downscale Ratio", blurDownscaleRatio, 1, 16);
					if (paintController != null)
					{
						var paintManagers = paintController.AllPaintManagers();
						foreach (var paintManager in paintManagers)
						{
							var blurTool = paintManager.ToolsManager.CurrentTool as BlurTool;
							if (blurTool != null)
							{
								blurTool.Iterations = blurIterationsCount;
								blurTool.BlurStrength = blurStrength;
								blurTool.DownscaleRatio = blurDownscaleRatio;
							}
						}
						Undo.RecordObject(paintController, "PaintController Edit");
					}
				}
				else if (tool == PaintTool.BlurGaussian)
				{
					blurGaussianKernelSize = EditorGUI.IntSlider(EditorGUILayout.GetControlRect(), "Blur Kernel Size", blurGaussianKernelSize, 3, 7);
					blurGaussianSpread = EditorGUI.Slider(EditorGUILayout.GetControlRect(), "Blur Spread", blurGaussianSpread, 0.01f, 5f);
					if (paintController != null)
					{
						var paintManagers = paintController.AllPaintManagers();
						foreach (var paintManager in paintManagers)
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
			}

			var mode = PaintMode.Default;
			var paintModeChanged = paintModeDrawer.Draw(paintModeProperty, "Paint Mode", PaintManagerHelper.PaintingModeTooltip, ref mode);
			if (paintModeChanged)
			{
				paintController.PaintMode = mode;
				EditorHelper.MarkComponentAsDirty(paintController);
			}
		}
		
		private void DrawBrushBlock()
		{
			paintController = target as PaintController;
			var brushProperty = this.brushProperty;
			if (selectedPresetIndex != 0)
			{
				var presets = new SerializedObject(BrushPresets.Instance).FindProperty("Presets");
				var arrayIndex = selectedPresetIndex - 1;
				brushProperty = presets.GetArrayElementAtIndex(arrayIndex);
				if (Application.isPlaying)
				{
					paintController.Brush.SetValues(BrushPresets.Instance.Presets[arrayIndex]);
				}
				else
				{
					paintController.Brush = BrushPresets.Instance.Presets[arrayIndex].Clone();
				}
			}

			EditorGUILayout.PropertyField(brushProperty, new GUIContent("Brush", PaintManagerHelper.BrushTooltip));
			brushProperty.serializedObject.ApplyModifiedProperties();
		}

		private void DrawPresetsBlock()
		{
			//getting all presets
			var options = new string[BrushPresets.Instance.Presets.Count + 1];
			options[0] = DefaultPresetName;
			var unnamedPresetsCount = 0;
			for (var i = 1; i < options.Length; i++)
			{
				var preset = BrushPresets.Instance.Presets[i - 1];
				if (preset != null)
				{
					if (string.IsNullOrEmpty(preset.Name))
					{
						unnamedPresetsCount++;
					}
					options[i] = string.IsNullOrEmpty(preset.Name) ? 
						"Unnamed brush " + unnamedPresetsCount : 
						"[" + i + "] " + preset.Name;
				}
			}

			//preset popup
			EditorGUI.BeginDisabledGroup(showDialogName || useSharedSettingsProperty.boolValue);
			EditorGUI.BeginChangeCheck();
			selectedPresetIndex = EditorGUILayout.Popup("Preset", selectedPresetIndex, options);
			if (useSharedSettingsProperty.boolValue)
			{
				selectedPresetIndex = 0;
			}
			var presetChanged = EditorGUI.EndChangeCheck();
			EditorGUI.EndDisabledGroup();

			//set as common
			if (presetChanged)
			{ 
				Undo.RecordObjects(targets, "Brush Preset Update");
				foreach (var script in targets)
				{
					var currentPaintController = script as PaintController;
					if (currentPaintController != null)
					{
						currentPaintController.Brush = selectedPresetIndex == 0 ? 
							currentPaintController.Brush : 
							BrushPresets.Instance.Presets[selectedPresetIndex - 1].Clone();
						EditorHelper.MarkAsDirty(currentPaintController);
						serializedObject.Update();
					}
				}
				
				//expand preset
				if (selectedPresetIndex != 0)
				{
					var presets = new SerializedObject(BrushPresets.Instance).FindProperty("Presets");
					var brushProperty = presets.GetArrayElementAtIndex(selectedPresetIndex - 1);
					brushProperty.isExpanded = true;
				}
			}

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
					if (selectedPresetIndex == 0)
					{
						savedName = "Brush " + (BrushPresets.Instance.Presets.Count + 1);
					}
					else
					{
						savedName = BrushPresets.Instance.Presets[selectedPresetIndex - 1].Name;
					}
				}
				EditorGUI.BeginDisabledGroup(selectedPresetIndex == 0);
				if (GUILayout.Button("Rename", GUILayout.ExpandWidth(true)))
				{
					rename = true;
					showDialogName = true;
					showWarning = false;
					savedName = BrushPresets.Instance.Presets[selectedPresetIndex - 1].Name;
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(selectedPresetIndex == 0 || !enableButtons);
				if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
				{
					var result = EditorUtility.DisplayDialog("Remove selected brush?", "Are you sure that you want to remove ''" + BrushPresets.Instance.Presets[selectedPresetIndex - 1].Name + "'' brush?", "Remove", "Cancel");
					if (result)
					{
						BrushPresets.Instance.Presets.RemoveAt(selectedPresetIndex - 1);
						selectedPresetIndex = 0;
						foreach (var script in targets)
						{
							var paintManager = script as PaintManager;
							if (paintManager != null)
							{
								paintManager.Brush.Name = string.Empty;
								EditorHelper.MarkAsDirty(paintManager);
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

				var hasPresetWithSameName = showWarning = savedNameTrimmed == DefaultPresetName || hasSavedPresetWithSameName || string.IsNullOrEmpty(savedNameTrimmed);
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
						var selectedBrush = selectedPresetIndex == 0 ? paintController.Brush : BrushPresets.Instance.Presets[selectedPresetIndex - 1];
						var preset = new Brush(selectedBrush)
						{
							Name = savedNameTrimmed
						};
						if (rename)
						{
							BrushPresets.Instance.Presets[selectedPresetIndex - 1] = preset;
						}
						else
						{
							BrushPresets.Instance.Presets.Insert(selectedPresetIndex, preset);
							if (sortPresetsByName)
							{
								BrushPresets.Instance.Presets.Sort((s1, s2) => string.Compare(s1.Name, s2.Name, StringComparison.Ordinal));
								selectedPresetIndex = BrushPresets.Instance.Presets.IndexOf(preset) + 1;
							}
							else
							{
								selectedPresetIndex++;
							}
						}
						foreach (var script in targets)
						{
							var paintManager = script as PaintManager;
							if (paintManager != null)
							{
								paintManager.Brush = preset.Clone();
								EditorHelper.MarkAsDirty(paintManager);
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
	}
}