using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core.PaintModes;
using XDPaint.Tools;

namespace XDPaint.Core.Materials
{
	[Serializable]
	public class Brush : IBrush, IDisposable
	{
		#region Events
		
		public delegate void ChangeColorHandler(Color color);
		public delegate void ChangeTextureHandler(Texture texture);
		public event ChangeColorHandler OnChangeColor;
		public event ChangeTextureHandler OnChangeTexture;
		
		#endregion
		
		#region Properties and variables

		[SerializeField] private string name = "Brush 1";
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		[SerializeField] private Material material;
		public Material Material
		{
			get { return material; }
		}
		
		[SerializeField] private FilterMode filterMode = FilterMode.Point;
		public FilterMode FilterMode
		{
			get { return filterMode; }
			set { filterMode = value; }
		}

		[SerializeField] private Color color = Color.white;
		public Color Color { get { return color; } }
		
		[SerializeField] private Texture sourceTexture;

		public Texture SourceTexture
		{
			get { return sourceTexture; }
			set { sourceTexture = value; }
		}
                 
		[SerializeField] private RenderTexture renderTexture;
		public RenderTexture RenderTexture { get { return renderTexture; } }

		private Vector2 sourceTextureSize;
		public Vector2 SourceTextureSize { get { return sourceTextureSize; } }

		private float minSize;
		public float MinSize
		{
			get { return minSize; }
			private set { minSize = value; }
		}

		[SerializeField] private float size = 1f;
		public float Size
		{
			get { return size; }
			set
			{
				size = value;
				size = Mathf.Clamp(size, minSize, float.MaxValue);
			}
		}

		[SerializeField] private float renderAngle;
		public float RenderAngle
		{
			get { return renderAngle; }
			set
			{
				if (renderAngle != value)
				{
					renderAngle = value;
					renderQuaternion = Quaternion.identity;
					renderQuaternion.eulerAngles = new Vector3(0, 0, renderAngle);
					if (initialized)
					{
						Render();
					}
				}
			}
		}

		private Quaternion renderQuaternion; 
		public Quaternion RenderQuaternion { get { return renderQuaternion; } }
		
		[SerializeField] private float hardness = 0.9f;
		public float Hardness
		{
			get { return hardness; }
			set
			{
				hardness = value;
				if (initialized)
				{
					renderMaterial.SetFloat(HardnessParam, hardness);
					Render();
				}
			}
		}
		
		[SerializeField] private bool preview;
		public bool Preview
		{
			get { return preview; }
			set { preview = value; }
		}

		private IPaintMode paintMode;
		private Material renderMaterial;
		private Mesh quadMesh;
		private CommandBufferBuilder commandBufferBuilder;
		private RenderTargetIdentifier renderTarget;
		private bool initialized;
		private int padding = 2;

		public const string SrcColorBlend = "_SrcColorBlend";
		public const string DstColorBlend = "_DstColorBlend";
		public const string SrcAlphaBlend = "_SrcAlphaBlend";
		public const string DstAlphaBlend = "_DstAlphaBlend";
		public const string BlendOpColor = "_BlendOpColor";
		public const string BlendOpAlpha = "_BlendOpAlpha";
		public const string HardnessParam = "_Hardness";
		public const string ScaleUVParam = "_ScaleUV";
		public const string OffsetParam = "_Offset";
		private const int DefaultPadding = 2;
		private const float SqrtTwo = 1.41421356237309504880168872420969807856967187537694f;
		#endregion

		public Brush()
		{
		}

		public Brush(Brush brush)
		{
			material = brush.material;
			color = brush.Color;
			sourceTexture = brush.SourceTexture;
			renderTexture = brush.RenderTexture;
			size = brush.Size;
			renderAngle = brush.renderAngle;
			hardness = brush.hardness;
		}
	
