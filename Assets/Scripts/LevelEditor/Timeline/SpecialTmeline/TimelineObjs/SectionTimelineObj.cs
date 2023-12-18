using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;
using Jukebox;

namespace HeavenStudio.Editor.Track
{
    public class SectionTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text sectionLabel;
        [SerializeField] private GameObject chartLine;
        [SerializeField] private SectionDialog sectionDialog;

        new private void Update()
        {
            base.Update();
            if (hovering)
            {
                SpecialTimeline.hoveringTypes |= SpecialTimeline.HoveringTypes.SectionChange;
            }

            UpdateLabel();
        }

        public void UpdateLabel()
        {
            //<sprite="categoryMarker" name="cat0">
            if (string.IsNullOrEmpty(chartEntity["sectionName"]))
                sectionLabel.text = $"<sprite=\"categoryMarker\" name=\"cat{chartEntity["category"]}\"> x{chartEntity["weight"]:0}";
            else
                sectionLabel.text = $"<sprite=\"categoryMarker\" name=\"cat{chartEntity["category"]}\"> x{chartEntity["weight"]:0} | {chartEntity["sectionName"]}";
            if (!moving)
                SetX(chartEntity);
        }

        public override void Init()
        {
            UpdateLabel();
        }

        public override void OnLeftClick()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.ChartSection)
                StartMove();
        }

        public override void OnRightClick()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.ChartSection)
            {
                sectionDialog.SetSectionObj(this);
                sectionDialog.SwitchSectionDialog();
            }
        }

        public override bool OnMove(float beat, bool final = false)
        {
            if (beat < 0) beat = 0;
            foreach (RiqEntity sectionChange in GameManager.instance.Beatmap.SectionMarkers)
            {
                if (this.chartEntity == sectionChange)
                    continue;
                if (beat > sectionChange.beat - Timeline.instance.snapInterval && beat < sectionChange.beat + Timeline.instance.snapInterval)
                    return false;
            }
            if (final)
                CommandManager.Instance.AddCommand(new Commands.MoveMarker(chartEntity.guid, beat, type));
            else
                SetX(beat);
            return true;
        }

        public override void SetVisibility(Timeline.CurrentTimelineState.State state)
        {
            if (state == Timeline.CurrentTimelineState.State.ChartSection || state == Timeline.CurrentTimelineState.State.Selection)
            {
                gameObject.SetActive(true);
                if (state == Timeline.CurrentTimelineState.State.ChartSection)
                {
                    chartLine.SetActive(true);
                    sectionLabel.gameObject.SetActive(true);
                }
                else
                {
                    chartLine.SetActive(false);
                    sectionLabel.gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);

            }
        }

        public void Remove()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.ChartSection)
            {
                DeleteObj();
            }
        }
    }
}