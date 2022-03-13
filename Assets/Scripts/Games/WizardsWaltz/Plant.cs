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
        }

        private void Start()
        {
            spriteRenderer.sortingOrder = order;
            animator.Play("Appear", 0, 0);
        }

        private void Update()
        {
            if (!passed && Conductor.instance.songPositionInBeats > createBeat + game.beatInterval)
            {
                StartCoroutine(FadeOut());
                passed = true;
            }

            if (hit) return;

            float stateBeat = Conductor.instance.GetPositionFromMargin(createBeat + game.beatInterval, 1f);
            StateCheck(stateBeat);

            if (PlayerInput.Pressed(true))
            {
                if (state.perfect)
                {
                    Ace();
                } else if (state.notPerfect())
                {
                    Miss();
                }
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

        public void Miss()
        {
            game.wizard.Magic(this, false);
            hit = true;
        }

        public override void OnAce()
        {
            Ace();
        }

        public IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(Conductor.instance.secPerBeat * game.beatInterval / 2f);
            Destroy(gameObject);
        }
    }
}