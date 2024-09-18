using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Management;


namespace Rokid.XR.Core
{
    public enum StereoRenderingMode
    {
        MULTI_PASS,
        SINGLE_PASS
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct UserDefinedSettings
    {
        public StereoRenderingMode stereoRenderingMode;
    }
    /// <summary>
    /// Rokid XR Loader for UXR Plugin.
    /// Loads Display and Input Subsystems.
    /// </summary>
    public class RokidXRLoader : XRLoaderHelper
    {
        private static List<XRDisplaySubsystemDescriptor> _displaySubsystemDescriptors =
            new List<XRDisplaySubsystemDescriptor>();

        private static List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors =
            new List<XRInputSubsystemDescriptor>();

        /// <summary>
        /// Pairs the native enum to set the graphics API being used.
        /// </summary>
        private enum UXRGraphicsApi
        {
            kOpenGlEs2 = 1,
            kOpenGlEs3 = 2,
            kVulkan = 4,
            kNone = -1,
        }

        /// <summary>
        /// Describes the possible orientation of the viewport.
        /// </summary>
        private enum UXRViewportOrientation
        {
            kLandscapeLeft = 0,
            kLandscapeRight = 1,
            kPortrait = 2,
            kPortraitUpsideDown = 3,
        }

        /// <summary>
        /// Gets a value indicating whether the subsystems are initialized or not.
        /// </summary>
        ///
        /// <returns>
        /// True after a successful call to Initialize() without a posterior call to
        /// Deinitialize().
        /// </returns>
        //internal static bool _isInitialized { get; private set; }
        public static bool _isInitialized { get; set; }

        /// <summary>
        /// Gets a value indicating whether the subsystems are started or not.
        /// </summary>
        ///
        /// <returns>
        /// True after a successful call to Start() without a posterior call to Stop().
        /// </returns>
        internal static bool _isStarted { get; private set; }

        /// <summary>
        /// Initialize the loader. This should initialize all subsystems to support the desired
        /// runtime setup this loader represents.
        /// </summary>
        ///
        /// <returns>Whether or not initialization succeeded.</returns>
        public override bool Initialize()
        {
            InitSettings();
#if UNITY_ANDROID && !UNITY_EDITOR
            UXRSDKInitialize();
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(
               _displaySubsystemDescriptors, "CardboardDisplay");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(
                _inputSubsystemDescriptors, "CardboardInput");
#endif
            // add for XRI 
            OnXRILoaderInitialize?.Invoke(this);
            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// Ask loader to start all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully started.</returns>
        public override bool Start()
        {
            StartSubsystem<XRDisplaySubsystem>();
            StartSubsystem<XRInputSubsystem>();
            // add for XRI      
            OnXRILoaderStart?.Invoke(this);
            _isStarted = true;
            return true;
        }

        /// <summary>
        /// Ask loader to stop all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not all subsystems were successfully stopped.</returns>
        public override bool Stop()
        {
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
            // add for XRI      
            OnXRILoaderStop?.Invoke(this);
            _isStarted = false;
            return true;
        }

        /// <summary>
        /// Ask loader to deinitialize all initialized subsystems.
        /// </summary>
        ///
        /// <returns>Whether or not deinitialization succeeded.</returns>
        public override bool Deinitialize()
        {
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();
            // add for XRI
            OnXRILoaderDeinitialize?.Invoke(this);
            UXRSDKDeinitialize();
            _isInitialized = false;
            return true;
        }

        /// <summary>
        /// Sets the screen parameters in the XR scene.
        /// </summary>
        ///
        /// <param name="renderingArea">
        /// The rectangle where the XR scene will be rendered.
        /// </param>
        public static void RecalculateRectangles(Rect renderingArea)
        {

            setScreenParams((int)renderingArea.width, (int)renderingArea.height);

            UnityEngine.Debug.Log("-uxr- ScreenParam h:" + Screen.height + ",renderingArea: " + renderingArea.ToString());
            UnityEngine.Debug.Log("-uxr-  displays len:" + Display.displays.Length + ",glass systemHeight: " + Display.displays[Display.displays.Length - 1].systemHeight +
               ",renderingHeight: " + Display.displays[Display.displays.Length - 1].renderingHeight);

        }

        /// <summary>
        /// Sets which viewport orientation is being used by Unity to the native implementation.
        /// </summary>
        ///
        /// <param name="screenOrientation">
        /// The required screen orientation.
        /// </param>
        public static void SetViewportOrientation(ScreenOrientation screenOrientation)//TODO
        {
            /*switch (screenOrientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    UXRUnity_setViewportOrientation(
                        UXRViewportOrientation.kLandscapeLeft);
                    break;
                case ScreenOrientation.LandscapeRight:
                    UXRUnity_setViewportOrientation(
                        UXRViewportOrientation.kLandscapeRight);
                    break;
                case ScreenOrientation.Portrait:
                    UXRUnity_setViewportOrientation(UXRViewportOrientation.kLandscapeLeft);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    UXRUnity_setViewportOrientation(
                        UXRViewportOrientation.kPortraitUpsideDown);
                    break;
                default:
                    Debug.LogWarning(
                      "The UXR Plugin does not support the selected screen orientation." +
                      "Setting landscape left as default.");
                    UXRUnity_setViewportOrientation(
                        UXRViewportOrientation.kLandscapeLeft);
                    break;
            }*/
        }

        /// <summary>
        /// Sets which Graphics API is being used by Unity to the native implementation.
        /// </summary>
        private static void SetGraphicsApi()
        {
            switch (SystemInfo.graphicsDeviceType)
            {
#if !UNITY_2023_1_OR_NEWER
                case GraphicsDeviceType.OpenGLES2:
                    UXRUnity_setGraphicsApi(UXRGraphicsApi.kOpenGlEs2);
                    break;
#endif
                case GraphicsDeviceType.OpenGLES3:
                    UXRUnity_setGraphicsApi(UXRGraphicsApi.kOpenGlEs3);
                    break;
#if UNITY_ANDROID && !UNITY_EDITOR
                case GraphicsDeviceType.Vulkan:
                    UXRUnity_setGraphicsApi(UXRGraphicsApi.kVulkan);
                    break;
#endif
                default:
                    UnityEngine.Debug.LogErrorFormat(
                     "The UXR Plugin cannot be initialized given that the selected " +
                     "Graphics API ({0}) is not supported. Please use OpenGL ES, " +
                     "according to the target platform.", SystemInfo.graphicsDeviceType);
                    break;
            }
        }


        /// <summary>
        /// For Android, initializes JavaVM and Android activity context.
        /// Then,it sets the screen size in pixels.
        /// </summary>
        private void UXRSDKInitialize()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            // Initializes SDK.
#if UNITY_ANDROID && !UNITY_EDITOR
            UXRUnity_initializeAndroid(activity.GetRawObject());
#endif
            SetViewportOrientation(Screen.orientation);
            // Safe area is required to avoid rendering behind the notch. If the device does not
            // have any notch, it will be equivalent to the full screen area.
            RecalculateRectangles(Screen.safeArea);
        }


