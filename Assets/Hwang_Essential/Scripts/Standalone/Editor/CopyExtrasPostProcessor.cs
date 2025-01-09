using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using System.IO;

public class CopyExtrasPostProcessor
{
    [PostProcessBuild]
    private static void OnPostProcessBuild(BuildTarget target, string path)
    {
        string platformDir = null;
        switch (target)
        {
#if !UNITY_2019_2_OR_NEWER
            case BuildTarget.StandaloneLinux:
                platformDir = "x86";
                break;
            case BuildTarget.StandaloneLinuxUniversal:
#endif
            case BuildTarget.StandaloneLinux64:
                platformDir = "x86_64";
                break;
#if UNITY_2017_3_OR_NEWER
            case BuildTarget.StandaloneOSX:
#else
            case BuildTarget.StandaloneOSXIntel:
                platformDir = "x86";
                break;
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
#endif
                platformDir = "x86_64";
                break;
            case BuildTarget.StandaloneWindows:
                platformDir = "x86";
                break;
            case BuildTarget.StandaloneWindows64:
                platformDir = "x86_64";
                break;
            default:
                return;
        }

        if (!string.IsNullOrEmpty(platformDir))
        {
            string extraPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Extras");
            string extraPlatformPath = Path.Combine(extraPath, platformDir);
            string buildPath = Path.GetDirectoryName(path);
            if (Directory.Exists(extraPath) && Directory.Exists(extraPlatformPath))
            {
                int count = CopyAllFiles(extraPlatformPath, buildPath, true);
                if (count > 0)
                {
                    Debug.Log(string.Format("Extra {0} files are copied into build directory.", count));
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(extraPath);
                    Directory.CreateDirectory(extraPlatformPath);
                }
                catch (Exception)
                {
                }
                Debug.Log("To add extra files into standalone build, you can put them into Extras directory before build.");
            }
        }
    }

    private static int CopyAllFiles(string sourcePath, string targetPath, bool includeSubDirs)
    {
        int count = 0;
        if (Directory.Exists(sourcePath) && Directory.Exists(targetPath))
        {
            string[] entries = Directory.GetFileSystemEntries(sourcePath);
            foreach (string entry in entries)
            {
                string entryName = Path.GetFileName(entry);
                string sourceFilePath = Path.Combine(sourcePath, entryName);
                string targetFilePath = Path.Combine(targetPath, entryName);
                if (File.Exists(sourceFilePath))
                {
                    try
                    {
                        File.Copy(sourceFilePath, targetFilePath, true);
                        count++;
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (includeSubDirs && Directory.Exists(sourceFilePath))
                {
                    try
                    {
                        Directory.CreateDirectory(targetFilePath);
                        count += CopyAllFiles(sourceFilePath, targetFilePath, true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        return count;
    }
}
