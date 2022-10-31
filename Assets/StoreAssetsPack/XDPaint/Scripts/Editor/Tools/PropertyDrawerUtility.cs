using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace XDPaint.Editor.Tools
{
    public class PropertyDrawerUtility
    {
        public static T GetActualObjectForSerializedProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
        {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj == null)
            {
                return null;
            }
            T actualObject;
            if (obj.GetType().IsArray)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                actualObject = ((T[])obj).Length > index ? ((T[])obj)[index] : ((T[])obj)[((T[])obj).Length - 1];
            }
            else if (obj.GetType() == typeof(List<T>))
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                actualObject = ((List<T>)obj).Count > index ? ((List<T>) obj)[index] : ((List<T>) obj)[((List<T>)obj).Count - 1];
            }
            else
            {
                actualObject = obj as T;
            }
            return actualObject;
        }
    }
}