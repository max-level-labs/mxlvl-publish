using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

public static class FileIO {

    public static void DeleteKey(string key)
    {
#if UNITY_WEBGL
        RemoveFromLocalStorage(key: key);
#else
        UnityEngine.PlayerPrefs.DeleteKey(key: key);
#endif
    }

    public static bool HasKey(string key)
    {
            
#if UNITY_WEBGL
        return (HasKeyInLocalStorage(key) == 1);
#else
        return (UnityEngine.PlayerPrefs.HasKey(key: key));
#endif
    }

    public static string GetString(string key)
    {

#if UNITY_WEBGL
        return LoadFromLocalStorage(key: key);
#else
        return (UnityEngine.PlayerPrefs.GetString(key: key));
#endif
    }

    public static void SetString(string key, string value)
    {

#if UNITY_WEBGL
        SaveToLocalStorage(key: key, value: value);
#else
        UnityEngine.PlayerPrefs.SetString(key: key, value: value);
#endif

    }

    public static string GetOrSetString(string key, string value)
    {
#if UNITY_WEBGL
        if (HasKey(key))
        {
            return GetString(key);
        }
        else
        {
            SetString(key, value);
            return value;
        }
#else
        return value;
#endif
    }

  

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SaveToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern string LoadFromLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern void RemoveFromLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern int HasKeyInLocalStorage(string key);
#endif
}

