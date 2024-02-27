using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class FireworksBomb : MonoBehaviour
    {
        public BezierCurve3D curve;
        public bool applause;
        private bool exploded;
        private Fireworks game;
        private double startBeat;
        private Animator anim;

        void Awake()
        {
            game = Fireworks.instance;
            anim = GetComponent<Animator>();
        }

        public void Init(double beat)
        {
            game.ScheduleInput(beat, 1f, Fireworks.InputAction_BasicPress, Just, Out, Out);
            startBeat = beat;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (exploded) return;
            float flyPos = cond.GetPositionFromBeat(startBeat, 1f);
            transform.position = curve.GetPoint(flyPos);
            if (flyPos > 2) Destroy(gameObject);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            anim.DoScaledAnimationAsync("ExplodeBomb", 0.25f);
            exploded = true;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 3f, delegate { Destroy(gameObject); })
            });
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("fireworks/miss");

                return;
            }
            Success(caller);
        }

        void Success(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("fireworks/taikoExplode");
            game.FadeFlashColor(Color.white, new Color(1, 1, 1, 0), 0.5f);
            if (applause) SoundByte.PlayOneShot("applause", caller.timer + caller.startBeat + 1f);
        }

        void Out(PlayerActionEvent caller) { }
    }
}


