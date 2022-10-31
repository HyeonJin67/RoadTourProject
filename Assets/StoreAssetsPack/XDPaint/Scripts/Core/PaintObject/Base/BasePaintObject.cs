using System;
#if (UNITY_2017 || UNITY_2018) && UNITY_EDITOR
using System.Collections;
using XDPaint.Controllers;
#endif
using UnityEngine;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintObject.States;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
    public abstract class BasePaintObject : BasePaintObjectRenderer
    {
        #region Events
        
        public delegate void PaintDataHandler(BasePaintObject sender, Vector2 paintPosition, float brushSize, float pressure, Color brushColor, PaintTool tool);
        public delegate void PaintHandler(BasePaintObject sender, Vector2 paintPosition, float pressure);
        public delegate void MouseUVHandler(BasePaintObject sender, Vector2 uv, Vector2 paintPosition, float pressure);
        public delegate void MouseUpHandler(BasePaintObject sender, bool inBounds);
        public delegate void TexturesKeeperHandler(BasePaintObject sender);
        public delegate void DrawPointHandler(BasePaintObject sender, Vector2 position, float pressure);
        public delegate void DrawLineHandler(BasePaintObject sender, Vector2 lineStartPosition, Vector2 lineEndPosition, float lineStartPressure, float lineEndPressure);
        public event PaintDataHandler OnPaintDataHandler;
        public event PaintHandler OnPaintHandler;
        public event MouseUVHandler OnMouseHoverHandler;
        public event MouseUVHandler OnMouseDownHandler;
        public event MouseUVHandler OnMouseHandler;
        public event MouseUpHandler OnMouseUpHandler;
        public event TexturesKeeperHandler OnUndoHandler;
        public event TexturesKeeperHandler OnRedoHandler;
        public event DrawPointHandler OnDrawPointHandler;
        public event DrawLineHandler OnDrawLineHandler;
        
        #endregion

        #region Properties and variables
        
        public bool IsPainting { get; private set; }
        public bool IsPainted { get; private set; }
        public bool ProcessInput = true;

        private Camera thisCamera;
        public new Camera Camera
        {
            protected get { return thisCamera; }
            set
            {
                thisCamera = value;
                base.Camera = thisCamera;
            }
        }
        protected Vector2? PaintPosition { private get; set; }
        protected Transform ObjectTransform { get; private set; }
        
        private float pressure = 1f;
        private float Pressure
        {
            get { return Mathf.Clamp(pressure, 0.01f, 10f); }
            set { pressure = value; }
        }

        public TexturesKeeper TextureKeeper;

        private LineData lineData;
        private Vector2 previousPaintPosition;
#pragma warning disable 414
        private bool initialized;
#pragma warning restore 414
        private bool shouldReDraw;
        private bool shouldClearTexture = true;
        private bool writeClear;
        private const float HalfTextureRatio = 0.5f;
        
        #endregion

        #region Abstract methods
        protected abstract void Init();
        protected abstract void OnPaint(Vector3 position, Vector2? uv = null);
        protected abstract bool IsInBounds(Vector3 position);

        #endregion

        public void Init(Camera camera, Transform objectTransform, Paint paint, 
            IRenderTextureHelper renderTextureHelper, bool copySourceTextureToPaintTexture)
        {
            RenderTextureHelper = renderTextureHelper;
            thisCamera = camera;
            InitRenderer(Camera, paint, copySourceTextureToPaintTexture);
            ObjectTransform = objectTransform;
            PaintMaterial = paint;
            lineData = new LineData();
            InitStateKeeper();
            Init();
            initialized = true;
        }

        public override void DoDispose()
        {
            initialized = false;
            if (TextureKeeper != null)
            {
                TextureKeeper.Reset();
            }
            base.DoDispose();
        }

        private void InitStateKeeper()
        {
            if (TextureKeeper != null)
            {
                TextureKeeper.Reset();
            }
            TextureKeeper = new TexturesKeeper();
            TextureKeeper.Init(OnExtraDraw, Settings.Instance.UndoRedoEnabled);
            TextureKeeper.OnResetState = () => shouldClearTexture = true;
            TextureKeeper.OnChangeState = () => shouldReDraw = true;
            TextureKeeper.OnUndo = () =>
            {
                if (OnUndoHandler != null)
                {
                    OnUndoHandler(this);
                }
            };
            TextureKeeper.OnRedo = () =>
            {
                if (OnRedoHandler != null)
                {
                    OnRedoHandler(this);
                }
            };
        }
        
        #region Input
        
        public void OnMouseHover(Vector3 position, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            if (!IsPainting)
            {
                if (triangle != null)
                {
                    OnPaint(position, triangle.UVHit);
                    if (OnMouseHoverHandler != null && PaintPosition != null)
                    {
                        OnMouseHoverHandler(this, triangle.UVHit, PaintPosition.Value, 1f);
                    }
                }
                else
                {
                    OnPaint(position);
                    if (OnMouseHoverHandler != null && PaintPosition != null)
                    {
                        var uv = new Vector2(PaintPosition.Value.x / PaintMaterial.SourceTexture.width, PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                        OnMouseHoverHandler(this, uv, PaintPosition.Value, 1f);
                    }
                }
            }
        }

        public void OnMouseDown(Vector3 position, float pressure = 1f, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            if (triangle != null && triangle.Transform != ObjectTransform)
                return;
            IsPaintingDone = false;
            InBounds = false;
            Pressure = pressure;

            if (!IsPainting && PaintPosition == null)
            {
                if (triangle != null)
                {
                    OnPaint(position, triangle.UVHit);
                }
                else
                {
                    OnPaint(position);
                }
            }
            
            if (PaintPosition != null)
            {
                if (OnMouseDownHandler != null)
                {
                    if (triangle == null)
                    {
                        if (IsInBounds(position))
                        {
                            var uv = new Vector2(PaintPosition.Value.x / PaintMaterial.SourceTexture.width,  PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                            OnMouseDownHandler(this, uv, PaintPosition.Value, Pressure);
                        }
                    }
                    else
                    {
                        OnMouseDownHandler(this, triangle.UVHit, PaintPosition.Value, Pressure);
                    }
                }
            }
        }

        public void OnMouseButton(Vector3 position, float pressure = 1f, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;

            if (triangle == null)
            {
                IsPainting = true;
                lineData.AddBrush(pressure * Brush.Size);
                OnPaint(position);
                Pressure = pressure;
                if (PaintPosition != null)
                {
                    IsPainting = true;
                }
                if (InBounds && PaintPosition != null && OnMouseHandler != null)
                {
                    var uv = new Vector2(PaintPosition.Value.x / PaintMaterial.SourceTexture.width, PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                    OnMouseHandler(this, uv, PaintPosition.Value, Pressure);
                }
            }
            else if (triangle.Transform == ObjectTransform)
            {
                IsPainting = true;
                lineData.AddTriangleBrush(triangle, pressure * Brush.Size);
                Pressure = pressure;
                OnPaint(position, triangle.UVHit);
                if (OnMouseHandler != null && PaintPosition != null)
                {
                    OnMouseHandler(this, triangle.UVHit, PaintPosition.Value, Pressure);
                }
            }
            else
            {
                PaintPosition = null;
                lineData.Clear();
            }
        }

        public void OnMouseUp(Vector3 position)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            FinishPainting();
            if (OnMouseUpHandler != null)
            {
                OnMouseUpHandler(this, IsInBounds(position));
            }
        }
        
        #endregion
        
        #region DrawFromCode
        
        /// <summary>
        /// Draws point with pressure
        /// </summary>
        /// <param name="position"></param>
        /// <param name="pressure"></param>
        public void DrawPoint(Vector2 position, float pressure = 1f)
        {
            Pressure = pressure;
            PaintPosition = position;
            IsPainting = true;
            IsPaintingDone = true;
            if (OnPaintDataHandler != null)
            { 
                OnPaintDataHandler(this, position, Brush.Size, Pressure, Brush.Color, Tool.Type);
            }
            if (OnDrawPointHandler != null)
            {
                OnDrawPointHandler(this, position, pressure);
            }
            lineData.Clear();
            lineData.AddPosition(position);
            Render();
            FinishPainting();
        }
        
        /// <summary>
        /// Draws line with pressure
        /// </summary>
        /// <param name="positionStart"></param>
        /// <param name="positionEnd"></param>
        /// <param name="pressureStart"></param>
        /// <param name="pressureEnd"></param>
        public void DrawLine(Vector2 positionStart, Vector2 positionEnd, float pressureStart = 1f, float pressureEnd = 1f)
        {
            Pressure = pressureEnd;
            PaintPosition = positionEnd;
            IsPainting = true;
            IsPaintingDone = true;
            if (OnPaintDataHandler != null)
            { 
                OnPaintDataHandler(this, positionStart, Brush.Size, pressureStart, Brush.Color, Tool.Type);
                OnPaintDataHandler(this, positionEnd, Brush.Size, Pressure, Brush.Color, Tool.Type);
            }
            if (OnDrawLineHandler != null)
            {
                OnDrawLineHandler(this, positionStart, positionEnd, pressureStart, pressureEnd);
            }
            lineData.AddBrush(pressureStart * Brush.Size);
            lineData.AddBrush(Pressure * Brush.Size);
            lineData.AddPosition(positionStart);
            lineData.AddPosition(positionEnd);
            OnRender();
            Render();
            FinishPainting();
        }
        
        #endregion

        /// <summary>
        /// Resets all states, bake paint result into PaintTexture, save paint result to TextureKeeper
        /// </summary>
        public void FinishPainting()
        {
            if (IsPainting)
            {
                Pressure = 1f;
                if (PaintMode.UsePaintInput)
                {
                    BakeInputToPaint(this);
#if (UNITY_2017 || UNITY_2018) && UNITY_EDITOR
                    PaintController.Instance.StartCoroutine(DoInNextFrameAction(() =>
                    {
                        if (initialized)
                        {
                            ClearTexture(RenderTarget.PaintInput);
                        }
                    }));
#else
                    ClearTexture(RenderTarget.PaintInput);
#endif
                }
                IsPainting = false;
                if (IsPaintingDone && Tool.RenderToTextures && Settings.Instance.UndoRedoEnabled)
                {
                    SaveUndoTexture();
                }
                lineData.Clear();
                if (!PaintMode.UsePaintInput)
                {
                    ClearTexture(RenderTarget.PaintInput);
                    Render();
                }
            }
            
            PaintMaterial.SetPaintPreviewVector(Vector4.zero);
            PaintPosition = null;
            IsPaintingDone = false;
            InBounds = false;
            previousPaintPosition = default(Vector2);
        }

#if (UNITY_2017 || UNITY_2018) && UNITY_EDITOR
        private IEnumerator DoInNextFrameAction(Action action)
        {
            yield return null;
            action();
        }
#endif
        
        /// <summary>
        /// Renders Points and Lines, restoring textures when Undo/Redo invoking
        /// </summary>
        public void OnRender()
        {
            if (shouldClearTexture)
            {
                ClearTexture(RenderTarget.Paint);
                ClearTexture(RenderTarget.PaintInput);
                BlitTexture();
                shouldClearTexture = false;
                if (writeClear && Tool.RenderToTextures && Settings.Instance.UndoRedoEnabled)
                {
                    SaveUndoTexture();
                    writeClear = false;
                }
            }

            if (shouldReDraw)
            {
                TextureKeeper.OnReDraw();
                shouldReDraw = false;
            }
            
            if (IsPainting && PaintPosition != null && previousPaintPosition != PaintPosition.Value && Tool.AllowRender)
            {
                IsPainted = true;
                if (lineData.HasOnePosition())
                {
                    DrawPoint();
                    previousPaintPosition = PaintPosition.Value;
                }
                else
                {
                    if (lineData.HasNotSameTriangles())
                    {
                        DrawLine(false);
                    }
                    else
                    {
                        DrawLine(true);
                    }
                    previousPaintPosition = PaintPosition.Value;
                }
            }
            else
            {
                IsPainted = false;
            }
        }

        [Obsolete("Method was obsolete, please use Render instead")]
        public void RenderCombined()
        {
            Render();
        }

        /// <summary>
        /// Combines textures, render preview
        /// </summary>
        public void Render()
        {
            DrawPreProcess(this);
            ClearTexture(RenderTarget.Combined);
            DrawProcess(this);
        }

        private void SaveUndoTexture()
        {
            var texture = PaintMaterial.Material.GetTexture(Paint.PaintTextureShaderParam) as RenderTexture;
            var state = RenderTextureFactory.CreateTemporaryRenderTexture(texture);
            Graphics.Blit(texture, state);
            TextureKeeper.OnMouseUp(state);
        }
        
        /// <summary>
        /// Restores texture when Undo/Redo invoking
        /// </summary>
        /// <param name="renderTexture"></param>
        private void OnExtraDraw(RenderTexture renderTexture)
        {
            if (!PaintMode.UsePaintInput)
            {
                ClearTexture(RenderTarget.PaintInput);
            }
            if (renderTexture != null)
            {
                var paintTexture = RenderTextureHelper.GetTexture(RenderTarget.Paint);
                Graphics.Blit(renderTexture, paintTexture);
            }
            Render();
        }

        /// <summary>
        /// Gets position for draw point
        /// </summary>
        /// <param name="holePosition"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Rect GetPosition(Vector2 holePosition, float scale)
        {
            var positionX = (int) holePosition.x;
            var positionY = (int) holePosition.y;
            var positionRect = new Rect(
                (positionX - HalfTextureRatio * Brush.RenderTexture.width * scale) /
                PaintMaterial.SourceTexture.width,
                (positionY - HalfTextureRatio * Brush.RenderTexture.width * scale) /
                PaintMaterial.SourceTexture.height,
                Brush.RenderTexture.width / (float)PaintMaterial.SourceTexture.width * scale,
                Brush.RenderTexture.height / (float)PaintMaterial.SourceTexture.height * scale
            );
            return positionRect;
        }
        
        /// <summary>
        /// Marks RenderTexture to be cleared
        /// </summary>
        /// <param name="writeToUndo"></param>
        public void ClearTexture(bool writeToUndo = false)
        {
            shouldClearTexture = true;
            writeClear = writeToUndo;
        }
        
        /// <summary>
        /// Renders quad(point)
        /// </summary>
        private void DrawPoint()
        {
            if (OnDrawPointHandler != null)
            {
                OnDrawPointHandler(this, PaintPosition.Value, Brush.Size * Pressure);
            }
            var positionRect = GetPosition(PaintPosition.Value, Brush.Size * Pressure);
            UpdateQuad(OnPaintPointOrLine, positionRect);
        }

        /// <summary>
        /// Renders a few quads (line)
        /// </summary>
        /// <param name="interpolate"></param>
        private void DrawLine(bool interpolate)
        {
            Vector2[] positions;
            Vector2[] paintPositions;
            if (interpolate)
            {
                paintPositions = lineData.GetPositions();
                positions = paintPositions;
            }
            else
            {
                paintPositions = lineData.GetPositions();
                var triangles = lineData.GetTriangles();
                positions = GetLinePositions(paintPositions[0], paintPositions[1], triangles[0], triangles[1]);
            }
            if (positions.Length > 0)
            {
                var brushes = lineData.GetBrushes();
                if (brushes.Length != 2)
                {
                    Debug.LogWarning("Incorrect length of brushes sizes array!");
                }
                else
                {
                    if (OnDrawLineHandler != null)
                    {
                        OnDrawLineHandler(this, paintPositions[0], paintPositions[1], brushes[0], brushes[1]);
                    }
                    RenderLine(OnPaintPointOrLine, positions, Brush.RenderTexture, Brush.Size, brushes);
                }
            }
        }

        /// <summary>
        /// Invokes Paint Handler after drawing point or line
        /// </summary>
        /// <param name="paintPosition"></param>
        private void OnPaintPointOrLine(Vector2 paintPosition)
        {
            if (OnPaintHandler != null)
            {
                OnPaintHandler(this, paintPosition, pressure);
            }
        }
        
        /// <summary>
        /// Post paint method, used by OnPaint method
        /// </summary>
        protected void OnPostPaint()
        {
            if (PaintPosition != null && IsPainting)
            {
                if (OnPaintDataHandler != null)
                {
                    OnPaintDataHandler(this, PaintPosition.Value, Brush.Size, Pressure, PaintMaterial.Material.color, Tool.Type);
                }
                lineData.AddPosition(PaintPosition.Value);
                
                if (Brush.Preview)
                {
                    var brushOffset = GetPreviewVector();
                    PaintMaterial.SetPaintPreviewVector(brushOffset);
                }
            }
            else if (PaintPosition == null)
            {
                lineData.Clear();
            }
            
            if (Brush.Preview)
            {
                if (PaintPosition != null)
                {
                    var brushOffset = GetPreviewVector();
                    PaintMaterial.SetPaintPreviewVector(brushOffset);
                }
                else
                {
                    PaintMaterial.SetPaintPreviewVector(Vector4.zero);
                }
            }
        }

        /// <summary>
        /// Returns Vector4 for brush preview 
        /// </summary>
        /// <returns></returns>
        private Vector4 GetPreviewVector()
        {
            var brushRatio = new Vector2(
                                 PaintMaterial.SourceTexture.width / (float) Brush.RenderTexture.width,
                                 PaintMaterial.SourceTexture.height / (float) Brush.RenderTexture.height) / Brush.Size / Pressure;
            var brushOffset = new Vector4(
                PaintPosition.Value.x / PaintMaterial.SourceTexture.width * brushRatio.x,
                PaintPosition.Value.y / PaintMaterial.SourceTexture.height * brushRatio.y,
                1f / brushRatio.x, 1f / brushRatio.y);
            return brushOffset;
        }
    }
}