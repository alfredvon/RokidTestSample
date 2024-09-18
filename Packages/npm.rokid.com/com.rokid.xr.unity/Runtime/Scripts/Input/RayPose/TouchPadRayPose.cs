using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class TouchPadRayPose : BaseRayPose
    {
        private float yaw;
        private float pitch;
        private bool dragging = false;
        private float minSpeed = 0.7f, maxSpeed = 1.0f;

        private RayInteractor rayInteractor;

        private void Awake()
        {
            rayInteractor = GetComponent<RayInteractor>();
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
            renderCamera = Camera.main;
            InputModuleManager.OnModuleChange += OnModuleChange;
        }

        private void OnModuleChange(InputModuleType oldType, InputModuleType currentType)
        {
            if (currentType == InputModuleType.TouchPad)
            {
                if (oldType == InputModuleType.Mouse)
                {
                    transform.SetPose(InteractorPose[InputModuleType.Mouse]);
                }
                else
                {
                    ResetPose();
                }
            }
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            dragging = true;
        }


        private void OnDestroy()
        {
            InputModuleManager.OnModuleChange -= OnModuleChange;
        }

        private void OnEnable()
        {
            TouchPadEventInput.OnMouseMove += OnMouseMove;
        }

        private void OnDisable()
        {
            TouchPadEventInput.OnMouseMove -= OnMouseMove;
        }


        /// <summary>
        /// 重置射线位姿
        /// </summary>
        private void ResetPose()
        {
            // RKLog.Debug("====TouchPadRayPose====: Reset Pose");
            transform.position = MainCameraCache.mainCamera.transform.position;
            Vector3 localCenter = Utils.GetCameraCenter(rayInteractor.CurrentRayLength);
            Vector3 worldCenter = MainCameraCache.mainCamera.transform.localToWorldMatrix * localCenter;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, worldCenter - transform.position);
            SavePose();
        }

        private void SavePose()
        {
            InteractorPose[InputModuleType.TouchPad] = transform.GetPose();
        }

        private void OnMouseMove(Vector2 delta)
        {
            if (updateType == PoseUpdateType.Auto)
            {
                float speed = Mathf.Clamp(delta.magnitude / (Time.deltaTime * 25), minSpeed, maxSpeed);
                pitch = transform.rotation.eulerAngles.x;
                yaw = transform.rotation.eulerAngles.y;
                pitch += delta.y * speed;
                yaw += delta.x * speed;
                transform.rotation = Quaternion.Euler(pitch, yaw, 0);
                if (!dragging)
                {
                    LimitInViewField();
                }
                SavePose();
            }
        }

        public override void UpdateTargetPoint(Vector3 point)
        {
            if (updateType == PoseUpdateType.FollowTargetPoint)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.forward, point - transform.position);
                SavePose();
            }
        }

        protected override void Update()
        {
            if (TouchPadEventInput.Instance.GetTouchInputActive() == false)
            {
                base.Update();
                LimitInViewField();
            }
        }
    }
}

