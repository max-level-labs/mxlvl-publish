using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Specialized;

public class MxlvlWizardText
{
    public string WindowTitle = "Mxlvl Wizard";
}

public class FileUploader
{

}

[InitializeOnLoad]
public class MxlvlEditor : EditorWindow
{
    protected static Type WindowType = typeof(MxlvlEditor);
    public static MxlvlWizardText CurrentLang = new MxlvlWizardText();

    protected Vector2 scrollPos = Vector2.zero;

    private string webglBuildFolder = string.Empty;

    private string version = string.Empty;

    private string gameId = string.Empty;

    private string[] uploadFiles = {"webgl_build.json",
                                "webgl_build.asm.code.unityweb",
                                "webgl_build.asm.framework.unityweb",
                                "webgl_build.asm.memory.unityweb",
                                "webgl_build.data.unityweb",
                                "UnityLoader.js" };

    private string buildFolder = "Build";



    private void Awake()
    {

    }

    private static void SaveSettings()
    {
        EditorUtility.SetDirty(MxlvlPublish.MxlvlPublisherSettings);
    }

    static MxlvlEditor()
    {

#if UNITY_2018
        EditorApplication.projectChanged += EditorUpdate;
        EditorApplication.hierarchyChanged += EditorUpdate;
#else
        EditorApplication.projectWindowChanged += EditorUpdate;
        EditorApplication.hierarchyWindowChanged += EditorUpdate;
#endif

    }

    [MenuItem("Window/Mxlvl Unity Publisher/Mxlvl Wizard", false, 0)]
    protected static void MenuItemOpenWizard()
    {
        MxlvlEditor win = GetWindow(WindowType, false, CurrentLang.WindowTitle, true) as MxlvlEditor;
        win.version = MxlvlPublish.MxlvlPublisherSettings.Version;
        win.webglBuildFolder = MxlvlPublish.MxlvlPublisherSettings.WebGLBuildPath;
        win.gameId = MxlvlPublish.MxlvlPublisherSettings.PublisherKey;
    }


    private static void EditorUpdate()
    {
        if (MxlvlPublish.MxlvlPublisherSettings == null)
        {
            MxlvlPublish.CreateSettings();
        }
        if (MxlvlPublish.MxlvlPublisherSettings == null)
        {
            return;
        }
    }

