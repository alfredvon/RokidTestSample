using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Gesture ray enters,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayPointerEnter instead")]
    public interface IGesPointerEnter
    {
        void OnGesPointerEnter();
    }

    /// <summary>
    /// Gesture ray exits,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayPointerExit instead")]
    public interface IGesPointerExit
    {
        void OnGesPointerExit();
    }

    /// <summary>
    /// Gesture Ray drag starts,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayDragBegin instead")]
    public interface IGesBeginDrag
    {
        void OnGesBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Gesture ray drag ends,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayEndDrag instead")]
    public interface IGesEndDragWithData
    {
        void OnGesEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Gesture ray drag ends,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayEndDrag instead")]
    public interface IGesEndDrag
    {
        void OnGesEndDrag();
    }

    /// <summary>
    /// Gesture ray drag,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayDragToTarget instead")]
    public interface IGesDrag
    {
        [Obsolete("Use IRayDragToTarget instead")]
        void OnGesDrag(Vector3 delta);
    }

    /// <summary>
    /// Gesture ray drag,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IRayDragToTarget instead")]
    public interface IGesDragToTarget
    {
        void OnGesDragToTarget(Vector3 targetPos);
    }

    /// <summary>
    /// The gesture ray is hovering and the grip or pinch type is triggered
    /// </summary>
    [Obsolete("Use IRayPointerHover instead")]
    public interface IGesPointerHover
    {
        void OnGesPointerHover(RaycastResult result);
    }

    /// <summary>
    /// Gesture ray click,grip or pinch gesture type trigger
    /// </summary>
    [Obsolete("Use IRayPointerClick instead")]
    public interface IGesPointerClick
    {
        void OnGesPointerClick();
    }
}
