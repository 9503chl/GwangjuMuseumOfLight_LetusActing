using UnityEngine;
using UnityEditor;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(View), false)]
    public class ViewInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            base.OnInspectorGUI();

            // Add a button for custom view
            if (!Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create custom view script"))
                {
                    ViewMenuItem.CreateCustomViewScript("CustomView");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }
}
