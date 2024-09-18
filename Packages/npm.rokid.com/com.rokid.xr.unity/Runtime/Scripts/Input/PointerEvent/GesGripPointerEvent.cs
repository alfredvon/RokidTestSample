using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Gesture ray enters,grip gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesPointerEnter instead")]
    public interface IGesGripPointerEnter
    {
        void OnGesGripPointerEnter();
    }
    /// <summary>
    /// Gesture ray exits,grip gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesPointerExit instead")]
    public interface IGesGripPointerExit
    {
        void OnGesGripPointerExit();
    }
    /// <summary>
    /// Gesture Ray drag starts,grip gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesBeginDrag instead")]
    public interface IGesGripBeginDrag
    {
        void OnGesGripBeginDrag(PointerEventData eventData);
    }
    /// <summary>
    /// Gesture ray drag ends,grip gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesEndDrag instead")]
    public interface IGesGripEndDrag
    {
        void OnGesGripEndDrag();
    }
    /// <summary>
    /// Gesture ray drag,grip gesture type triggers
    /// </summary>

    [Obsolete("Please use IGesDrag instead")]
    public interface IGesGripDrag
    {
        void OnGesGripDrag(Vector3 delta);
    }
    /// <summary>
    /// The gesture ray is hovering and the grip type is triggered
    /// </summary>
    [Obsolete("Use RKPointerListener.OnPointerHover instead")]
    public interface IGesGripPointerHover
    {
        void OnGesGripPointerHover(RaycastResult result);
    }
    /// <summary>
    /// Gesture ray click,grip gesture type trigger
    /// </summary>
    [Obsolete]
    public interface IGesGripPointerClick
    {
        void OnGesGripPointerClick();
    }
}
