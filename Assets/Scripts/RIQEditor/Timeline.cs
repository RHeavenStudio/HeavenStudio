using System;
using System.Collections;
using System.Collections.Generic;
using Starpelly;
using TMPro;
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

        public int layerCount = 10;

        [SerializeField] private TimelineBlockManager BlockManager;
        
        [SerializeField] private ScrollRect RealScrollRect;
        [SerializeField] private RectTransform ScrollRectContent;
        [SerializeField] private RectTransform Viewport;
        [SerializeField] private RectTransform Content;
        [SerializeField] private RectTransform TimelineContent; // Holds things like Blocks and Nodes
        [SerializeField] private RectTransform TimebarBG;

        [Header("Beats")]
        [SerializeField] private RectTransform BeatsHolder;
        [SerializeField] private RectTransform BeatLine;
        [SerializeField] private RectTransform PlaybackSlider;
        [SerializeField] private TMP_Text PlaybackBeatTXT;
        private List<RectTransform> beatLines = new();

        public void Load()
        {
            BlockManager.Load();
        }

        private void Update()
        {
            UpdateImportant();
            UpdatePerBeat();
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    PlayCheck(false);
                }
                else
                {
                    PlayCheck(true);
                }
            }

            PlaybackSlider.anchoredPosition =
                PlaybackSlider.anchoredPosition.ModifyX(Conductor.instance.songPositionInBeats * pixelsPerBeat);
            PlaybackBeatTXT.text = Conductor.instance.songPositionInBeats.ToString("F");

            BeatsHolder.anchoredPosition = BeatsHolder.anchoredPosition.ModifyX(Mathp.Round2Nearest(-Content.anchoredPosition.x, pixelsPerBeat));
            
            if (zoom != lastZoom)
            {
                OnZoom();
            }
            lastZoom = zoom;
        }

        private void UpdateImportant()
        {
            zoom = RealScrollRect.content.localScale.x;
            
            RealScrollRect.content.anchoredPosition =
                RealScrollRect.content.anchoredPosition.ModifyX(Mathf.Clamp(RealScrollRect.content.anchoredPosition.x,
                    -Mathf.Infinity, 0));
            ScrollRectContent.anchoredPosition = RealScrollRect.content.anchoredPosition;
            TimebarBG.anchoredPosition = new Vector2(-ScrollRectContent.anchoredPosition.x, TimebarBG.anchoredPosition.y);
        }

        private void UpdatePerBeat()
        {
            pixelsPerBeat = 100 * zoom;
            timelineWidth = Viewport.rect.width;
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
        
        #region PlayChecks
        public void PlayCheck(bool fromStart)
        {
            if (fromStart)
            {
                if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                {
                    Play(false, PlaybackSlider.anchoredPosition.x);
                }
                else
                {
                    Stop(PlaybackSlider.anchoredPosition.x);
                }
                    
            }
            else
            {
                if (!Conductor.instance.isPlaying)
                {
                    Play(false, PlaybackSlider.anchoredPosition.x);
                }
                else if (!Conductor.instance.isPaused)
                {
                    Pause();
                }
            }
        }

        public void Play(bool fromStart, float time)
        {
            GameManager.instance.Play(time);
        }

        public void Pause()
        {
            GameManager.instance.Pause();
        }

        public void Stop(float time)
        {
            GameManager.instance.Stop(time);
        }
        #endregion

        #region Custom

        public float LayerHeight()
        {
            return TimelineContent.rect.size.y / layerCount;
        }

        public float LayerToY(int layer)
        {
            return -(layer * LayerHeight());
        }

        #endregion
    }
}
