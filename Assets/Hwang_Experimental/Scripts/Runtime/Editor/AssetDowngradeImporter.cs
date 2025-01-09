using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class AssetDowngradeImporter : AssetPostprocessor
{
#if !UNITY_2019_3_OR_NEWER
    private static Dictionary<string, string> replacePairs = new Dictionary<string, string>()
    {
        { // Graphic Raycaster
            "m_Script: {fileID: 11500000, guid: dc42784cf147c0c48a680349fa168899, type: 3}",
            "m_Script: {fileID: 1301386320, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // CanvasScaler
            "m_Script: {fileID: 11500000, guid: 0cd44c1031e13a943bb63640046fad76, type: 3}",
            "m_Script: {fileID: 1980459831, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // EventSystem
            "m_Script: {fileID: 11500000, guid: 76c392e42b5098c458856cdf6ecaaaa1, type: 3}",
            "m_Script: {fileID: -619905303, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // StandaloneInputModule
            "m_Script: {fileID: 11500000, guid: 4f231c4fb786f3946a6b90b886c48677, type: 3}",
            "m_Script: {fileID: 1077351063, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Text
            "m_Script: {fileID: 11500000, guid: 5f7201a12d95ffc409449d95f23cf332, type: 3}",
            "m_Script: {fileID: 708705254, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Image
            "m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}",
            "m_Script: {fileID: -765806418, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Mask
            "m_Script: {fileID: 11500000, guid: 31a19414c41e5ae4aae2af33fee712f6, type: 3}",
            "m_Script: {fileID: -1200242548, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Shadow
            "m_Script: {fileID: 11500000, guid: cfabb0440166ab443bba8876756fdfa9, type: 3}",
            "m_Script: {fileID: 1573420865, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // RawImage
            "m_Script: {fileID: 11500000, guid: 1344c3c82d62a2a41a3576d8abb8e3ea, type: 3}",
            "m_Script: {fileID: -98529514, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Button
            "m_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}",
            "m_Script: {fileID: 1392445389, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // InputField
            "m_Script: {fileID: 11500000, guid: d199490a83bb2b844b9695cbf13b01ef, type: 3}",
            "m_Script: {fileID: 575553740, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Scrollbar
            "m_Script: {fileID: 11500000, guid: 2a4db7a114972834c8e4117be1d82ba3, type: 3}",
            "m_Script: {fileID: -2061169968, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // ScrollRect
            "m_Script: {fileID: 11500000, guid: 1aa08ab6e0800fa44ae55d278d1423e3, type: 3}",
            "m_Script: {fileID: 1367256648, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Dropdown
            "m_Script: {fileID: 11500000, guid: 0d0b652f32a2cc243917e4028fa0f046, type: 3}",
            "m_Script: {fileID: 853051423, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Toggle
            "m_Script: {fileID: 11500000, guid: 9085046f02f69544eb97fd06b6048fe2, type: 3}",
            "m_Script: {fileID: 2109663825, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Slider
            "m_Script: {fileID: 11500000, guid: 67db9e8f0e2ae9c40bc1e2b64352a6b4, type: 3}",
            "m_Script: {fileID: -113659843, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Content Size Fitter
            "m_Script: {fileID: 11500000, guid: 3245ec927659c4140ac4f8d17403cc18, type: 3}",
            "m_Script: {fileID: 1741964061, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Grid Layout Group
            "m_Script: {fileID: 11500000, guid: 8a8695521f0d02e499659fee002a26c2, type: 3}",
            "m_Script: {fileID: -2095666955, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Vertical Layout Group
            "m_Script: {fileID: 11500000, guid: 59f8146938fff824cb5fd77236b75775, type: 3}",
            "m_Script: {fileID: 1297475563, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Horizontal Layout Group
            "m_Script: {fileID: 11500000, guid: 30649d3a9faa99c48a7b1166b86bf2a0, type: 3}",
            "m_Script: {fileID: -405508275, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        },
        { // Layout Element
            "m_Script: {fileID: 11500000, guid: 306cc8c2b49d7114eaa3623786fc2126, type: 3}",
            "m_Script: {fileID: 1679637790, guid: f70555f144d8491a825f0804e09c671c, type: 3}"
        }
    };

    public static bool DowngradeAsset(string path)
    {
        int count = 0;
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++)
        {
            foreach (KeyValuePair<string, string> pair in replacePairs)
            {
                int p = lines[i].IndexOf(pair.Key);
                if (p != -1)
                {
                    lines[i] = lines[i].Replace(pair.Key, pair.Value);
                    count++;
                }
            }
        }
        if (count > 0)
        {
            File.WriteAllLines(path, lines, Encoding.UTF8);
            return true;
        }
        return false;
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            string fileExt = Path.GetExtension(importedAsset);
            if (string.Compare(fileExt, ".unity", true) == 0 || string.Compare(fileExt, ".prefab", true) == 0)
            {
                if (DowngradeAsset(importedAsset))
                {
                    Debug.Log(string.Format("Downgraded asset : {0}", importedAsset));
                    AssetDatabase.ImportAsset(importedAsset, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
#endif
}
