using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Editor.Track
{
    public class TempoTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text tempoTXT;
        [SerializeField] private GameObject tempoLine;

        new private void Update()
        {
            base.Update();
            if (hovering)
            {
                SpecialTimeline.hoveringTypes |= SpecialTimeline.HoveringTypes.TempoChange;
                if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
                {
                    float newTempo = Input.mouseScrollDelta.y;

                    if (Input.GetKey(KeyCode.LeftShift))
                        newTempo *= 5f;
                    if (Input.GetKey(KeyCode.LeftControl))
                        newTempo *= 0.01f;

                    chartEntity["tempo"] += newTempo;

                    //make sure tempo is positive
                    if (chartEntity["tempo"] < 1)
                        chartEntity["tempo"] = 1;
                    
                    if (first && newTempo != 0)
                        Timeline.instance.UpdateStartingBPMText();

                    Timeline.instance.FitToSong();
                }
            }

            UpdateTempo();
        }

        private void UpdateTempo()
        {
            tempoTXT.text = chartEntity["tempo"].ToString("F") + $" BPM";
            if (!moving)
                SetX(chartEntity);
        }

        public override void Init()
        {
            UpdateTempo();
        }

        public override void OnLeftClick()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
                StartMove();
        }

        public override void OnRightClick()
        {
            if (first) return;
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
            {
                DeleteObj();
            }
        }

        public override bool OnMove(float beat, bool final = false)
        {
            foreach (var tempoChange in GameManager.instance.Beatmap.TempoChanges)
            {
                if (this.chartEntity == tempoChange)
                    continue;
                if (beat > tempoChange.beat - Timeline.instance.snapInterval && beat < tempoChange.beat + Timeline.instance.snapInterval)
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
            if (state == Timeline.CurrentTimelineState.State.TempoChange || state == Timeline.CurrentTimelineState.State.Selection)
            {
                gameObject.SetActive(true);
                if (state == Timeline.CurrentTimelineState.State.TempoChange)
                    tempoLine.SetActive(true);
                else
                    tempoLine.SetActive(false);
            }
            else
                gameObject.SetActive(false);   
        }
    }
}