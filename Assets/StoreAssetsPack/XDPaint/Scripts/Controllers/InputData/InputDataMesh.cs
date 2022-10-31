using UnityEngine;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Tools.Raycast;

namespace XDPaint.Controllers.InputData
{
    public class InputDataMesh : InputDataBase
    {
        private Ray? ray;
        private Triangle triangle;
        
        public override void OnUpdate()
        {
            base.OnUpdate();
            ray = null;
            triangle = null;
        }

        protected override void OnHoverSuccess(Vector3 position)
        {
            ray = Camera.ScreenPointToRay(Input.mousePosition);
            RaycastController.Instance.Raycast(ray.Value, out triangle);
            PaintManager.PaintObject.OnMouseHover(position, triangle);
        }

        protected override void OnDownSuccess(Vector3 position, float pressure = 1)
        {
            IsOnDownSuccess = true;
            if (ray == null)
            {
                ray = Camera.ScreenPointToRay(Input.mousePosition);
            }
            if (triangle == null)
            {
                RaycastController.Instance.Raycast(ray.Value, out triangle);
            }
            PaintManager.PaintObject.OnMouseDown(position, pressure, triangle);
        }

        public override void OnPress(Vector3 position, float pressure = 1.0f)
        {
            if (IsOnDownSuccess)
            {
                if (ray == null)
                {
                    ray = Camera.ScreenPointToRay(Input.mousePosition);
                }
                if (triangle == null)
                {
                    RaycastController.Instance.Raycast(ray.Value, out triangle);
                }
                PaintManager.PaintObject.OnMouseButton(position, pressure, triangle);
            }
        }
    }
}