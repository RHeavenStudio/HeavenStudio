using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class Football : PlayerActionObject
    {
        private DoubleDate game;

        void Awake()
        {
            game = DoubleDate.instance;
        }

        public void Init(float beat)
        {
            game.ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, Just, Miss, Empty);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShot("miss");
                game.Kick(false);
                Destroy(gameObject); //Remove this when doing the ball movement
                return;
            }
            Hit();
        }

        void Hit()
        {
            game.Kick(true, true);
            Jukebox.PlayOneShotGame("doubleDate/footballKick");
            Destroy(gameObject); //Remove this when doing the ball movement
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("doubleDate/weasel_hit");
            Jukebox.PlayOneShotGame("doubleDate/weasel_scream");
            Destroy(gameObject); //Remove this when doing the ball movement
        }

        void Empty(PlayerActionEvent caller) { }
    }
}
