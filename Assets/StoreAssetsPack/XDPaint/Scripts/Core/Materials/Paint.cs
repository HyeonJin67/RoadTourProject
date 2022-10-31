using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Tools;
using Object = UnityEngine.Object;

namespace XDPaint.Core.Materials
{
	[Serializable]
	public class Paint : IDisposable
	{
		#region Properties and variables
		private Material material;
		public Material Material { get { return material; } }

		[SerializeField] private string shaderTextureName = "_MainTex";
		public string ShaderTextureName
		{
			get { return shaderTextureName; }
			set { shaderTextureName = value; }
		}
		
		[SerializeField] private int defaultTextureWidth = 1024;
		public int DefaultTextureWidth
		{
			get { return defaultTextureWidth; }
			set { defaultTextureWidth = value; }
		}
        
		[SerializeField] private int defaultTextureHeight = 1024;
		public int DefaultTextureHeight
		{
			get { return defaultTextureHeight; }
			set { defaultTextureHeight = value; }
		}

		private int index;
		public int Index { get { return index; } }
		
		private Texture sourceTexture;
		public Texture SourceTexture { get { return sourceTexture; } }

		public Material SourceMaterial;
		private IRenderComponentsHelper renderComponentsHelper;
		private Material objectMaterial;
		private bool initialized;

		public const string PaintTextureShaderParam = "_PaintTex";
		public const string InputTextureShaderParam = "_InputTex";
		public const string BrushTextureShaderParam = "_BrushTex";
		public const string BrushOffsetShaderParam = "_BrushOffset";
		public const string SrcColorBlend = "_SrcColorBlend";
		public const string DstColorBlend = "_DstColorBlend";
		public const string SrcAlphaBlend = "_SrcAlphaBlend";
		public const string DstAlphaBlend = "_DstAlphaBlend";
		public const int BackgroundPass = 0;
		public const int PaintPass = 1;
		public const int BlendPass = 2;
		public const int ErasePass = 3;
		public const int PreviewPass = 4;
		#endregion

		public void Init(IRenderComponentsHelper renderComponents)
		{
			DoDispose();
			renderComponentsHelper = renderComponents;
			index = renderComponents.GetMaterialIndex(SourceMaterial);
			if (SourceMaterial != null || SourceMaterial != null && objectMaterial == null)
 			{
	            objectMaterial = Object.Instantiate(SourceMaterial);
            }
			else if (renderComponentsHelper.Material != null)
			{
				objectMaterial = Object.Instantiate(renderComponentsHelper.Material);
			}
			sourceTexture = renderComponentsHelper.GetSourceTexture(objectMaterial, shaderTextureName, defaultTextureWidth, defaultTextureHeight);
			material = new Material(Settings.Instance.PaintShader) {mainTexture = SourceTexture};
			initialized = true;
		}

		public void InitMaterial(Material material)
		{
			if (SourceMaterial == null)
			{
				if (material != null)
				{
					SourceMaterial = material;
				}
			}
		}
		
		[Obsolete("Method was obsolete, please use DoDispose instead")]
		public void Destroy()
		{
			DoDispose();
		}

		public void DoDispose()
		{
			if (objectMaterial != null)
			{
				Object.Destroy(objectMaterial);
				objectMaterial = null;
			}
			if (material != null)
			{
				Object.Destroy(material);
				material = null;
			}
			initialized = false;
		}

		public void RestoreTexture()
		{
			if (!initialized)
				return;
			if (SourceTexture != null)
			{
				objectMaterial.SetTexture(shaderTextureName, SourceTexture);
			}
			else
			{
				renderComponentsHelper.Material = SourceMaterial;
			}
		}

		public void SetObjectMaterialTexture(Texture texture)
		{
			if (!initialized)
				return;
			objectMaterial.SetTexture(shaderTextureName, texture);
			renderComponentsHelper.SetSourceMaterial(objectMaterial, index);
		}

		public void SetPreviewTexture(Texture texture)
		{
			if (!initialized)
				return;
			material.SetTexture(BrushTextureShaderParam, texture);
		}

		public void SetPaintTexture(Texture texture)
		{
			if (!initialized)
				return;
			material.SetTexture(PaintTextureShaderParam, texture);
		}
		
		public void SetInputTexture(Texture texture)
		{
			if (!initialized)
				return;
			material.SetTexture(InputTextureShaderParam, texture);
		}

		public void SetPaintPreviewVector(Vector4 brushOffset)
		{
			if (!initialized)
				return;
			material.SetVector(BrushOffsetShaderParam, brushOffset);
		}
		
		public void SetPaintTool(PaintTool paintTool)
		{
			if (!initialized)
				return;
			SetBlendingOptions(paintTool);
		}
		
		private void SetBlendingOptions(PaintTool paintTool)
		{
			if (paintTool == PaintTool.Erase)
			{
				material.SetInt(SrcColorBlend, (int) BlendMode.SrcAlpha);
				material.SetInt(DstColorBlend, (int) BlendMode.OneMinusSrcAlpha);
				material.SetInt(SrcAlphaBlend, (int) BlendMode.SrcAlpha);
				material.SetInt(DstAlphaBlend, (int) BlendMode.OneMinusSrcAlpha);
			}
			else
			{
				material.SetInt(SrcColorBlend, (int) BlendMode.SrcAlpha);
				material.SetInt(DstColorBlend, (int) BlendMode.OneMinusSrcAlpha);
				material.SetInt(SrcAlphaBlend, (int) BlendMode.SrcAlpha);
				material.SetInt(DstAlphaBlend, (int) BlendMode.One);
			}
		}
	}
}