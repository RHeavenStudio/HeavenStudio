using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class Rocket : PlayerActionObject
    {
        [SerializeField] ParticleSystem particleEffect;
        [SerializeField] Animator anim;
        public bool isSparkler;
        private Fireworks game;
        public float startBeat;
        private bool exploded;

        void Awake()
        {
            game = Fireworks.instance;
        }

        public void Init(float beat)
        {
            startBeat = beat;
            if (isSparkler) Jukebox.PlayOneShotGame("fireworks/sparkler");
            else Jukebox.PlayOneShotGame("fireworks/rocket");
            game.ScheduleInput(beat, isSparkler ? 1f : 3f, InputType.STANDARD_DOWN, Just, Out, Out);
            anim.DoScaledAnimationAsync(isSparkler ? "Sparkler" : "Rocket", isSparkler ? 1f : 0.5f);
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (!exploded && cond.isPlaying && !cond.isPaused) transform.position = new Vector3(transform.position.x, transform.position.y + (isSparkler ? 0.05f : 0.015f), transform.position.z);
            if (cond.GetPositionFromBeat(startBeat, isSparkler ? 1f : 3f) > 2.5f) Destroy(gameObject);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("fireworks/miss");
                particleEffect.Play();
                anim.gameObject.SetActive(false);
                return;
            }
            Success();
        }

        void Success()
        {
            Jukebox.PlayOneShotGame("fireworks/explodeRocket");
            particleEffect.Play();
            anim.gameObject.SetActive(false);
        }

        void Out(PlayerActionEvent caller) { }
    }
}
