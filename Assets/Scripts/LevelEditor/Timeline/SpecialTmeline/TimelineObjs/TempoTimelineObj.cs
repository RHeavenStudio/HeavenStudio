using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class TempoTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text tempoTXT;
        [SerializeField] private GameObject tempoLine;

        public DynamicBeatmap.TempoChange tempoChange;

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
                        newTempo /= 100f;

                    tempoChange.tempo += newTempo;

                    //make sure tempo is positive
                    if (tempoChange.tempo < 1)
                        tempoChange.tempo = 1;
                }
            }

            UpdateTempo();
        }

        private void UpdateTempo()
        {
            tempoTXT.text = $"{tempoChange.tempo} BPM";
            Timeline.instance.FitToSong();
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
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
            {
                GameManager.instance.Beatmap.tempoChanges.Remove(tempoChange);
                DeleteObj();
            }
        }

        public override bool OnMove(float beat)
        {
            foreach (var tempoChange in GameManager.instance.Beatmap.tempoChanges)
            {
                if (this.tempoChange == tempoChange)
                    continue;
                if (beat > tempoChange.beat - Timeline.instance.snapInterval && beat < tempoChange.beat + Timeline.instance.snapInterval)
                    return false;
            }
            this.tempoChange.beat = beat;
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