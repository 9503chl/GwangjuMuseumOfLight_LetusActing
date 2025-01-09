using UnityEditor;
using UnityEngine;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TextureLoader), true)]
    public class TextureLoaderInspector : Editor
    {
        private bool modified;
        private bool required;
        private bool loaded;
        private bool hasRenderer;
        private bool hasMaterial;
        private bool hasProperty;

        private void OnEnable()
        {
            hasRenderer = false;
            // Load all resources when inspector got focus
            foreach (TextureLoader loader in targets)
            {
                loader.Load();
                if (loader.GetComponent<Renderer>() != null)
                {
                    hasRenderer = true;
                }
            }
        }

        private void OnDisable()
        {
            // Unload all resources when inspector lost focus
            foreach (TextureLoader loader in targets)
            {
                loader.Unload();
            }
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

            // Does not draw "Script" property
            EditorGUILayout.Space();

            modified = false;
            required = false;
            loaded = false;
            hasMaterial = false;
            hasProperty = false;

            // Draw other properties
            while (property.NextVisible(false))
            {
                if (string.Compare(property.name, "materialIndex") == 0 || string.Compare(property.name, "textureName") == 0)
                {
                    if (!hasRenderer)
                    {
                        continue;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property, true);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    modified = true;
                }
            }

            // Draw preview
            int rowCount = 0;
            GUILayout.BeginHorizontal();
            foreach (TextureLoader loader in targets)
            {
                if (loader.GetComponent<Renderer>() == null && loader.GetComponent<RawImage>() == null)
                {
                    required = true;
                }
                else
                {
                    if (modified)
                    {
                        loader.Load();
                    }

                    GUIContent preview = loader.Preview;
                    if (preview != null)
                    {
                        loaded = true;
                        Material material = loader.GetMaterial();
                        if (material != null)
                        {
                            hasMaterial = true;
                            if (string.IsNullOrEmpty(loader.TextureName))
                            {
                                hasProperty = true;
                            }
                            else
                            {
#if UNITY_2018_2_OR_NEWER
                                string[] propertyNames = material.GetTexturePropertyNames();
                                for (int i = 0; i < propertyNames.Length; i++)
                                {
                                    if (string.Compare(propertyNames[i], loader.TextureName, true) == 0)
                                    {
                                        hasProperty = true;
                                        break;
                                    }
                                }
#else
                                hasProperty = material.HasProperty(loader.TextureName);
#endif
                            }
                        }
                        GUILayout.FlexibleSpace();
#if UNITY_2019_3_OR_NEWER
                        EditorGUILayout.HelpBox(preview);
#else
                        GUILayout.Box(preview);
#endif
                        GUILayout.FlexibleSpace();

                        if (modified && loader.enabled)
                        {
                            loader.Apply();
                        }

                        if (++rowCount % 2 == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (required)
            {
                EditorGUILayout.HelpBox("Renderer or RawImage component is required!", MessageType.Warning);
            }
            else if (!loaded)
            {
                EditorGUILayout.HelpBox("Texture not found in resources.", MessageType.Info);
            }
            else if (hasRenderer)
            {
                if (!hasMaterial)
                {
                    EditorGUILayout.HelpBox("Material index is out of range.", MessageType.Warning);
                }
                else if (!hasProperty)
                {
                    EditorGUILayout.HelpBox("There is no matching texture property.", MessageType.Warning);
                }
                else if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Texture will be applied only while playing.", MessageType.Info);
                }
            }
        }
    }
}
