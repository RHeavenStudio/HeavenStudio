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

                    chartEntity["volume"] += newVolume;

                    //make sure volume is positive
                    chartEntity["volume"] = Mathf.Clamp(chartEntity["volume"], 0, 100);

                    if (first && newVolume != 0)
                        Timeline.instance.UpdateStartingVolText();
                }
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
            if (first) return;
            if (Timeline.instance.timelineState.currentState == Timeline.CurrentTimelineState.State.MusicVolume)
            {
                DeleteObj();
            }
        }

        public override bool OnMove(float beat, bool final = false)
        {
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
    }
}