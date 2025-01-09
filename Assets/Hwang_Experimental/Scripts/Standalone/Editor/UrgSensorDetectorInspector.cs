using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(UrgSensorDetector), true)]
public class UrgSensorDetectorInspector : Editor
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

        UrgSensorDetector sensorDetector = target as UrgSensorDetector;
        bool disabled = false;

        // Draw other properties
        while (property.NextVisible(false))
        {
            if (string.Compare(property.name, "DeviceType") == 0)
            {
                disabled = Application.isPlaying && sensorDetector.Connected;
            }
            else if (string.Compare(property.name, "PortName") == 0 || string.Compare(property.name, "BoudRate") == 0)
            {
                if (sensorDetector.DeviceType != UrgSensorDetector.UrgCommunicateType.Serial)
                {
                    continue;
                }
                disabled = Application.isPlaying && sensorDetector.Connected;
            }
            else if (string.Compare(property.name, "IPAddress") == 0 || string.Compare(property.name, "PortNumber") == 0)
            {
                if (sensorDetector.DeviceType != UrgSensorDetector.UrgCommunicateType.Ethernet)
                {
                    continue;
                }
                disabled = Application.isPlaying && sensorDetector.Connected;
            }
            else if (string.Compare(property.name, "radiusMin") == 0 || string.Compare(property.name, "radiusMax") == 0)
            {
                if (sensorDetector.CropMethod != UrgSensorDetector.AreaCroppingMethod.Radius)
                {
                    continue;
                }
            }
            else if (string.Compare(property.name, "rectSize") == 0 || string.Compare(property.name, "rectOffset") == 0)
            {
                if (sensorDetector.CropMethod != UrgSensorDetector.AreaCroppingMethod.Rect)
                {
                    continue;
                }
            }
            else if (string.Compare(property.name, "skipCount") == 0)
            {
                disabled = !sensorDetector.ContinuousMode;
            }
            if (disabled)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
            }
            EditorGUILayout.PropertyField(property, true);
            if (disabled)
            {
                disabled = false;
                EditorGUI.EndDisabledGroup();
            }
            else if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (string.Compare(property.name, "startDegree") == 0 || string.Compare(property.name, "endDegree") == 0 ||
                    string.Compare(property.name, "continuousMode") == 0 || string.Compare(property.name, "acquireStrength") == 0 ||
                    string.Compare(property.name, "groupSize") == 0 || string.Compare(property.name, "skipCount") == 0)
                {
                    sensorDetector.RestartAcquire();
                }
                else if (string.Compare(property.name, "rotateDegree") == 0 || string.Compare(property.name, "isFlipped") == 0)
                {
                    sensorDetector.RecalculateAll();
                }
            }
        }
    }
}
