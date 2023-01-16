using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class Hair : PlayerActionObject
    {
        public float createBeat;
        public GameObject hairSprite;
        public GameObject stubbleSprite;
        public GameObject missedSprite;
        private RhythmTweezers game;
        private Tweezers tweezers;
        private bool plucked;

        private void Awake()
        {
            game = RhythmTweezers.instance;
            tweezers = game.Tweezers;
        }

        private void Start() {
            game.ScheduleInput(createBeat, game.tweezerBeatOffset + game.beatInterval, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Just, Miss, Out);
        }

        private void Update()
        {
        }

        public void Ace()
        {
            tweezers.Pluck(true, this);
            tweezers.hitOnFrame++;
            plucked = true;
        }

        public void NearMiss()
        {
            tweezers.Pluck(false, this);
            tweezers.hitOnFrame++;
            plucked = true;
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                NearMiss();
                return; 
            }
            Ace();
        }

        private void Miss(PlayerActionEvent caller) 
        {
            // this is where perfect challenge breaks
        }

        private void Out(PlayerActionEvent caller) {}
    }
}