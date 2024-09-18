using System;
using UnityEngine;

namespace Rokid.UXR.Arithmetic
{
    public class SwipeLogic
    {
        private float prePitch, triggerYawThreshold = 60;
        private float resetPassTime = 0, resetTime = 2.0f;
        private float minPitch = 0, maxPitch = 0;
        private float deltaTime = 0; float maxGyro = 0;
        private Vector3 currentForward;
        private Quaternion swipeOrientation;
        private event Action OnSuccess;
        private event Func<bool> CanSwipe;

        public SwipeLogic(Quaternion swipeOrientation, Action OnSuccess, Func<bool> CanSwipe)
        {
            this.swipeOrientation = swipeOrientation;
            this.OnSuccess = OnSuccess;
            this.CanSwipe = CanSwipe;
            Input.gyro.enabled = true;
        }

        public void Update()
        {
            Vector3 acceleration = Input.acceleration;
            Vector3 gravityNormalized = swipeOrientation * NormalizeAcceleration(acceleration);
            Vector3 gyroRotationRate = swipeOrientation * Input.gyro.rotationRate;
            currentForward = Vector3.Lerp(currentForward, gravityNormalized, Time.deltaTime * 10);
            Quaternion targetOrientation = Quaternion.FromToRotation(Vector3.back, currentForward);
            Logic(targetOrientation, currentForward, gyroRotationRate);
        }

        public void Release()
        {
            OnSuccess = null;
            CanSwipe = null;
        }

        private Vector3 NormalizeAcceleration(Vector3 acceleration)
        {
            float magnitude = acceleration.magnitude;
            if (magnitude > 0.0001f)
            {
                return acceleration / magnitude;
            }
            else
            {
                return Vector3.zero;
            }
        }


        private void Logic(Quaternion rayRotation, Vector3 gravityForward, Vector3 gyro)
        {
            float pitch = 0;
            if (gravityForward.y < 0 && gravityForward.z < 0)
            {
                pitch = rayRotation.eulerAngles.x - 360;
            }
            else if (gravityForward.y < 0 && gravityForward.z > 0)
            {
                pitch = 180 - rayRotation.eulerAngles.x;
            }
            else if (gravityForward.y > 0 && gravityForward.z < 0)
            {
                pitch = rayRotation.eulerAngles.x;
            }
            else
            {
                pitch = 180 - rayRotation.eulerAngles.x;
            }
            pitch = Mathf.Clamp(pitch, -180, 180);
            float deltaPitch = prePitch - pitch;
            prePitch = pitch;
            float pitchSpeed = deltaPitch / Time.deltaTime;

            if (Mathf.Abs(gravityForward.x) < 0.7f && CanSwipe())
            {
                if (Mathf.Abs(gyro.x) > maxGyro)
                {
                    maxGyro = Mathf.Abs(gyro.x);
                }
                if (pitchSpeed > 0 && pitchSpeed < 1000)
                {
                    //向上移动
                    if (pitch < minPitch)
                    {
                        minPitch = Mathf.Clamp(pitch, -90, 0);
                        resetPassTime = 0;
                        deltaTime = 0;
                    }
                }
                deltaTime += Time.deltaTime;
                if (Mathf.Abs(gyro.x) < 1.5f && deltaTime > 0.3f)
                {
                    //向下移动
                    maxPitch = pitch;
                    if (minPitch < 0 && maxPitch < 50 && maxPitch - minPitch > triggerYawThreshold && maxGyro > 12)
                    {
                        minPitch = maxPitch = 0;
                        resetPassTime = 0;
                        deltaTime = 0;
                        maxGyro = 0;
                        OnSuccess?.Invoke();
                    }
                }
                resetPassTime += Time.deltaTime;
                if (resetPassTime > resetTime)
                {
                    minPitch = maxPitch = 0;
                    resetPassTime = 0;
                    maxGyro = 0;
                }
            }
            else
            {
                minPitch = maxPitch = 0;
                resetPassTime = 0;
                maxGyro = 0;
            }
        }
    }
}
