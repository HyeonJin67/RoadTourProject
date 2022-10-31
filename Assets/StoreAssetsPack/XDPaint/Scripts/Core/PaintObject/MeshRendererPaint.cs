using UnityEngine;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Core.PaintObject
{
	public sealed class MeshRendererPaint : BasePaintObject
	{
		private Renderer renderer;

		protected override void Init()
		{
			renderer = ObjectTransform.GetComponent<Renderer>();

			Mesh mesh = null;
			var meshFilter = ObjectTransform.GetComponent<MeshFilter>();
			if (meshFilter != null)
			{
				mesh = meshFilter.sharedMesh;
			}
			else
			{
				var skinnedMeshRenderer = ObjectTransform.GetComponent<SkinnedMeshRenderer>();
				if (skinnedMeshRenderer != null)
				{
					mesh = skinnedMeshRenderer.sharedMesh;
				}
			}
			if (mesh == null)
			{
				Debug.LogError("Can't find MeshFilter or SkinnedMeshRenderer component!");
			}
			if (Camera.orthographic)
			{
				Debug.LogWarning("Camera is not perspective!");
			}
		}

		protected override bool IsInBounds(Vector3 position)
		{
			var bounds = new Bounds();
			if (renderer != null)
			{
				bounds = renderer.bounds;
			}
			var ray = Camera.ScreenPointToRay(position);
			var inBounds = bounds.IntersectRay(ray);
			return inBounds;
		}

		protected override void OnPaint(Vector3 position, Vector2? uv = null)
		{
			InBounds = IsInBounds(position);
			if (!InBounds)
			{
				PaintPosition = null;
				OnPostPaint();
				return;
			}

			var hasRaycast = uv != null;
			if (hasRaycast)
			{
				PaintPosition = new Vector2(
					PaintMaterial.SourceTexture.width * uv.Value.x, 
					PaintMaterial.SourceTexture.height * uv.Value.y);
				IsPaintingDone = true;
			}
			else
			{
				PaintPosition = null;
			}
			OnPostPaint();
		}
	}
}