using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DG.Tweening;

namespace HeavenStudio.Editor.Track
{
    public class VolumeTimelineObj : SpecialTimelineObj
    {
        [Header("Components")]
        [SerializeField] private TMP_Text volumeTXT;
        [SerializeField] private GameObject volumeLine;

        public DynamicBeatmap.VolumeChange volumeChange;

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
                        newVolume /= 100f;

                    volumeChange.volume += newVolume;

                    //make sure volume is positive
                    volumeChange.volume = Mathf.Clamp(volumeChange.volume, 0, 100);
                }
            }

            UpdateVolume();
        }

        private void UpdateVolume()
        {
            volumeTXT.text = $"{volumeChange.volume}%";
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
                GameManager.instance.Beatmap.volumeChanges.Remove(volumeChange);
                DeleteObj();
            }
        }

        public override bool OnMove(float beat)
        {
            foreach (var volumeChange in GameManager.instance.Beatmap.volumeChanges)
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