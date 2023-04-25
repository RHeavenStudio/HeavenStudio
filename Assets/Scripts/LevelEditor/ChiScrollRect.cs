using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenStudio.Editor
{
    // Thank you Unity forums!
    // https://forum.unity.com/threads/scroll-view-does-not-scroll-with-mousewheel-when-mouse-is-over-a-button-inside-the-scroll-view.848848/
    
    public class ChiScrollRect : ScrollRect, IPointerEnterHandler, IPointerExitHandler
    {
        private static string mouseScrollWheelAxis = "Mouse ScrollWheel";
        private bool swallowMouseWheelScrolls = true;
        private bool isMouseOver = false;
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;
        }
    
        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
        }
    
        private void Update()
        {
            // Detect the mouse wheel and generate a scroll. This fixes the issue where Unity will prevent our ScrollRect
            // from receiving any mouse wheel messages if the mouse is over a raycast target (such as a button).
            if (isMouseOver && IsMouseWheelRolling())
            {
                var delta = UnityEngine.Input.GetAxis(mouseScrollWheelAxis);
    
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.scrollDelta = new Vector2(0f, delta * scrollSensitivity);
    
                swallowMouseWheelScrolls = false;
                OnScroll(pointerData);
                swallowMouseWheelScrolls = true;
            }
        }
    
        public override void OnScroll(PointerEventData data)
        {
            if (IsMouseWheelRolling() && swallowMouseWheelScrolls)
            {
                // Eat the scroll so that we don't get a double scroll when the mouse is over an image
            }
            else
            {
                base.OnScroll(data);
            }
        }
    
        private static bool IsMouseWheelRolling()
        {
            return UnityEngine.Input.GetAxis(mouseScrollWheelAxis) != 0;
        }
    }
}
