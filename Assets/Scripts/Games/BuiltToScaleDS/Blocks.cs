using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_BuiltToScaleDS
{
    using HeavenStudio.Util;
    public class Blocks : PlayerActionObject
    {
        public float createBeat;
        public float createLength;
        public Animator anim;

        private bool moving = true;
        private BuiltToScaleDS game;

        private void Awake()
        {
            game = BuiltToScaleDS.instance;
        }

        private void Update()
        {
            if (!moving) return;

            var windupBeat = createBeat + (createLength * 4f);
            var hitBeat = windupBeat + createLength;
            var currentBeat = Conductor.instance.songPositionInBeats;

            var shooterState = game.shooterAnim.GetCurrentAnimatorStateInfo(0);
            if (currentBeat > windupBeat && currentBeat < hitBeat
                && !shooterState.IsName("Windup")
                && game.shooterAnim.IsAnimationNotPlaying())
            {
                game.shooterAnim.Play("Windup", 0, 0);
            }

            float stateBeat = Conductor.instance.GetPositionFromMargin(hitBeat, 2f);
            StateCheck(stateBeat);

            if (PlayerInput.Pressed())
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

            if (moving)
            {
                var sinkBeat = hitBeat + (createLength * 2f);

                if (currentBeat < sinkBeat)
                {
                    game.SetBlockTime(this, createBeat, createLength);
                }
                else
                {
                    moving = false;
                    Jukebox.PlayOneShotGame("builtToScaleDS/Sink");
                }
            }
        }

        void Ace()
        {
            moving = false;
            game.shootingThisFrame = true;

            game.Shoot();
            game.SpawnObject(BuiltToScaleDS.BTSObject.HitPieces);
            Destroy(gameObject);

            Jukebox.PlayOneShotGame("builtToScaleDS/Hit");
        }

        void Miss()
        {
            moving = false;
            game.shootingThisFrame = true;

            game.Shoot();
            game.SpawnObject(BuiltToScaleDS.BTSObject.MissPieces);
            Destroy(gameObject);

            Jukebox.PlayOneShotGame("builtToScaleDS/Crumble");
        }

        public override void OnAce()
        {
            Ace();
        }
    }
}