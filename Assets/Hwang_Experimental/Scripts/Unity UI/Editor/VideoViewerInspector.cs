using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEngine.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VideoViewer), true)]
    public class VideoViewerInspector : Editor
    {
        private SerializedProperty sourceVideoPlayerProperty;
        private SerializedProperty playOnEnableProperty;
        private SerializedProperty showBannerOnStopProperty;
        private SerializedProperty resizeToFitVideoProperty;
        private SerializedProperty overrideVideoSizeProperty;
        private SerializedProperty videoWidthProperty;
        private SerializedProperty videoHeightProperty;
        private SerializedProperty renderAlphaChannelProperty;

        public virtual void OnEnable()
        {
            sourceVideoPlayerProperty = serializedObject.FindProperty("SourceVideoPlayer");
            playOnEnableProperty = serializedObject.FindProperty("PlayOnEnable");
            showBannerOnStopProperty = serializedObject.FindProperty("ShowBannerOnStop");
            resizeToFitVideoProperty = serializedObject.FindProperty("ResizeToFitVideo");
            overrideVideoSizeProperty = serializedObject.FindProperty("OverrideVideoSize");
            videoWidthProperty = serializedObject.FindProperty("VideoWidth");
            videoHeightProperty = serializedObject.FindProperty("VideoHeight");
            renderAlphaChannelProperty = serializedObject.FindProperty("RenderAlphaChannel");
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector when "Script" is missing or null
            if (target == null)
            {
                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();

            // Draw only known properties
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sourceVideoPlayerProperty);
            if (EditorGUI.EndChangeCheck())
            {
                VideoViewer videoViewer = target as VideoViewer;
                if (sourceVideoPlayerProperty.objectReferenceValue != null)
                {
                    VideoPlayer videoPlayer = sourceVideoPlayerProperty.objectReferenceValue as VideoPlayer;
                    videoViewer.GetComponent<RawImage>().texture = videoPlayer.targetTexture;
                }
            }
            EditorGUILayout.PropertyField(playOnEnableProperty);
            EditorGUILayout.PropertyField(showBannerOnStopProperty);
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            EditorGUILayout.PropertyField(resizeToFitVideoProperty);
            EditorGUILayout.PropertyField(overrideVideoSizeProperty);
            if (overrideVideoSizeProperty.boolValue)
            {
                EditorGUILayout.PropertyField(videoWidthProperty);
                EditorGUILayout.PropertyField(videoHeightProperty);
            }
            EditorGUILayout.PropertyField(renderAlphaChannelProperty);
            EditorGUI.EndDisabledGroup();

            // Apply modified properties in inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
