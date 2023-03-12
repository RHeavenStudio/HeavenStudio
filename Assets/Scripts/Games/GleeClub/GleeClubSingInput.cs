using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_GleeClub
{
    public class GleeClubSingInput : PlayerActionObject
    {
        public float pitch = 1f;
        bool shouldClose = true;

        private GleeClub game;

        void Awake()
        {
            game = GleeClub.instance;
        }

        public void Init(float beat, float length, bool close)
        {
            shouldClose = close;
            game.ScheduleInput(beat - 1, 1, InputType.STANDARD_UP, Just, Miss, Out);
            if (close) game.ScheduleInput(beat, length, InputType.STANDARD_DOWN, JustClose, MissClose, Out);
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            if (!game.playerChorusKid.singing)
            {
                game.playerChorusKid.currentPitch = pitch;
                game.playerChorusKid.StartSinging();
            }
            if (!shouldClose) CleanUp();
        }

        public void JustClose(PlayerActionEvent caller, float state)
        {
            game.playerChorusKid.StopSinging();
            CleanUp();
        }

        public void MissClose(PlayerActionEvent caller)
        {
            CleanUp();
        }

        public void Miss(PlayerActionEvent caller)
        {
            if (!shouldClose) CleanUp();
        }

        public void Out(PlayerActionEvent caller)
        {

        }

        void CleanUp()
        {
            Destroy(gameObject);
        }
    }
}


