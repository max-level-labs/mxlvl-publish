using System;
using UnityEngine;

[Serializable]
public class MxlvlPublisherSettings : ScriptableObject
{
    public string PublisherKey;
    public string WebGLBuildPath;
    public string Version = "0.01";
}