		public void Init(IPaintMode mode)
		{
			paintMode = mode;
			if (mode == null)
			{
				Debug.LogError("Mode is null!");
				return;
			}
			if (OnChangeColor != null)
			{
				OnChangeColor(color);
			}
			if (OnChangeTexture != null)
			{
				OnChangeTexture(renderTexture);
			}
			if (sourceTexture == null)
			{
				sourceTexture = Settings.Instance.DefaultBrush;
			}
			commandBufferBuilder = new CommandBufferBuilder("XDPaintBrush");
			InitRenderTexture();
			InitMaterials(mode);
			Render();
			initialized = true;
		}

		[Obsolete("Method was obsolete, please use DoDispose instead")]
		public void Destroy()
		{
			DoDispose();
		}

		public void DoDispose()
		{
			if (commandBufferBuilder != null)
			{
				commandBufferBuilder.Release();
			}
			if (quadMesh != null)
			{
				UnityEngine.Object.Destroy(quadMesh);
			}
			if (material != null)
			{
				UnityEngine.Object.Destroy(material);
			}
			renderTexture.ReleaseTexture();
			initialized = false;
		}

		public void SetValues(Brush brush)
		{
			name = brush.name;
			if (brush.material != null)
			{
				material = brush.material;
			}
			filterMode = brush.filterMode;
			color = brush.color;
			if (brush.renderTexture != null)
			{
				renderTexture = brush.renderTexture;
			}
			size = brush.size;
			renderAngle = brush.renderAngle;
			hardness = brush.hardness;
			preview = brush.preview;
			if (Application.isPlaying && initialized)
			{
				SetTexture(brush.sourceTexture, true, false);
			}
			else
			{
				sourceTexture = brush.sourceTexture;
			}
		}

