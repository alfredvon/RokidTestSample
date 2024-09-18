using UnityEngine;


namespace Rokid.XR.Core
{

    /// <summary>
    /// Stereo rendering mode.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Submit separate draw calls for each eye.
        /// </summary>
        MultiPass,

        /// <summary>
        /// Submit one draw call for both eyes.
        /// </summary>
        SinglePassInstance,
    };

    /// <summary>
    /// Rokid XR Settings for XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    [System.Serializable]
    public class RokidXRRuntimeSettings : ScriptableObject
    {
        public RenderMode m_RenderMode = RenderMode.MultiPass;
        private static RokidXRRuntimeSettings instance;
        public static RokidXRRuntimeSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<RokidXRRuntimeSettings>("RokidXRRuntimeSettings");
                }
                return instance;
            }
        }
    }
}
