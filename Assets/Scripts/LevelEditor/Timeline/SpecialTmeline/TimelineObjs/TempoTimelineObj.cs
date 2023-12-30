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
        [SerializeField] private TempoDialog tempoDialog;

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

                    if (newTempo != 0)
                    {
                        SetTempo(chartEntity["tempo"] + newTempo);
                        tempoDialog.RefreshDialog();
                    }

                }
            }
            UpdateTempo();
        }

        public void SetTempo(float tempo)
        {
            chartEntity["tempo"] = Mathf.Clamp(tempo, 1, 10000);
            if (first)
            {
                Timeline.instance.UpdateStartingBPMText();
            }
            Timeline.instance.FitToSong();
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
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
            {
                tempoDialog.SetTempoObj(this);
                tempoDialog.SwitchTempoDialog();
            }
        }

        public override bool OnMove(float beat, bool final = false)
        {
            if (beat < 0) beat = 0;
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

        public void Remove()
        {
            if (first) return;
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.TempoChange)
            {
                DeleteObj();
            }
        }
    }
}