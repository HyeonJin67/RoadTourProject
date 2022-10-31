using UnityEngine;
using System;

namespace Generic
{

	/// <summary>
	/// 리소스 로더
	/// 게임에 필요한 리소스 및 프리팹로딩을 제공하는 스테틱 클래스이다.
	/// </summary>
	public static class ResourceLoader
	{
		/////////////////////////////////////////////////////////////////////////////////////////
		/// Public Method
		/////////////////////////////////////////////////////////////////////////////////////////
		public static T Instantiate<T>(GameObject gameObject) where T : UnityEngine.Object
		{
			try
			{
				return GameObject.Instantiate(gameObject).GetComponent<T>();
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("LoadResource Fail! [{0}]\n{1}", gameObject.name, e);
				return null;
			}
		}

		public static T LoadResource<T>(string path) where T : UnityEngine.Object
		{
			try
			{
				return Resources.Load<T>(path);
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("LoadResource Fail! [{0}]\n{1}", path, e);
				return null;
			}
		}

		public static T[] LoadResourceAll<T>(string path) where T : UnityEngine.Object
		{
			try
			{
				return Resources.LoadAll<T>(path);
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("LoadResource Fail! [{0}]\n{1}", path, e);
				return null;
			}
		}

		public static void CreatePrefab<T>(string path, Action<T> onFinish) where T : UnityEngine.MonoBehaviour
		{
			try
			{
				var obj = Resources.Load(path);
				var go = GameObject.Instantiate(obj) as GameObject;
				onFinish.SafeInvoke(go.GetComponent<T>());
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("CreatePrefab Fail! [{0}]\n{1}", path, e);
			}
		}

		public static GameObject CreatePrefab(string path)
		{
			try
			{
				var obj = Resources.Load(path);
				var go = GameObject.Instantiate(obj) as GameObject;

				return go;
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("CreatePrefab Fail! [{0}]\n{1}", path, e);
				return null;
			}
		}

		public static T CreatePrefab<T>(string path) where T : UnityEngine.MonoBehaviour
		{
			try
			{
				var obj = Resources.Load(path);
				var go = GameObject.Instantiate(obj) as GameObject;

				return go.GetComponent<T>();
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("CreatePrefab Fail! [{0}]\n{1}", path, e);
				return null;
			}
		}

		//public static GameObject CreatePrefabFromAssetBundle(string assetBundlePath)
		//{
		//	try
		//	{
		//		string assetBundleName = GetAssetBundleName(assetBundlePath);
		//		AssetBundleManager.Instance.StoreAssetBundle(assetBundleName);
		//		string error;
		//		var assetBundle = AssetBundleManager.Instance.GetAssetBundleRef(assetBundleName, out error);
		//		var obj = assetBundle.LoadAsset(GetAssetName(assetBundlePath));

		//		var go = GameObject.Instantiate(obj) as GameObject;

		//		return go;
		//	}
		//	catch (Exception e)
		//	{
		//		Debug.LogErrorFormat("CreatePrefabFromAssetBundle Fail! [{0}]\n{1}", assetBundlePath, e);
		//		return null;
		//	}
		//}


		//public static void CreatePrefabFromAssetBundle<T>(string assetBundlePath, Action<T> onFinish)
		//{
		//	try
		//	{
		//		string assetBundleName = GetAssetBundleName(assetBundlePath);
		//		AssetBundleManager.Instance.StoreAssetBundle(assetBundleName);
		//		string error;
		//		var assetBundle = AssetBundleManager.Instance.GetAssetBundleRef(assetBundleName, out error);

		//		var assetName = GetAssetName(assetBundlePath);
		//		var obj = assetBundle.LoadAsset(assetName);
		//		var go = GameObject.Instantiate(obj) as GameObject;

		//		onFinish.SafeInvoke(go.GetComponent<T>());
		//	}
		//	catch (Exception e)
		//	{
		//		Debug.LogErrorFormat("CreatePrefabFromAssetBundle Fail! [{0}]\n{1}", assetBundlePath, e);
		//	}
		//}

		//public static T CreatePrefabFromAssetBundle<T>(string assetBundlePath) where T : UnityEngine.MonoBehaviour
		//{
		//	try
		//	{
		//		string assetBundleName = GetAssetBundleName(assetBundlePath);
		//		AssetBundleManager.Instance.StoreAssetBundle(assetBundleName);
		//		string error;
		//		var assetBundle = AssetBundleManager.Instance.GetAssetBundleRef(assetBundleName, out error);
		//		var obj = assetBundle.LoadAsset(GetAssetName(assetBundlePath));

		//		var go = GameObject.Instantiate(obj) as GameObject;

		//		return go.GetComponent<T>();
		//	}
		//	catch (Exception e)
		//	{
		//		Debug.LogErrorFormat("CreatePrefabFromAssetBundle Fail! [{0}]\n{1}", assetBundlePath, e);
		//		return null;
		//	}
		//}

		//public static T LoadResourceFromAssetBundle<T>(string assetBundlePath) where T : UnityEngine.Object
		//{
		//	try
		//	{
		//		string assetBundleName = GetAssetBundleName(assetBundlePath);
		//		AssetBundleManager.Instance.StoreAssetBundle(assetBundleName);
		//		string error;

		//		var assetBundle = AssetBundleManager.Instance.GetAssetBundleRef(assetBundleName, out error);
		//		if(assetBundle == null)
		//			return null;

		//		var asset = assetBundle.LoadAsset(GetAssetName(assetBundlePath), typeof(T)) as T;

		//		return asset;
		//	}
		//	catch (Exception e)
		//	{
		//		Debug.LogErrorFormat("LoadResource Fail! [{0}]\n{1}", assetBundlePath, e);
		//		return null;
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////////////////
		/// Private Method
		/////////////////////////////////////////////////////////////////////////////////////////
		private static string GetAssetBundleName(string assetBundlePath)
		{
			return assetBundlePath.Substring(0, assetBundlePath.LastIndexOf('/'));
		}

		private static string GetAssetName(string assetBundlePath)
		{
			int lastSlashIndex = assetBundlePath.LastIndexOf('/');
			return assetBundlePath.Substring(lastSlashIndex + 1);
		}
	}

}
