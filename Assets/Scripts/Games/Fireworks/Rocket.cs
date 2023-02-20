using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Fireworks
{
    public class Rocket : PlayerActionObject
    {
        [SerializeField] ParticleSystem particleBarelyEffect;
        [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();
        [SerializeField] ParticleSystem selectedParticleEffect;
        [SerializeField] Animator anim;
        public bool isSparkler;
        private Fireworks game;
        public float startBeat;
        private bool exploded;
        private float startY;

        void Awake()
        {
            game = Fireworks.instance;
            startY = transform.position.y;
        }

        public void Init(float beat)
        {
            startBeat = beat;
            game.ScheduleInput(beat, isSparkler ? 1f : 3f, InputType.STANDARD_DOWN, Just, Out, Out);
            anim.DoScaledAnimationAsync(isSparkler ? "Sparkler" : "Rocket", isSparkler ? 1f : 0.5f);
        }

        void Update()
        {
            var cond = Conductor.instance;
            float normalizedBeat = cond.GetPositionFromBeat(startBeat, isSparkler ? 1f : 3f);
            if (!exploded && cond.isPlaying && !cond.isPaused) 
            {
                EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
                float newPosY = func(startY, 7f, normalizedBeat * (isSparkler ? 0.5f : 0.4f));
                transform.position = new Vector3(transform.position.x, newPosY, transform.position.z);
            } 
            if (normalizedBeat > 3f && !selectedParticleEffect.isPlaying) Destroy(gameObject);
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("fireworks/miss");
                particleBarelyEffect.Play();
                anim.gameObject.SetActive(false);
                return;
            }
            Success();
        }

        void Success()
        {
            Jukebox.PlayOneShotGame("fireworks/explodeRocket");
            selectedParticleEffect.Play();
            anim.gameObject.SetActive(false);
        }

        void Out(PlayerActionEvent caller) { }
    }
}
