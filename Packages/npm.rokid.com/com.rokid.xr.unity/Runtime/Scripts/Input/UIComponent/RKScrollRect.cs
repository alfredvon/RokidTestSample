using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 自定义RKScrollRect 拓展了近场抓取逻辑
    /// </summary>
    [Obsolete]
    public class RKScrollRect : UGUIScrollRect, IGesPinchBeginDrag, IGesPinchDrag, IGesPinchEndDrag
    {
        [SerializeField]
        private bool m_NearDragging = false;

        public void OnGesPinchBeginDrag(PointerEventData eventData)
        {
            // RKLog.Debug($"====RKScrollRect==== OnNearGesBeginDrag");
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = viewRect.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_NearDragging = true;
            m_Dragging = true;
        }

        public void OnGesPinchDrag(PointerEventData eventData)
        {
            if (!m_NearDragging)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            localCursor = viewRect.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            // RKLog.Debug($"====RKScrollRect==== OnNearDrag");
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }
            SetContentAnchoredPosition(position);
        }



        public void OnGesPinchEndDrag(PointerEventData eventData)
        {
            // RKLog.Debug("====RKScrollRect==== OnNearGesEndDrag");
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            m_NearDragging = false;
            m_Dragging = false;
        }

        public override void OnScroll(PointerEventData data)
        {
            if (m_NearDragging)
                return;
            base.OnScroll(data);
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (m_NearDragging)
                return;
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (m_NearDragging)
                return;
            // RKLog.Debug($"====RKScrollRect==== OnBeginDrag");
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (m_NearDragging)
                return;
            // RKLog.Debug($"====RKScrollRect==== OnDrag");
            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (m_NearDragging)
                return;
            // RKLog.Debug($"====RKScrollRect==== OnEndDrag");
            base.OnEndDrag(eventData);
        }
    }
}
