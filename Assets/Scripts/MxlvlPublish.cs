using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MxlvlPublish {

    public const string versionMXLVL = "0.01";

    internal const string serverSettingsAssetFile = "MxlvlPublisherSettings";

    public static PublisherSettings MxlvlPublisherSettings = (PublisherSettings)Resources.Load(MxlvlPublish.serverSettingsAssetFile, typeof(PublisherSettings));


    static MxlvlPublish()
    {
#if UNITY_EDITOR
        if (MxlvlPublisherSettings == null)
        {
            // create PhotonServerSettings
            CreateSettings();
        }
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void CreateSettings()
    {
#if UNITY_EDITOR
        MxlvlPublish.MxlvlPublisherSettings = (PublisherSettings)Resources.Load(MxlvlPublish.serverSettingsAssetFile, typeof(PublisherSettings));
        if (MxlvlPublish.MxlvlPublisherSettings != null)
        {
            return;
        }

        if (MxlvlPublish.MxlvlPublisherSettings == null)
        {
            UnityEngine.Debug.Log("Mxlvl Publisher Settings Not Found - Generating");
            string _MxlvlResourcesPath = MxlvlPublish.FindMxlvlAssetFolder();

            _MxlvlResourcesPath += "Resources/";


            string serverSettingsAssetPath = _MxlvlResourcesPath + MxlvlPublish.serverSettingsAssetFile + ".asset";
            string settingsPath = Path.GetDirectoryName(serverSettingsAssetPath);
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
                AssetDatabase.ImportAsset(settingsPath);
            }

            MxlvlPublish.MxlvlPublisherSettings = (PublisherSettings)ScriptableObject.CreateInstance("PublisherSettings");
            if (MxlvlPublish.MxlvlPublisherSettings != null)
            {
                AssetDatabase.CreateAsset(MxlvlPublish.MxlvlPublisherSettings, serverSettingsAssetPath);
            }
        }
#endif
    }

    public static string FindMxlvlAssetFolder()
    {
        return "Assets/Mxlvl.Publish/";
    }
}
