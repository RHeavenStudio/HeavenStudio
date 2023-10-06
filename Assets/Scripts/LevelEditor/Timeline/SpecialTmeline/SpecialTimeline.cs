using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using TMPro;
using Starpelly;
using Jukebox;
using Jukebox.Legacy;

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

        public Dictionary<Guid, SpecialTimelineObj> specialTimelineObjs = new();

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
            GameManager.instance.SortEventsList();

            bool first = true;
            foreach (var tempoChange in GameManager.instance.Beatmap.TempoChanges)
            {
                AddTempoChange(false, tempoChange, first);
                first = false;
            }

            first = true;
            foreach (var volumeChange in GameManager.instance.Beatmap.VolumeChanges)
            {
                AddVolumeChange(false, volumeChange, first);
                first = false;
            }

            foreach (var sectionChange in GameManager.instance.Beatmap.SectionMarkers)
                AddChartSection(false, sectionChange);

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

            if (Timeline.instance.userIsEditingInputField || Editor.instance.inAuthorativeMenu)
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
                            case Timeline.CurrentTimelineState.State.ChartSection:
                                if (!hoveringTypes.HasFlag(HoveringTypes.SectionChange))
                                    AddChartSection(true);
                                break;
                        }
                    }
                }
            }
            hoveringTypes = 0;
        }

        public void FixObjectsVisibility()
        {
            foreach (SpecialTimelineObj obj in specialTimelineObjs.Values)
            {
                obj.SetVisibility(Timeline.instance.timelineState.currentState);
            }
        }

        public void ClearSpecialTimeline()
        {
            foreach (SpecialTimelineObj obj in specialTimelineObjs.Values)
            {
                Destroy(obj.gameObject);
            }
            specialTimelineObjs.Clear();
        }

        public void AddTempoChange(bool create, RiqEntity tempoChange_ = null, bool first = false)
        {
            if (create)
            {
                foreach (var e in GameManager.instance.Beatmap.TempoChanges)
                {
                    if (Timeline.instance.MousePos2BeatSnap > e.beat - Timeline.instance.snapInterval && Timeline.instance.MousePos2BeatSnap < e.beat + Timeline.instance.snapInterval)
                        return;
                }
            }

            GameObject tempoChange = Instantiate(RefTempoChange.gameObject, this.transform);

            tempoChange.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();
            tempoChange.transform.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();
            tempoChange.transform.GetChild(2).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.TempoLayerCol.Hex2RGB();

            tempoChange.SetActive(true);

            TempoTimelineObj tempoTimelineObj = tempoChange.GetComponent<TempoTimelineObj>();
            tempoTimelineObj.type = HoveringTypes.TempoChange;

            if (create)
            {
                float lastTempo = Conductor.instance.GetBpmAtBeat(tempoChange.transform.localPosition.x);
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    lastTempo *= 2f;
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    lastTempo /= 2f;
                }
                RiqEntity tempoC = GameManager.instance.Beatmap.AddNewTempoChange(Timeline.instance.MousePos2BeatSnap, lastTempo);
                tempoTimelineObj.chartEntity = tempoC;
                CommandManager.Instance.AddCommand(new Commands.AddMarker(tempoC, tempoC.guid, HoveringTypes.TempoChange));
            }
            else
            {
                tempoTimelineObj.chartEntity = tempoChange_;
                tempoTimelineObj.first = first;
            }
            tempoTimelineObj.SetVisibility(Timeline.instance.timelineState.currentState);

            specialTimelineObjs.Add(tempoTimelineObj.chartEntity.guid, tempoTimelineObj);

            Timeline.instance.FitToSong();
        }

        public void AddVolumeChange(bool create, RiqEntity volumeChange_ = null, bool first = false)
        {
            if (create)
            {
                foreach (var e in GameManager.instance.Beatmap.VolumeChanges)
                {
                    if (Timeline.instance.MousePos2BeatSnap > e.beat - Timeline.instance.snapInterval && Timeline.instance.MousePos2BeatSnap < e.beat + Timeline.instance.snapInterval)
                        return;
                }
            }

            GameObject volumeChange = Instantiate(RefVolumeChange.gameObject, this.transform);

            volumeChange.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();
            volumeChange.transform.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();
            volumeChange.transform.GetChild(2).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.MusicLayerCol.Hex2RGB();

            volumeChange.SetActive(true);

            VolumeTimelineObj volumeTimelineObj = volumeChange.GetComponent<VolumeTimelineObj>();
            volumeTimelineObj.type = HoveringTypes.VolumeChange;

            if (create)
            {
                RiqEntity volumeC = GameManager.instance.Beatmap.AddNewVolumeChange(Timeline.instance.MousePos2BeatSnap, 100f);
                volumeTimelineObj.chartEntity = volumeC;
                GameManager.instance.Beatmap.VolumeChanges.Add(volumeC);
                CommandManager.Instance.AddCommand(new Commands.AddMarker(volumeC, volumeC.guid, HoveringTypes.VolumeChange));
            }
            else
            {
                volumeTimelineObj.chartEntity = volumeChange_;
                volumeTimelineObj.first = first;
            }
            volumeTimelineObj.SetVisibility(Timeline.instance.timelineState.currentState);

            specialTimelineObjs.Add(volumeTimelineObj.chartEntity.guid, volumeTimelineObj);
        }

        public void AddChartSection(bool create, RiqEntity chartSection_ = null)
        {
            if (create)
            {
                foreach (var e in GameManager.instance.Beatmap.SectionMarkers)
                {
                    if (Timeline.instance.MousePos2BeatSnap > e.beat - Timeline.instance.snapInterval && Timeline.instance.MousePos2BeatSnap < e.beat + Timeline.instance.snapInterval)
                        return;
                }
            }

            GameObject chartSection = Instantiate(RefSectionChange.gameObject, this.transform);

            chartSection.transform.GetChild(0).GetComponent<Image>().color = EditorTheme.theme.properties.SectionLayerCol.Hex2RGB();
            chartSection.transform.GetChild(1).GetComponent<Image>().color = EditorTheme.theme.properties.SectionLayerCol.Hex2RGB();
            chartSection.transform.GetChild(2).GetComponent<Image>().color = EditorTheme.theme.properties.SectionLayerCol.Hex2RGB();
            chartSection.transform.GetChild(3).GetComponent<TMP_Text>().color = EditorTheme.theme.properties.SectionLayerCol.Hex2RGB();

            chartSection.SetActive(true);

            SectionTimelineObj sectionTimelineObj = chartSection.GetComponent<SectionTimelineObj>();
            sectionTimelineObj.type = HoveringTypes.SectionChange;

            if (create)
            {
                RiqEntity sectionC = GameManager.instance.Beatmap.AddNewSectionMarker(Timeline.instance.MousePos2BeatSnap, "New Section");

                sectionTimelineObj.chartEntity = sectionC;
                GameManager.instance.Beatmap.SectionMarkers.Add(sectionC);
                CommandManager.Instance.AddCommand(new Commands.AddMarker(sectionC, sectionC.guid, HoveringTypes.SectionChange));
            }
            else
            {
                sectionTimelineObj.chartEntity = chartSection_;
            }
            sectionTimelineObj.SetVisibility(Timeline.instance.timelineState.currentState);

            specialTimelineObjs.Add(sectionTimelineObj.chartEntity.guid, sectionTimelineObj);
            //auto-open the dialog
            if (create)
                sectionTimelineObj.OnRightClick();
        }
    }
}
