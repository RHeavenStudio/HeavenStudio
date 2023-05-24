using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockerBendInput : MonoBehaviour
    {
        private List<int> pitches = new List<int>();

        private bool G5;

        private Rockers game;

        public void Init(bool G5, int[] pitches, float beat, float length)
        {
            game = Rockers.instance;
            this.G5 = G5;
            this.pitches = pitches.ToList();
            game.ScheduleInput(beat, length, InputType.DIRECTION_DOWN, Just, Miss, Empty);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Destroy(gameObject);
                return;
            }
            game.Soshi.BendUp(G5, pitches.ToArray());
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


