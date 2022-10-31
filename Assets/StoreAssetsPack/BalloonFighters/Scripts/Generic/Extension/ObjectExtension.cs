using System;
using System.Collections;
using System.Collections.Generic;


public static class ObjectExtension{

    public static KeyValuePair<string , object> ToJsonDic(object obj)
    {
        var type = obj.GetType();
        if (type.IsGenericType)
        {
            if (type == typeof(KeyValuePair<,>))
            {
                var key = type.GetProperty("Key");
                var value = type.GetProperty("Value");
                var keyObj = key.GetValue(obj, null);
                var valueObj = value.GetValue(obj, null);
                return new KeyValuePair<string, object>(keyObj.ToString(), valueObj);
            }
        }
        throw new ArgumentException(" ### -> public static KeyValuePair<object , object > CastFrom(Object obj) : Error : obj argument must be KeyValuePair<,>");
    }
}
