using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ClappyTrio
{
    public class ClappyTrioPlayer : PlayerActionObject
    {
        ClappyTrio game;
        private float lastClapBeat;
        private float lastClapLength;

        public bool clapStarted = false;
        public bool canHit;

        private GameObject clapEffect;

        private void Awake()
        {
            game = ClappyTrio.instance;
            clapEffect = transform.GetChild(4).GetChild(3).gameObject;
        }

        private void Update()
        {
            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Clap(false);
            }
        }

        public void QueueClap(float startBeat, float length)
        {
            lastClapBeat = startBeat;
            lastClapLength = length;

            game.ScheduleInput(startBeat, length, InputType.STANDARD_DOWN, Just, Miss, Out);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (!canHit) { 
                Clap(false); 
                return; 
            }
            if (state >= 1f || state <= -1f) {  //todo: proper near miss feedback
                Clap(false); 
                return; 
            }
            Clap(true);
        }

        private void Miss(PlayerActionEvent caller) {
            game.playerHitLast = false;

            if (clapStarted)
                this.canHit = false;
        }

        private void Out(PlayerActionEvent caller) {}

        private void Clap(bool just)
        {
            if (just)
            {
                clapEffect.SetActive(true);
                Jukebox.PlayOneShotGame("clappyTrio/rightClap");

                if (this.canHit)
                    game.playerHitLast = true;
            }
            else
            {
                clapEffect.SetActive(false);
                Jukebox.PlayOneShot("miss");
                game.playerHitLast = false;

                if (clapStarted)
                    this.canHit = false;
            }

            clapStarted = false;
            game.SetFace(game.Lion.Count - 1, 4);
            this.GetComponent<Animator>().Play("Clap", 0, 0);
        }
    }
}