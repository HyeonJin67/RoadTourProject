using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Controllers;
using XDPaint.Controllers.InputData;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Core;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Core.PaintObject;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;
using IDisposable = XDPaint.Core.IDisposable;

namespace XDPaint
{
    [DisallowMultipleComponent]
    public class PaintManager : MonoBehaviour, IDisposable
    {
        #region Events

        public delegate void InitHandler(PaintManager paintManager);
        public delegate void DisposeHandler();
        public event InitHandler OnInitialized;
        public event DisposeHandler OnDisposed;

        #endregion
        
        #region Properties and variables

        public GameObject ObjectForPainting;
        public Paint Material = new Paint();
        public bool ShouldOverrideCamera;
        public bool CopySourceTextureToPaintTexture = true;
        public BasePaintObject PaintObject { get; private set; }
        [SerializeField] private PaintMode paintModeType;
        
        [SerializeField] private FilterMode filterMode;
        public FilterMode FilterMode
        {
            get { return filterMode; }
            set { filterMode = value; }
        }

        [NonSerialized] private Brush currentBrush;
        [SerializeField] private Brush brush = new Brush();
        public Brush Brush
        {
            get
            {
                if (Application.isPlaying)
                {
                    return currentBrush;
                }
                return brush;
            }
            set
            {
                if (Application.isPlaying)
                {
                    currentBrush = value;
                }
                else
                {
                    brush = value;
                }
                if (Application.isPlaying)
                {
                    currentBrush.Init(mode);
                    currentBrush.SetPaintTool(PaintController.Instance.UseSharedSettings
                        ? PaintController.Instance.Tool
                        : paintTool);
                }
                if (initialized)
                {
                    PaintObject.Brush = currentBrush;
                    Material.SetPreviewTexture(currentBrush.RenderTexture);
                }
            }
        }
        
        [SerializeField] private ToolsManager toolsManager;
        public ToolsManager ToolsManager { get { return toolsManager; } }
        
        [SerializeField] private PaintTool paintTool;
        public PaintTool Tool
        {
            get { return toolsManager.CurrentTool.Type; } 
            set
            {
                paintTool = value;
                if (initialized)
                {
                    currentBrush.SetPaintTool(paintTool);
                    Material.SetPaintTool(paintTool);
                    Material.SetPreviewTexture(currentBrush.RenderTexture);
                    if (toolsManager != null)
                    {
                        toolsManager.SetTool(paintTool);
                        toolsManager.CurrentTool.SetPaintMode(mode);
                        PaintObject.Tool = toolsManager.CurrentTool;
                    }
                }
            }
        }

        [SerializeField] private Camera overrideCamera;
        public Camera Camera
        {
            private get { return ShouldOverrideCamera || overrideCamera == null ? Camera.main : overrideCamera; }
            set
            {
                overrideCamera = value;
                if (InputController.Instance != null)
                {
                    InputController.Instance.Camera = overrideCamera;
                }
                if (RaycastController.Instance != null)
                {
                    RaycastController.Instance.Camera = overrideCamera;
                }
                if (initialized)
                {
                    PaintObject.Camera = overrideCamera;
                }
            }
        }
        

        [SerializeField] private bool useSourceTextureAsBackground = true;
        public bool UseSourceTextureAsBackground
        {
            get { return useSourceTextureAsBackground; }
            set
            {
                useSourceTextureAsBackground = value;
                if (!Application.isPlaying)
                {
                    if (!useSourceTextureAsBackground)
                    {
                        ClearTrianglesNeighborsData();
                    }
                }
            }
        }

        [SerializeField] private bool useNeighborsVerticesForRaycasts;
        public bool UseNeighborsVerticesForRaycasts
        {
            get { return useNeighborsVerticesForRaycasts; }
            set
            {
                useNeighborsVerticesForRaycasts = value;
                if (!Application.isPlaying)
                {
                    if (!useNeighborsVerticesForRaycasts)
                    {
                        ClearTrianglesNeighborsData();
                    }
                }
                if (initialized)
                {
                    PaintObject.UseNeighborsVertices = useNeighborsVerticesForRaycasts;
                }
            }
        }

        public bool HasTrianglesData { get { return triangles != null && triangles.Length > 0; } }
        public bool Initialized { get { return initialized; } }

