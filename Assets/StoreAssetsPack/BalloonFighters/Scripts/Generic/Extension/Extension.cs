using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

namespace Generic
{
    public static class Extension
    {
        static public void SafeDestroy(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour != null)
                GameObject.Destroy(monoBehaviour.gameObject);
        }

        static public void SafeInvoke(this Action action)
        {
            if (action != null)
                action.Invoke();
        }
        static public void SafeInvoke<T>(this Action<T> action, T arg1)
        {
            if (action != null)
                action.Invoke(arg1);
        }
        static public void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action != null)
                action.Invoke(arg1, arg2);
        }
        static public void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            if (action != null)
                action.Invoke(arg1, arg2, arg3);
        }
        static public void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (action != null)
                action.Invoke(arg1, arg2, arg3, arg4);
        }
        static public void SafeStopCoroutine(this MonoBehaviour mono, Coroutine coroutine)
        {
            if (coroutine != null)
                mono.StopCoroutine(coroutine);
        }
        public static void MonoInvoke(this MonoBehaviour m, Action method, float time)
        {
            if (m != null && method !=null)
                m.Invoke(method.Method.Name, time);
        }

        public static float GetAngleBetween3DVector(this Vector3 vec1, Vector3 vec2)
        {
            float theta = Vector3.Dot(vec1, vec2) / (vec1.magnitude * vec2.magnitude);
            Vector3 dirAngle = Vector3.Cross(vec1, vec2);
            float angle = Mathf.Acos(theta) * Mathf.Rad2Deg;
            if (dirAngle.z < 0.0f) angle = 360 - angle;
            return angle;
        }
    }
}
