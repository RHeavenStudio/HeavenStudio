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

        public DynamicBeatmap.VolumeChange volumeChange;

        private void Update()
        {
            if (hovering)
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

        public override void OnRightClick()
        {
            GameManager.instance.Beatmap.volumeChanges.Remove(volumeChange);
            DeleteObj();
        }
    }
}