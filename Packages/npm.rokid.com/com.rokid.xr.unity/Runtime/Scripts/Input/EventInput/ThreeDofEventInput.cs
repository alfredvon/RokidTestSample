using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Native;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine.UI;
using Rokid.UXR.Arithmetic;

namespace Rokid.UXR.Interaction
{
    public enum RingButtonsType
    {
        TouchPress = 1 << 1,
        DoubleClick = 1 << 2,
        LongPress = 1 << 3,
        Click = 1 << 4,
        Touching = 1 << 5,
    }

    public enum ThreeDofType
    {
        StationPro,
        Phone,
        Ring,
        Station2
    }

    /// <summary>
    /// The 3dof event input class provides an external interface for 3dof interaction
    /// </summary>
    public class ThreeDofEventInput : MonoSingleton<ThreeDofEventInput>, IEventInput
    {
        public enum SleepActiveType
        {
            Shake,
            AnyKeyDown
        }
        public static event Action<Quaternion> OnPhoneRayRotation;
        public static event Action<Quaternion> OnOriRot;
        public static event Action OnActiveThreeDofModule;
        public static event Action OnReleaseThreeDofModule;
        public static event Action OnThreeDofReset;
        public static event Action<bool> OnThreeDofSleep;
        public static event Action OnSwipeTriggerSuccess;
        public Transform Interactor { get; set; }
        private float[] data = new float[4];
        private bool initialize = false;
        private HandType hand = HandType.RightHand;
        public HandType HoldHandType { get { return hand; } set { hand = value; } }
        private int pixelDragThreshold = 60;
        public int PixelDragThreshold { get { return pixelDragThreshold; } set { pixelDragThreshold = value; } }
        private BaseRayCaster raycaster;
        public BaseRayCaster GetRayCaster(HandType hand = HandType.None)
        {
            if (raycaster == null && Interactor != null)
            {
                raycaster = Interactor.GetComponent<BaseRayCaster>();
            }
            return raycaster;
        }
        private ISelector raySelector;
        public ISelector GetRaySelector(HandType hand = HandType.None)
        {
            if (raySelector == null && Interactor != null)
            {
                raySelector = Interactor.GetComponentInChildren<ISelector>();
            }
            return raySelector;
        }
        private IRayPose rayPose;
        public IRayPose GetRayPose(HandType hand = HandType.None)
        {
            if (rayPose == null && Interactor != null)
            {
                rayPose = Interactor.GetComponentInChildren<IRayPose>();
            }
            return rayPose;
        }
        private Quaternion rayRotation = Quaternion.identity;
        private Quaternion preRayRotation = Quaternion.identity;
        private ThreeDofType threeDofType = ThreeDofType.StationPro;
        private float raySleepTime = 10.0f;
        private float raySleepElasptime = 0;
        private float height = -0.5f;
        private bool raySleep;
        private RingButtonsType ringButtonsEvent = 0;
        private RingButtonsType preRingButtonsEvent = 0;
        private bool ringUp;
        private bool ringDown;
        private bool dragging;
        private bool lockInput;
        private SwipeLogic horizontalUpSwipe;
        private SwipeLogic verticalLeftSwipe;
        private SwipeLogic verticalRightSwipe;
        private float deltaTime;
        private bool triggerSuccess;

        private SleepActiveType activeType = SleepActiveType.Shake;

        public void SetActiveType(SleepActiveType activeType)
        {
            this.activeType = activeType;
        }

