using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class TimelineScrollRect : ScrollRect, IPointerDownHandler
    {
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnEndDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnDrag(eventData);
        }

        public override void OnScroll(PointerEventData data)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
        }
    }
}