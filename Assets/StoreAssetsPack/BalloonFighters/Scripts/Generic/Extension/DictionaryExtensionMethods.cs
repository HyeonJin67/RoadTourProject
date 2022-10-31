using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensionMethods
{
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<Dictionary<string, object>> list, string value)
    {
        return list.GetDictionary("index", value);
    }
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<Dictionary<string, object>> list, int value)
    {
        return list.GetDictionary("index", value);
    }
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<Dictionary<string, object>> list, long value)
    {
        return list.GetDictionary("index", value);
    }

    public static Dictionary<string, object> GetDictionary(this List<Dictionary<string, object>> list, string key, int value)
    {
        return GetDictionary(list, key, value.ToString());
    }
    public static Dictionary<string, object> GetDictionary(this List<Dictionary<string, object>> list, string key, long value)
    {
        return GetDictionary(list, key, value.ToString());
    }
    public static Dictionary<string, object> GetDictionary(this List<Dictionary<string, object>> list, string key, string value)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var dic = list[i];
            if (dic.ContainsKey(key) == true)
            {
                if (dic[key].Equals(value) == true)
                    return dic;
            }
        }
        return null;
    }
    public static List<Dictionary<string, object>> GetList(this List<Dictionary<string, object>> list, string key, int value)
    {
        return GetList(list, key, value.ToString());
    }
    public static List<Dictionary<string, object>> GetList(this List<Dictionary<string, object>> list, string key, long value)
    {
        return GetList(list, key, value.ToString());
    }
    public static List<Dictionary<string, object>> GetList(this List<Dictionary<string, object>> list, string key, string value)
    {
        var resultList = new List<Dictionary<string, object>>();

        for (var i = 0; i < list.Count; i++)
        {
            var dic = list[i];
            if (dic.ContainsKey(key) == true)
            {
                if (dic[key].Equals(value) == true)
                {
                    resultList.Add(dic);
                }
            }
        }
        return resultList;
    }

    public static Dictionary<string, object> ToDictionary(this List<object> row, List<object> keys)
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        if (keys != null && row.Count == keys.Count)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                if (string.IsNullOrEmpty((string)keys[i]) == false)
                    dic.Add((string)keys[i], row[i]);
            }
        }
        return dic;
    }


    public static Dictionary<string, object> ElementAt(this List<Dictionary<string, object>> list, int index)
    {
        if (index < 0)
            return null;
        if (list.Count < index)
            return null;

        return list[index];
    }
    public static Dictionary<string, object> ElementAt(this List<List<object>> list, int index, bool firstColumIsKeys = true)
    {
        if (index < 0)
            return null;
        if (list.Count <= index)
            return null;

        if (firstColumIsKeys == true)
        {
            index += 1;
            if (list.Count <= index)
                return new Dictionary<string, object>();
        }

        var keys = list[0];
        var row = list[index];
        var dic = row.ToDictionary(keys);
        return dic;
    }

    public static int GetColIndex(this List<List<object>> list, string key)
    {
        int index = -1;
        if (list.Count > 0 && list[0] != null)
        {
            for (var i = 0; i < list[0].Count; i++)
            {
                if (list[0][i].Equals(key) == true)
                {
                    index = i;
                    break;
                }
            }
        }

        return index;
    }
    public static List<Dictionary<string, object>> GetList(this List<List<object>> list, string key, int value)
    {
        return GetList(list, key, value.ToString());
    }
    public static List<Dictionary<string, object>> GetList(this List<List<object>> list, string key, long value)
    {
        return GetList(list, key, value.ToString());
    }
    public static List<Dictionary<string, object>> GetList(this List<List<object>> list, string key, string value)
    {
        var resultList = new List<Dictionary<string, object>>();

        int colIndex = GetColIndex(list, key);
        if (colIndex != -1)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var row = list[i];
                if (colIndex < row.Count)
                {
                    if (row[colIndex].Equals(value) == true)
                    {
                        var keys = list[0];
                        var dic = row.ToDictionary(keys);
                        resultList.Add(dic);
                    }
                }
            }
        }
        return resultList;
    }
    public static Dictionary<string, object> GetDictionary(this List<List<object>> list, string key, int value)
    {
        return GetDictionary(list, key, value.ToString());
    }
    public static Dictionary<string, object> GetDictionary(this List<List<object>> list, string key, long value)
    {
        return GetDictionary(list, key, value.ToString());
    }
    public static Dictionary<string, object> GetDictionary(this List<List<object>> list, string key, string value)
    {
        int colIndex = GetColIndex(list, key);
        if (colIndex != -1)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var row = list[i];
                if (colIndex < row.Count)
                {
                    if (row[colIndex].Equals(value) == true)
                    {
                        var keys = list[0];
                        var dic = row.ToDictionary(keys);
                        return dic;
                    }
                }
            }
        }

        return null;
    }
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<List<object>> list, int value)
    {
        return GetDictionary(list, "index", value);
    }
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<List<object>> list, long value)
    {
        return GetDictionary(list, "index", value);
    }
    public static Dictionary<string, object> GetDictionaryFromIndex(this List<List<object>> list, string value)
    {
        return GetDictionary(list, "index", value);
    }
    public static long GetLong(this List<object> list, int index)
    {
        long value = 0;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        long.TryParse(list[index].ToString(), out value);
        return value;
    }
    public static int GetInt(this List<object> list, int index)
    {
        int value = 0;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        int.TryParse(list[index].ToString(), out value);
        return value;
    }
    public static float GetFloat(this List<object> list, int index)
    {
        float value = 0;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        float.TryParse(list[index].ToString(), out value);
        return value;
    }
    public static double GetDouble(this List<object> list, int index)
    {
        double value = 0;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        double.TryParse(list[index].ToString(), out value);
        return value;
    }
    public static bool GetBool(this List<object> list, int index)
    {
        bool value = false;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        bool.TryParse(list[index].ToString(), out value);
        return value;
    }

    public static string GetString(this List<object> list, int index)
    {
        string value = string.Empty;

        if (list == null)
            return value;
        if (index < 0)
            return value;
        if (index >= list.Count)
            return value;

        value = list[index].ToString();
        return value;
    }

}

