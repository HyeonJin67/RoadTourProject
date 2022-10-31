using System;
using System.Collections.Generic;
using UnityEngine;

namespace XDPaint.Tools.Raycast
{
	[Serializable]
	public class Triangle
	{
		//Triangle id
		public ushort Id;
		//Index 0
		public ushort I0;
		//Index 1
		public ushort I1;
		//Index 2
		public ushort I2;		
		//Neighbors
		public List<ushort> N = new List<ushort>();
		
		private RaycastMeshData meshData;
		private Barycentric barycentricLocal;

		public Transform Transform { get { return meshData.Transform; } }

		public Vector3 Position0
		{
			get
			{
				if (meshData.UseLossyScale)
				{
					return Vector3.Scale(meshData.Vertices[I0], Transform.lossyScale);
				}
				return meshData.Vertices[I0];
			}
		}

		public Vector3 Position1
		{
			get
			{
				if (meshData.UseLossyScale)
				{
					return Vector3.Scale(meshData.Vertices[I1], Transform.lossyScale);
				}
				return meshData.Vertices[I1];
			}
		}
		
		public Vector3 Position2
		{
			get
			{
				if (meshData.UseLossyScale)
				{
					return Vector3.Scale(meshData.Vertices[I2], Transform.lossyScale);
				}
				return meshData.Vertices[I2];
			}
		}
		
		public Vector3 Hit
		{
			get
			{
				if (barycentricLocal == null)
				{
					barycentricLocal = new Barycentric();
				}
				return barycentricLocal.Interpolate(Position0, Position1, Position2);
			}
			set
			{
				barycentricLocal = new Barycentric(Position0, Position1, Position2, value);
			}
		}

		public Vector3 WorldHit
		{
			get
			{
				var localHit = Hit;
				return Transform.localToWorldMatrix.MultiplyPoint(localHit);
			}
		}

		private Vector2 uvHit;
		public Vector2 UVHit
		{
			get { return uvHit; }
			set { uvHit = value; }
		}

		public Vector2 UV0 { get { return meshData.UV[I0]; } }
		public Vector2 UV1 { get { return meshData.UV[I1]; } }
		public Vector2 UV2 { get { return meshData.UV[I2]; } }

		public Triangle(ushort id, ushort index0, ushort index1, ushort index2)
		{
			Id = id;
			I0 = index0;
			I1 = index1;
			I2 = index2;
		}

		public void SetTrianglesContainer(RaycastMeshData container)
		{
			meshData = container;
		}
	}
}