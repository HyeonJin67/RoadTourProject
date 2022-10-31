using System;
using UnityEngine;

namespace Generic
{

	/// <summary>
	/// Mono singleton.
	/// 
	/// </summary>
	public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance = null;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					var instances = FindObjectsOfType<T>();
					if (1 < instances.Length)
						Debug.LogWarningFormat("MonoSingleton is not single! Type[{0}] Count[{1}], Remove other objects except first one.", typeof(T), instances.Length);

					if (instances.Length == 0)
					{
						GameObject obj = new GameObject();
						obj.name = typeof(T).ToString();
						_instance = obj.AddComponent<T>();
					}
					else
					{
						_instance = instances[0];
						for (int i = 1; i < instances.Length; ++i)
						{
							GameObject.Destroy(instances[i].gameObject);
						}
					}
				}

				return _instance;
			}
		}

		protected virtual void Awake()
		{
			var instances = FindObjectsOfType<T>();
			if (1 < instances.Length)
			{
				Debug.LogWarningFormat("MonoSingleton is not single! Type[{0}] Count[{1}], Remove other objects except first one.", typeof(T), instances.Length);
				GameObject.Destroy(gameObject);
			}
			else
				DontDestroyOnLoad(this);
		}

		void OnDestroy()
		{
			_instance = null;
		}
	}

	/// <summary>
	/// Singleton.
	/// </summary>
	public class Singleton<T> where T : class, new()
	{
		private static T _instance = null;

		public static T Instance
		{
			get
			{
				{
					if (_instance == null)
						_instance = new T();

					return _instance;
				}
			}
		}
	}

}