using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

namespace HeavenStudio.Common
{
    public class LeftClickTMP_Dropdown : TMP_Dropdown
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) return;
            base.OnPointerClick(eventData);
        }
    }
}

