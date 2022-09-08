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

        public DynamicBeatmap.TempoChange tempoChange;

        private void Update()
        {
            if (hovering)
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

        public override void OnRightClick()
        {
            GameManager.instance.Beatmap.tempoChanges.Remove(tempoChange);
            DeleteObj();
        }
    }
}