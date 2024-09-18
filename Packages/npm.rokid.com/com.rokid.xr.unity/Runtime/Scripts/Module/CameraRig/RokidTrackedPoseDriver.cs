using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Rokid.UXR.Module
{
    public class RokidTrackedPoseDriver : TrackedPoseDriver
    {
        [SerializeField]
        bool m_RotationExcludeRoll = false;

        public bool rotationExcludeRoll { get { return m_RotationExcludeRoll; } set { m_RotationExcludeRoll = value; } }


        /// <summary>
        /// Sets the transform that is being driven by the <see cref="TrackedPoseDriver"/>. will only correct set the rotation or position depending on the <see cref="PoseDataFlags"/>
        /// </summary>
        /// <param name="newPosition">The position to apply.</param>
        /// <param name="newRotation">The rotation to apply.</param>
        /// <param name="poseFlags">The flags indicating which of the position/rotation values are provided by the calling code.</param>
        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation, PoseDataFlags poseFlags)
        {
            if ((trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.RotationOnly) &&
                (poseFlags & PoseDataFlags.Rotation) > 0)
            {
                if (m_RotationExcludeRoll)
                {
                    Vector3 euler = newRotation.eulerAngles;
                    transform.localRotation = Quaternion.Euler(euler.x, euler.y, 0);
                }
                else
                {
                    transform.localRotation = newRotation;
                }
            }

            if ((trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.PositionOnly) &&
                (poseFlags & PoseDataFlags.Position) > 0)
            {
                transform.localPosition = newPosition;
            }
        }
    }
}
