using System.Collections.Generic;
using System.Globalization;
using Starpelly;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private float contentPosX => Mathf.Abs(scrollRect.content.localPosition.x / scrollRect.content.localScale.x);
        private float secPerBeat => 60.0f / GameManager.instance.Beatmap.bpm;

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

            var x = Mathf.FloorToInt(contentPosX);
            var pos = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
            transform.localPosition = pos;
            GetComponent<RectTransform>().anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0);

            UpdateCount();
        }

        #endregion

        private void UpdateCount()
        {
            var changeScale = (scrollRect.viewport.rect.size.x != lastTimelineSize || scrollRect.content.localScale.x != lastContentScale);
            if (changeScale)
            {
                var rightSide = (scrollRect.viewport.GetComponent<RectTransform>().rect.width / scrollRect.content.localScale.x) + contentPosX;

                for (int i = 0; i < Lines.Count; i++)
                    Destroy(Lines[i].gameObject);
                Lines.Clear();

                count = Mathf.RoundToInt(rightSide - contentPosX) + 2;

                for (int i = 0; i < count; i++)
                {
                    var line = Instantiate(transform.GetChild(0).gameObject, transform);
                    line.transform.localPosition = new Vector3(i, line.transform.localPosition.y, line.transform.localPosition.z);

                    var halfBeatRect = line.transform.GetChild(2).GetComponent<RectTransform>();
                    halfBeatRect.anchoredPosition = new Vector3(scrollRect.content.localScale.x * 0.5f, halfBeatRect.anchoredPosition.y);
                    
                    line.SetActive(true);

                    Lines.Add(line);
                }
                UpdateGridNum();

                if (count > 0) transform.GetChild(0).gameObject.SetActive(false);
            }
            if (scrollRect.content.anchoredPosition.x != lastPosX)
            {
                UpdateGridNum();
            }

            lastContentScale = scrollRect.content.localScale.x;
            lastTimelineSize = scrollRect.viewport.rect.size.x;
            lastPosX = scrollRect.content.anchoredPosition.x;
        }

        private void UpdateGridNum()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if (line.transform.childCount == 0) continue;

                var newNum = Mathf.RoundToInt(rectTransform.anchoredPosition.x + ((i) / snap));
                line.transform.GetChild(0).GetComponent<TMP_Text>().text = newNum.ToString(CultureInfo.CurrentCulture);
            }
        }
    }
}