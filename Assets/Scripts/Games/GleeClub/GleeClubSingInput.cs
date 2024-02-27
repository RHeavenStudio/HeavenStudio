using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_GleeClub
{
    public class GleeClubSingInput : MonoBehaviour
    {
        public float pitch = 1f;
        bool shouldClose = true;
        bool shouldOpen = true;

        private GleeClub game;

        void Awake()
        {
            game = GleeClub.instance;
        }

        public void Init(double beat, float length, int close)
        {
            shouldClose = close != (int)GleeClub.MouthOpenClose.OnlyOpen;
            shouldOpen = close != (int)GleeClub.MouthOpenClose.OnlyClose;
            if (shouldOpen) game.ScheduleInput(beat - 1, 1, GleeClub.InputAction_BasicRelease, Just, Miss, Out);
            if (shouldClose) game.ScheduleInput(beat, length, GleeClub.InputAction_BasicPress, JustClose, MissClose, Out);
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
            game.missed = true;
            CleanUp();
        }

        public void Miss(PlayerActionEvent caller)
        {
            game.missed = true;
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


