using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

namespace Rokid.XR.Core.Editor
{
    [InitializeOnLoad]
    public class RokidXREnvFix : EditorWindow
    {
        #region  Config
        private const string ignorePrefix = "RokidXREnvFix";
        private static FixItem[] fixItems;
        private static RokidXREnvFix window;
        private Vector2 scrollPosition;
        private static string minUnityVersion = "2020.3.26";
        private static AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        private static AndroidArchitecture targetArchitecture = AndroidArchitecture.ARM64;

        #endregion

        static RokidXREnvFix()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            bool show = false;

            if (fixItems == null) { RegisterItems(); }
            foreach (var item in fixItems)
            {
                if (!item.IsIgnored() && !item.IsValid() && item.Level > MessageType.Warning && item.IsAutoPop())
                {
                    show = true;
                }
            }

            if (show)
            {
                ShowWindow();
            }

            EditorApplication.update -= OnUpdate;
        }

        private static void RegisterItems()
        {
            fixItems = new FixItem[]
            {
                new CheckBuildTarget(MessageType.Error),
                new CheckUnityMinVersion(MessageType.Error), 
            #if UNITY_2020_3_OR_NEWER 
	            new CheckCardboardPackage(MessageType.Error),//Should NOT add Cardboard Package into project
            #endif 
	            //new CheckMTRendering(MessageType.Warning),
                new CheckAndroidGraphicsAPI(MessageType.Error),
                new CheckAndroidOrientation(MessageType.Warning),
	            // new CheckColorSpace(MessageType.Warning),
	            new CheckOptimizedFramePacing(MessageType.Warning),
                new CheckMiniumAPILevel(MessageType.Error),
                new CheckArchitecture(MessageType.Error),
                new CheckXRPluginManagement(MessageType.Error)
            };
        }

        [MenuItem("UXR/Env/Project Environment Fix", false)]
        public static void ShowWindow()
        {
            RegisterItems();
            window = GetWindow<RokidXREnvFix>(true);
            window.minSize = new Vector2(320, 300);
            window.maxSize = new Vector2(320, 800);
            window.titleContent = new GUIContent("UXR SDK | Environment Fix");
        }

        //[MenuItem("UXR/Env/Delete Ignore", false)]
        public static void DeleteIgnore()
        {
            foreach (var item in fixItems)
            {
                item.DeleteIgnore();
            }
        }

        public void OnGUI()
        {
            string resourcePath = GetResourcePath();
            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "RokidIcon.png");
            Rect rect = GUILayoutUtility.GetRect(position.width, 80, GUI.skin.box);
            if (logo != null)
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            string aboutText = "This window provides tips to help fix common issues with UXR SDK and your project.";
            EditorGUILayout.LabelField(aboutText, EditorStyles.textArea);

            int ignoredCount = 0;
            int fixableCount = 0;
            int invalidNotIgnored = 0;

            for (int i = 0; i < fixItems.Length; i++)
            {
                FixItem item = fixItems[i];

                bool ignored = item.IsIgnored();
                bool valid = item.IsValid();
                bool fixable = item.IsFixable();

                if (!valid && !ignored && fixable)
                {
                    fixableCount++;
                }

                if (!valid && !ignored)
                {
                    invalidNotIgnored++;
                }

                if (ignored)
                {
                    ignoredCount++;
                }
            }

            Rect issuesRect = EditorGUILayout.GetControlRect();
            GUI.Box(new Rect(issuesRect.x - 4, issuesRect.y, issuesRect.width + 8, issuesRect.height), "Tips", EditorStyles.toolbarButton);

