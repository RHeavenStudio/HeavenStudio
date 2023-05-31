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
        public Vector2 relativeMousePos;
        
        public float timelineX { get; private set; }
        public float timelineWidth { get; private set; }
        public float timelineHeight { get; private set; }
        public float pixelsPerBeat { get; private set; }
        
        public float mousePos2Beat { get; private set; }
        public int mousePos2Layer { get; private set; }
        
        /// <summary>
        /// The time at the start rect of the timeline.
        /// </summary>
        public float timeLeft { get; private set; }
        /// <summary>
        /// The time at the end rect of the timeline.
        /// </summary>
        public float timeRight { get; private set; }

        public int layerCount = 10;
        
        private bool clickedInTimebar;
        
        private float lastPlayBeat;

        [SerializeField] private TimelineBlockManager BlockManager;
        
        [SerializeField] private ScrollRect RealScrollRect;
        [SerializeField] private RectTransform ScrollRectContent;
        [SerializeField] private RectTransform Viewport;
        [SerializeField] private RectTransform Content;
        [SerializeField] private RectTransform TimelineContent; // Holds things like Blocks and Nodes
        [SerializeField] private RectTransform TimebarBG;
        [SerializeField] private RectTransform LayerBG;
        
        public RectTransform BlocksHolder;
        public RectTransform FullHeightBlocksHolder;

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

            for (var i = 0; i < layerCount; i++)
            {
                var layer = Instantiate(LayerBG, LayerBG.transform.parent);
                layer.GetComponent<RawImage>().enabled = (i % 2 == 0);
            }
            LayerBG.gameObject.SetActive(false);
        }

        public void Load()
        {
            BlockManager.Load();
        }

        private void LateUpdate()
        {
            UpdateImportant();
            UpdatePerBeat();
            CalculateLeftRight();
            
            // RealScrollRect.content.sizeDelta = RealScrollRect.content.sizeDelta.ModifyX(timeRight * pixelsPerBeat);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayCheck();
            }

            if (Input.GetMouseButtonDown(0))
            {
                clickedInTimebar = false;
                if (EditorMain.MouseInRectTransform(TimebarBG) && EditorMain.MouseInRectTransform(Viewport))
                {
                    clickedInTimebar = true;
                }
            }

            if (clickedInTimebar && Input.GetMouseButton(0))
            {
                if (mousePos2Beat != lastPlayBeat)
                    Conductor.instance.SetBeat(mousePos2Beat);

                lastPlayBeat = mousePos2Beat;
            }


            PlaybackSlider.anchoredPosition =
                PlaybackSlider.anchoredPosition.ModifyX(Conductor.instance.songPositionInBeats * pixelsPerBeat);
            // PlaybackBeatTXT.text = Conductor.instance.songPositionInBeats.ToString("F");

            var beatsHolderX = Mathp.Round2Nearest(-Content.anchoredPosition.x, pixelsPerBeat);
            BeatsHolder.anchoredPosition = BeatsHolder.anchoredPosition.ModifyX(beatsHolderX);

            for (var i = 0; i < beatLines.Count; i++)
            {
                var line = beatLines[i];
                line.anchoredPosition = new Vector2(i * pixelsPerBeat, line.anchoredPosition.y);
                line.transform.GetChild(0).GetComponent<TMP_Text>().text = (i + (beatsHolderX / pixelsPerBeat)).ToString("F1");
            }

            BlockManager.UpdateBlockManager();
        }

        private void UpdateImportant()
        {
            zoom = RealScrollRect.content.localScale.x;
            
            // RealScrollRect.content.anchoredPosition = RealScrollRect.content.anchoredPosition.ModifyX(Mathf.Clamp(RealScrollRect.content.anchoredPosition.x, -Mathf.Infinity, 0));
            ScrollRectContent.anchoredPosition = RealScrollRect.content.anchoredPosition;
            TimebarBG.anchoredPosition = new Vector2(-ScrollRectContent.anchoredPosition.x, TimebarBG.anchoredPosition.y);
        }

        private void UpdatePerBeat()
        {
            pixelsPerBeat = 100 * zoom;
            timelineX = -Content.anchoredPosition.x;
            timelineWidth = Viewport.rect.width;
            timelineHeight = TimelineContent.rect.height;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Content, Input.mousePosition,
                EditorMain.Instance.EditorCamera, out relativeMousePos);

            mousePos2Beat = Mathp.Round2Nearest((relativeMousePos.x / pixelsPerBeat) + (0.5f), 1.0f);
            mousePos2Layer = Mathf.FloorToInt(-(relativeMousePos.y + TimebarBG.rect.height) / LayerHeight());
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
                line.anchoredPosition = new Vector2(i * pixelsPerBeat, line.anchoredPosition.y);
                beatLines.Add(line.GetComponent<RectTransform>());
            }
            BeatLine.gameObject.SetActive(false);
        }
        
        #region Custom
        
        public void PlayCheck()
        {
            if (Conductor.instance.isPlaying)
            {
                GameManager.instance.Pause();
                if (!Input.GetKey(KeyCode.LeftShift))
                    Conductor.instance.SetBeat(lastPlayBeat);
            }
            else
            {
                lastPlayBeat = Conductor.instance.songPositionInBeats;
                GameManager.instance.Play(lastPlayBeat);
            }
        }

        public float LayerHeight()
        {
            return TimelineContent.rect.size.y / layerCount;
        }
        
        public float TimebarHeight()
        {
            return TimebarBG.rect.height;
        }

        public float LayerToY(int layer)
        {
            return -(layer * LayerHeight());
        }

        #endregion
    }
}
