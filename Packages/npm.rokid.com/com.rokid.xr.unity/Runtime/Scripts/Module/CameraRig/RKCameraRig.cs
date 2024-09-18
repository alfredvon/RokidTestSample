using System;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;

namespace Rokid.UXR.Module
{
    public class RKCameraRig : MonoBehaviour
    {
        /// <summary>
        /// This enum is used to indicate which parts of the pose will be applied to the parent transform
        /// </summary>
        public enum HeadTrackingType
        {
            /// <summary>
            /// With this setting, both the pose's rotation and position will be applied to the parent transform
            /// </summary>
            RotationAndPosition = 0,
            /// <summary>
            /// With this setting, only the pose's rotation will be applied to the parent transform
            /// </summary>
            RotationOnly = 1,
            /// <summary>
            /// With this setting, only the pose's position will be applied to the parent transform
            /// </summary>
            PositionOnly = 2,
            /// <summary>
            /// With this setting, none value will be applied to the parent transform
            /// </summary>
            None = 3
        }
        bool m_RotationExcludeRoll = false;

        bool m_PreRotationExcludeRoll = false;

        [SerializeField]
        HeadTrackingType m_HeadTrackingType;

        HeadTrackingType m_PreHeadTrackingType;

        /// <summary>
        /// is exclude roll
        /// </summary>
        public bool rotationExcludeRoll
        {
            get { return m_RotationExcludeRoll; }
            set { m_RotationExcludeRoll = value; NativeInterface.NativeAPI.SetPersistValue("rokid.camera.lock.roll", value.ToString()); }
        }

        /// <summary>
        /// The tracking type being used by the tracked pose driver
        /// </summary>
        public HeadTrackingType headTrackingType
        {
            get { return m_HeadTrackingType; }
            set { m_HeadTrackingType = value; }
        }

        /// <summary>
        /// The update type being used by the tracked pose driver
        /// </summary>
        public enum UpdateType
        {
            /// <summary>
            /// Sample input at both update, and directly before rendering. For smooth head pose tracking, 
            /// we recommend using this value as it will provide the lowest input latency for the device. 
            /// This is the default value for the UpdateType option
            /// </summary>
            UpdateAndBeforeRender,
            /// <summary>
            /// Only sample input during the update phase of the frame.
            /// </summary>
            Update,
            /// <summary>
            /// Only sample input directly before rendering
            /// </summary>
            BeforeRender,
        }

        [SerializeField]
        UpdateType m_UpdateType;

        UpdateType m_PreUpdateType;

        public UpdateType updateType
        {
            get
            {
                return m_UpdateType;
            }
            set
            {
                m_UpdateType = value;
            }
        }

        /// <summary>
        /// Is auto recenter space when scene loaded or init
        /// </summary>
        [SerializeField]
        private bool m_AutoRecenterSpace = true;

        public bool autoRecenterSpace
        {
            get
            {
                return m_AutoRecenterSpace;
            }
            set
            {
                m_AutoRecenterSpace = value;
            }
        }

        private RokidTrackedPoseDriver m_TrackedPoseDriver;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest);
        }

        private void Start()
        {
            if (Utils.IsAndroidPlatform())
            {
                // GetComponent<Camera>().fieldOfView = NativeInterface.NativeAPI.GetVFov(true);
                m_TrackedPoseDriver = gameObject.GetComponent<RokidTrackedPoseDriver>();
                if (m_TrackedPoseDriver == null)
                {
                    m_TrackedPoseDriver = gameObject.AddComponent<RokidTrackedPoseDriver>();
                }
                if (m_HeadTrackingType != HeadTrackingType.None)
                {
                    switch (m_HeadTrackingType)
                    {
                        case HeadTrackingType.RotationAndPosition:
                            m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                            break;
                        case HeadTrackingType.RotationOnly:
                            m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                            break;
                        case HeadTrackingType.PositionOnly:
                            m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.PositionOnly;
                            break;
                    }
                }
                else
                {
                    m_TrackedPoseDriver.enabled = false;
                }
                string value = NativeInterface.NativeAPI.GetPersistValue("rokid.camera.lock.roll");
                if (!string.IsNullOrEmpty(value))
                {
                    m_RotationExcludeRoll = Convert.ToBoolean(value);
                }
                m_TrackedPoseDriver.rotationExcludeRoll = m_RotationExcludeRoll;
                m_PreRotationExcludeRoll = m_RotationExcludeRoll;
                RKLog.KeyInfo($"====RKCameraRig====: SetHeadTrackingType :{m_HeadTrackingType}");
                NativeInterface.NativeAPI.SetTrackingType((int)m_HeadTrackingType);
                if (m_AutoRecenterSpace)
                    NativeInterface.NativeAPI.Recenter();
                m_PreHeadTrackingType = m_HeadTrackingType;
                m_PreUpdateType = m_UpdateType;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (m_AutoRecenterSpace)
                NativeInterface.NativeAPI.Recenter();
        }

        private void OnDestroy()
        {
            if (Utils.IsAndroidPlatform())
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (Utils.IsAndroidPlatform())
            {
                if (m_PreHeadTrackingType != m_HeadTrackingType)
                {
                    m_PreHeadTrackingType = m_HeadTrackingType;
                    if (m_HeadTrackingType == HeadTrackingType.None)
                    {
                        m_TrackedPoseDriver.enabled = false;
                    }
                    else
                    {
                        m_TrackedPoseDriver.enabled = true;
                        switch (m_HeadTrackingType)
                        {
                            case HeadTrackingType.RotationAndPosition:
                                m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                                break;
                            case HeadTrackingType.RotationOnly:
                                m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                                break;
                            case HeadTrackingType.PositionOnly:
                                m_TrackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.PositionOnly;
                                break;
                        }
                    }
                    NativeInterface.NativeAPI.SetTrackingType((int)m_HeadTrackingType);
                }
                if (m_PreUpdateType != m_UpdateType)
                {
                    m_PreUpdateType = m_UpdateType;
                    m_TrackedPoseDriver.updateType = (TrackedPoseDriver.UpdateType)m_UpdateType;
                }
                if (m_PreRotationExcludeRoll != m_RotationExcludeRoll)
                {
                    m_PreRotationExcludeRoll = m_RotationExcludeRoll;
                    m_TrackedPoseDriver.rotationExcludeRoll = m_RotationExcludeRoll;
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus == false)
            {
                NativeInterface.NativeAPI.SetTrackingType((int)m_HeadTrackingType);
            }
        }
    }
}
