using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomCursor), true)]
    public class CustomCursorInspector : Editor
    {
        private bool changed = false;

        private void OnDisable()
        {
            // Save ScriptableObject asset when some properties have changed
            if (changed)
            {
                changed = false;
                AssetDatabase.SaveAssets();
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

            // Draw "Script" property at first
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();

            // Draw other properties
            while (property.NextVisible(false))
            {
                // Find consecutive textures when added texture name ends with number
                if (string.Compare(property.name, "Texture") == 0)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        if (property.objectReferenceValue != null)
                        {
                            FindConsecutiveTextures(target as CustomCursor, property.objectReferenceValue as Texture2D);
                        }
                        changed = true;
                    }
                }
                else if (string.Compare(property.name, "AnimationTextures") == 0)
                {
                    int oldArraySize = property.arraySize;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        if (oldArraySize == 0)
                        {
                            FindConsecutiveTextures(target as CustomCursor);
                        }
                        changed = true;
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        changed = true;
                    }
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }

        private void FindConsecutiveTextures(CustomCursor customCursor, Texture2D sourceTexture)
        {
            if (customCursor != null && sourceTexture != null)
            {
                string textureName = sourceTexture.name;
                StringBuilder sb = new StringBuilder();
                for (int i = textureName.Length - 1; i >= 0; i--)
                {
                    if (textureName[i] < '0' || textureName[i] > '9')
                    {
                        break;
                    }
                    sb.Insert(0, textureName[i]);
                }
                if (sb.Length > 0)
                {
                    List<Texture2D> textures = new List<Texture2D>();
                    int startNum = Convert.ToInt32(sb.ToString());
                    int endNum = Convert.ToInt32(new string('9', sb.Length));
                    string texturePath = AssetDatabase.GetAssetPath(sourceTexture);
                    string fileExt = Path.GetExtension(texturePath);
                    string prefix = texturePath.Substring(0, texturePath.Length - fileExt.Length - sb.Length);
                    Texture2D texture;
                    string number;
                    for (int i = startNum; i <= endNum; i++)
                    {
                        number = Convert.ToString(i);
                        if (sb.Length > number.Length)
                        {
                            number = new string('0', sb.Length - number.Length) + number;
                        }
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + number + fileExt);
                        if (texture != null)
                        {
                            textures.Add(texture);
                        }
                        else
                        {
                            break;
                        }
                    }
                    customCursor.AnimationTextures = textures.ToArray();
                }
            }
        }

        private void FindConsecutiveTextures(CustomCursor customCursor)
        {
            if (customCursor != null && customCursor.AnimationTextures.Length == 1 && customCursor.AnimationTextures[0] != null)
            {
                FindConsecutiveTextures(customCursor, customCursor.AnimationTextures[0]);
            }
        }
    }
}
