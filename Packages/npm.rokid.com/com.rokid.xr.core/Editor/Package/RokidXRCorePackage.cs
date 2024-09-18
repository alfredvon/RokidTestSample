using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace Rokid.XR.Core.Editor
{
    /// <summary>
    /// Rokid XR for XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    public class RokidXRCorePackage : IXRPackage
    {
        /// <summary>
        /// Package metadata instance.
        /// </summary>
        public IXRPackageMetadata metadata => new PackageMetadata();

        /// <summary>
        /// Populates package settings instance.
        /// </summary>
        ///
        /// <param name="obj">
        /// Settings object.
        /// </param>
        /// <returns>Settings analysis result. Given that nothing is done, returns true.</returns>
        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return true;
        }

        private class LoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName => "Rokid XR Core";

            public string loaderType => typeof(RokidXRLoader).FullName;

            public List<BuildTargetGroup> supportedBuildTargets => new List<BuildTargetGroup>()
            {
                BuildTargetGroup.Android,BuildTargetGroup.Standalone
            };
        }

        private class PackageMetadata : IXRPackageMetadata
        {
            public string packageName => "Rokid XR Core";

            public string packageId => "com.rokid.xr.core";

            public string settingsType => typeof(RokidXRSettings).FullName;

            public List<IXRLoaderMetadata> loaderMetadata => new List<IXRLoaderMetadata>()
            {
                new LoaderMetadata()
            };
        }
    }
}

