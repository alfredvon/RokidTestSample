using System;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofRayPose : BaseRayPose
    {
        private Vector3 offsetPosition = new Vector3(0.1f, -0.5f, 0);
        private Vector3 offsetYaw = new Vector3(0, 5, 0);
        private Transform cameraOffset;
        [SerializeField]
        private Text logText;
        private Vector3 oriForward = Vector3.forward;

        protected override void Start()
        {
            base.Start();
            ThreeDofEventInput.OnPhoneRayRotation += OnPhoneRayRotation;
            cameraOffset = new GameObject("CameraOffset").transform;
            cameraOffset.SetParent(this.transform);
        }

        private void OnDestroy()
        {
            ThreeDofEventInput.OnPhoneRayRotation -= OnPhoneRayRotation;
        }

        private void OnPhoneRayRotation(Quaternion rayRotation)
        {
            if (Utils.IsUnityEditor() || updateType == PoseUpdateType.FollowTargetPoint)
                return;
            Vector3 curForward = rayRotation * Vector3.forward;
            float angle = Vector3.Angle(curForward, oriForward) * 33;
            oriForward = curForward;
            float smoothSpeed = Mathf.Clamp(angle, 10, 100);
            if (MainCameraCache.mainCamera.transform.parent != null)
            {
                this.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(offsetYaw) * MainCameraCache.mainCamera.transform.parent.rotation * rayRotation, Time.deltaTime * smoothSpeed);
            }
            else
            {
                this.transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(offsetYaw) * rayRotation, Time.deltaTime * smoothSpeed);
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateRayState();
        }

        private void LateUpdate()
        {
            cameraOffset.position = MainCameraCache.mainCamera.transform.position;
            Vector3 targetForward = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.forward, Vector3.up);
            cameraOffset.rotation = Quaternion.FromToRotation(Vector3.forward, targetForward);
            if (cameraOffset.forward != new Vector3(0, 0, -1))
            {
                Vector3 followPosition = cameraOffset.TransformPoint(offsetPosition);
                this.transform.position = followPosition;
                if (logText != null)
                {
                    logText.text = $"rayPosition:{followPosition}\r\nrayForward:{transform.forward}\r\ncameraPosition:{MainCameraCache.mainCamera.transform.position}\r\ncameraForward:{MainCameraCache.mainCamera.transform.forward}\r\ncameraOffsetPosition:{cameraOffset.position}\r\ncameraOffsetForward:{cameraOffset.transform.forward}";
                }
            }
        }


        public override void UpdateTargetPoint(Vector3 point)
        {
            if (updateType == PoseUpdateType.FollowTargetPoint)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.forward, point - transform.position);
            }
        }

        private void UpdateRayState()
        {
            if (Utils.IsAndroidPlatform())
            {
                switch (ThreeDofEventInput.Instance.HoldHandType)
                {
                    case HandType.None:
                        offsetPosition = new Vector3(0, ThreeDofEventInput.Instance.GetRayHeight(), 0);
                        offsetYaw = Vector3.zero;
                        break;
                    case HandType.LeftHand:
                        offsetPosition = new Vector3(-0.1f, ThreeDofEventInput.Instance.GetRayHeight(), 0);
                        offsetYaw = new Vector3(0, 0, 0);
                        break;
                    case HandType.RightHand:
                        offsetPosition = new Vector3(0.1f, ThreeDofEventInput.Instance.GetRayHeight(), 0);
                        offsetYaw = new Vector3(0, 0, 0);
                        break;
                }
            }
        }
    }
}
