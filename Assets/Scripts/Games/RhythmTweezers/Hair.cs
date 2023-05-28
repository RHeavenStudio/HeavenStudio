using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class Hair : MonoBehaviour
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

        public void StartInput(float beat, float length)
        {
            game.ScheduleInput(beat, length, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Just, Miss, Out);
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