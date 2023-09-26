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

        public RiqEntity volumeChange;

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

                    volumeChange["volume"] += newVolume;

                    //make sure volume is positive
                    volumeChange["volume"] = Mathf.Clamp(volumeChange["volume"], 0, 100);

                    if (first && newVolume != 0)
                        Timeline.instance.UpdateStartingVolText();
                }
            }
            UpdateVolume();
        }

        private void UpdateVolume()
        {
            volumeTXT.text = $"{volumeChange["volume"].ToString("F")}%";
            SetX(volumeChange);
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
            if (first) return;
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
            {
                GameManager.instance.Beatmap.VolumeChanges.Remove(volumeChange);
                DeleteObj();
            }
        }

        public override bool OnMove(float beat)
        {
            foreach (var volumeChange in GameManager.instance.Beatmap.VolumeChanges)
            {
                if (this.volumeChange == volumeChange)
                    continue;
                if (beat > volumeChange.beat - Timeline.instance.snapInterval && beat < volumeChange.beat + Timeline.instance.snapInterval)
                    return false;
            }
            this.volumeChange.beat = beat;
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
    }
}