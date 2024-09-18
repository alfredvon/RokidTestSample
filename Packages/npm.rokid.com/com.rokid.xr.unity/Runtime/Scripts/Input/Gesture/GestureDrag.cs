using System;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Rokid.UXR.Interaction
{
    [Obsolete("Use Draggable Instead")]
    public class GestureDrag : MonoBehaviour, IGesDrag, IGesBeginDrag, IGesEndDrag, IBezierCurveDrag
    {
        private float smoothSpeed = 8;
        private Vector3 allDelta;
        private Vector3 oriPos;
        [SerializeField]
        private bool lookAtCamera = true;

        private Vector3 preCameraPos;

        private Vector3 cameraPosDelta;

        [SerializeField]
        private bool UseBezierCurve = false;
        [SerializeField]
        private float moveLerpTime = 0.05f;

        private Vector3 HitLocalPos;

        private bool IsDragging;

        public GameObject targetObj => this.gameObject;

        private void Start()
        {
            preCameraPos = MainCameraCache.mainCamera.transform.position;
        }

        private void Update()
        {
            cameraPosDelta = MainCameraCache.mainCamera.transform.position - preCameraPos;
            preCameraPos = MainCameraCache.mainCamera.transform.position;
            // RKLog.Info("cameraPosDelta.sqrMagnitude:" + cameraPosDelta.sqrMagnitude);
        }

        public void OnGesBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
            HitLocalPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            RKLog.Info($"{gameObject.name}: Begin Drag, DragHitLocalPos={HitLocalPos}");

            allDelta = Vector3.zero;
            oriPos = transform.position;
        }

        #region BezierCurve
        public bool IsEnablePinchBezierCurve()
        {
            return false;
        }

        public bool IsEnableGripBezierCurve()
        {
            return UseBezierCurve;
        }

        public bool IsInBezierCurveDragging()
        {
            return IsDragging;
        }

        public Vector3 GetBezierCurveEndPoint()
        {
            return transform.TransformPoint(HitLocalPos);
        }
        #endregion

        public void OnGesDrag(Vector3 delta)
        {
            float disToCamera = Vector3.Distance(MainCameraCache.mainCamera.transform.position, transform.position);
            if (disToCamera > 0.5f)
            {
                //远场拖拽
                preFilter(ref delta);
                allDelta += delta;
                Vector3 targetPos = oriPos + allDelta;
                smoothSpeed = 10;
                filter(ref targetPos, ref allDelta, ref oriPos, delta);
                if (lookAtCamera)
                {
                    if (GetComponent<CanvasRenderer>())
                    {
                        Vector3 director = targetPos - MainCameraCache.mainCamera.transform.position;
                        transform.localRotation = Quaternion.LookRotation(director);
                    }
                    else
                    {
                        //模型
                        Vector3 director = MainCameraCache.mainCamera.transform.position - targetPos;
                        director.y = 0;
                        transform.localRotation = Quaternion.LookRotation(director);
                    }
                }

                if (UseBezierCurve)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos, (moveLerpTime == 0f) ? 1f : 1f - Mathf.Pow(moveLerpTime, Time.deltaTime));
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
                }
            }
            else
            {
                //近场拖拽
                filter01(ref delta);
                allDelta += delta;
                Vector3 targetPos = oriPos + allDelta;
                if (lookAtCamera)
                {
                    if (GetComponent<CanvasRenderer>())
                    {
                        Vector3 director = targetPos - MainCameraCache.mainCamera.transform.position;
                        transform.localRotation = Quaternion.LookRotation(director);
                    }
                    else
                    {
                        //模型
                        Vector3 director = MainCameraCache.mainCamera.transform.position - targetPos;
                        director.y = 0;
                        transform.localRotation = Quaternion.LookRotation(director);
                    }
                }
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed * 2);
            }
        }

        private void preFilter(ref Vector3 delta)
        {
            if (cameraPosDelta.sqrMagnitude < 0.0001f)
            {
                Vector3 oriDelta = new Vector3(delta.x, delta.y, delta.z);
                float disToCamera = Vector3.Distance(MainCameraCache.mainCamera.transform.position, transform.position);
                delta *= disToCamera * 2;
                // 0.判断delta在相机空间中的分量
                Vector3 cameraSpaceDelta = MainCameraCache.mainCamera.transform.InverseTransformDirection(delta);
                if (cameraSpaceDelta.z * cameraSpaceDelta.z > (cameraSpaceDelta.x * cameraSpaceDelta.x + cameraSpaceDelta.y * cameraSpaceDelta.y))
                {
                    //修改拖拽的朝向
                    delta = Vector3.Normalize(MainCameraCache.mainCamera.transform.position - transform.position) * -(MainCameraCache.mainCamera.transform.InverseTransformDirection(oriDelta).z) * 5;
                }
                else
                {
                    delta = MainCameraCache.mainCamera.transform.rotation * cameraSpaceDelta;
                }
            }
        }

        private void filter(ref Vector3 targetPos, ref Vector3 allDelta, ref Vector3 oriPos, Vector3 delta)
        {
            float disToCamera = Vector3.Distance(MainCameraCache.mainCamera.transform.position, transform.position);
            Vector3 cameraSpaceDelta = MainCameraCache.mainCamera.transform.InverseTransformDirection(delta);
            if (disToCamera < 0.5f && cameraSpaceDelta.z < 0)
            {
                targetPos = MainCameraCache.mainCamera.transform.position + MainCameraCache.mainCamera.transform.forward * 0.5f;
                allDelta = Vector3.zero;
                oriPos = transform.position;
                smoothSpeed = 5;
            }
        }

        private void filter01(ref Vector3 delta)
        {
            Vector3 cameraSpaceDelta = MainCameraCache.mainCamera.transform.InverseTransformDirection(delta);
            if (cameraSpaceDelta.z > 0)
            {
                cameraSpaceDelta.z *= 3;
            }
            delta = MainCameraCache.mainCamera.transform.rotation * cameraSpaceDelta;
        }

        public void OnGesEndDrag()
        {
            RKLog.Info($"{gameObject.name}: End Drag");
            IsDragging = false;
        }

        public Vector3 GetBezierCurveEndNormal()
        {
            return Vector3.forward;
        }

        public Vector3 GetBezierCurveEndPoint(int pointerId)
        {
            // throw new NotImplementedException();
            return Vector3.zero;
        }

        public Vector3 GetBezierCurveEndNormal(int pointerId)
        {
            // throw new NotImplementedException();
            return Vector3.zero;
        }
    }
}

