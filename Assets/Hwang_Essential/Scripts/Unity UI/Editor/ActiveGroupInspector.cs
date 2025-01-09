using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ActiveGroup), true)]
    public class ActiveGroupInspector : Editor
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
                EditorGUILayout.PropertyField(property, true);

                // Add a button below of "GameObjects" property when expanded
                if (string.Compare(property.name, "gameObjects") == 0 && property.isExpanded)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("Find game objects in children"))
                    {
                        // Find GameObject components in children for all selected ActiveGroup
                        foreach (ActiveGroup group in targets)
                        {
                            // Add children of ActiveGroup
                            group.Clear();
                            for (int i = 0; i < group.transform.childCount; i++)
                            {
                                group.Add(group.transform.GetChild(i).gameObject);
                            }
                            group.OnValidate();
                        }
                    }
                    GUILayout.Space(20);
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