        [SerializeField] private TrianglesContainer trianglesContainer;
        [SerializeField] private Triangle[] triangles;
        private CanvasGraphicRaycaster canvasGraphicRaycaster;
        private IRenderTextureHelper renderTextureHelper;
        private IRenderComponentsHelper renderComponentsHelper;
        private ObjectComponentType componentType;
        private IPaintMode mode;
        private InputDataBase input;
        private bool initialized;

        #endregion

        private void Start()
        {
            if (initialized)
                return;
            
            Init();
        }

        private void Update()
        {
            if (initialized && (PaintObject.IsPainting || currentBrush.Preview))
            {
                Render();
            }
        }

        private void OnDestroy()
        {
            DoDispose();
        }

        public void Init()
        {
            initialized = false;
            if (ObjectForPainting == null)
            {
                Debug.LogError("ObjectForPainting is null!");
                return;
            }

            RestoreSourceMaterialTexture();
            
            if (renderComponentsHelper == null)
            {
                renderComponentsHelper = new RenderComponentsHelper();
            }
            renderComponentsHelper.Init(ObjectForPainting, out componentType);
            if (componentType == ObjectComponentType.Unknown)
            {
                Debug.LogError("Unknown component type!");
                return;
            }

            if (ControllersContainer.Instance == null)
            {
                var containerGameObject = new GameObject(Settings.Instance.ContainerGameObjectName);
                containerGameObject.AddComponent<ControllersContainer>();
            }

            if (renderComponentsHelper.IsMesh())
            {
                var paintComponent = renderComponentsHelper.PaintComponent;
                var renderComponent = renderComponentsHelper.RendererComponent;
                var mesh = renderComponentsHelper.GetMesh();
                if (trianglesContainer != null)
                {
                    triangles = trianglesContainer.Data;
                }
                if (triangles == null || triangles.Length == 0)
                {
                    if (mesh != null)
                    {
                        Debug.LogWarning("PaintManager does not have triangles data! Getting it may take a while.");
                        triangles = TrianglesData.GetData(mesh, useNeighborsVerticesForRaycasts);
                    }
                    else
                    {
                        Debug.LogError("Mesh is null!");
                        return;
                    }
                }
                RaycastController.Instance.InitObject(Camera, paintComponent, renderComponent, triangles);
            }

            //init material
            Material.Init(renderComponentsHelper);
            InitRenderTexture();
            
            //register PaintManager
            PaintController.Instance.RegisterPaintManager(this);

            InitBrush();
            InitPaintObject();
            if (toolsManager != null)
            {
                toolsManager.DoDispose();
            }
            toolsManager = new ToolsManager(this, paintTool);
            toolsManager.Init();
            toolsManager.CurrentTool.SetPaintMode(mode);
            PaintObject.Tool = toolsManager.CurrentTool;
            
            InputController.Instance.Camera = Camera;
            SubscribeInputEvents(componentType);
            initialized = true;
            Tool = paintTool;
            Render();

            if (OnInitialized != null)
            {
                OnInitialized(this);
            }
        }

        public void DoDispose()
        {
            if (initialized)
            {
                //unregister PaintManager
                PaintController.Instance.UnRegisterPaintManager(this);
                //restore source material and texture
                RestoreSourceMaterialTexture();
                //free tools
                toolsManager.DoDispose();
                //free brush resources
                if (brush != null)
                {
                    brush.OnChangeTexture -= Material.SetPreviewTexture;
                    brush.DoDispose();
                }
                //destroy created material
                Material.DoDispose();
                //free RenderTextures
                renderTextureHelper.ReleaseTextures();
                //destroy raycast data
                if (renderComponentsHelper.IsMesh())
                {
                    var renderComponent = renderComponentsHelper.RendererComponent;
                    RaycastController.Instance.DestroyMeshData(renderComponent);
                }
                //unsubscribe input events
                UnsubscribeInputEvents();
                input.OnDestroy();
                //free undo/redo RenderTextures and meshes
                PaintObject.DoDispose();
                initialized = false;
                if (OnDisposed != null)
                {
                    OnDisposed();
                }
            }
        }

        public void Render()
        {
            if (initialized)
            {
                PaintObject.OnRender();
                PaintObject.Render();
            }
        }
        
