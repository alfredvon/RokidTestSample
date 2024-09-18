using System;
using System.Collections.Generic;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Base Ray Pose
    /// </summary>
    public abstract class BaseRayPose : MonoBehaviour, IRayPose
    {
        [Serializable]
        public class EditorParams
        {
            [Tooltip("是否使用鼠标模拟射线Rotate")]
            public bool useMouseRotate = true;
            [Tooltip("射线是否跟随相机,只在编辑器生效")]
            public bool followCameraInEditor = true;
            [Tooltip("相机空间下的局部坐标,只有在followCamera为真的情况下生效")]
            public Vector3 localPositionInCameraSpace = new Vector3(0, -0.1f, 0);
            public float maxDistanceInEditor = 10.0f;
            [HideInInspector]
            public float raycastDistance = -1.0f;
            public BaseRayCaster rayCaster;
            public static readonly RaycastHit[] hits = new RaycastHit[4];
            public readonly List<RaycastResult> sortedRaycastResults = new List<RaycastResult>();
            public Transform rayOrigin;
        }

        [SerializeField, Tooltip("编辑器模拟参数")]
        private EditorParams editorParams;

        private Transform hostTransform;

        protected PoseUpdateType updateType = PoseUpdateType.Auto;

        protected static Dictionary<InputModuleType, Pose> InteractorPose = new Dictionary<InputModuleType, Pose>(){
            {InputModuleType.TouchPad,Pose.identity},{InputModuleType.Mouse,Pose.identity}
        };

        protected virtual void Start()
        {
            if (editorParams.rayCaster == null)
                editorParams.rayCaster = GetComponent<BaseRayCaster>();
            hostTransform = editorParams.rayOrigin == null ? transform : editorParams.rayOrigin;
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (editorParams.useMouseRotate && updateType == PoseUpdateType.Auto)
            {
                UpdateInEditor();
            }
#endif
        }

#if UNITY_EDITOR
        protected virtual void UpdateInEditor()
        {
            if (editorParams.followCameraInEditor)
            {
                hostTransform.position = MainCameraCache.mainCamera.transform.TransformPoint(editorParams.localPositionInCameraSpace);
            }

            if (editorParams.rayCaster != null)
            {
                Ray ray = MainCameraCache.mainCamera.ScreenPointToRay(Input.mousePosition);
                editorParams.rayCaster.Raycast(ray, Mathf.Infinity, editorParams.sortedRaycastResults);
                RaycastResult raycastResult = editorParams.rayCaster.FirstRaycastResult(editorParams.sortedRaycastResults);
                if (raycastResult.gameObject != null)
                {
                    Vector3 targetPos = raycastResult.worldPosition;
                    Vector3 toDirection = targetPos - hostTransform.position;
                    hostTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
                    editorParams.raycastDistance = Mathf.Min(editorParams.maxDistanceInEditor, raycastResult.distance);
                }
                else if (editorParams.raycastDistance > 0.0f)
                {
                    Vector3 targetPos = ray.origin + Mathf.Min(editorParams.maxDistanceInEditor, editorParams.raycastDistance) * ray.direction;
                    Vector3 toDirection = targetPos - hostTransform.position;
                    hostTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
                }
                else
                {
                    hostTransform.localRotation = Quaternion.identity;
                }
            }
        }
#endif


        #region LimitInViewField
        protected Camera renderCamera;
        public Vector4 cursorSize;

        private Vector4 _halfFovTan = Vector4.zero;
        private Vector4 halfFovTan
        {
            get
            {
                if (_halfFovTan != Vector4.zero)
                {
                    return _halfFovTan;
                }
#if UNITY_EDITOR
                _halfFovTan.x = _halfFovTan.y = _halfFovTan.z = _halfFovTan.w = Mathf.Tan(renderCamera.fieldOfView / 2 * Mathf.Deg2Rad);
                return _halfFovTan;
#endif
                float[] fov = new float[4];
                NativeInterface.NativeAPI.GetUnityEyeFrustumHalf(false, ref fov);
                if (fov[0] > 0 && fov[1] > 0 && fov[2] > 0 && fov[3] > 0)
                {
                    _halfFovTan.x = Mathf.Tan(fov[0] * Mathf.Deg2Rad); // Left 
                    _halfFovTan.y = Mathf.Tan(fov[1] * Mathf.Deg2Rad); // Right
                    _halfFovTan.z = Mathf.Tan(fov[2] * Mathf.Deg2Rad); // Top 
                    _halfFovTan.w = Mathf.Tan(fov[3] * Mathf.Deg2Rad); // Bottom
                    return _halfFovTan;
                }
                return Vector4.zero;
            }
        }


        Vector4 GetCameraCorner(float dis)
        {
            Vector4 corner = Vector4.zero;
            corner.z = dis * halfFovTan.z;  // Top 
            corner.w = dis * halfFovTan.w;  // Bottom
#if UNITY_EDITOR
            corner.x = corner.y = corner.z * renderCamera.aspect;
#else
            corner.x = dis * halfFovTan.x;  // Left 
            corner.y = dis * halfFovTan.y;  // Right
#endif  

            return corner;
        }

        protected void LimitInViewField()
        {
            transform.position = renderCamera.transform.position;
            Vector3 pointPosition = transform.forward + transform.position;
            Vector3 pointInCamera = renderCamera.transform.InverseTransformPoint(pointPosition);
            //点在相机forward上的距离
            float dis = pointInCamera.z;
            if (dis < 0)
            {
                dis = -dis;
                pointInCamera.z = dis;
            }

            Vector4 corner = GetCameraCorner(dis);
            if (pointInCamera.x < -corner.x + cursorSize.x) // LEFT
            {
                pointInCamera.x = -corner.x + cursorSize.x;
            }
            else if (pointInCamera.x > corner.y - cursorSize.w) // Right
            {
                pointInCamera.x = corner.y - cursorSize.w;
            }
            if (pointInCamera.y > corner.z - cursorSize.y)  // Top
            {
                pointInCamera.y = corner.z - cursorSize.y;
            }
            else if (pointInCamera.y < -corner.w + cursorSize.z) // Bottom
            {
                pointInCamera.y = -corner.w + cursorSize.z;
            }

            pointPosition = renderCamera.transform.TransformPoint(pointInCamera);
            transform.forward = pointPosition - transform.position;
        }

        #endregion

        #region  Interface
        public virtual void UpdateTargetPoint(Vector3 point)
        {

        }

        public void SetPoseUpdateType(PoseUpdateType type)
        {
            this.updateType = type;
        }

        public PoseUpdateType GetPoseUpdateType()
        {
            return updateType;
        }

        public Pose RayPose => transform.GetPose();
        #endregion
    }
}
