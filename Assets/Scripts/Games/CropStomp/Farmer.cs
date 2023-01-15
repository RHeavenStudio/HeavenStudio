using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CropStomp
{
    public class Farmer : PlayerActionObject
    {
        public float nextStompBeat;

        private CropStomp game;

        PlayerActionEvent stomp;

        public void Init()
        {
            game = CropStomp.instance;
        }

        private void Update()
        {
            if (!game.isMarching)
                return;
            Conductor cond = Conductor.instance;

            if (stomp == null)
            {
                if (GameManager.instance.currentGame == "cropStomp")
                    stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, InputType.STANDARD_DOWN, Just, Miss, Out);
            }

            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                game.bodyAnim.Play("Crouch", 0, 0);
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            // REMARK: does not count for performance
            Stomp(state >= 1f || state <= -1f);
        }

        private void Miss(PlayerActionEvent caller) 
        {
            if (GameManager.instance.currentGame != "cropStomp") return;
            if (!game.isMarching)
                return;
            // REMARK: does not count for performance
            nextStompBeat += 2f;
            stomp?.Disable();
            stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, InputType.STANDARD_DOWN, Just, Miss, Out);
        }

        private void Out(PlayerActionEvent caller) {}

        void Stomp(bool ng)
        {
            if (GameManager.instance.currentGame != "cropStomp") return;
            if (!game.isMarching)
                return;
            if (ng)
            {
                game.bodyAnim.Play("Crouch", 0, 0);
            }
            else
            {
                game.Stomp();
                game.bodyAnim.Play("Stomp", 0, 0);
            }
            nextStompBeat += 2f;
            stomp?.Disable();
            stomp = game.ScheduleUserInput(nextStompBeat - 1f, 1f, InputType.STANDARD_DOWN, Just, Miss, Out);
        }
    }
}
