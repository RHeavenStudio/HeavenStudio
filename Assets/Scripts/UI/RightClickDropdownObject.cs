using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavenStudio.Common
{
    public class RightClickDropdownObject : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Event[] events;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
        }

        private struct Event
        {
            public string name;
            public UnityEvent action;
        }
    }
}

