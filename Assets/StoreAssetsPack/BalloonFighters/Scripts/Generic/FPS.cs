using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generic
{	
	public class FPS : MonoBehaviour
	{
		float _prevTime = 0.0f;
		float _deltaTime = 1.0f;

#if SERVICE_MODE || CLEAN_SCREEN
		void Start()
		{
			GameObject.Destroy(this);
		}
#else
		void Start()
		{
			_prevTime = Time.realtimeSinceStartup;
		}
#endif

		void Update()
		{
			float currentTime = Time.realtimeSinceStartup;
			_deltaTime = currentTime - _prevTime;
			_prevTime = currentTime;
		}

		void OnGUI()
		{
			int w = Screen.width;
			int h = Screen.height;

			GUIStyle style = new GUIStyle();

			Rect rect = new Rect(0, 75, w, h * 2 / 100);
			style.alignment = TextAnchor.LowerRight;
			style.fontSize = h * 2 / 80;
			style.normal.textColor = new Color(1f, 0, 0f, 1.0f);
			float sec = _deltaTime * 1000.0f;
			float fps = 1.0f / _deltaTime;
			string text = string.Format("{0:0.0} ms ({1:0.} fps)", sec, fps);
			GUI.Label(rect, text, style);
		}
	}

}
