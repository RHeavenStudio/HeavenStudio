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

        private void Start()
        {
            game = CropStomp.instance;
        }

        private void Update()
        {
            if (!game.isMarching)
                return;
            
            float normalizedBeat = Conductor.instance.GetPositionFromMargin(nextStompBeat, 1f);

            StateCheck(normalizedBeat);

            if (normalizedBeat > Minigame.LateTime())
            {
                nextStompBeat += 2f;
                ResetState();
                return;
            }

            if (PlayerInput.Pressed())
            {
                if (state.perfect)
                {
                    game.Stomp();
                    game.bodyAnim.Play("Stomp", 0, 0);
                    nextStompBeat += 2f;
                    ResetState();
                }
                else if (state.notPerfect())
                {
                    game.bodyAnim.Play("Crouch", 0, 0);
                    nextStompBeat += 2f;
                    ResetState();
                }
                else
                {
                    game.bodyAnim.Play("Crouch", 0, 0);
                }
            }
        }
    }
}
