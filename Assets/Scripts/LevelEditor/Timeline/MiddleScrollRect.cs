using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenStudio.Editor.Track
{
    public class MiddleScrollRect : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnEndDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;
            base.OnDrag(eventData);
        }
    }
}