        /// <summary>
        ///  the XR provider is deinitialized.
        /// </summary>
        private void UXRSDKDeinitialize()
        {

        }

        #region ForXRI
        public static Action<RokidXRLoader> OnXRILoaderInitialize;
        public static Action<RokidXRLoader> OnXRILoaderStart;
        public static Action<RokidXRLoader> OnXRILoaderStop;
        public static Action<RokidXRLoader> OnXRILoaderDeinitialize;

        public void CreateCustomSubsystem<TDescriptor, TSubsystem>(List<TDescriptor> descriptors, string id)
            where TDescriptor : ISubsystemDescriptor
            where TSubsystem : ISubsystem
        {
            CreateSubsystem<TDescriptor, TSubsystem>(descriptors, id);
        }

        public void StartCustomSubsystem<T>() where T : class, ISubsystem
        {
            StartSubsystem<T>();
        }

        public void StopCustomSubsystem<T>() where T : class, ISubsystem
        {
            StopSubsystem<T>();
        }

        public void DestroyCustomSubsystem<T>() where T : class, ISubsystem
        {
            DestroySubsystem<T>();
        }
        #endregion


        #region  InitSettings
        private void InitSettings()
        {
            RokidXRRuntimeSettings settings = RokidXRRuntimeSettings.Instance;
            if (settings != null)
            {
                UserDefinedSettings userDefinedSettings = new UserDefinedSettings()
                {
                    stereoRenderingMode = (StereoRenderingMode)settings.m_RenderMode
                };
                Debug.Log("RokidXRLoader Set Render Mode:" + userDefinedSettings.stereoRenderingMode);
#if UNITY_ANDROID && !UNITY_EDITOR
                setUserDefinedSettings(ref userDefinedSettings);
#endif
            }
            else
            {
                Debug.LogError("rokid xr loader can not find package settings");
            }
        }
        #endregion

        #region NativeAPI

        [DllImport(Constants.UXR_GFX_PLUGIN)]
        private static extern void setScreenParams(int viewport_width, int viewport_height);

        [DllImport(Constants.UXR_GFX_PLUGIN)]
        private static extern void UXRUnity_setGraphicsApi(UXRGraphicsApi graphics_api);

        [DllImport(Constants.UXR_GFX_PLUGIN)]
        private static extern void UXRUnity_setViewportOrientation(
            UXRViewportOrientation viewport_orientation);

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(Constants.UXR_GFX_PLUGIN)]
        private static extern void UXRUnity_initializeAndroid(IntPtr context);
#endif

        [DllImport(Constants.UXR_GFX_PLUGIN)]
        private static extern void setUserDefinedSettings(
            ref UserDefinedSettings settings);
        #endregion

    }
}
