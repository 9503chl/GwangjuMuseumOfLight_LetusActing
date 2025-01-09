using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SystemFontLoader), true)]
    public class SystemFontLoaderInspector : Editor
    {
        private static readonly List<string> fontNames = new List<string>();
        private int fontIndex;
        private Vector2 scrollPosition;
        private bool previewMode;

        private void OnEnable()
        {
            if (fontNames.Count == 0)
            {
                fontNames.AddRange(Font.GetOSInstalledFontNames());
            }
            fontIndex = -1;
        }

        private void OnDisable()
        {
            if (previewMode)
            {
                previewMode = false;
                SystemFontLoader loader = target as SystemFontLoader;
                if (loader != null)
                {
                    loader.Restore();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            // Show list of installed fonts in OS
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Installed fonts in current OS");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(210));
            EditorGUI.BeginChangeCheck();
            fontIndex = GUILayout.SelectionGrid(fontIndex, fontNames.ToArray(), 1);
            if (EditorGUI.EndChangeCheck())
            {
                if (fontIndex >= 0 && fontIndex < fontNames.Count)
                {
                    EditorGUI.FocusTextInControl(null);
                    if (previewMode)
                    {
                        foreach (Object target in targets)
                        {
                            SystemFontLoader loader = target as SystemFontLoader;
                            if (loader != null)
                            {
                                loader.ApplyFont(fontNames[fontIndex], loader.DesiredFontSize);
                            }
                        }
                    }
                    else
                    {
                        foreach (Object target in targets)
                        {
                            SystemFontLoader dynamicFont = target as SystemFontLoader;
                            if (dynamicFont != null)
                            {
                                dynamicFont.ToggleDesiredFont(fontNames[fontIndex]);
                            }
                        }
                        fontIndex = -1;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            // Add a checkbox for preview mode
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                previewMode = EditorGUILayout.ToggleLeft("PREVIEW MODE", previewMode);
                if (EditorGUI.EndChangeCheck())
                {
                    if (previewMode)
                    {
                        SystemFontLoader loader = target as SystemFontLoader;
                        fontIndex = fontNames.IndexOf(loader.GetComponent<Text>().font.name);
                    }
                    else
                    { 
                        foreach (Object target in targets)
                        {
                            SystemFontLoader loader = target as SystemFontLoader;
                            if (loader != null)
                            {
                                loader.Restore();
                            }
                        }
                        fontIndex = -1;
                    }
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
