using System;
using UnityEditor;

namespace UnityEngine
{
    [CustomEditor(typeof(WaypointGenerator), true)]
    public class WaypointGeneratorInspector : Editor
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

            // Draw "Script" property at first
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();

            WaypointGenerator generator = target as WaypointGenerator;
            int numPoints = generator.GetAllPoints().Length;

            // Draw other properties
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Number of points", string.Format("{0}", numPoints));

            if (GUILayout.Button("Add point"))
            {
                generator.AddPoint();
            }
            EditorGUI.BeginDisabledGroup(numPoints == 0);
            if (GUILayout.Button("Remove last point"))
            {
                generator.RemoveLastPoint();
            }
            if (GUILayout.Button("Remove all points"))
            {
                generator.RemoveAllPoints();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(numPoints == 0);
            if (GUILayout.Button("Save waypoint"))
            {
                generator.SaveWaypoint();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!generator.HasWaypoint);
            if (GUILayout.Button("Restore waypoint"))
            {
                generator.RestoreWaypoint();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
    }
}