    protected virtual void OnGUI()
    {
        GUI.skin.label.wordWrap = true;

        // setup info text
        GUI.skin.label.richText = true;
        GUILayout.Label("Welcome to the Mxlvl Publisher tool for Unity.\nThis window will let you publish your game to the Mxlvl platform <b>AFTER</b> you have built a WebGL platform version of your game.\n\n<b>-</b>Enter the Publisher ID below\n<b>-</b>Select folder location of the WebGL platform build\n<b>-</b>Click Publish\n\n*upload can take up to 5 minutes");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Version");
        this.version = EditorGUILayout.TextField(this.version).Trim();
        GUILayout.EndHorizontal();

        GUILayout.Label("Game Deployment ID");
        string setGameId = EditorGUILayout.TextField(this.gameId).Trim();
        GUILayout.Space(5);

        if (setGameId != this.gameId)
            this.gameId = setGameId;

        EditorGUILayout.TextField(this.webglBuildFolder).Trim();
        if (GUILayout.Button("Select WebGL Build Folder"))
        {
            this.webglBuildFolder = EditorUtility.OpenFolderPanel("WebGL Platform Build Folder", "", "");
            this.HasUpdated();

        }
        GUILayout.Space(5);

        string dirToBuildFiles = Path.Combine(this.webglBuildFolder, buildFolder);

        if (!string.IsNullOrEmpty(this.webglBuildFolder))
        {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            foreach (string file in this.uploadFiles)
            {
                string filePath = Path.Combine(dirToBuildFiles, file);
                if (!File.Exists(filePath))
                {
                    GUILayout.Label("File Not Found: " + file);
                    Debug.Log("File Not Found: " + file);
                }
            }
            GUILayout.EndScrollView();
        }
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(this.webglBuildFolder));
        if (GUILayout.Button("[Publish]"))
        {
            this.HasUpdated();
    
            try
            {
                UploadUrls urls = GetS3PresignedUrls(this.gameId);
                foreach (string file in this.uploadFiles)
                {
                    continue;
                    string upload_url_name = file.Replace(".unityweb", "");
                    upload_url_name = upload_url_name.Replace('.', '_');
                    string filePath = Path.Combine(dirToBuildFiles, file);
                    if (File.Exists(filePath))
                    {
                        if (file == "webgl_build.asm.code.unityweb")
                        {
                            Debug.Log(ExecCommand.ExecuteCommand(string.Format(urls.webgl_build_asm_code_unityweb, filePath)));
                        }
                        if (upload_url_name == "webgl_build_asm_framework")
                        {
                            Debug.Log(ExecCommand.ExecuteCommand(string.Format(urls.webgl_build_asm_framework_unityweb, filePath)));
                        }
                        if (upload_url_name == "webgl_build_asm_memory")
                        {
                            Debug.Log(ExecCommand.ExecuteCommand(string.Format(urls.webgl_build_asm_memory_unityweb, filePath)));
                        }
                        if (upload_url_name == "webgl_build_data")
                        {
                            Debug.Log(ExecCommand.ExecuteCommand(string.Format(urls.webgl_build_data_unityweb, filePath)));
                        }
                    }
                }

            }catch(Exception e)
            {
    
            }
        }
        EditorGUI.EndDisabledGroup();

        GUI.skin.label.richText = false;

        
    }

    public UploadUrls GetS3PresignedUrls(string game_deployment_id)
    {
        try
        {

            List<IMultipartFormSection> form = new List<IMultipartFormSection>();
            string dirToBuildFiles = Path.Combine(this.webglBuildFolder, buildFolder);

            form.Add(new MultipartFormFileSection("file", System.IO.File.ReadAllBytes(Path.Combine(dirToBuildFiles, "UnityLoader.js")), "UnityLoader.js", "javascript"));
            form.Add(new MultipartFormFileSection("file", System.IO.File.ReadAllBytes(Path.Combine(dirToBuildFiles, "webgl_build.json")), "webgl_build.json", "json"));
            form.Add(new MultipartFormDataSection("version", "1.1.0"));
            form.Add(new MultipartFormDataSection("access_key", game_deployment_id));

            string URL = "https://mxlvl-api.herokuapp.com/api/games/upload/";
            using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
            {
                www.SendWebRequest();
                Debug.Log("Sent Request");
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    while (www.responseCode == 0)
                    {

                    }
                    string text = System.Text.Encoding.ASCII.GetString(www.downloadHandler.data);
                    Debug.Log(text + " : " + www.responseCode + " " + www.downloadedBytes + " data leng: " + www.downloadHandler.data.Length);
                    UploadUrls url = JsonUtility.FromJson<UploadUrls>(text);
                    return url;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        return null;
    }

    public void HasUpdated()
    {
        if (MxlvlPublish.MxlvlPublisherSettings.PublisherKey != this.gameId)
        {
            MxlvlPublish.MxlvlPublisherSettings.PublisherKey = this.gameId;
            MxlvlEditor.SaveSettings();
        }

        if (MxlvlPublish.MxlvlPublisherSettings.WebGLBuildPath != this.webglBuildFolder)
        {
            MxlvlPublish.MxlvlPublisherSettings.WebGLBuildPath = this.webglBuildFolder;
            MxlvlEditor.SaveSettings();
        }

        if (MxlvlPublish.MxlvlPublisherSettings.WebGLBuildPath != this.version)
        {
            MxlvlPublish.MxlvlPublisherSettings.Version = this.version;
            MxlvlEditor.SaveSettings();
        }

    }

}