            if (invalidNotIgnored > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                {
                    for (int i = 0; i < fixItems.Length; i++)
                    {
                        FixItem item = fixItems[i];

                        if (!item.IsIgnored() && !item.IsValid())
                        {
                            invalidNotIgnored++;

                            GUILayout.BeginVertical("box");
                            {
                                item.DrawGUI();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    // Aligns buttons to the right
                                    GUILayout.FlexibleSpace();

                                    if (item.IsFixable())
                                    {
                                        if (GUILayout.Button("Fix"))
                                            item.Fix();
                                    }

                                    //if (GUILayout.Button("Ignore"))
                                    //    check.Ignore();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();

            if (invalidNotIgnored == 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("No issue found");

                        if (GUILayout.Button("Close Window"))
                            Close();
                    }
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal("box");
            {
                if (fixableCount > 0)
                {
                    if (GUILayout.Button("Accept All"))
                    {
                        if (EditorUtility.DisplayDialog("Accept All", "Are you sure?", "Yes, Accept All", "Cancel"))
                        {
                            for (int i = 0; i < fixItems.Length; i++)
                            {
                                FixItem item = fixItems[i];

                                if (!item.IsIgnored() &&
                                    !item.IsValid())
                                {
                                    if (item.IsFixable())
                                        item.Fix();
                                }
                            }
                        }
                    }
                }

            }
            GUILayout.EndHorizontal();
        }

        private string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path + "\\Textures\\";
        }

        private abstract class FixItem
        {
            protected string key;
            protected MessageType level;

            public MessageType Level
            {
                get
                {
                    return level;
                }
            }

            public FixItem(MessageType level)
            {
                this.level = level;
            }

            public void Ignore()
            {
                EditorPrefs.SetBool(ignorePrefix + key, true);
            }

            public bool IsIgnored()
            {
                return EditorPrefs.HasKey(ignorePrefix + key);
            }

            public void DeleteIgnore()
            {
                UnityEngine.Debug.Log("DeleteIgnore" + ignorePrefix + key);
                EditorPrefs.DeleteKey(ignorePrefix + key);
            }

            public abstract bool IsValid();

            public abstract bool IsAutoPop();

            public abstract void DrawGUI();

            public abstract bool IsFixable();

            public abstract void Fix();

            protected void DrawContent(string title, string msg)
            {
                EditorGUILayout.HelpBox(title, level);
                EditorGUILayout.LabelField(msg, EditorStyles.textArea);
            }
        }

        private class CheckCardboardPackage : FixItem
        {
            private enum PackageStates
            {
                None,
                WaitingForList,
                Failed,
                Added,
            }

            UnityEditor.PackageManager.Requests.ListRequest request;
            PackageStates packageState = PackageStates.None;
            bool isValid = true;
            bool initOnStart;
            bool hasLoader;

            public CheckCardboardPackage(MessageType level) : base(level)
            {
                key = this.GetType().Name;
                request = null;
                isValid = true;
#if UNITY_2020_3_OR_NEWER
                EditorApplication.update -= CheckPackageUpdate;
                EditorApplication.update += CheckPackageUpdate;
#endif
            }

#if UNITY_2020_3_OR_NEWER
            void CheckPackageUpdate()
            {
                switch (packageState)
                {
                    case PackageStates.None:
                        request = UnityEditor.PackageManager.Client.List(true, false);
                        packageState = PackageStates.WaitingForList;
                        break;

                    case PackageStates.WaitingForList:
                        if (request.IsCompleted)
                        {
                            if (request.Error != null || request.Status == UnityEditor.PackageManager.StatusCode.Failure)
                            {
                                packageState = PackageStates.Failed;
                                break;
                            }

                            UnityEditor.PackageManager.PackageCollection col = request.Result;
                            foreach (var info in col)
                            {
                                if (info.name == "com.google.xr.cardboard")
                                {
                                    packageState = PackageStates.Added;

                                    isValid = false;

                                    break;
                                }
                            }
                            if (packageState != PackageStates.Added) isValid = true;
                        }
                        break;

                    default:
                        break;
                }
            }

#endif

            public override bool IsValid()
            {
                return isValid;//!(isvalid && initOnStart && hasLoader);
            }

            public override void DrawGUI()
            {
                string message = @"You must remove Cardboard XR Plugin " + @"
	            In dropdown list of Window> Package Manager >Select Cardboard XR Plugin> Remove";
                DrawContent("Please Remove Cardboard XR Plugin", message);
            }

            public override bool IsFixable()
            {
                if (isValid)
                {
                    XRGeneralSettings sets = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

                    if (sets != null)
                    {
                        initOnStart = sets.InitManagerOnStart;

                        XRManagerSettings plugins = sets.AssignedSettings;
                        var loaders = sets.Manager.activeLoaders;

                        hasLoader = false;

                        for (int i = 0; i < loaders.Count;)
                        {
                            if (loaders[i].GetType() == typeof(Rokid.XR.Core.RokidXRLoader))
                                hasLoader = true;
                            break;
                        }
                        if (!hasLoader || !initOnStart) return true;
                    }
                }
                return false;
            }

            public override void Fix()
            {
                var sets = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                bool initOnStart = sets.InitManagerOnStart;

                if (!initOnStart) sets.InitManagerOnStart = true;

                XRManagerSettings plugins = sets.AssignedSettings;
                var loaders = sets.Manager.activeLoaders;
                bool hasLoader = false;

                for (int i = 0; i < loaders.Count;)
                {
                    if (loaders[i].GetType() == typeof(Rokid.XR.Core.RokidXRLoader))
                        hasLoader = true;
                    break;
                }

                if (!hasLoader)
                {
                    var xrLoader = new Rokid.XR.Core.RokidXRLoader();
                    plugins.TryAddLoader(xrLoader);
                }
            }

            public override bool IsAutoPop()
            {
                return false;
            }
        }

        private class CheckAndroidGraphicsAPI : FixItem
        {
            public CheckAndroidGraphicsAPI(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
                    {
                        return false;
                    }
                    var graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                    if (graphics != null && graphics.Length >= 1 &&
                        graphics[0] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public override void DrawGUI()
            {
                string message = @"Graphics APIs should be set to OpenGLES.  Player Settings > Other Settings > Graphics APIs , choose 'OpenGLES3'.";
                DrawContent("Graphics APIs is not OpenGLES", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[1] { GraphicsDeviceType.OpenGLES3 });
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CheckMTRendering : FixItem
        {
            public CheckMTRendering(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return !PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
                }
                else
                {
                    return false;
                }
            }

            public override void DrawGUI()
            {
                string message = @"In order to run correct on mobile devices, the RenderingThreadingMode should be set. 
	in dropdown list of Player Settings > Other Settings > Multithreaded Rendering, close toggle.";
                DrawContent("Multithreaded Rendering not close", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CheckAndroidOrientation : FixItem
        {
            public CheckAndroidOrientation(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait;
            }

            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the orientation should be set to portrait. 
	in dropdown list of Player Settings > Resolution and Presentation > Default Orientation, choose 'Portrait'.";
                DrawContent("Orientation is not portrait", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CheckColorSpace : FixItem
        {
            public CheckColorSpace(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return PlayerSettings.colorSpace == ColorSpace.Gamma;
            }

            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the colorSpace should be set to gamma. 
	in dropdown list of Player Settings > Other Settings > Color Space, choose 'Gamma'.";
                DrawContent("ColorSpace is not Linear", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.colorSpace = ColorSpace.Gamma;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CheckAndroidPermission : FixItem
        {
            public CheckAndroidPermission(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return PlayerSettings.Android.forceInternetPermission;
                }
                else
                {
                    return false;
                }
            }

            public override void DrawGUI()
            {
                string message = @"In order to run correct on mobile devices, the internet access premission should be set. 
	in dropdown list of Player Settings > Other Settings > Internet Access, choose 'Require'.";
                DrawContent("internet access permission not available", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.Android.forceInternetPermission = true;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        //todo...添加最低版本号的检查
        private class CheckUnityMinVersion : FixItem
        {
            string unityVersion;//

            public CheckUnityMinVersion(MessageType level) : base(level)
            {
                key = this.GetType().Name;
                unityVersion = Application.unityVersion;
            }

            public override void DrawGUI()
            {
                string message = @"The minimum Unity version required is 2020.3.36";
                DrawContent("Unity version not valid ", message);
            }

            public override void Fix()
            {

            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return unityVersion.CompareTo(minUnityVersion) == 1;
            }

            public override bool IsValid()
            {
                return unityVersion.CompareTo(minUnityVersion) == 1;
            }
        }

        private class CheckOptimizedFramePacing : FixItem
        {
            public CheckOptimizedFramePacing(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The optimizedFramePacing need to unselect";
                DrawContent("OptimizedFramePacing", message);
            }

            public override void Fix()
            {
                PlayerSettings.Android.optimizedFramePacing = false;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return PlayerSettings.Android.optimizedFramePacing == false;
            }
        }

        private class CheckMiniumAPILevel : FixItem
        {
            public CheckMiniumAPILevel(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The minSdkVersion needs to be " + minSdkVersion.ToString();
                DrawContent("MinSDKVersion", message);
            }

            public override void Fix()
            {
                PlayerSettings.Android.minSdkVersion = minSdkVersion;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return PlayerSettings.Android.minSdkVersion >= minSdkVersion;
            }
        }

        private class CheckArchitecture : FixItem
        {
            public CheckArchitecture(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The Target Architecture should be " + targetArchitecture;
                DrawContent("Target Architecture", message);
            }

            public override void Fix()
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = targetArchitecture;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP &&
                    PlayerSettings.Android.targetArchitectures == targetArchitecture;
            }
        }


        private class CheckBuildTarget : FixItem
        {
            public CheckBuildTarget(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The Build Target should be Android";
                DrawContent("Build Target", message);
            }

            public override void Fix()
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            }
        }

        private class CheckXRPluginManagement : FixItem
        {
            public CheckXRPluginManagement(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"Rokid XR Core should be activated in the XR Plug-in Management in dropdown list of Player Settings > Project Settings > XR Plug-in Management > Rokid XR Core";
                DrawContent("XR Plug-in Management", message);
            }

            public override void Fix()
            {
                try
                {
                    var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                    var pluginsSettings = buildTargetSettings.AssignedSettings;
                    for (int i = 0; i < pluginsSettings.activeLoaders.Count; i++)
                    {
                        var v = pluginsSettings.activeLoaders[i];
                        UnityEngine.Debug.Log(v.GetType().ToString());
                        XRPackageMetadataStore.RemoveLoader(pluginsSettings, v.GetType().ToString(), BuildTargetGroup.Android);
                    }
                    var didAssign = XRPackageMetadataStore.AssignLoader(pluginsSettings, "Rokid.XR.Core.XRLoader", BuildTargetGroup.Android);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return false;
            }

            public override bool IsValid()
            {
                try
                {
                    var buildTargetSettings =
                        XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                    var pluginsSettings = buildTargetSettings.AssignedSettings;
                    if (pluginsSettings.activeLoaders.Count > 1 || pluginsSettings.activeLoaders.Count == 0)
                    {
                        return false;
                    }

                    foreach (var v in pluginsSettings.activeLoaders)
                    {
                        if (v is Rokid.XR.Core.RokidXRLoader)
                            return true;
                    }

                    return false;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                    return false;
                }
            }
        }
    }
}
