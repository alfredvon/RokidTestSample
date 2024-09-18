using System;
using System.Runtime.InteropServices;
using AOT;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        public partial class NativeAPI
        {
            /// <summary>
            /// usb connection callback successful
            /// </summary>
            public static event Action OnUSBConnect;
            /// <summary>
            /// usb disconnect callback
            /// </summary>
            public static event Action OnUSBDisConnect;

            /// <summary>
            /// On glass bright update callback
            /// </summary>
            public static event Action<int> OnGlassBrightUpdate;
            // <summary>
            /// On glass Volume update callback
            /// </summary>
            public static event Action<float> OnGlassVolumeUpdate;

            // <summary>
            /// On glass PSensor update callback
            /// </summary>
            public static event Action<bool> OnGlassPSensorUpdate;

            // <summary>
            /// On glass Key update callback
            /// </summary>
            public static event Action<int> OnGlassKeyUpdate;

            private static AndroidJavaClass versionInfo = new AndroidJavaClass("android.os.Build$VERSION");

            private static string UXR_SERVICE_USBDEVICE = "UXRUSBDevice.";
            private const string MinSystemBuildVersion = "20240410"; //RG-stationPro sys version check

            /// <summary>
            /// Get glasses name
            /// </summary>
            /// <returns></returns>
            public static string GetGlassName()
            {
#if UNITY_EDITOR
                return "PC";
#else
                IntPtr namePtr = getGlassName();
                string name = Marshal.PtrToStringAnsi(namePtr);
                return name;
#endif
            }

            /// <summary>
            /// Get the glasspid of the glasses
            /// </summary>
            /// <returns></returns>
            public static int GetGlassPID()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return 0;
                }
                return getGlassProductId();
            }

            /// <summary>
            /// Get glasses sn
            /// </summary>
            /// <returns></returns>
            public static string GetGlassSN()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return null;
                }
                IntPtr snPtr = getGlassSn();
                string sn = Marshal.PtrToStringAnsi(snPtr);
                return sn;
            }

            /// <summary>
            /// Get the glasses type ids
            /// </summary>
            /// <returns></returns>
            public static string GetGlassTypeId()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return null;
                }
                IntPtr typeIdPtr = getGlassTypeId();
                string typeId = Marshal.PtrToStringAnsi(typeIdPtr);
                return typeId;
            }

            /// <summary>
            /// Get the glasses firmware version
            /// </summary>
            /// <returns></returns>
            public static string GetGlassFirmwareVersion()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return null;
                }
                IntPtr versionPtr = getGlassFirmwareVersion();
                string version = Marshal.PtrToStringAnsi(versionPtr);
                return version;
            }

            /// <summary>
            /// Get glasses brightness
            /// </summary>
            /// <returns></returns>
            public static int GetGlassBrightness()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return 0;
                }
                return getGlassBrightness();
            }

            /// <summary>
            /// Set the brightness range of the glasses to 1-100
            /// </summary>
            /// <param name="value"></param>
            public static void SetGlassBrightness(int value)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setGlassBrightness(value);
            }
            /// <summary>
            /// Get glasses volume
            /// </summary>
            /// <returns></returns>
            public static float GetGlassVolume()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return 0;
                }
                return getGlassVolume();
            }

            /// <summary>
            /// Set the volume range of the glasses to 0-1
            /// </summary>
            /// <param name="value"></param>
            public static void SetGlassVolume(float value)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setGlassVolume(value);
            }

            /// <summary>
            /// Get Build INCREMENTAL
            /// </summary>
            /// <value></value>
            public static string INCREMENTAL
            {
                get
                {
                    if (Utils.IsAndroidPlatform())
                    {
                        return versionInfo.GetStatic<string>("INCREMENTAL");
                    }
                    return "";
                }
            }

            /// <summary>
            /// Station Pro System Check
            /// </summary>
            /// <returns></returns>
            public static bool SystemEnvCheck()
            {
                if (SystemInfo.deviceModel.Equals("Rokid RG-stationPro"))
                {
                    return NativeInterface.NativeAPI.INCREMENTAL.Split('-')[1].CompareTo(MinSystemBuildVersion) >= 0;
                }
                return true;
            }


            /// <summary>
            /// Set the glasses status callback
            /// </summary>
            [Obsolete("Use RegisterUSBStatusCallback instead")]
            public static void setUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setOnUsbStatusUpdate(OnUsbStatusUpdateCallByC);//注册
            }

            /// <summary>
            /// Register usb status event
            /// </summary>
            public static void RegisterUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setOnUsbStatusUpdate(OnUsbStatusUpdateCallByC);//注册
            }

            /// <summary>
            /// Unregister usb status event
            /// </summary>
            public static void UnRegisterUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                clearOnUsbStatusUpdate();//注销
            }

            /// <summary>
            /// Pause app
            /// </summary>
            public static void PauseApp()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                CallBridge.CallAndroid(Request.Build()
                               .Name(UXR_SERVICE_USBDEVICE + "pauseApp"));
            }

            delegate void OnUsbStatusUpdate(bool isConnect);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnUsbStatusUpdate(OnUsbStatusUpdate cb);


            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnUsbStatusUpdate();

            [MonoPInvokeCallback(typeof(OnUsbStatusUpdate))]
            static void OnUsbStatusUpdateCallByC(bool isConnect)
            {
                if (isConnect)
                {
                    OnUSBConnect?.Invoke();
                }
                else
                {
                    OnUSBDisConnect?.Invoke();
                }
            }

            /// <summary>
            /// Kill the uxr process
            /// </summary>
            /// <returns></returns>
            public static bool KillProcess()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return false;
                }
                AndroidJavaObject obj = CallBridge.CallAndroid(Request.Build()
                .Name(UXR_SERVICE_USBDEVICE + "killProcess"));
                return CallBridge.CovertBool(obj);
            }


            /// <summary>
            /// Set Glass 2DMode
            /// </summary>
            /// <returns></returns>
            public static void SetGlass2DMode()
            {
                if (Utils.IsAndroidPlatform())
                {
                    CallBridge.CallAndroid(Request.Build()
                    .Name(UXR_SERVICE_USBDEVICE + "setGlass2DMode"));
                }
            }

            /// <summary>
            /// Set Glass mode
            /// </summary>
            /// <param name="mode"></param>
            public static void SetGlassKeyMeshMode(int mode)
            {
                if (Utils.IsAndroidPlatform())
                    setGlassKeyMeshMode(mode);
            }

            /// <summary>
            /// Finish the uxr activity
            /// </summary>
            /// <returns></returns>
            public static bool FinishActivity()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return false;
                }
                AndroidJavaObject obj = CallBridge.CallAndroid(Request.Build()
                .Name(UXR_SERVICE_USBDEVICE + "activityFinish"));
                return CallBridge.CovertBool(obj);
            }

            /// <summary>
            /// Whether the usb is successfully connected
            /// </summary>
            /// <returns></returns>
            public static bool IsUSBConnect()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return false;
                }
                return isUsbConnect();
            }

            /// <summary>
            /// Register glass bright events
            /// </summary>
            [Obsolete("Use RegisterGlassBrightUpdate(Action<int> glassBrightUpdate) instead")]
            public static void RegisterGlassBrightUpdate()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setOnGlassBrightUpdate(OnGlassBrightUpdateCallByC);
            }


            /// <summary>
            /// Register glass bright events
            /// </summary>
            public static void RegisterGlassBrightUpdate(Action<int> glassBrightUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassBrightUpdate += glassBrightUpdate;
                setOnGlassBrightUpdate(OnGlassBrightUpdateCallByC);
            }

            /// <summary>
            /// UnRegister glass bright events
            /// </summary>
            [Obsolete("Use UnregisterOnGlassBrightUpdate(Action<int> glassBrightUpdate) instead")]
            public static void UnregisterOnGlassBrightUpdate()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                clearOnGlassBrightUpdate();//注销
            }


            /// <summary>
            /// UnRegister glass bright events
            /// </summary>
            public static void UnregisterOnGlassBrightUpdate(Action<int> glassBrightUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassBrightUpdate -= glassBrightUpdate;
                clearOnGlassBrightUpdate();//注销
            }

            /// <summary>
            /// Register glasses Volume events
            /// </summary>
            public static void RegisterGlassVolumeUpdate(Action<float> glassVolumeUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassVolumeUpdate += glassVolumeUpdate;
                setOnGlassVolumeUpdate(OnGlassVolumeUpdateCallByC);
            }

            /// <summary>
            /// UnRegister glass Volume events
            /// </summary>
            public static void UnregisterOnGlassVolumeUpdate(Action<float> glassVolumeUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassVolumeUpdate -= glassVolumeUpdate;
                clearOnGlassVolumeUpdate();//注销
            }

            /// <summary>
            /// Register glass Volume events
            /// </summary>
            public static void RegisterGlassPSensorUpdate(Action<bool> glassPSensorUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassPSensorUpdate += glassPSensorUpdate;
                setOnGlassPSensorUpdate(OnGlassPSensorUpdateCallByC);
            }

            /// <summary>
            /// UnRegister glass Volume events
            /// </summary>
            public static void UnregisterGlassPSensorUpdate(Action<bool> glassPSensorUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassPSensorUpdate -= glassPSensorUpdate;
                clearOnGlassVolumeUpdate();//注销
            }


            /// <summary>
            /// Register glass Volume events
            /// </summary>
            public static void RegisterGlassKeyUpdate(Action<int> glassKeyUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassKeyUpdate += glassKeyUpdate;
                setOnGlassKeyUpdate(OnGlassKeyUpdateCallByC);
            }

            /// <summary>
            /// UnRegister glass Volume events
            /// </summary>
            public static void UnregisterGlassKeyUpdate(Action<int> glassKeyUpdate)
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                OnGlassKeyUpdate -= glassKeyUpdate;
                clearOnGlassKeyUpdate();//注销
            }


            /// <summary>
            /// Get persist value
            /// </summary>
            public static string GetPersistValue(string key)
            {
                if (Utils.IsAndroidPlatform())
                {
                    IntPtr value = getPropertiesValue(key);
                    return Marshal.PtrToStringAnsi(value);
                }
                return null;
            }

            /// <summary>
            /// Set persist value
            /// </summary>
            public static void SetPersistValue(string key, string value)
            {
                if (Utils.IsAndroidPlatform())
                    setPropertiesValue(key, value);
            }

            /// <summary>
            /// Vibrator 1-Tick,2-Click
            /// </summary>
            public static void Vibrate(int effectId)
            {
                if (Utils.IsAndroidPlatform())
                    vibrate(effectId);
            }

            /// <summary>
            /// Set IPD
            /// </summary>
            /// <param name="value">min 53 max 75</param>
            public static void SetIPD(int value)
            {
                if (Utils.IsAndroidPlatform())
                {
                    int val = Mathf.Clamp(value, 53, 75);
                    set_ipd(val);
                }
            }

            /// <summary>
            /// GetIPD
            /// </summary>
            /// <returns> min 53 max 75</returns>
            public static int GetIPD()
            {
                if (Utils.IsAndroidPlatform())
                {
                    return get_ipd();
                }
                return 64;
            }

            /// <summary>
            /// Notify 3Dof Tracker Recenter
            /// </summary>
            public static void Notify3DofTrackerRecenter()
            {
                if (Utils.IsAndroidPlatform())
                {
                    notifyHandTrackerRecenter();
                }
            }

            #region  NativeAPI
            delegate void OnGlassBrightUpdateC(int brightness);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnGlassBrightUpdate(OnGlassBrightUpdateC cb);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnGlassBrightUpdate();
            [MonoPInvokeCallback(typeof(OnGlassBrightUpdateC))]
            static void OnGlassBrightUpdateCallByC(int brightness)
            {
                RKLog.KeyInfo("Bright Update Call By C !!!" + brightness);
                OnGlassBrightUpdate?.Invoke(brightness);
            }

            delegate void OnGlassVolumeUpdateC(float volume);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnGlassVolumeUpdate(OnGlassVolumeUpdateC cb);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnGlassVolumeUpdate();
            [MonoPInvokeCallback(typeof(OnGlassVolumeUpdateC))]
            static void OnGlassVolumeUpdateCallByC(float volume)
            {
                RKLog.KeyInfo("Volume Update Call By C !!!" + volume);
                OnGlassVolumeUpdate?.Invoke(volume);
            }


            [DllImport(ApiConstants.UXR_GFX_PLUGIN, CharSet = CharSet.Ansi)]
            static extern void setPropertiesValue(string key, string value);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN, CharSet = CharSet.Ansi)]
            static extern IntPtr getPropertiesValue(string key);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void vibrate(int effectId);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void set_ipd(int ipd);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern int get_ipd();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnGlassPSensorUpdate(OnGlassPSensorUpdateC cb);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnGlassPSensorUpdate();
            delegate void OnGlassPSensorUpdateC(bool flag);

            [MonoPInvokeCallback(typeof(OnGlassPSensorUpdateC))]
            static void OnGlassPSensorUpdateCallByC(bool flag)
            {
                RKLog.KeyInfo("Volume Update Call By C !!!" + flag);
                OnGlassPSensorUpdate?.Invoke(flag);
            }

            delegate void OnGlassKeyUpdateC(int value);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnGlassKeyUpdate(OnGlassKeyUpdateC cb);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnGlassKeyUpdate();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setGlassKeyMeshMode(int mode);

            [MonoPInvokeCallback(typeof(OnGlassKeyUpdateC))]
            static void OnGlassKeyUpdateCallByC(int value)
            {
                RKLog.KeyInfo("GlassKey Update Call By C !!!" + value);
                OnGlassKeyUpdate?.Invoke(value);
            }

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void notifyHandTrackerRecenter();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern IntPtr getGlassSn();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern IntPtr getGlassTypeId();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern IntPtr getGlassFirmwareVersion();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern int getGlassBrightness();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setGlassBrightness(int value);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern float getGlassVolume();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setGlassVolume(float value);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern bool isUsbConnect();
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            public static extern int getGlassProductId();
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            public static extern IntPtr getGlassName();
            #endregion
        }
    }
}


