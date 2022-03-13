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

        private void Update()
        {
            if (plucked) return;

            float stateBeat = Conductor.instance.GetPositionFromMargin(createBeat + game.tweezerBeatOffset + game.beatInterval, 1f);
            StateCheck(stateBeat);

            if (PlayerInput.Pressed(true))
            {
                if (state.perfect)
                {
                    Ace();
                }
                else if (state.notPerfect())
                {
                    Miss();
                }
            }
        }

        public void Ace()
        {
            tweezers.Pluck(true, this);
            tweezers.hitOnFrame++;
            plucked = true;
        }

        public void Miss()
        {
            tweezers.Pluck(false, this);
            tweezers.hitOnFrame++;
            plucked = true;
        }

        public override void OnAce()
        {
            Ace();
        }
    }
}