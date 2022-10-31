using UnityEngine;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintObject
{
    public sealed class SpriteRendererPaint : BasePaintObject
    {
        private SpriteRenderer renderer;
        private Vector2 scratchBoundsSize;
        private Vector2 pivotOffset;

        protected override void Init()
        {
            renderer = ObjectTransform.GetComponent<SpriteRenderer>();
            GetScratchBounds();
            if (!Camera.orthographic)
            {
                Debug.LogWarning("Camera is not orthographic!");
            }

            var sprite = renderer.sprite;
            pivotOffset = new Vector2(
                sprite.pivot.x - PaintMaterial.SourceTexture.width / 2f,
                sprite.pivot.y - PaintMaterial.SourceTexture.height / 2f);
        }

        protected override bool IsInBounds(Vector3 position)
        {
            var clickPosition = Camera.ScreenToWorldPoint(position);
            var bounds = renderer.bounds;
            var offset = new Vector3(
                Brush.RenderTexture.width * Brush.Size / PaintMaterial.SourceTexture.width * bounds.size.x,
                Brush.RenderTexture.height * Brush.Size/ PaintMaterial.SourceTexture.height * bounds.size.y);
            bounds.size += offset;
            clickPosition.z = bounds.center.z;
            var inBounds = bounds.Contains(clickPosition);
            return inBounds;
        }

        private void GetScratchBounds()
        {
            if (renderer != null)
            {
                scratchBoundsSize = renderer.bounds.size;
            }
        }

        protected override void OnPaint(Vector3 position, Vector2? uv = null)
        {
            InBounds = IsInBounds(position);
            if (InBounds)
            {
                IsPaintingDone = true;
            }

            var clickPosition = Camera.ScreenToWorldPoint(position);
            var surfaceLocalClickPosition = ObjectTransform.InverseTransformPoint(clickPosition);
            var lossyScale = ObjectTransform.lossyScale;
            var xSign = Mathf.Sign(lossyScale.x);
            var ySign = Mathf.Sign(lossyScale.y);
            var zSign = Mathf.Sign(lossyScale.z);
            surfaceLocalClickPosition = Vector3.Scale(surfaceLocalClickPosition, new Vector3(xSign, ySign, zSign));
            var clickLocalPosition = new Vector2(surfaceLocalClickPosition.x * lossyScale.x, surfaceLocalClickPosition.y * lossyScale.y);
            GetScratchBounds();
            var bottomLeftLocalPosition = (Vector2)ObjectTransform.InverseTransformPoint(ObjectTransform.position) - scratchBoundsSize / 2f;
            var scratchSurfaceClickLocalPosition = clickLocalPosition - bottomLeftLocalPosition;
            var ppi = new Vector2(
                PaintMaterial.SourceTexture.width / scratchBoundsSize.x / lossyScale.x, 
                PaintMaterial.SourceTexture.height / scratchBoundsSize.y / lossyScale.y);
            PaintPosition = new Vector2( 
                scratchSurfaceClickLocalPosition.x * lossyScale.x * ppi.x + pivotOffset.x, 
                scratchSurfaceClickLocalPosition.y * lossyScale.y * ppi.y + pivotOffset.y);

            OnPostPaint();
        }
    }
}