public static class DictionaryExtensionMethods
{
    public static long GetLong(this Dictionary<string, object> dic, string key)
    {
        long value = 0;
        dic.TryGetLong(key, out value);
        return value;
    }

    public static int GetInt(this Dictionary<string, object> dic, string key)
    {
        int value = 0;
        dic.TryGetInt(key, out value);
        return value;
    }
    public static float GetFloat(this Dictionary<string, object> dic, string key)
    {
        float value = 0;
        dic.TryGetFloat(key, out value);
        return value;

    }
    public static double GetDouble(this Dictionary<string, object> dic, string key)
    {
        double value = 0;
        dic.TryGetDouble(key, out value);
        return value;

    }
    public static bool GetBool(this Dictionary<string, object> dic, string key)
    {
        bool value = false;
        dic.TryGetBool(key, out value);
        return value;
    }
    public static string GetString(this Dictionary<string, object> dic, string key)
    {
        string value = string.Empty;
        dic.TryGetString(key, out value);
        return value;
    }

    public static object[] GetList(this Dictionary<string, object> dic, string key)
    {
        if (dic.ContainsKey(key) == true)
            return (object[])dic[key];
        return null;
    }

    public static bool TryGetLong(this Dictionary<string, object> dic, string key, out long result)
	{
		result = 0;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			if (long.TryParse(dic[key].ToString(), out result) == true)
				return true;
		}

		return false;
	}

	public static bool TryGetInt(this Dictionary<string, object> dic, string key, out int result)
	{
		result = 0;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			if (int.TryParse(dic[key].ToString(), out result) == true)
				return true;
		}

		return false;
	}
	public static bool TryGetFloat(this Dictionary<string, object> dic, string key, out float result)
	{
		result = 0;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			if (float.TryParse(dic[key].ToString(), out result) == true)
				return true;
		}

		return false;
	}
	public static bool TryGetDouble(this Dictionary<string, object> dic, string key, out double result)
	{
		result = 0;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			if (double.TryParse(dic[key].ToString(), out result) == true)
				return true;
		}

		return false;
	}
	public static bool TryGetBool(this Dictionary<string, object> dic, string key, out bool result)
	{
		result = false;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			if (bool.TryParse(dic[key].ToString(), out result) == true)
				return true;
		}

		return false;
	}
	public static bool TryGetString(this Dictionary<string, object> dic, string key, out string result)
	{
		result = string.Empty;

		if (dic == null)
			return false;

		if (string.IsNullOrEmpty(key) == true)
			return false;

		if (dic.ContainsKey(key) == true) 
		{
			result = dic[key].ToString();
		}

		return false;
	}

}