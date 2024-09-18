using UnityEditor;
namespace Rokid.XR.Core.Editor
{
    [CustomEditor(typeof(RokidXRSettings))]
    class RokidXRSettingsEditor : UnityEditor.Editor
    {
        UnityEditor.Editor m_Editor;

        void OnEnable()
        {
            CreateCachedEditor(RokidXRRuntimeSettings.Instance, typeof(RokidXRRuntimeSettingsEditor), ref m_Editor);
        }

        public override void OnInspectorGUI()
        {
            if (m_Editor != null)
                m_Editor.OnInspectorGUI();
        }
    }
}
