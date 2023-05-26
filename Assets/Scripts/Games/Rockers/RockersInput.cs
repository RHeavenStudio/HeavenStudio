using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockersInput : MonoBehaviour
    {
        private List<int> pitches = new List<int>();

        private bool gleeClub;

        private Rockers.PremadeSamples sample;
        private int sampleTones;

        private Rockers game;

        public void Init(bool gleeClub, int[] pitches, float beat, float length, Rockers.PremadeSamples sample, int sampleTones)
        {
            game = Rockers.instance;
            this.gleeClub = gleeClub;
            this.pitches = pitches.ToList();
            this.sample = sample;
            this.sampleTones = sampleTones;
            game.ScheduleInput(beat, length, InputType.STANDARD_UP, Just, Miss, Empty);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) 
            {
                Destroy(gameObject);
                return;
            }
            game.Soshi.StrumStrings(gleeClub, pitches.ToArray(), sample, sampleTones);
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            Destroy(gameObject);
        }

        private void Empty(PlayerActionEvent caller)
        {

        }
    }
}

