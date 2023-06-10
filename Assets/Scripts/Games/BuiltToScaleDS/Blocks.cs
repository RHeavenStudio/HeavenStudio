using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_BuiltToScaleDS
{
    using HeavenStudio.Util;
    public class Blocks : MonoBehaviour
    {
        public double createBeat;
        public float createLength;
        public Animator anim;

        private bool moving = true;
        private BuiltToScaleDS game;
        double windupBeat;
        double hitBeat;
        double sinkBeat;

        private void Awake()
        {
            game = BuiltToScaleDS.instance;

        }

        private void Start()
        {          
            windupBeat = createBeat + (createLength * 4f);
            hitBeat = windupBeat + createLength;
            sinkBeat = hitBeat + (createLength * 2f);

            game.ScheduleInput(windupBeat, createLength, InputType.STANDARD_DOWN, Just, Miss, Out);
        }

        private void Update()
        {
            if (!moving) return;
            double currentBeat = Conductor.instance.songPositionInBeatsAsDouble;

            var shooterState = game.shooterAnim.GetCurrentAnimatorStateInfo(0);
            if (currentBeat > windupBeat && currentBeat < hitBeat
                && !shooterState.IsName("Windup")
                && !game.lastShotOut)
            {
                game.shooterAnim.Play("Windup", 0, 0);
            }

            if (moving && currentBeat < sinkBeat)
                game.SetBlockTime(this, createBeat, createLength);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            var shooterState = game.shooterAnim.GetCurrentAnimatorStateInfo(0);
            if (!shooterState.IsName("Windup")) return;

            // near miss
            if (state >= 1f || state <= -1f) {
                NearMiss();
                return;
            }
            // hit
            Hit();
        }

        private void Miss(PlayerActionEvent caller) 
        {
            double sinkBeat = hitBeat + (createLength * 2f);
            MultiSound.Play(
                new MultiSound.Sound[] {
                    new MultiSound.Sound("builtToScaleDS/Sink", sinkBeat),
                }, forcePlay: true
            );

            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(sinkBeat, delegate { moving = false; }),
            });
        }

        private void Out(PlayerActionEvent caller) {}

        void Hit()
        {
            moving = false;
            game.shootingThisFrame = true;

            game.Shoot();
            game.SpawnObject(BuiltToScaleDS.BTSObject.HitPieces);
            Destroy(gameObject);

            SoundByte.PlayOneShotGame("builtToScaleDS/Hit");
        }

        void NearMiss()
        {
            moving = false;
            game.shootingThisFrame = true;

            game.Shoot();
            game.SpawnObject(BuiltToScaleDS.BTSObject.MissPieces);
            Destroy(gameObject);

            SoundByte.PlayOneShotGame("builtToScaleDS/Crumble");
        }
    }
}