        public void FillTrianglesData(bool fillNeighbors = true)
        {
            if (renderComponentsHelper == null)
            {
                renderComponentsHelper = new RenderComponentsHelper();
            }
            ObjectComponentType componentType;
            renderComponentsHelper.Init(ObjectForPainting, out componentType);
            if (componentType == ObjectComponentType.Unknown)
            {
                return;
            }
            if (renderComponentsHelper.IsMesh())
            {
                var mesh = renderComponentsHelper.GetMesh();
                if (mesh != null)
                {
                    triangles = TrianglesData.GetData(mesh, fillNeighbors);
                    if (fillNeighbors)
                    {
                        Debug.Log("Added triangles with neighbors data. Triangles count: " + triangles.Length);
                    }
                    else
                    {
                        Debug.Log("Added triangles data. Triangles count: " + triangles.Length);
                    }
                }
                else
                {
                    Debug.LogError("Mesh is null!");
                }
            }
        }

        public void SetPaintMode(PaintMode paintMode)
        {
            paintModeType = paintMode;
            if (Application.isPlaying)
            {
                mode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
                toolsManager.CurrentTool.SetPaintMode(mode);
                PaintObject.SetPaintMode(mode);
                currentBrush.SetPaintMode(mode);
                currentBrush.SetPaintTool(paintTool);
                Material.SetPreviewTexture(currentBrush.RenderTexture);
            }
        }

        public IPaintMode GetPaintMode()
        {
            if (initialized && Application.isPlaying)
            {
                mode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
            }
            return mode;
        }
        
        public void ClearTrianglesData()
        {
            triangles = null;
        }

        public void ClearTrianglesNeighborsData()
        {
            if (triangles != null)
            {
                foreach (var triangle in triangles)
                {
                    triangle.N.Clear();
                }
            }
        }
                
        public Triangle[] GetTriangles()
        {
            return triangles;
        }

        public void SetTriangles(Triangle[] trianglesData)
        {
            triangles = trianglesData;
        }

        public void SetTriangles(TrianglesContainer trianglesContainerData)
        {
            trianglesContainer = trianglesContainerData;
            triangles = trianglesContainer.Data;
        }

        [Obsolete("This method marked as obsolete and will be removed in future updates, use GetPaintInputTexture()", false)]
        public RenderTexture GetRenderTextureLine()
        {
            return GetPaintInputTexture();
        }

        public RenderTexture GetPaintTexture()
        {
            return renderTextureHelper.GetTexture(RenderTarget.Paint);
        }

        public RenderTexture GetPaintInputTexture()
        {
            return renderTextureHelper.GetTexture(RenderTarget.PaintInput);
        }

        public RenderTexture GetResultRenderTexture()
        {
            return renderTextureHelper.GetTexture(RenderTarget.Combined);
        }

