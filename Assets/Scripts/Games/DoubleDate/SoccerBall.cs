using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class SoccerBall : PlayerActionObject
    {
        private DoubleDate game;

        void Awake()
        {
            game = DoubleDate.instance;
        }

        public void Init(float beat)
        {
            game.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Just, Miss, Empty);
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
            game.Kick();
            Jukebox.PlayOneShotGame("doubleDate/kick");
            Destroy(gameObject); //Remove this when doing the ball movement
        }

        void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("doubleDate/weasel_hide");
            Destroy(gameObject); //Remove this when doing the ball movement
        }

        void Empty(PlayerActionEvent caller) { }
    }
}
