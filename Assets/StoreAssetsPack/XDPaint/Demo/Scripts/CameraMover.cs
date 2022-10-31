using UnityEngine;

namespace XDPaint.Demo
{
	public class CameraMover : MonoBehaviour
	{
		public Transform Target;
		public float Distance = 10.0f;
		public float AxisRatio = 0.02f;
		public int MinDistance = 3;
		public int MaxDistance = 10;

		private int fingerId = -1;
		private float x, y;
		private float defaultDistance;
		private readonly Vector2 axisMoveSpeedMouse = new Vector2(170f, 70f);
		private readonly Vector2 axisMoveSpeedTouch = new Vector2(17f, 7f);

		void Awake()
		{
			defaultDistance = Distance;
		}
		
		void Update()
		{
			Distance += Input.GetAxis("Mouse ScrollWheel") * Distance;
			Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
			
			if (!Input.touchSupported || Input.mousePresent)
			{
				if (Input.GetMouseButton(0))
				{
					x += Input.GetAxis("Mouse X") * axisMoveSpeedMouse.x * AxisRatio;
					y -= Input.GetAxis("Mouse Y") * axisMoveSpeedMouse.y * AxisRatio;
				}
			}
			
			if (Input.touchSupported)
			{
				foreach (var touch in Input.touches)
				{
					if (touch.phase == TouchPhase.Began && fingerId == -1)
					{
						fingerId = touch.fingerId;
					}
					if (touch.fingerId == fingerId)
					{
						if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
						{
							x += Input.touches[fingerId].deltaPosition.x * axisMoveSpeedTouch.x * AxisRatio;
							y -= Input.touches[fingerId].deltaPosition.y * axisMoveSpeedTouch.y * AxisRatio;
						}
						if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
						{
							fingerId = -1;
						}
					}
				}
			}
			
			var rotation = Quaternion.Euler(y, x, 0);
			var position = rotation * new Vector3(0.0f, 0.0f, -Distance) + Target.position;
			transform.position = position;
			transform.rotation = rotation;
		}

		public void Reset()
		{
			Distance = defaultDistance;
			x = 0;
			y = 0;
			Update();
		}
	}
}