        /// <summary>
        /// Returns result texture
        /// </summary>
        /// <returns></returns>
        public Texture2D GetResultTexture()
        {
            RenderTexture temp = null;
            var renderTexture = renderTextureHelper.GetTexture(RenderTarget.Combined);
            if (renderComponentsHelper.ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var spriteRenderer = renderComponentsHelper.RendererComponent as SpriteRenderer;
                if (spriteRenderer != null && spriteRenderer.material != null && spriteRenderer.material.shader == Settings.Instance.SpriteMaskShader)
                {
                    temp = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format);
                    var rti = new RenderTargetIdentifier(temp);
                    var commandBufferBuilder = new CommandBufferBuilder("ResultTexture");
                    commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(rti).ClearRenderTarget().Execute();
                    Graphics.Blit(spriteRenderer.material.mainTexture, temp, spriteRenderer.material);
                    commandBufferBuilder.Release();
                }
            }
            var resultTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = temp != null ? temp : renderTexture;
            resultTexture.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0, false);
            resultTexture.Apply();
            RenderTexture.active = previousRenderTexture;
            if (temp != null)
            {
                RenderTexture.ReleaseTemporary(temp);
            }
            return resultTexture;
        }
        
        /// <summary>
        /// Bakes into Material source texture
        /// </summary>
        public void Bake()
        {
            PaintObject.FinishPainting();
            Render();
            var prevRenderTexture = RenderTexture.active;
            var renderTexture = GetResultRenderTexture();
            RenderTexture.active = renderTexture;
            if (Material.SourceTexture != null)
            {
                var texture = Material.SourceTexture as Texture2D;
                if (texture != null)
                {
                    texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                    texture.Apply();
                }
            }
            RenderTexture.active = prevRenderTexture;
        }

        /// <summary>
        /// Restore source material and texture
        /// </summary>
        private void RestoreSourceMaterialTexture()
        {
            if (initialized && Material.SourceMaterial != null)
            {
                if (Material.SourceMaterial.GetTexture(Material.ShaderTextureName) == null)
                {
                    Material.SourceMaterial.SetTexture(Material.ShaderTextureName, Material.SourceTexture);
                }
                renderComponentsHelper.SetSourceMaterial(Material.SourceMaterial, Material.Index);
            }
        }
        
        public void InitBrush()
        {
            if (PaintController.Instance.UseSharedSettings)
            {
                currentBrush = PaintController.Instance.Brush;
            }
            else
            {
                if (currentBrush != null)
                {
                    currentBrush.OnChangeTexture -= Material.SetPreviewTexture;
                }
                currentBrush = brush;
                currentBrush.Init(mode);
                if (PaintObject != null)
                {
                    PaintObject.Brush = currentBrush;
                }
                currentBrush.SetPaintMode(mode);
                currentBrush.SetPaintTool(paintTool);
            }
            Material.SetPreviewTexture(currentBrush.RenderTexture);
            currentBrush.OnChangeTexture -= Material.SetPreviewTexture;
            currentBrush.OnChangeTexture += Material.SetPreviewTexture;
        }

        private void InitPaintObject()
        {
            if (PaintObject != null)
            {
                UnsubscribeInputEvents();
                PaintObject.DoDispose();
            }
            if (renderComponentsHelper.ComponentType == ObjectComponentType.RawImage)
            {
                PaintObject = new CanvasRendererPaint();
            }
            else if (renderComponentsHelper.ComponentType == ObjectComponentType.SpriteRenderer)
            {
                PaintObject = new SpriteRendererPaint();
            }
            else
            {
                PaintObject = new MeshRendererPaint();
            }
            PaintObject.Init(Camera, ObjectForPainting.transform, Material, renderTextureHelper, CopySourceTextureToPaintTexture);
            PaintObject.Brush = currentBrush;
            PaintObject.SetPaintMode(mode);
            PaintObject.UseNeighborsVertices = useNeighborsVerticesForRaycasts;
        }

        private void InitRenderTexture()
        {
            mode = PaintController.Instance.GetPaintMode(PaintController.Instance.UseSharedSettings ? PaintController.Instance.PaintMode : paintModeType);
            if (renderTextureHelper == null)
            {
                renderTextureHelper = new RenderTextureHelper();
            }
            renderTextureHelper.Init(Material.SourceTexture.width, Material.SourceTexture.height, filterMode);
            if (Material.SourceTexture != null)
            {
                Graphics.Blit(Material.SourceTexture, renderTextureHelper.GetTexture(RenderTarget.Combined));
            }
            Material.SetObjectMaterialTexture(renderTextureHelper.GetTexture(RenderTarget.Combined));
            Material.SetPaintTexture(renderTextureHelper.GetTexture(RenderTarget.Paint));
            Material.SetInputTexture(renderTextureHelper.GetTexture(RenderTarget.PaintInput));
        }

        #region Setup input events
        
        private void SubscribeInputEvents(ObjectComponentType componentType)
        {
            if (input != null)
            {
                UnsubscribeInputEvents();
                input.OnDestroy();
            }
            input = new InputDataResolver().Resolve(componentType);
            input.Init(this, Camera);
            UpdatePreviewInput();
            InputController.Instance.OnUpdate -= input.OnUpdate;
            InputController.Instance.OnUpdate += input.OnUpdate;
            InputController.Instance.OnMouseDown -= input.OnDown;
            InputController.Instance.OnMouseDown += input.OnDown;
            InputController.Instance.OnMouseButton -= input.OnPress;
            InputController.Instance.OnMouseButton += input.OnPress;
            InputController.Instance.OnMouseUp -= input.OnUp;
            InputController.Instance.OnMouseUp += input.OnUp;
        }

        private void UnsubscribeInputEvents()
        {
            InputController.Instance.OnUpdate -= input.OnUpdate;
            InputController.Instance.OnMouseHover -= input.OnHover;
            InputController.Instance.OnMouseDown -= input.OnDown;
            InputController.Instance.OnMouseButton -= input.OnPress;
            InputController.Instance.OnMouseUp -= input.OnUp;
        }

        public void UpdatePreviewInput()
        {
            if (currentBrush.Preview)
            {
                InputController.Instance.OnMouseHover -= input.OnHover;
                InputController.Instance.OnMouseHover += input.OnHover;
            }
            else
            {
                InputController.Instance.OnMouseHover -= input.OnHover;
            }
        }

        #endregion
    }
}