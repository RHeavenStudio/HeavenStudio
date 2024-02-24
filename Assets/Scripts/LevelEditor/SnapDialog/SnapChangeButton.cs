using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor 
{
    public class SnapChangeButton : Button, IPointerDownHandler
    {
        public SnapDialog SnapDialog;
        public bool isDown;

        // public override void OnPointerDown(PointerEventData eventData)
        // {
        //     if (eventData.button == PointerEventData.InputButton.Left)
        //     {
        //         SnapDialog.ChangeCommon(isDown);
        //     }
        // }
    }
}
