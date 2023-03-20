using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor
{
    public class ButtonDoubleClick : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            var tap = eventData.clickCount;
            if (tap == 2)
            {
                onDoubleClick.Invoke();
            }
        }
    }
}
