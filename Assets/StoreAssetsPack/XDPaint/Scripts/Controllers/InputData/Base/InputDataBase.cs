using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;

namespace XDPaint.Controllers.InputData.Base
{
    public abstract class InputDataBase
    {
        protected PaintManager PaintManager;
        protected Camera Camera;
        protected bool IsOnDownSuccess;
        private CanvasGraphicRaycaster canvasGraphicRaycaster;
        
        public virtual void Init(PaintManager paintManager, Camera camera)
        {
            PaintManager = paintManager;
            Camera = camera;
            if (Settings.Instance.CheckCanvasRaycasts)
            {
                var rawImage = paintManager.ObjectForPainting.GetComponent<RawImage>();
                if (rawImage != null && rawImage.canvas != null)
                {
                    canvasGraphicRaycaster = rawImage.canvas.GetComponent<CanvasGraphicRaycaster>();
                    if (canvasGraphicRaycaster == null)
                    {
                        canvasGraphicRaycaster = rawImage.canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                    }
                }
                else
                {
                    var canvas = InputController.Instance.Canvas;
                    if (canvas == null)
                    {
                        canvas = Object.FindObjectOfType<Canvas>();
                    }
                    if (canvas != null)
                    {
                        canvasGraphicRaycaster = canvas.GetComponent<CanvasGraphicRaycaster>();
                        if (canvasGraphicRaycaster == null)
                        {
                            canvasGraphicRaycaster = canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                        }
                    }
                }
            }
        }
        
        public virtual void OnDestroy()
        {
            if (canvasGraphicRaycaster != null)
            {
                Object.Destroy(canvasGraphicRaycaster);
            }
        }

        public virtual void OnUpdate()
        {
            
        }

        public void OnHover(Vector3 position)
        {
            if (Settings.Instance.CheckCanvasRaycasts && canvasGraphicRaycaster != null)
            {
                var raycasts = canvasGraphicRaycaster.GetRaycasts(position);
                if (raycasts == null || raycasts.Count == 0 || CheckRaycasts(raycasts))
                {
                    OnHoverSuccess(position);
                }
                else
                {
                    OnHoverFailed(position);
                }
            }
            else
            {
                OnHoverSuccess(position);
            }
        }
        
        protected virtual void OnHoverSuccess(Vector3 position)
        {
            PaintManager.PaintObject.OnMouseHover(position);
        }
        
        protected virtual void OnHoverFailed(Vector3 position)
        {
            PaintManager.Material.SetPaintPreviewVector(Vector4.zero);
        }

        public void OnDown(Vector3 position, float pressure = 1.0f)
        {
            if (Settings.Instance.CheckCanvasRaycasts && canvasGraphicRaycaster != null)
            {
                var raycasts = canvasGraphicRaycaster.GetRaycasts(position);
                if (raycasts == null || raycasts.Count == 0 || CheckRaycasts(raycasts))
                {
                    OnDownSuccess(position, pressure);
                }
                else
                {
                    OnDownFailed(position, pressure);
                }
            }
            else
            {
                OnDownSuccess(position, pressure);
            }
        }
        
        protected virtual void OnDownSuccess(Vector3 position, float pressure = 1.0f)
        {
            PaintManager.PaintObject.OnMouseDown(position, pressure);
            IsOnDownSuccess = true;
        }
        
        protected virtual void OnDownFailed(Vector3 position, float pressure = 1.0f)
        {
            IsOnDownSuccess = false;
        }

        public virtual void OnPress(Vector3 position, float pressure = 1.0f)
        {
            if (IsOnDownSuccess)
            {
                PaintManager.PaintObject.OnMouseButton(position, pressure);
            }
        }

        public virtual void OnUp(Vector3 position)
        {
            if (IsOnDownSuccess)
            {
                PaintManager.PaintObject.OnMouseUp(position);
            }
        }

        private bool CheckRaycasts(List<RaycastResult> raycasts)
        {
            var result = true;
            if (raycasts.Count > 0)
            {
                var ignoreRaycasts = InputController.Instance.IgnoreForRaycasts;
                foreach (var raycast in raycasts)
                {
                    if (raycast.gameObject == PaintManager.ObjectForPainting.gameObject && PaintManager.Initialized)
                    {
                        break;
                    }

                    if (!ignoreRaycasts.Contains(raycast.gameObject))
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
    }
}