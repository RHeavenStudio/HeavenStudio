using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;
using HeavenStudio.Common;

namespace HeavenStudio.Deprecated
{
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        public void OnEnter(string tooltipText)
        {
            this.GetComponent<Image>().enabled = true;
            this.transform.GetChild(0).GetComponent<TMP_Text>().text = tooltipText;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        public void OnExit()
        {
            this.GetComponent<Image>().enabled = false;
            this.transform.GetChild(0).gameObject.SetActive(false);
        }

        public void AddTooltip(GameObject g, string tooltipText)
        {
            EventTrigger et = g.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { OnEnter(tooltipText); });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnExit(); });

            et.triggers.Add(pointerEnter);
            et.triggers.Add(pointerExit);
        }
    }
}