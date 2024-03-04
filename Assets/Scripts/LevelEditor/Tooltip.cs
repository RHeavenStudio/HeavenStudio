using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

namespace HeavenStudio.Editor
{
    public class Tooltip : MonoBehaviour
    {
        private RectTransform rectTransform;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform background;
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup group;

        public static Tooltip instance { get; private set; }

        private void Awake()
        {
            instance = this;
            rectTransform = GetComponent<RectTransform>();
            group.alpha = 0;
        }

        private void Update()
        {
            Vector3 anchoredPosition = Input.mousePosition;
            Camera camera = Editor.instance.EditorCamera;
            Vector3 canvasScale = Editor.instance.MainCanvas.transform.localScale;
            Vector2 scale = new Vector2(canvasScale.x, canvasScale.y);
            float toolTipScale = camera.pixelWidth / 1280f;

            if (anchoredPosition.x + background.rect.width * toolTipScale > camera.pixelWidth)
            {
                anchoredPosition.x = camera.pixelWidth - background.rect.width * toolTipScale;
            }
            if (anchoredPosition.x < -camera.pixelWidth)
            {
                anchoredPosition.x = -camera.pixelWidth;
            }

            if (anchoredPosition.y + background.rect.height * toolTipScale > camera.pixelHeight)
            {
                anchoredPosition.y = camera.pixelHeight - background.rect.height * toolTipScale;
            }
            if (anchoredPosition.y < -camera.pixelHeight)
            {
                anchoredPosition.y = -camera.pixelHeight;
            }

            anchoredPosition.z = camera.nearClipPlane;
            anchoredPosition = camera.ScreenToWorldPoint(anchoredPosition);
            rectTransform.anchoredPosition = anchoredPosition / scale;
        }

        public static void OnEnter(string tooltipText, string altTooltipText)
        {
            instance.OnEnterPrivate(tooltipText, altTooltipText);
        }

        public static void OnExit()
        {
            instance.OnExitPrivate();
            Editor.instance.tooltipText.text = "";
            Editor.instance.tooltipText.ForceMeshUpdate();
        }

        private void OnEnterPrivate(string tooltipText, string altTooltipText)
        {
            group.alpha = 1;

            text.text = tooltipText;
            text.ForceMeshUpdate();

            Vector2 textSize = text.GetRenderedValues(false);
            Vector2 paddingSize = new Vector2(8, 8);

            background.sizeDelta = textSize + paddingSize;
            Editor.instance.tooltipText.text = altTooltipText.Replace("\n", "");
            Editor.instance.tooltipText.ForceMeshUpdate();
        }

        private void OnExitPrivate()
        {
            group.alpha = 0;
        }

        private void SetText(string tooltipText)
        {
            text.text = tooltipText;
            text.ForceMeshUpdate();

            Vector2 textSize = text.GetRenderedValues(false);
            Vector2 paddingSize = new Vector2(8, 8);

            background.sizeDelta = textSize + paddingSize;
        }

        public static void AddTooltip(GameObject g, string tooltipText, string altTooltipText = null)
        {
            altTooltipText ??= tooltipText;

            EventTrigger et = g.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { OnEnter(tooltipText, altTooltipText); });

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnExit(); });

            et.triggers.Add(pointerEnter);
            et.triggers.Add(pointerExit);
        }
    }
}