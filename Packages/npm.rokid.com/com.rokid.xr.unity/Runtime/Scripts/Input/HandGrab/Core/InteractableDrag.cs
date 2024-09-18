using System;
using UnityEngine;
using UnityEngine.Events;

namespace Rokid.UXR.Interaction
{
    [Obsolete]
    public class InteractableDrag : MonoBehaviour, IHandHoverBegin, IHandHoverUpdate, IHandHoverEnd, IGrabbedToHand,
        IReleasedFromHand, IGrabbedUpdate
    {
        [Tooltip("The flags used to attach this object to the hand.")]
        public GrabFlags grabbedFlags = GrabFlags.ParentToHand | GrabFlags.ReleaseFromOtherHand | GrabFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform grabbedOffset;

        [Tooltip("When detaching the object, should it return to its original parent?")]
        public bool restoreOriginalParent = false;
        public UnityEvent OnPickUp;
        public UnityEvent OnDropDown;
        public UnityEvent OnHeldUpdate;

        [HideInInspector] public GrabInteractable interactable;

        protected virtual void Awake()
        {
            interactable = GetComponent<GrabInteractable>();
        }

        #region HandEvent

        private bool threeGesDragEnable = false;
        public void OnHandHoverBegin(Hand hand)
        {
        }

        public void OnHandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
            {
                hand.GrabObject(gameObject, startingGrabType, grabbedFlags, grabbedOffset);
            }
        }

        public void OnHandHoverEnd(Hand hand)
        {
        }

        public void OnGrabbedToHand(Hand hand)
        {
            var threeGesDrag = GetComponent<GestureDrag>();
            if (threeGesDrag)
            {
                threeGesDragEnable = threeGesDrag || threeGesDrag.enabled;
                threeGesDrag.enabled = false;
            }
            hand.HoverLock(null);
            OnPickUp?.Invoke();
        }

        public void OnGrabbedUpdate(Hand hand)
        {
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.ReleaseObject(gameObject, restoreOriginalParent);
            }

            OnHeldUpdate?.Invoke();
        }

        public void OnReleasedFromHand(Hand hand)
        {
            var threeGesDrag = GetComponent<GestureDrag>();
            if (threeGesDrag)
            {
                threeGesDrag.enabled = threeGesDragEnable;
            }
            hand.HoverUnlock(null);
            OnDropDown?.Invoke();
        }

        #endregion
    }

}