        protected override void Awake()
        {
            NativeInterface.NativeAPI.OpenPhoneTracker();
            try
            {
                NativeInterface.NativeAPI.OpenRingTracker();
                NativeInterface.NativeAPI.RegisterRingButtonEvents(this.gameObject.name, "OnRingButtonsEvent");
            }
            catch (Exception e)
            {
                RKLog.Warning(e.ToString());
            }
            InputModuleManager.OnModuleActive += OnModuleActive;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;

            horizontalUpSwipe = new SwipeLogic(Quaternion.identity, SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
            verticalLeftSwipe = new SwipeLogic(Quaternion.AngleAxis(-90, Vector3.up), SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
            // verticalRightSwipe = new SwipeLogic(Quaternion.AngleAxis(90, Vector3.up), SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
        }


        private void SwipeUpdate()
        {
            if (triggerSuccess == false)
            {
                horizontalUpSwipe.Update();
                verticalLeftSwipe.Update();
                // verticalRightSwipe.Update();
            }
            if (triggerSuccess)
            {
                deltaTime += Time.deltaTime;
                if (deltaTime > 0.5f)
                {
                    deltaTime = 0;
                    triggerSuccess = false;
                }
            }
        }

        private bool IsTouchOperate()
        {
            return RKTouchInput.Instance.TouchInside(0) && RKTouchInput.Instance.TouchInside(1);
        }

        private void SwipeSuccess()
        {
            if (!triggerSuccess)
            {
                triggerSuccess = true;
                if (!lockInput)
                {
                    ActiveModule();
                    ResetImuAxisY();
                    Sleep(false);
                }
                OnSwipeTriggerSuccess?.Invoke();
            }
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            if (InputModuleManager.Instance.GetThreeDofActive())
                dragging = true;
        }

        private void OnRingButtonsEvent(string buttonsEvent)
        {
            // RKLog.KeyInfo($"OnRingButtonsEvent:{buttonsEvent}");
            this.ringButtonsEvent = (RingButtonsType)Convert.ToInt32(buttonsEvent);
        }

        private void ProcessRingButtonEvent()
        {
            if (threeDofType == ThreeDofType.Ring)
            {
                this.ringDown = false;
                this.ringUp = false;
                if (ContainTargetRingType(RingButtonsType.TouchPress, this.ringButtonsEvent) && !ContainTargetRingType(RingButtonsType.TouchPress, this.preRingButtonsEvent))
                {
                    this.ringDown = true;
                }
                if (!ContainTargetRingType(RingButtonsType.TouchPress, this.ringButtonsEvent) && ContainTargetRingType(RingButtonsType.TouchPress, this.preRingButtonsEvent))
                {
                    this.ringUp = true;
                }
                this.preRingButtonsEvent = this.ringButtonsEvent;
            }
        }

        private bool ContainTargetRingType(RingButtonsType containType, RingButtonsType oriType)
        {
            return (containType & oriType) == containType;
        }

        public bool GetRingButtonEvents(RingButtonsType targetButtonsType)
        {
            return (this.ringButtonsEvent & targetButtonsType) == targetButtonsType;
        }

        public bool GetRingDown()
        {
            return this.ringDown;
        }

        public bool GetRingUp()
        {
            return this.ringUp;
        }

        public ThreeDofType GetThreeDofType()
        {
            return threeDofType;
        }

        private void OnModuleActive(InputModuleType moduleType)
        {
            if (moduleType != InputModuleType.ThreeDof)
            {
                raySleepElasptime = 0;
                raySleep = false;
                OnThreeDofSleep?.Invoke(false);
            }
        }

        private void Update()
        {
            if (!initialize)
                return;
            if (Utils.IsAndroidPlatform())
            {
                UpdateThreeDofType();
                SwipeUpdate();
                if (lockInput)
                    return;
                if (InputModuleManager.Instance.GetThreeDofActive())
                {
                    if (CanResetImu() || IsDoubleClick())
                    {
                        ResetImuAxisY();
                    }

                    if (Input.anyKeyDown)
                    {
                        raySleepElasptime = 0;
                        raySleep = false;
                        OnThreeDofSleep?.Invoke(false);
                    }
                    GetData();
                    ProcessData();
                    ProcessRingButtonEvent();
                }
                else
                {
                    //只有station pro 设备能直接双击+重置
                    if (IsDoubleClick() && threeDofType == ThreeDofType.StationPro)
                    {
                        ActiveModule();
                        ResetImuAxisY();
                    }
                    else if (CanActiveModule())
                    {
                        ActiveModule();
                    }
                }
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.H))
            {
                ActiveModule();
                ResetImuAxisY();
            }
#endif      
        }

        private void UpdateThreeDofType()
        {
            if (threeDofType != ThreeDofType.StationPro && (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                       || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                       || Input.GetKeyUp(KeyCode.JoystickButton0)))
            {
                threeDofType = ThreeDofType.StationPro;
            }
            else if (threeDofType != ThreeDofType.Phone && Input.touchCount > 0 && Utils.IsPhone())
            {
                threeDofType = ThreeDofType.Phone;
            }
            else if (threeDofType != ThreeDofType.Station2 && Input.touchCount > 0 && !Utils.IsPhone())
            {
                threeDofType = ThreeDofType.Station2;
            }
            else if (threeDofType != ThreeDofType.Ring && GetRingButtonEvents(RingButtonsType.Touching))
            {
                threeDofType = ThreeDofType.Ring;
            }
        }

        private void GetData()
        {
            switch (threeDofType)
            {
                case ThreeDofType.Station2:
                case ThreeDofType.StationPro:
                case ThreeDofType.Phone:
                    NativeInterface.NativeAPI.GetPhonePose(data);
                    break;
                case ThreeDofType.Ring:
                    NativeInterface.NativeAPI.GetRingPose(data);
                    break;
            }
            rayRotation[0] = data[0];
            rayRotation[1] = data[1];
            rayRotation[2] = -data[2];
            rayRotation[3] = data[3];
        }

        private bool CanActiveModule()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    return Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                      || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                      || Input.GetKeyUp(KeyCode.JoystickButton0);
                case ThreeDofType.Ring:
                    return GetRingButtonEvents(RingButtonsType.Touching);
            }
            return false;
        }

