using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using TMPro;
using Starpelly;

namespace HeavenStudio.Editor.Track
{
    public class TempoTimeline : MonoBehaviour
    {
        [Header("Components")]
        private RectTransform rectTransform;
        [SerializeField] private RectTransform RefTempoChange;
        public TMP_InputField StartingBPM;
        private RectTransform StartingBPMRect;
        public TMP_InputField FirstBeatOffset;

        public List<TempoTimelineObj> tempoTimelineObjs = new List<TempoTimelineObj>();

        private bool firstUpdate;

        private void Start()
        {
            rectTransform = this.GetComponent<RectTransform>();
            StartingBPMRect = StartingBPM.GetComponent<RectTransform>();

            for (int i = 0; i < GameManager.instance.Beatmap.tempoChanges.Count; i++)
            {
                DynamicBeatmap.TempoChange tempoChange = GameManager.instance.Beatmap.tempoChanges[i];
                AddTempoChange(false, tempoChange);
            }
        }

        private void Update()
        {
            if (!firstUpdate)
            {
                UpdateStartingBPMText();
                UpdateOffsetText();
                firstUpdate = true;
            }

            if (Timeline.instance.userIsEditingInputField)
                return;

            if (Timeline.instance.timelineState.tempoChange && !Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (tempoTimelineObjs.FindAll(c => c.hovering == true).Count == 0)
                        {
                            AddTempoChange(true);
                        }
                    }
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(StartingBPMRect, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    float increase = Input.mouseScrollDelta.y;
                    if (Input.GetKey(KeyCode.LeftControl))
                        increase /= 100f;
                    if (Input.GetKey(KeyCode.LeftShift))
                        increase *= 5f;

                    if (increase != 0f)
                    {
                        GameManager.instance.Beatmap.bpm += increase;
                        UpdateStartingBPMText();
                        UpdateStartingBPMFromText(); // In case the scrolled-to value is invalid.
                        
                    }
                }
            }
        }

        public void UpdateStartingBPMText()
        {
            StartingBPM.text = GameManager.instance.Beatmap.bpm.ToString("G");
        }

        public void UpdateOffsetText()
        {
            FirstBeatOffset.text = (GameManager.instance.Beatmap.firstBeatOffset * 1000f).ToString("G");
        }

        public void UpdateStartingBPMFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(StartingBPM.text))
                StartingBPM.text = "120";
            
            var newBPM = Convert.ToSingle(StartingBPM.text);

            // Failsafe against negative BPM.
            if (newBPM < 1f)
            {
                StartingBPM.text = "1";
                newBPM = 1;
            }

            // Limit decimal places to 4.
            newBPM = (float)System.Math.Round(newBPM, 4);

            GameManager.instance.Beatmap.bpm = newBPM;

            // In case the newBPM ended up differing from the inputted string.
            UpdateStartingBPMText();

            Timeline.instance.FitToSong();
        }

        public void UpdateOffsetFromText()
        {
            // Failsafe against empty string.
            if (String.IsNullOrEmpty(FirstBeatOffset.text))
                FirstBeatOffset.text = "0";
            
            // Convert ms to s.
            var newOffset = Convert.ToSingle(FirstBeatOffset.text) / 1000f;

            // Limit decimal places to 4.
            newOffset = (float)System.Math.Round(newOffset, 4);

            GameManager.instance.Beatmap.firstBeatOffset = newOffset;

            UpdateOffsetText();
        }

        public void ClearTempoTimeline()
        {
            foreach (TempoTimelineObj tempoTimelineObj in tempoTimelineObjs)
            {
                Destroy(tempoTimelineObj.gameObject);
            }
            tempoTimelineObjs.Clear();
        }

        public void AddTempoChange(bool create, DynamicBeatmap.TempoChange tempoChange_ = null)
        {      
            GameObject tempoChange = Instantiate(RefTempoChange.gameObject, this.transform);

            tempoChange.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();
            tempoChange.transform.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();
            tempoChange.transform.GetChild(2).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();

            tempoChange.SetActive(true);

            TempoTimelineObj tempoTimelineObj = tempoChange.GetComponent<TempoTimelineObj>();

            if (create == true)
            {
                tempoChange.transform.position = new Vector3(Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition).x + 0.08f, tempoChange.transform.position.y);
                tempoChange.transform.localPosition = new Vector3(Starpelly.Mathp.Round2Nearest(tempoChange.transform.localPosition.x, Timeline.SnapInterval()), tempoChange.transform.localPosition.y);

                DynamicBeatmap.TempoChange tempoC = new DynamicBeatmap.TempoChange();
                tempoC.beat = tempoChange.transform.localPosition.x;
                tempoC.tempo = GameManager.instance.Beatmap.bpm;

                tempoTimelineObj.tempoChange = tempoC;
                GameManager.instance.Beatmap.tempoChanges.Add(tempoC);
            }
            else
            {
                tempoChange.transform.localPosition = new Vector3(tempoChange_.beat, tempoChange.transform.localPosition.y);

                tempoTimelineObj.tempoChange = tempoChange_;
            }

            tempoTimelineObjs.Add(tempoTimelineObj);

            Timeline.instance.FitToSong();
        }
    }

}