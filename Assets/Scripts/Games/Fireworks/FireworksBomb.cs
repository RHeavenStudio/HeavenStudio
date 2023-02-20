using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class FireworksBomb : PlayerActionObject
    {
        public BezierCurve3D curve;
        private bool exploded;
        private Fireworks game;
        private float startBeat;
        private Animator anim;

        void Awake()
        {
            game = Fireworks.instance;
            anim = GetComponent<Animator>();
        }

        public void Init(float beat)
        {
            Jukebox.PlayOneShotGame("fireworks/bomb");
            game.ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, Just, Out, Out);
            startBeat = beat;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (exploded) return;
            float flyPos = cond.GetPositionFromBeat(startBeat, 2f);
            transform.position = curve.GetPoint(flyPos);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            Success();
        }

        void Success()
        {
            Jukebox.PlayOneShotGame("fireworks/explodeBomb");
            anim.DoScaledAnimationAsync("ExplodeBomb", 0.25f);
            exploded = true;
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 3f, delegate { Destroy(gameObject); })
            });
        }

        void Out(PlayerActionEvent caller) { }
    }
}


