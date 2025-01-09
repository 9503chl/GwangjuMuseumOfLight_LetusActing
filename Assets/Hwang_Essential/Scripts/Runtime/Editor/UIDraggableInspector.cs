using UnityEditor;
using UnityEngine;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIDraggable), true)]
    public class UIDraggableInspector : Editor
    {
        private SerializedProperty clippingModeProperty;

        public virtual void OnEnable()
        {
            clippingModeProperty = serializedObject.FindProperty("clippingMode");
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
                if (string.Compare(property.name, "clippingArea") == 0 && clippingModeProperty != null)
                {
                    EditorGUI.BeginDisabledGroup(clippingModeProperty.enumValueIndex == (int)UIDraggable.DragClippingMode.DoNotClipping);
                    EditorGUILayout.PropertyField(property);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
