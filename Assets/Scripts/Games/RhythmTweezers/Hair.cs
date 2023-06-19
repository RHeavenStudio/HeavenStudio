using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class Hair : MonoBehaviour
    {
        public double createBeat;
        public GameObject hairSprite;
        public GameObject stubbleSprite;
        public GameObject missedSprite;
        private RhythmTweezers game;
        private Tweezers tweezers;

        private void Awake()
        {
            game = RhythmTweezers.instance;
        }

        public void StartInput(double beat, double length, Tweezers tweezer)
        {
            tweezers = tweezer;
            game.ScheduleInput(beat, length, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Just, Miss, Out);
        }

        public void Ace()
        {
            tweezers.Pluck(true, this);
            tweezers.hitOnFrame++;
        }

        public void NearMiss()
        {
            tweezers.Pluck(false, this);
            tweezers.hitOnFrame++;
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