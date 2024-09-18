using System;
using System.Runtime.InteropServices;
using AOT;
using Rokid.UXR.Utility;
using Rokid.UXR.Module;
using UnityEngine;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        public partial class NativeAPI
        {
            /// <summary>
            /// Glasses IMU update
            /// Acc Gyt Gnt timeStamp
            /// </summary>
            public static event Action<float[], float[], float[], long> OnGlassIMUSensorUpdate;
            /// <summary>
            /// Glasses IMU rotation callback
            /// Input Value  GameRotation Rotation timeStamp
            /// </summary>
            public static event Action<float[], float[], long> OnGlassIMURotationUpdate;
            /// <summary>
            /// Register glasses IMU rotation
            /// </summary>
            public static void RegisterRotationEvent()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setOnRotationUpdate(OnRotationUpdateCallByC);//注册
            }

            /// <summary>
            /// Unregister glasses IMU rotation
            /// </summary>
            public static void UnregisterRotationEvent()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                clearOnRotationUpdate();
            }

            /// <summary>
            /// Open phone IMU tracking
            /// </summary>
            public static void OpenPhoneTracker()
            {
                if (Utils.IsAndroidPlatform())
                    openPhoneTracker();
            }

            /// <summary>
            /// Close phone IMU tracking 
            /// </summary>
            public static void ClosePhoneTracker()
            {
                if (Utils.IsAndroidPlatform())
                    closePhoneTracker();
            }

            /// <summary>
            /// Get phone IMU pose
            /// </summary>
            /// <param name="orientation"></param>
            public static void GetPhonePose(float[] orientation)
            {
                if (Utils.IsAndroidPlatform())
                    getPhonePose(orientation);
            }

            /// <summary>
            /// Open Ring IMU tracking
            /// </summary>
            public static void OpenRingTracker()
            {
                if (Utils.IsAndroidPlatform())
                {
                    ModuleManager.Instance.RegistModule("com.rokid.ringctrl.RingControl", false);
                    CallBridge.CallAndroid(Request.Build().Name("RingControl." + "start"));
                }
            }

            /// <summary>
            /// Close Ring IMU tracking 
            /// </summary>
            public static void CloseRingTracker()
            {
                if (Utils.IsAndroidPlatform())
                {
                    CallBridge.CallAndroid(Request.Build().Name("RingControl." + "stop"));
                }
            }

            /// <summary>
            /// Get Ring pose
            /// </summary>
            /// <param name="orientation"></param>
            public static void GetRingPose(float[] orientation)
            {
                if (Utils.IsAndroidPlatform())
                {
                    string q = CallBridge.CovertString(CallBridge.CallAndroid(Request.Build().Name("RingControl." + "getRingQuaternionJson")));
                    float[] ori = Newtonsoft.Json.JsonConvert.DeserializeObject<float[]>(q);
                    orientation[0] = ori[0];
                    orientation[1] = ori[1];
                    orientation[2] = ori[2];
                    orientation[3] = ori[3];
                }
            }

            /// <summary>
            /// Register Ring Button Events
            /// </summary>
            /// <param name="objName"></param>
            /// <param name="funcName"></param>
            public static void RegisterRingButtonEvents(string objName, string funcName)
            {
                CallBridge.CallAndroid(
                    Request.Build()
                    .Name("RingControl.setOnRingEventCallback")
                    .AndroidCallback(CallBridge.CreateCallback(objName, funcName))
                    );
            }

            /// <summary>
            /// Reset the y-axis of the phone's IMU
            /// </summary>
            public static void RecenterPhonePose()
            {
                if (Utils.IsAndroidPlatform())
                    recenterPhonePose();
            }

            /// <summary>
            /// Reset all the three axis of the phone's IMU
            /// </summary>
            public static void RecenterPhonePoseYPR()
            {
                if (Utils.IsAndroidPlatform())
                    recenterPhonePoseYPR();
            }

            /// <summary>
            /// Register glasses imu sensor events
            /// </summary>
            public static void RegisterGlassSensorEvent()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                setOnSensorUpdate(OnSensorUpdateCallByC);//注册
            }
            /// <summary>
            /// Unregister glasses imu sensor events
            /// </summary>
            public static void UnregisterGlassSensorEvent()
            {
                if (!Utils.IsAndroidPlatform())
                {
                    return;
                }
                clearOnSensorUpdate();//注销
            }

            #region NativeInterface
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void closePhoneTracker();
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void recenterPhonePose();
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void getPhonePose(float[] orientation);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void recenterPhonePoseYPR();
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void openPhoneTracker();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnSensorUpdate(OnSensorUpdate cb);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnSensorUpdate();
            delegate void OnSensorUpdate(IntPtr ptrAcc, IntPtr ptrGyt, IntPtr ptrGnt, long timestamp);
            static float[] resultAcc = new float[3];
            static float[] resultGyt = new float[3];
            static float[] resultGnt = new float[3];

            [MonoPInvokeCallback(typeof(OnSensorUpdate))]
            static void OnSensorUpdateCallByC(IntPtr ptrAcc, IntPtr ptrGyr, IntPtr ptrGnt, long timestamp)
            {
                RKLog.Debug("OnSensorUpdateCallByC " + timestamp);
                Marshal.Copy(ptrAcc, resultAcc, 0, 3);
                Marshal.Copy(ptrGyr, resultGyt, 0, 3);
                Marshal.Copy(ptrGnt, resultGnt, 0, 3);
                OnGlassIMUSensorUpdate?.Invoke(resultAcc, resultGyt, resultGnt, timestamp);
            }
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnRotationUpdate();

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnRotationUpdate(OnRotationUpdate cb);
            delegate void OnRotationUpdate(IntPtr ptrGameRotation, IntPtr ptrRotation, long timestamp);
            static float[] resultGameRotation = new float[4];
            static float[] resultRotation = new float[5];

            [MonoPInvokeCallback(typeof(OnRotationUpdate))]
            static void OnRotationUpdateCallByC(IntPtr ptrGameRotation, IntPtr ptrRotation, long timestamp)
            {
                RKLog.Debug("OnRotationUpdateCallByC: " + timestamp);
                Marshal.Copy(ptrGameRotation, resultGameRotation, 0, 4);
                Marshal.Copy(ptrRotation, resultRotation, 0, 5);
                OnGlassIMURotationUpdate?.Invoke(resultGameRotation, resultRotation, timestamp);
            }
            #endregion

        }
    }
}