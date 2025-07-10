using System.Text;
using UnityEngine;
using UnityEditor;

public class DefineActiveInputHandling
{
#if !UNITY_2019_2_OR_NEWER
    [InitializeOnLoadMethod]
    private static void OnInitialized()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAssemblyReloaded;
    }

    private static bool GetPlayerSettingsProperty(string name)
    {
        PlayerSettings[] playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (playerSettings.Length > 0)
        {
            SerializedObject settingsObject = new SerializedObject(playerSettings[0]);
            return settingsObject.FindProperty(name).boolValue;
        }
        return false;
    }

    private static void OnAssemblyReloaded()
    {
        AssemblyReloadEvents.afterAssemblyReload -= OnAssemblyReloaded;
        bool inputSystemEnabled = GetPlayerSettingsProperty("enableNativePlatformBackendsForNewInputSystem");
        bool inputManagerEnabled = !GetPlayerSettingsProperty("disableOldInputManagerSupport");
        Debug.Log("inputSystemEnabled = " + inputSystemEnabled + ", inputManagerEnabled = " + inputManagerEnabled);
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        string[] parts = defineSymbols.Split(';');
        StringBuilder sb = new StringBuilder();
        foreach (string part in parts)
        {
            if (string.Compare(part, "ENABLE_INPUT_SYSTEM") != 0 && string.Compare(part, "ENABLE_LEGACY_INPUT_MANAGER") != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(';');
                }
                sb.Append(part);
            }
        }
        if (inputManagerEnabled)
        {
            if (sb.Length > 0)
            {
                sb.Append(';');
            }
            sb.Append("ENABLE_LEGACY_INPUT_MANAGER");
        }
#if UNITY_2018_3_OR_NEWER
        if (inputSystemEnabled)
        {
            if (sb.Length > 0)
            {
                sb.Append(';');
            }
            sb.Append("ENABLE_INPUT_SYSTEM");
        }
#endif
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sb.ToString());
    }
#endif
}
