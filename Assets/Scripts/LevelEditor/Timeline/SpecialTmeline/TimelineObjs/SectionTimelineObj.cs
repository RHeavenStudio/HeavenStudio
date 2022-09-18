using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class SectionTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text sectionLabel;
        [SerializeField] private GameObject chartLine;
        [SerializeField] private SectionDialog sectionDialog;

        public DynamicBeatmap.ChartSection chartSection;

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
            sectionLabel.text = chartSection.sectionName;
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

        public override bool OnMove(float beat)
        {
            foreach (var sectionChange in GameManager.instance.Beatmap.beatmapSections)
            {
                if (this.chartSection == sectionChange)
                    continue;
                if (beat > sectionChange.beat - Timeline.instance.snapInterval && beat < sectionChange.beat + Timeline.instance.snapInterval)
                    return false;
            }
            this.chartSection.beat = beat;
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
    }
}