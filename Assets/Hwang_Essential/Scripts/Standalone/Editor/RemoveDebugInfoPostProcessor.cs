using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using System.IO;

public class RemoveDebugInfoPostProcessor
{
    [PostProcessBuild]
    private static void OnPostProcessBuild(BuildTarget target, string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string burstDebugInformationPath = Path.Combine(buildPath, string.Format("{0}_BurstDebugInformation_DoNotShip", Application.productName));
        if (Directory.Exists(burstDebugInformationPath))
        {
            try
            {
                Directory.Delete(burstDebugInformationPath, true);
                Debug.Log("Burst debug information (that no one wants) has been removed.");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to delete Burst debug information : {0]", ex));
            }
        }
        string il2cppDebugInformationPath = Path.Combine(buildPath, string.Format("{0}_BackUpThisFolder_ButDontShipItWithYourGame", Application.productName));
        if (Directory.Exists(il2cppDebugInformationPath))
        {
            try
            {
                Directory.Delete(il2cppDebugInformationPath, true);
                Debug.Log("IL2CPP debug information (that no one wants) has been removed.");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to delete IL2CPP debug information : {0]", ex));
            }
        }
    }
}