        private bool CanResetImu()
        {
            return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET_RAY);
        }

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("ThreeDofRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/ThreeDofRayInteractor"));
                }
                Interactor = go.transform.GetComponentInChildren<ModuleInteractor>().transform;
                Interactor.name = "ThreeDofRayInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
        }

        public void Release()
        {
            OnReleaseThreeDofModule?.Invoke();
            horizontalUpSwipe.Release();
            verticalLeftSwipe.Release();
            // verticalRightSwipe.Release();
            Destroy(this.gameObject);
        }

        public void ActiveModule()
        {
            if (!InputModuleManager.Instance.GetThreeDofActive())
            {
                ThreeDofEventInput.OnActiveThreeDofModule?.Invoke();
                RKVirtualController.Instance.Change(ControllerType.PHONE3DOF);
                EventSystem.current.pixelDragThreshold = PixelDragThreshold;
                NativeInterface.NativeAPI.Vibrate(2);
            }
        }

        protected override void OnDestroy()
        {
            // station 设备 SDK不再关闭3dof 射线算法
            if (!SystemInfo.deviceModel.Contains("station"))
            {
                NativeInterface.NativeAPI.ClosePhoneTracker();
            }
            try
            {
                NativeInterface.NativeAPI.CloseRingTracker();
            }
            catch (System.Exception e)
            {
                RKLog.Warning(e.ToString());
            }
            OnPhoneRayRotation = null;
            InputModuleManager.OnModuleActive -= OnModuleActive;
            initialize = false;
        }

        void ProcessData()
        {
            OnPhoneRayRotation?.Invoke(rayRotation);
            OnOriRot?.Invoke(rayRotation);
            // LogThreeDofInfo(rayRotation.eulerAngles);
            if (InputModuleManager.Instance.GetThreeDofActive())
            {
                Vector3 preForward = preRayRotation * Vector3.forward;
                Vector3 forward = rayRotation * Vector3.forward;

                if (raySleep == false && Vector3.Angle(preForward, forward) < 0.05f)
                {
                    raySleepElasptime += Time.deltaTime;
                    if (raySleepElasptime > raySleepTime)
                    {
                        raySleep = true;
                        OnThreeDofSleep.Invoke(true);
                    }
                }

                if (Vector3.Angle(preForward, forward) > 0.05f)
                {
                    raySleepElasptime = 0;
                }

                if ((activeType == SleepActiveType.Shake && Vector3.Angle(preForward, forward) > 0.05f) || Input.anyKeyDown || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET_RAY) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_SINGLE_TAP) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_DOUBLE_TAP) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_LONG_TAP))
                {
                    raySleepElasptime = 0;
                    if (raySleep)
                    {
                        raySleep = false;
                        OnThreeDofSleep.Invoke(false);
                    }
                }
                preRayRotation = rayRotation;
            }
        }

        #region LogThreeDofInfo
        private float logTime = 5;
        private float logElapsedTime = 0;
        private void LogThreeDofInfo(Vector3 euler)
        {
            logElapsedTime += Time.deltaTime;
            if (logElapsedTime > logTime)
            {
                logElapsedTime = 0;
                RKLog.KeyInfo($"====ThreeDofEventInput==== Ori Ray Euler : {euler} ");
            }
        }
        #endregion

        public void ResetImuAxisY()
        {
            NativeInterface.NativeAPI.RecenterPhonePose();
            OnThreeDofReset?.Invoke();
            // RKLog.KeyInfo("====ThreeDofEventInput==== 重置3dof射线");
        }

        #region IsDoubleClick
        float doubleClickTime = 0.7f;
        float clickTime = 0;
        int clickCount = 0;
        //Only for station pro
        Vector2 touchPos = Vector2.zero;
        private bool IsDoubleClick()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    if (Input.GetKeyDown(KeyCode.JoystickButton3))
                    {
                        clickCount++;
                    }
                    break;
                case ThreeDofType.Ring:
                    // Do nothing 手环内置重置射线
                    break;
            }
            if (clickCount == 1)
            {
                clickTime += Time.deltaTime;
            }
            if (clickTime < doubleClickTime)
            {
                if (clickCount == 2)
                {
                    clickTime = 0;
                    clickCount = 0;
                    touchPos = Vector2.zero;
                    return true;
                }
            }
            else
            {
                clickCount = 0;
                clickTime = 0;
                touchPos = Vector2.zero;
            }
            return false;
        }
        #endregion

        public float ForwardSpeed()
        {
            if (RKTouchInput.Instance.GetInsideTouchCount() > 0)
            {
                switch (threeDofType)
                {
                    case ThreeDofType.Station2:
                        return RKTouchInput.Instance.GetInsideTouchDeltaPosition().y * 0.02f;
                    case ThreeDofType.Phone:
                        return RKTouchInput.Instance.GetInsideTouchDeltaPosition().y * 0.02f;
                }
                return 0;
            }
            else
            {
                return (Input.GetKey(KeyCode.UpArrow) ? 1 : (Input.GetKey(KeyCode.DownArrow) ? -1 : 0)) * 0.05f;
            }
        }

        /// <summary>
        /// 设置射线相对头的高度,默认位-0.5,范围(-0.5f,0)
        /// </summary>
        /// <param name="height"></param>
        public void SetRayHeight(float height)
        {
            height = Mathf.Clamp(height, -0.5f, 0);
            this.height = height;
        }
        public float GetRayHeight()
        {
            return height;
        }

        public void Sleep(bool sleep)
        {
            OnThreeDofSleep?.Invoke(sleep && !lockInput);
        }

        public void Lock(bool isLock)
        {
            this.lockInput = isLock;
        }
    }
}
