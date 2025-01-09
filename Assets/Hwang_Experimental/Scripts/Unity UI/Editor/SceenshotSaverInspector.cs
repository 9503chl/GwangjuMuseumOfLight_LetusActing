using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(ScreenshotSaver), true)]
    public class ScreenshotSaverInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            ScreenshotSaver saver = target as ScreenshotSaver;
            if (saver == null)
            {
                return;
            }

            string screenshotPath = Path.GetDirectoryName(saver.GetScreenshotPath());

            // Add a button to save screenshot in editor
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Save current screen"))
            {
                saver.SaveScreenshot();
            }
            EditorGUI.BeginDisabledGroup(!Directory.Exists(screenshotPath));
            if (GUILayout.Button("Open screenshot folder"))
            {
                EditorUtility.RevealInFinder(screenshotPath);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
