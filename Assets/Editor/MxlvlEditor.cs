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
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
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
    }




    private static void EditorUpdate()
    {
        if (PhotonNetwork.PhotonServerSettings == null)
        {
            PhotonNetwork.CreateSettings();
        }
        if (PhotonNetwork.PhotonServerSettings == null)
        {
            return;
        }
    }

    private static void SaveSettings()
    {
        EditorUtility.SetDirty(PhotonNetwork.PhotonServerSettings);
    }

    protected virtual void OnGUI()
    {
        string game_deployment_id = "6fa9d96e-a257-40b1-bed3-86a64abb8eb8";
        GUI.skin.label.wordWrap = true;

        // setup info text
        GUI.skin.label.richText = true;
        GUILayout.Label("Welcome to the Mxlvl Publisher tool for Unity.\nThis window will let you publish your game to the Mxlvl platform <b>AFTER</b> you have built a WebGL platform version of your game.\n\n<b>-</b>Enter the Publisher ID below\n<b>-</b>Select folder location of the WebGL platform build\n<b>-</b>Click Publish\n\n*upload can take up to 5 minutes");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Version");
        EditorGUILayout.TextField("0.0.1").Trim();
        GUILayout.EndHorizontal();

        GUILayout.Label("Game Deployment ID");
        EditorGUILayout.TextField("e8aefb90-a930-4d95-9315-ab1aa60ca930").Trim();
        GUILayout.Space(5);

        EditorGUILayout.TextField(this.webglBuildFolder).Trim();
        if (GUILayout.Button("Select WebGL Build Folder"))
        {
            this.webglBuildFolder = EditorUtility.OpenFolderPanel("WebGL Platform Build Folder", "", "");
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
            try
            {
                UploadUrls urls = GetS3PresignedUrls(game_deployment_id);
                foreach (string file in this.uploadFiles)
                {
                    Debug.Log(file);
                    string upload_url_name = file.Replace(".unityweb", "");
                    upload_url_name = upload_url_name.Replace('.', '_');
                    string filePath = Path.Combine(dirToBuildFiles, file);
                    if (File.Exists(filePath))
                    {
                        Debug.Log(upload_url_name);
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
                Debug.Log(e.Message);
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
            form.Add(new MultipartFormDataSection("version", "1.0.0"));

            string URL = string.Format("https://mxlvl-api.herokuapp.com/api/games/{0}/upload/", game_deployment_id);
            UnityWebRequest www = UnityWebRequest.Post(URL, "");
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
                string text = www.downloadHandler.text;
                Debug.Log(text);
                UploadUrls url = JsonUtility.FromJson<UploadUrls>(text);
                return url;
            }


        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        return null;
    }

    public UploadUrls GetS3PresignedUrlsX()
    {
        try
        {
            string URL = "https://mxlvl-api.herokuapp.com/api/games/b37eeb4b-2eef-49c9-b4c1-7561441e2b37/upload/urls/";
            UnityWebRequest www = UnityWebRequest.Post(URL, "");
            www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                while (www.responseCode == 0)
                {
                }
                string text = www.downloadHandler.text;
                Debug.Log(text);
                UploadUrls url = JsonUtility.FromJson<UploadUrls>(text) ;
                return url;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        return null;
    }
}

