using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockerBendInput : MonoBehaviour
    {
        private int pitch;

        private Rockers game;

        public void Init(int pitch, float beat, float length)
        {
            game = Rockers.instance;
            this.pitch = pitch;
            game.ScheduleInput(beat, length, InputType.DIRECTION_DOWN, Just, Miss, Empty);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                game.Soshi.BendUp(pitch);
                Destroy(gameObject);
                return;
            }
            game.Soshi.BendUp(pitch);
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


