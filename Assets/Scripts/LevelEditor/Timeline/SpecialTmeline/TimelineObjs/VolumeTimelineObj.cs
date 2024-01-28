using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Editor.Track
{
    public class VolumeTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text volumeTXT;
        [SerializeField] private GameObject volumeLine;
        [SerializeField] private VolumeDialog volumeDialog;

        new private void Update()
        {
            base.Update();
            if (hovering)
            {
                SpecialTimeline.hoveringTypes |= SpecialTimeline.HoveringTypes.VolumeChange;

                if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
                {
                    float newVolume = Input.mouseScrollDelta.y;

                    if (Input.GetKey(KeyCode.LeftShift))
                        newVolume *= 5f;
                    if (Input.GetKey(KeyCode.LeftControl))
                        newVolume *= 0.01f;

                    if (newVolume != 0)
                    {
                        SetVolume(chartEntity["volume"] + newVolume);
                        volumeDialog.RefreshDialog();
                    }
                }
            }
            UpdateVolume();
        }

        public void SetVolume(float volume)
        {
            chartEntity["volume"] = Mathf.Clamp(volume, 0, 100);
            if (first)
            {
                Timeline.instance.UpdateStartingVolText();
            }
            UpdateVolume();
        }

        private void UpdateVolume()
        {
            volumeTXT.text = $"{chartEntity["volume"].ToString("F")}%";
            if (!moving)
                SetX(chartEntity);
        }

        public override void Init()
        {
            UpdateVolume();
        }

        public override void OnLeftClick()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
                StartMove();
        }

        public override void OnRightClick()
        {
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
            {
                volumeDialog.SetVolumeObj(this);
                volumeDialog.SwitchVolumeDialog();
            }
        }

        public override bool OnMove(float beat, bool final = false)
        {
            if (beat < 0) beat = 0;
            foreach (var volumeChange in GameManager.instance.Beatmap.VolumeChanges)
            {
                if (this.chartEntity == volumeChange)
                    continue;
                if (beat > volumeChange.beat - Timeline.instance.snapInterval && beat < volumeChange.beat + Timeline.instance.snapInterval)
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
            if (state == Timeline.CurrentTimelineState.State.MusicVolume || state == Timeline.CurrentTimelineState.State.Selection)
            {
                gameObject.SetActive(true);
                if (state == Timeline.CurrentTimelineState.State.MusicVolume)
                    volumeLine.SetActive(true);
                else
                    volumeLine.SetActive(false);
            }
            else
                gameObject.SetActive(false);   
        }

        public void Remove()
        {
            if (first) return;
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
            {
                DeleteObj();
            }
        }
    }
}