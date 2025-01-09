using UnityEditor;
using UnityEngine;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UITweener), true)]
    public class UITweenerInspector : Editor
    {
        private SerializedProperty fromValueProperty;
        private SerializedProperty toValueProperty;
        private SerializedProperty freezeXProperty;
        private SerializedProperty freezeYProperty;
        private SerializedProperty freezeZProperty;
        private SerializedProperty useRigidbodyProperty;

        private SerializedProperty loopTypeProperty;
        private SerializedProperty easeTypeProperty;
        private SerializedProperty animationCurveProperty;
        private SerializedProperty startDelayProperty;
        private SerializedProperty durationProperty;
        private SerializedProperty initOnPlayProperty;
        private SerializedProperty playOnEnableProperty;
        private SerializedProperty stopAfterLoopProperty;
        private SerializedProperty ignoreTimeScaleProperty;

        private SerializedProperty onFinishProperty;

        public virtual void OnEnable()
        {
            fromValueProperty = serializedObject.FindProperty("FromValue");
            toValueProperty = serializedObject.FindProperty("ToValue");
            freezeXProperty = serializedObject.FindProperty("FreezeX");
            freezeYProperty = serializedObject.FindProperty("FreezeY");
            freezeZProperty = serializedObject.FindProperty("FreezeZ");
            useRigidbodyProperty = serializedObject.FindProperty("UseRigidbody");

            loopTypeProperty = serializedObject.FindProperty("LoopType");
            easeTypeProperty = serializedObject.FindProperty("EaseType");
            animationCurveProperty = serializedObject.FindProperty("CustomAnimationCurve");
            startDelayProperty = serializedObject.FindProperty("Delay");
            durationProperty = serializedObject.FindProperty("Duration");
            initOnPlayProperty = serializedObject.FindProperty("InitOnPlay");
            playOnEnableProperty = serializedObject.FindProperty("PlayOnEnable");
            stopAfterLoopProperty = serializedObject.FindProperty("StopAfterLoop");
            ignoreTimeScaleProperty = serializedObject.FindProperty("IgnoreTimeScale");

            onFinishProperty = serializedObject.FindProperty("onFinish");
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();

            if (fromValueProperty != null)
            {
                EditorGUILayout.PropertyField(fromValueProperty, new GUIContent("From"));
            }
            if (toValueProperty != null)
            {
                EditorGUILayout.PropertyField(toValueProperty, new GUIContent("To"));
            }
            if (freezeXProperty != null || freezeYProperty != null || freezeZProperty != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Freeze");
                if (freezeXProperty != null)
                {
                    EditorGUI.showMixedValue = freezeXProperty.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    bool freeze = EditorGUILayout.ToggleLeft("X", freezeXProperty.boolValue, GUILayout.Width(40));
                    if (EditorGUI.EndChangeCheck())
                    {
                        freezeXProperty.boolValue = freeze;
                    }
                    EditorGUI.showMixedValue = false;
                }
                if (freezeYProperty != null)
                {
                    EditorGUI.showMixedValue = freezeYProperty.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    bool freeze = EditorGUILayout.ToggleLeft("Y", freezeYProperty.boolValue, GUILayout.Width(40));
                    if (EditorGUI.EndChangeCheck())
                    {
                        freezeYProperty.boolValue = freeze;
                    }
                    EditorGUI.showMixedValue = false;
                }
                if (freezeZProperty != null)
                {
                    EditorGUI.showMixedValue = freezeZProperty.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    bool freeze = EditorGUILayout.ToggleLeft("Z", freezeZProperty.boolValue, GUILayout.Width(40));
                    if (EditorGUI.EndChangeCheck())
                    {
                        freezeZProperty.boolValue = freeze;
                    }
                    EditorGUI.showMixedValue = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (useRigidbodyProperty != null)
            {
                EditorGUILayout.PropertyField(useRigidbodyProperty);
            }

            EditorGUILayout.PropertyField(loopTypeProperty);
            EditorGUILayout.PropertyField(easeTypeProperty);
            if (easeTypeProperty.intValue == (int)TweeningEaseType.Custom)
            {
                EditorGUILayout.PropertyField(animationCurveProperty, new GUIContent("Animation Curve"), GUILayout.MinHeight(50));
            }
            EditorGUILayout.PropertyField(startDelayProperty, new GUIContent("Start Delay"));
            EditorGUILayout.PropertyField(durationProperty);
            EditorGUILayout.PropertyField(initOnPlayProperty);
            EditorGUILayout.PropertyField(playOnEnableProperty);
            if (loopTypeProperty.intValue != (int)TweeningLoopType.Once)
            {
                EditorGUILayout.PropertyField(stopAfterLoopProperty);
            }
            EditorGUILayout.PropertyField(ignoreTimeScaleProperty);
            EditorGUILayout.PropertyField(onFinishProperty);

            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                bool isEnabled = false;
                foreach (UITweener tweener in targets)
                {
                    if (tweener != null && tweener.isActiveAndEnabled)
                    {
                        isEnabled = true;
                        break;
                    }
                }
                EditorGUI.BeginDisabledGroup(!isEnabled);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Play forward"))
                {
                    foreach (UITweener tweener in targets)
                    {
                        if (tweener != null && tweener.isActiveAndEnabled)
                        {
                            tweener.PlayForward();
                        }
                    }
                }
                if (GUILayout.Button("Play reverse"))
                {
                    foreach (UITweener tweener in targets)
                    {
                        if (tweener != null && tweener.isActiveAndEnabled)
                        {
                            tweener.PlayReverse();
                        }
                    }
                }
                if (GUILayout.Button("Stop"))
                {
                    foreach (UITweener tweener in targets)
                    {
                        if (tweener != null && tweener.isActiveAndEnabled)
                        {
                            tweener.Stop();
                        }
                    }
                }
                if (GUILayout.Button("Finish"))
                {
                    foreach (UITweener tweener in targets)
                    {
                        if (tweener != null && tweener.isActiveAndEnabled)
                        {
                            tweener.Finish();
                        }
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
