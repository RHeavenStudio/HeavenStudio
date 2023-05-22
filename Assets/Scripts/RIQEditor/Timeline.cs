using System;
using System.Collections;
using System.Collections.Generic;
using Starpelly;
using UnityEngine;
using UnityEngine.UI;

namespace HeavenStudio.RIQEditor
{
    public class Timeline : MonoBehaviour
    {
        public float zoom { get; private set; }
        private float lastZoom;
        
        public float timelineWidth { get; private set; }
        public float pixelsPerBeat { get; private set; }
        
        [SerializeField] private ScrollRect RealScrollRect;
        [SerializeField] private RectTransform ScrollRectContent;
        [SerializeField] private RectTransform Viewport;
        [SerializeField] private RectTransform Content;
        [SerializeField] private RectTransform TimebarBG;

        [Header("Beats")]
        [SerializeField] private RectTransform BeatsHolder;
        [SerializeField] private RectTransform BeatLine;
        [SerializeField] private RectTransform PlaybackSlider;
        private List<RectTransform> beatLines = new();

        private void Start()
        {
        }

        private void Update()
        {
            zoom = RealScrollRect.content.localScale.x;
            
            pixelsPerBeat = 100 * zoom;
            
            timelineWidth = Viewport.rect.width;
            
            ScrollRectContent.anchoredPosition = RealScrollRect.content.anchoredPosition;
            TimebarBG.anchoredPosition = new Vector2(-ScrollRectContent.anchoredPosition.x, TimebarBG.anchoredPosition.y);
            
            BeatsHolder.anchoredPosition = BeatsHolder.anchoredPosition.ModifyX(Mathp.Round2Nearest(-Content.anchoredPosition.x, pixelsPerBeat));
            
            if (zoom != lastZoom)
            {
                OnZoom();
            }
            lastZoom = zoom;
        }
        
        public void OnZoom()
        {
            foreach (var t in beatLines)
                Destroy(t.gameObject);

            beatLines.Clear();

            var newLineCount = Mathf.RoundToInt(timelineWidth / pixelsPerBeat) + 2;

            for (int i = 0; i < newLineCount; i++)
            {
                var line = Instantiate(BeatLine, BeatsHolder);
                line.gameObject.SetActive(true);
                line.anchoredPosition = new Vector2((i * pixelsPerBeat) - 2, line.anchoredPosition.y);
                beatLines.Add(line.GetComponent<RectTransform>());
            }
        }
    }
}
