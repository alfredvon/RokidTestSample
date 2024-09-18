using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Rokid.XR.Core.Editor
{
    /// <summary>
    /// Rokid XR Settings for XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    [System.Serializable]
    [XRConfigurationData("Rokid XR Core", Constants.SettingsKey)]
    public class RokidXRSettings : ScriptableObject
    {
#if UNITY_EDITOR
        private void OnEnable()
        {
            GetOrCreateInstance();
        }
#endif

        public static RokidXRRuntimeSettings GetOrCreateInstance()
        {
            if (RokidXRRuntimeSettings.Instance != null)
            {
                return RokidXRRuntimeSettings.Instance;
            }


            RokidXRRuntimeSettings instance = ScriptableObject.CreateInstance<RokidXRRuntimeSettings>();
            if (instance != null)
            {
                string newAssetName = String.Format(s_PackageSettingsAssetName);
                string assetPath = GetAssetPathForComponents(s_PackageSettingsDefaultSettingsPath);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPath = Path.Combine(assetPath, newAssetName);
                    AssetDatabase.CreateAsset(instance, assetPath);
                    AssetDatabase.Refresh();
                }
            }
            return instance;
        }

        internal static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach (var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), true) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }


        internal static readonly string s_PackageSettingsAssetName = "RokidXRRuntimeSettings.asset";

        internal static readonly string[] s_PackageSettingsDefaultSettingsPath = { "XR", "Resources" };
    }
}
