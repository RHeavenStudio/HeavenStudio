using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeavenStudio.Games.Scripts_WizardsWaltz
{
    public class Plant : PlayerActionObject
    {
        public Animator animator;
        public SpriteRenderer spriteRenderer;
        public float createBeat;

        private WizardsWaltz game;
        private bool hit = false;
        private bool passed = false;

        public int order = 0;

        private void Awake()
        {
            game = WizardsWaltz.instance;
            spriteRenderer.sortingOrder = order;
            animator.Play("Appear", 0, 0);
        }

        private void Start() {
            game.ScheduleInput(createBeat, game.beatInterval, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Just, Miss, Out);
        }

        private void Update()
        {
            if (!passed && Conductor.instance.songPositionInBeats > createBeat + game.beatInterval)
            {
                StartCoroutine(FadeOut());
                passed = true;
            }
        }

        public void Bloom()
        {
            animator.Play("Hit", 0, 0);
        }

        public void IdlePlant()
        {
            animator.Play("IdlePlant", 0, 0);
        }

        public void IdleFlower()
        {
            animator.Play("IdleFlower", 0, 0);
        }

        public void Eat()
        {
            animator.Play("Eat", 0, 0);
        }

        public void EatLoop()
        {
            animator.Play("EatLoop", 0, 0);
        }

        public void Ace()
        {
            game.wizard.Magic(this, true);
            hit = true;
        }

        public void NearMiss()
        {
            game.wizard.Magic(this, false);
            hit = true;
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

        public IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(Conductor.instance.secPerBeat * game.beatInterval / 2f);
            Destroy(gameObject);
        }
    }
}