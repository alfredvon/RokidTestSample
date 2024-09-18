using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Triggered when the mouse enters
    /// </summary>
    [Obsolete("Use IRayPointerEnter instead")]
    public interface IMouseEnter
    {
        void OnMouseEnter();
    }

    /// <summary>
    /// Triggered when the mouse exits
    /// </summary>
    [Obsolete("Use IRayPointerExit instead")]
    public interface IMouseExit
    {
        void OnMouseExit();
    }

    /// <summary>
    /// Triggered when mouse drag starts
    /// </summary>
    [Obsolete("Use IRayDragBegin instead")]
    public interface IMouseBeginDrag
    {
        void OnMouseBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the mouse drag ends
    /// </summary>
    [Obsolete("Use IRayEndDrag instead")]
    public interface IMouseEndDragWithData
    {
        void OnMouseEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the mouse drag ends
    /// </summary>
    [Obsolete("Use IRayEndDrag instead")]
    public interface IMouseEndDrag
    {
        void OnMouseEndDrag();
    }

    /// <summary>
    /// Triggered when mouse dragging
    /// </summary>
    [Obsolete("Use IRayDrag instead")]
    public interface IMouseDrag
    {
        void OnMouseRayDrag(Vector3 delta);
    }

    /// <summary>
    /// Triggered when mouse hovering
    /// </summary>
    [Obsolete("Use IRayPointerHover instead")]
    public interface IMouseHover
    {
        void OnMouseHover();
    }

    /// <summary>
    /// Triggered when the mouse clicks
    /// </summary>
    [Obsolete("Use IRayPointerClick instead")]
    public interface IMouseClick
    {
        void OnMouseClick();
    }
}
