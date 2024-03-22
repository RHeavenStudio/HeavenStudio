using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeftClickEvent : Selectable
{
    public UnityEvent OnLeftClick;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    
        if(eventData.button != PointerEventData.InputButton.Left) return;
        OnLeftClick?.Invoke();
    }
}