		private void InitQuadMesh()
		{
			var pixel = 1f / Mathf.Max(renderTexture.width, renderTexture.height);
			var pixelsPadding = pixel;
			var borderPixelPadding = 1f - pixelsPadding;
			var center = new Vector3(0.5f, 0.5f, 0);
			var quaternion = RenderQuaternion;
			if (quadMesh == null)
			{
				quadMesh = new Mesh();
			}
			quadMesh.vertices = new[]
			{
				quaternion * (new Vector3(pixelsPadding, borderPixelPadding, 0f) - center) + center,
				quaternion * (new Vector3(borderPixelPadding, borderPixelPadding, 0) - center) + center,
				quaternion * (new Vector3(borderPixelPadding, pixelsPadding, 0f) - center) + center,
				quaternion * (new Vector3(pixelsPadding, pixelsPadding, 0) - center) + center
			};
			quadMesh.uv = new[] { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
			quadMesh.triangles = new[] { 0, 1, 2, 2, 3, 0 };
			quadMesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
		}
		
		private void InitRenderTexture()
		{
			var max = Mathf.Max(sourceTexture.width, sourceTexture.height);
			var side = Mathf.RoundToInt(max * SqrtTwo);
			var paddingOffset = side % 2;
			padding = DefaultPadding + paddingOffset;
			minSize = 2f / (side - padding);
			var width = side + padding;
			sourceTextureSize = new Vector2(sourceTexture.width, sourceTexture.height);
			if (renderTexture != null && renderTexture.IsCreated())
			{
				renderTexture.Release();
				renderTexture.width = width;
				renderTexture.height = width;
				renderTexture.Create();
			}
			else
			{
				renderTexture = RenderTextureFactory.CreateRenderTexture(width, width, 0, RenderTextureFormat.ARGB32, filterMode);
			}
			renderTarget = new RenderTargetIdentifier(renderTexture);
			commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(renderTarget).ClearRenderTarget().Execute();
		}

		private void InitMaterials(IPaintMode mode)
		{
			paintMode = mode;
			if (material == null)
			{
				material = new Material(Settings.Instance.BrushShader);
			}
			material.color = color;
			material.mainTexture = renderTexture;
			if (renderMaterial == null)
			{
				renderMaterial = new Material(Settings.Instance.BrushRenderShader);
			}
			renderMaterial.mainTexture = sourceTexture;
			renderMaterial.color = color;
			renderMaterial.SetFloat(HardnessParam, hardness);
			var sizeInPixSource = 1f / sourceTexture.width;
			var scale = SqrtTwo + sizeInPixSource * padding / 2f;
			float offset;
			if (sourceTexture.width >= sourceTexture.height)
			{
				offset = sizeInPixSource * (renderTexture.width - sourceTexture.width) / 2f - sizeInPixSource / 2f;
			}
			else
			{
				offset = sizeInPixSource * (renderTexture.height - sourceTexture.height) / 2f - sizeInPixSource / 2f;
			}
			renderMaterial.SetFloat(ScaleUVParam, scale);
			renderMaterial.SetFloat(OffsetParam, offset);
		}

		public void Render()
		{
			InitQuadMesh();
			commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(renderTarget).ClearRenderTarget().DrawMesh(quadMesh, renderMaterial).Execute();
		}

		private void SetBlendingOptions(PaintTool paintTool)
		{
			if (paintTool == PaintTool.Erase && !paintMode.UsePaintInput)
			{
				SetEraseBlending();
			}
			else
			{
				SetPaintBlending();
			}
		}

		private void SetEraseBlending()
		{
			material.SetInt(BlendOpColor, (int)BlendOp.Add);
			material.SetInt(BlendOpAlpha, (int)BlendOp.ReverseSubtract);
			material.SetInt(SrcColorBlend, (int)BlendMode.Zero);
			material.SetInt(DstColorBlend, (int)BlendMode.One);
			material.SetInt(SrcAlphaBlend, (int)BlendMode.SrcAlpha);
			material.SetInt(DstAlphaBlend, (int)BlendMode.OneMinusSrcAlpha);
		}
		
		private void SetPaintBlending()
		{
			material.SetInt(BlendOpColor, (int) BlendOp.Add);
			material.SetInt(BlendOpAlpha, (int) BlendOp.Add);
			material.SetInt(SrcColorBlend, (int) BlendMode.SrcAlpha);
			material.SetInt(DstColorBlend, (int) BlendMode.OneMinusSrcAlpha);
			material.SetInt(SrcAlphaBlend, (int) BlendMode.SrcAlpha);
			material.SetInt(DstAlphaBlend, (int) BlendMode.One);
		}

		public void SetColor(Color colorValue, bool render = true, bool sendToEvent = true)
		{
			color = colorValue;
			if (!initialized) 
				return;
			
			material.color = color;
			renderMaterial.color = color;
			if (render)
			{
				Render();
			}
			if (sendToEvent && OnChangeColor != null)
			{
				OnChangeColor(color);
			}
		}

		public void SetTexture(Texture texture, bool render = true, bool sendToEvent = true, bool canUpdateRenderTexture = true)
		{
			var sourceTextureWidth = 0f;
			var sourceTextureHeight = 0f;
			if (sourceTexture != null)
			{
				sourceTextureWidth = sourceTexture.width;
				sourceTextureHeight = sourceTexture.height;
			}
			sourceTexture = texture;
			if (!initialized)
				return;

			renderMaterial.mainTexture = sourceTexture;
			material.mainTexture = sourceTexture;

			if (canUpdateRenderTexture && (sourceTextureWidth != sourceTexture.width || sourceTextureHeight != sourceTexture.height))
			{
				InitRenderTexture();
				render = true;
			}
			
			if (render)
			{
				Render();
			}
			
			material.mainTexture = renderTexture;

			if (sendToEvent && OnChangeTexture != null)
			{
				OnChangeTexture(renderTexture);
			}
		}

		public void SetPaintTool(PaintTool paintTool)
		{
			if (!initialized)
				return;
			SetBlendingOptions(paintTool);
		}

		public void SetPaintMode(IPaintMode mode)
		{
			paintMode = mode;
			var blendValue = paintMode.UsePaintInput ? (int)BlendMode.OneMinusSrcAlpha : (int)BlendMode.One;
			Material.SetInt(DstAlphaBlend, blendValue);
		}

		public Brush Clone()
		{
			var clone = MemberwiseClone() as Brush;
			return clone;
		}
	}
}