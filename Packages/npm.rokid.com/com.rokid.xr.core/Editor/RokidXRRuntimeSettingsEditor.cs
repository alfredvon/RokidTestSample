using UnityEngine;
using UnityEditor;

namespace Rokid.XR.Core.Editor
{
    [CustomEditor(typeof(RokidXRRuntimeSettings))]
    class RokidXRRuntimeSettingsEditor : UnityEditor.Editor
    {
        private const string StereoRenderingModeAndroid = "m_RenderMode";
        static GUIContent guiStereoRenderingMode = EditorGUIUtility.TrTextContent("Stereo Rendering Mode");
        private SerializedProperty stereoRenderingModeAndroid;

        void OnEnable()
        {
            if (serializedObject == null || serializedObject?.targetObject == null)
                return;
            if (stereoRenderingModeAndroid == null)
                stereoRenderingModeAndroid = serializedObject.FindProperty(StereoRenderingModeAndroid);
        }
        public override void OnInspectorGUI()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return;

            serializedObject.Update();
            EditorGUIUtility.labelWidth = 200.0f;
            BuildTargetGroup selectedBuildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Rokid settings cannot be changed when the editor is in play mode.", MessageType.Info);
                EditorGUILayout.Space();
            }
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            if (selectedBuildTargetGroup == BuildTargetGroup.Android)
            {
                EditorGUILayout.PropertyField(stereoRenderingModeAndroid, guiStereoRenderingMode);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndBuildTargetSelectionGrouping();

            serializedObject.ApplyModifiedProperties();
            EditorGUIUtility.labelWidth = 0f;
        }
    }
}
