using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TextureLoader), true)]
    public class TextureLoaderInspector : Editor
    {
        private SerializedProperty localizableProperty;
        private string assetTypeName;
        private string componentTypeName;
        private bool modified;
        private bool required;
        private bool loading;
        private bool loaded;
        private int rowCount;
        private bool hasRenderer;
        private bool hasMaterial;
        private bool hasProperty;

        private void OnEnable()
        {
            localizableProperty = serializedObject.FindProperty("localizable");
            hasRenderer = false;
            foreach (AssetLoader loader in targets)
            {
                if (assetTypeName == null || componentTypeName == null)
                {
                    assetTypeName = loader.GetAssetTypeName();
                    componentTypeName = loader.GetComponentTypeName();
                }
                if (loader.GetComponent<Renderer>() != null)
                {
                    hasRenderer = true;
                }
                if (!Application.isPlaying && !loader.WasLoaded)
                {
                    loader.Load();
                }
            }
        }

        private void OnDisable()
        {
            foreach (AssetLoader loader in targets)
            {
                if (!Application.isPlaying && loader.WasLoaded)
                {
                    loader.Unload();
                }
            }
        }

        private static bool HasField<T>(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                FieldInfo fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null && (fieldInfo.Attributes & FieldAttributes.FieldAccessMask) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector when "Script" is missing or null
            if (target == null)
            {
                base.OnInspectorGUI();
                return;
            }

            modified = false;
            required = false;
            loading = false;
            loaded = false;
            rowCount = 0;
            hasMaterial = false;
            hasProperty = false;

            // Update SerializedObject and get first property
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            // Does not draw "Script" property
            EditorGUILayout.Space();

            // Draw other properties
            while (property.NextVisible(false))
            {
                if (string.Compare(property.name, "materialName") == 0 || string.Compare(property.name, "propertyName") == 0)
                {
                    if (!hasRenderer)
                    {
                        continue;
                    }
                }
                EditorGUI.BeginChangeCheck();
                if (string.Compare(property.name, "language") == 0 && localizableProperty != null && !localizableProperty.boolValue)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(property);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.PropertyField(property);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (HasField<AssetLoader>(property.name))
                    {
                        modified = true;
                    }
                }
            }

            // Draw preview contents
            GUILayout.BeginHorizontal();
            foreach (TextureLoader loader in targets)
            {
                if (!loader.HasRequiredComponent())
                {
                    required = true;
                }
                else
                {
                    if (modified)
                    {
                        loader.Load();
                    }
                    if (loader.IsLoading)
                    {
                        loading = true;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.HelpBox("Loading...", MessageType.None);
                        GUILayout.FlexibleSpace();
                    }
                    else
                    {
                        GUIContent previewContent = loader.PreviewContent;
                        if (previewContent != null)
                        {
                            loaded = true;
                            Material material = loader.GetMaterial();
                            if (material != null)
                            {
                                hasMaterial = true;
                                if (string.IsNullOrEmpty(loader.PropertyName))
                                {
                                    hasProperty = true;
                                }
                                else
                                {
                                    hasProperty = material.HasProperty(loader.PropertyName);
                                }
                            }
                            GUILayout.FlexibleSpace();
                            if (previewContent == GUIContent.none)
                            {
                                EditorGUILayout.HelpBox("No content!", MessageType.Info);
                            }
                            else
                            {
#if UNITY_2018_1_OR_NEWER
                                EditorGUILayout.HelpBox(previewContent);
#else
                                TextAnchor alignment = GUI.skin.box.alignment;
                                GUI.skin.box.alignment = TextAnchor.MiddleLeft;
                                GUILayout.Box(previewContent);
                                GUI.skin.box.alignment = alignment;
#endif
                            }
                            GUILayout.FlexibleSpace();
                            if (++rowCount % 2 == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (required)
            {
                EditorGUILayout.HelpBox(string.Format("{0} component is required!", componentTypeName), MessageType.Warning);
            }
            else if (loading)
            {
                EditorUtility.SetDirty(target);
                Repaint();
            }
            else if (!loaded)
            {
                EditorGUILayout.HelpBox(string.Format("{0} not found in resources.", assetTypeName), MessageType.Info);
            }
            else
            {
                if (hasRenderer && !hasMaterial)
                {
                    EditorGUILayout.HelpBox("No matching material found.", MessageType.Warning);
                }
                else if (hasRenderer && !hasProperty)
                {
                    EditorGUILayout.HelpBox("No matching property found.", MessageType.Warning);
                }
                else if (hasRenderer && !Application.isPlaying)
                {
                    EditorGUILayout.HelpBox(string.Format("{0} can only be applied while playing.", assetTypeName), MessageType.Info);
                }
                else
                {
                    // Draw apply button
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("Apply now!"))
                    {
                        foreach (AssetLoader loader in targets)
                        {
                            loader.Apply();
                        }
                        EditorUtility.SetDirty(target);
                    }
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
