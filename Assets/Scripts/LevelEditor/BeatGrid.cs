using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;

namespace HeavenStudio.Editor.Track
{
    public class BeatGrid : MonoBehaviour
    {
        #region Public

        public float snap;
        public float count;

        #endregion

        #region Private

        private RectTransform rectTransform;

        private float lastPosX;
        private float lastContentScale;
        private float lastTimelineSize;
        private float lastZoom;

        private List<GameObject> Lines = new List<GameObject>();


        #endregion

        #region Exposed

        [Header("Components")]
        [SerializeField] private ScrollRect scrollRect;

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            UpdateCount();
        }

        private void LateUpdate()
        {
            if (Editor.instance.fullscreen) return;

            // var x = MathUtils.Round2Nearest(contentPosX, Timeline.instance.PixelsPerBeat);
            var x = MathUtils.Round2Nearest(-scrollRect.content.anchoredPosition.x, Timeline.instance.PixelsPerBeat);
            var pos = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
            transform.localPosition = pos;
            rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, transform.parent.parent.parent.GetComponent<RectTransform>().rect.height);

            UpdateCount();
        }

        #endregion

        private void UpdateCount()
        {
            var changeScale = (scrollRect.viewport.rect.size.x != lastTimelineSize || scrollRect.content.localScale.x != lastContentScale) || 
                (Timeline.instance.Zoom != lastZoom);
            if (changeScale)
            {
                for (int i = 0; i < Lines.Count; i++)
                    Destroy(Lines[i].gameObject);
                Lines.Clear();

                count = Mathf.RoundToInt(scrollRect.viewport.GetComponent<RectTransform>().rect.width / Timeline.instance.PixelsPerBeat) + 2;

                for (int i = 0; i < count; i++)
                {
                    var line = Instantiate(transform.GetChild(0).gameObject.GetComponent<RectTransform>(), transform);
                    line.anchoredPosition = new Vector3(i * Timeline.instance.PixelsPerBeat, line.transform.localPosition.y, line.transform.localPosition.z);

                    var halfBeatRect = line.transform.GetChild(2).GetComponent<RectTransform>();
                    halfBeatRect.anchoredPosition = new Vector3(Timeline.instance.PixelsPerBeat * 0.5f, halfBeatRect.anchoredPosition.y);
                    
                    line.gameObject.SetActive(true);

                    Lines.Add(line.gameObject);
                }
                UpdateGridNum();

                if (count > 0) transform.GetChild(0).gameObject.SetActive(false);
            }
            if (rectTransform.anchoredPosition.x != lastPosX)
            {
                UpdateGridNum();
            }

            lastContentScale = scrollRect.content.localScale.x;
            lastTimelineSize = scrollRect.viewport.rect.size.x;
            lastZoom = Timeline.instance.Zoom;
            lastPosX = rectTransform.anchoredPosition.x;
        }

        private void UpdateGridNum()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if (line.transform.childCount == 0) continue;

                var newNum = Mathf.RoundToInt((rectTransform.anchoredPosition.x / Timeline.instance.PixelsPerBeat) + ((i) / snap));
                line.transform.GetChild(0).GetComponent<TMP_Text>().text = newNum.ToString(CultureInfo.CurrentCulture);
            }
        }
    }
}