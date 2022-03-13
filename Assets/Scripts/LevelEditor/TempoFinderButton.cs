using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HeavenStudio.Editor 
{
    public class TempoFinderButton : Button, IPointerDownHandler
    {
        public TempoFinder tempoFinder;
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                tempoFinder.TapBPM();
            }
        }
    }
}
