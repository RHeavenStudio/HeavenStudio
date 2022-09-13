using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using TMPro;
using Starpelly;

namespace HeavenStudio.Editor.Track
{
    public class SpecialTimeline : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform RefTempoChange;
        [SerializeField] private RectTransform RefVolumeChange;
        [SerializeField] private RectTransform RefSectionChange;

        [Header("Components")]
        private RectTransform rectTransform;

        public List<SpecialTimelineObj> specialTimelineObjs = new List<SpecialTimelineObj>();

        [System.Flags]
        public enum HoveringTypes
        {
            TempoChange = 1,
            VolumeChange = 2,
            SectionChange = 4,
        }
        public static HoveringTypes hoveringTypes = 0;

        private bool firstUpdate;

        public static SpecialTimeline instance;

        private void Start()
        {
            instance = this;
            rectTransform = this.GetComponent<RectTransform>();

            Setup();
        }

        public void Setup()
        {
            ClearSpecialTimeline();

            foreach (var tempoChange in GameManager.instance.Beatmap.tempoChanges)
                AddTempoChange(false, tempoChange);

            foreach (var volumeChange in GameManager.instance.Beatmap.volumeChanges)
                AddVolumeChange(false, volumeChange);

            Timeline.instance.timelineState.SetState(Timeline.CurrentTimelineState.State.Selection);
            FixObjectsVisibility();
        }

        private void Update()
        {
            if (!firstUpdate)
            {
                hoveringTypes = 0;
                firstUpdate = true;
            }

            if (Timeline.instance.userIsEditingInputField)
                return;

            if (!Conductor.instance.NotStopped())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Editor.instance.EditorCamera))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        switch (Timeline.instance.timelineState.currentState)
                        {
                            case Timeline.CurrentTimelineState.State.TempoChange:
                                if (!hoveringTypes.HasFlag(HoveringTypes.TempoChange))
                                    AddTempoChange(true);
                                break;
                            case Timeline.CurrentTimelineState.State.MusicVolume:
                                if (!hoveringTypes.HasFlag(HoveringTypes.VolumeChange))
                                    AddVolumeChange(true);
                                break;
                        }
                    }
                }
            }
            hoveringTypes = 0;
        }

        public void FixObjectsVisibility()
        {
            foreach (SpecialTimelineObj obj in specialTimelineObjs)
            {
                obj.SetVisibility(Timeline.instance.timelineState.currentState);
            }
        }

        public void ClearSpecialTimeline()
        {
            foreach (SpecialTimelineObj obj in specialTimelineObjs)
            {
                obj.DeleteObj();
            }
            specialTimelineObjs.Clear();
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
            tempoTimelineObj.SetVisibility(Timeline.instance.timelineState.currentState);

            specialTimelineObjs.Add(tempoTimelineObj);

            Timeline.instance.FitToSong();
        }

        public void AddVolumeChange(bool create, DynamicBeatmap.VolumeChange volumeChange_ = null)
        {      
            GameObject volumeChange = Instantiate(RefVolumeChange.gameObject, this.transform);

            volumeChange.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();
            volumeChange.transform.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();
            volumeChange.transform.GetChild(2).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();

            volumeChange.SetActive(true);

            VolumeTimelineObj volumeTimelineObj = volumeChange.GetComponent<VolumeTimelineObj>();

            if (create == true)
            {
                volumeChange.transform.position = new Vector3(Editor.instance.EditorCamera.ScreenToWorldPoint(Input.mousePosition).x + 0.08f, volumeChange.transform.position.y);
                volumeChange.transform.localPosition = new Vector3(Starpelly.Mathp.Round2Nearest(volumeChange.transform.localPosition.x, Timeline.SnapInterval()), volumeChange.transform.localPosition.y);

                DynamicBeatmap.VolumeChange volumeC = new DynamicBeatmap.VolumeChange();
                volumeC.beat = volumeChange.transform.localPosition.x;
                volumeC.volume = GameManager.instance.Beatmap.musicVolume;

                volumeTimelineObj.volumeChange = volumeC;
                GameManager.instance.Beatmap.volumeChanges.Add(volumeC);
            }
            else
            {
                volumeChange.transform.localPosition = new Vector3(volumeChange_.beat, volumeChange.transform.localPosition.y);

                volumeTimelineObj.volumeChange = volumeChange_;
            }
            volumeTimelineObj.SetVisibility(Timeline.instance.timelineState.currentState);

            specialTimelineObjs.Add(volumeTimelineObj);
        }
    }
}
