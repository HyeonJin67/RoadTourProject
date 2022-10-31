using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XDPaint.Demo.UI
{
	public class ToggleDoubleClick : MonoBehaviour, IPointerDownHandler
	{
		[Serializable]
		public class OnDoubleClickEvent : UnityEvent<float>
		{
		}
		
		public Toggle Toggle;
		public OnDoubleClickEvent OnDoubleClick = new OnDoubleClickEvent();
		public float TimeBetweenTaps = 0.5f;
		
		private float firstTapTime;
		private bool doubleTapInitialized;
		
		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (Time.time - firstTapTime >= TimeBetweenTaps)
			{
				doubleTapInitialized = false;
			}
			else if (doubleTapInitialized)
			{
				OnDoubleClick.Invoke(transform.position.x);
				doubleTapInitialized = false;
			}

			if (!doubleTapInitialized)
			{
				doubleTapInitialized = true;
				firstTapTime = Time.time;
			}
		}
	}
}