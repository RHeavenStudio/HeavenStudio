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
        
        /// <summary>
        /// The time at the start rect of the timeline.
        /// </summary>
        public float timeLeft { get; private set; }
        /// <summary>
        /// The time at the end rect of the timeline.
        /// </summary>
        public float timeRight { get; private set; }

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

        [Header("Public Components")] 
        public TimelineZoom TimelineZoom;

        private void Start()
        {
            UpdateImportant();
            UpdatePerBeat();
            CalculateLeftRight();
            
            OnZoom(zoom);
        }

        public void Load()
        {
            BlockManager.Load();
        }

        private void Update()
        {
            UpdateImportant();
            UpdatePerBeat();
            CalculateLeftRight();
            
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

            for (var i = 0; i < beatLines.Count; i++)
            {
                var line = beatLines[i];
                line.anchoredPosition = new Vector2((i * pixelsPerBeat) - 2, line.anchoredPosition.y);
            }

            PlaybackSlider.anchoredPosition =
                PlaybackSlider.anchoredPosition.ModifyX(Conductor.instance.songPositionInBeats * pixelsPerBeat);
            PlaybackBeatTXT.text = Conductor.instance.songPositionInBeats.ToString("F");

            BeatsHolder.anchoredPosition = BeatsHolder.anchoredPosition.ModifyX(Mathp.Round2Nearest(-Content.anchoredPosition.x, pixelsPerBeat));

            BlockManager.UpdateBlockManager();
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
        
        private void CalculateLeftRight()
        {
            timeLeft = -Content.anchoredPosition.x / pixelsPerBeat;
            timeRight = (-Content.anchoredPosition.x + Viewport.rect.size.x) / pixelsPerBeat;
        }

        
        public void OnZoom(float zoom)
        {
            foreach (var t in beatLines)
                Destroy(t.gameObject);

            beatLines.Clear();

            var newLineCount = Mathf.RoundToInt(timelineWidth / (100 * zoom)) + 2;

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
