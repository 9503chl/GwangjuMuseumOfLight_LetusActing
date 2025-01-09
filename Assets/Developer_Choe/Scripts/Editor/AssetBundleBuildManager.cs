using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public class AssetBundleBuildManager
{
    [MenuItem("Mytool/AssetBundle Build")]
    public static void AssetBundleBuild()
    {
        string path = ProjectSettings.AssetBundlePath;
        if (!path.IsNullOrEmpty())
        {
            path = "./Bundle";
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        EditorUtility.DisplayDialog("AssetBundle Build", "Succeed in Build AssetBundle", "Ok");
    }
}