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

        private bool jump;

        private Rockers game;

        public void Init(bool gleeClub, int[] pitches, double beat, double length, Rockers.PremadeSamples sample, int sampleTones, bool jump = false)
        {
            game = Rockers.instance;
            this.gleeClub = gleeClub;
            this.pitches = pitches.ToList();
            this.sample = sample;
            this.sampleTones = sampleTones;
            this.jump = jump;
            game.ScheduleInput(beat, length, InputType.STANDARD_UP, Just, Miss, Empty);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) 
            {
                game.Soshi.StrumStrings(gleeClub, pitches.ToArray(), sample, sampleTones, false, jump, true);
                Destroy(gameObject);
                return;
            }
            game.Soshi.StrumStrings(gleeClub, pitches.ToArray(), sample, sampleTones, false, jump);
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.JJ.Miss();
            Destroy(gameObject);
        }

        private void Empty(PlayerActionEvent caller)
        {

        }
    }
}

