using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace UnityEngine
{
    [CustomEditor(typeof(SceneLoader), true)]
    public class SceneLoaderInspector : Editor
    {
        private string[] GetEditorBuildScenes()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    result.Add(Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path));
                }
            }
            return result.ToArray();
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector when "Script" is missing or null
            if (target == null)
            {
                base.OnInspectorGUI();
                return;
            }

            // Update SerializedObject and get first property
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            // Draw "Script" property at first
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();

            SceneLoader sceneLoader = target as SceneLoader;
            string[] scenes = GetEditorBuildScenes();

            // Draw other properties
            while (property.NextVisible(false))
            {
                if (string.Compare(property.name, "HomeSceneName") == 0)
                {
                    List<string> sceneNames = new List<string>(scenes);
                    int sceneIndex = sceneNames.IndexOf(property.stringValue) + 1;
                    if (sceneIndex == 0)
                    {
                        property.stringValue = string.Empty;
                    }
                    sceneNames.Insert(0, "< Scene Index = 0 >");
                    EditorGUI.BeginChangeCheck();
                    sceneIndex = EditorGUILayout.Popup("Home Scene", sceneIndex, sceneNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (sceneIndex == 0)
                        {
                            property.stringValue = string.Empty;
                        }
                        else
                        {
                            property.stringValue = sceneNames[sceneIndex];
                        }
                    }
                }
                else if (string.Compare(property.name, "PreviousSceneName") == 0)
                {
                    List<string> sceneNames = new List<string>(scenes);
                    int sceneIndex = sceneNames.IndexOf(property.stringValue) + 1;
                    if (sceneIndex == 0)
                    {
                        property.stringValue = string.Empty;
                    }
                    sceneNames.Insert(0, "< Scene Index - 1 >");
                    EditorGUI.BeginChangeCheck();
                    sceneIndex = EditorGUILayout.Popup("Previous Scene", sceneIndex, sceneNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (sceneIndex == 0)
                        {
                            property.stringValue = string.Empty;
                        }
                        else
                        {
                            property.stringValue = sceneNames[sceneIndex];
                        }
                    }
                }
                else if (string.Compare(property.name, "NextSceneName") == 0)
                {
                    List<string> sceneNames = new List<string>(scenes);
                    int sceneIndex = sceneNames.IndexOf(property.stringValue) + 1;
                    if (sceneIndex == 0)
                    {
                        property.stringValue = string.Empty;
                    }
                    sceneNames.Insert(0, "< Scene Index + 1 >");
                    EditorGUI.BeginChangeCheck();
                    sceneIndex = EditorGUILayout.Popup("Next Scene", sceneIndex, sceneNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (sceneIndex == 0)
                        {
                            property.stringValue = string.Empty;
                        }
                        else
                        {
                            property.stringValue = sceneNames[sceneIndex];
                        }
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Home scene"))
                {
                    sceneLoader.LoadHomeScene();
                }
                if (GUILayout.Button("Previous scene"))
                {
                    sceneLoader.LoadPreviousScene();
                }
                if (GUILayout.Button("Next scene"))
                {
                    sceneLoader.LoadNextScene();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
