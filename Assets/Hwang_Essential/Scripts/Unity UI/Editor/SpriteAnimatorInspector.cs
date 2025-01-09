using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpriteAnimator), true)]
    public class SpriteAnimatorInspector : Editor
    {
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

            // Draw other properties
            while (property.NextVisible(false))
            {
                // Find consecutive sprites when added sprite name ends with number
                if (string.Compare(property.name, "Sprites") == 0)
                {
                    int oldArraySize = property.arraySize;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        if (oldArraySize == 0)
                        {
                            FindConsecutiveSprites(target as SpriteAnimator);
                        }
                    }
                    if (property.isExpanded)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        EditorGUI.BeginDisabledGroup(property.arraySize != 1);
                        if (GUILayout.Button("Find consecutive sprites from a sprite"))
                        {
                            // Find consecutive sprites from a sprite
                            foreach (SpriteAnimator spriteAnimator in targets)
                            {
                                FindConsecutiveSprites(spriteAnimator);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        GUILayout.Space(20);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }

        private void FindConsecutiveSprites(SpriteAnimator spriteAnimator)
        {
            if (spriteAnimator.Sprites.Length == 1 && spriteAnimator.Sprites[0] != null)
            {
                string spriteName = spriteAnimator.Sprites[0].name;
                StringBuilder sb = new StringBuilder();
                for (int i = spriteName.Length - 1; i >= 0; i--)
                {
                    if (spriteName[i] < '0' || spriteName[i] > '9')
                    {
                        break;
                    }
                    sb.Insert(0, spriteName[i]);
                }
                if (sb.Length > 0)
                {
                    List<Sprite> sprites = new List<Sprite>();
                    int startNum = Convert.ToInt32(sb.ToString());
                    int endNum = Convert.ToInt32(new string('9', sb.Length));
                    string spritePath = AssetDatabase.GetAssetPath(spriteAnimator.Sprites[0]);
                    string fileExt = Path.GetExtension(spritePath);
                    string prefix = spritePath.Substring(0, spritePath.Length - fileExt.Length - sb.Length);
                    Sprite sprite;
                    string number;
                    for (int i = startNum; i <= endNum; i++)
                    {
                        number = Convert.ToString(i);
                        if (sb.Length > number.Length)
                        {
                            number = new string('0', sb.Length - number.Length) + number;
                        }
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(prefix + number + fileExt);
                        if (sprite != null)
                        {
                            sprites.Add(sprite);
                        }
                        else
                        {
                            break;
                        }
                    }
                    spriteAnimator.Sprites = sprites.ToArray();
                }
            }
        }
    }
}
