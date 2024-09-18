#if UNITY_EDITOR && UNITY_IOS

namespace Rokid.XR.Core.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>Processes the project files after the build is performed.</summary>
    public static class BuildPostProcessor
    {
        /// <summary>Unity callback to process after build.</summary>
        /// <param name="buildTarget">Target built.</param>
        /// <param name="path">Path to built project.</param>
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            
        }
    }
}

#endif
