using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TramAndPauline
{
    public class AgbAnimalKid : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform rootBody;
        [SerializeField] private Animator trampolineAnim;
        [SerializeField] private Animator bodyAnim;
        [SerializeField] private ParticleSystem transformParticle;
        [SerializeField] private ParticleSystem smokeParticle;

        [Header("Properties")]
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float jumpHeightIdle = 0.8f;
        [SerializeField] private float prepareHeight = 0.5f;

        private double jumpBeat = double.MinValue;
        private double prepareBeat = double.MinValue;
        private bool preparing = false;
        private bool isFox = true;

        private EasingFunction.Function upFunc;
        private EasingFunction.Function downFunc;

        private void Awake()
        {
            upFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
            downFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (!cond.isPlaying) return;

            float newY = 0f;

            float normalizedUpBeat = cond.GetPositionFromBeat(jumpBeat, 1);
            float normalizedDownBeat = cond.GetPositionFromBeat(jumpBeat + 1, 1);

            if (normalizedUpBeat >= 0f && normalizedUpBeat <= 1f)
            {
                newY = upFunc(0, jumpHeight, normalizedUpBeat);
            }
            else if (normalizedDownBeat >= 0f && normalizedDownBeat <= 1f)
            {
                newY = downFunc(jumpHeight, 0, normalizedDownBeat);
            }
            else if (!preparing)
            {
                if (isBarely)
                {
                    bodyAnim.Play("BarelyIdle", 0, 0);
                }
                else bodyAnim.Play(isFox ? "FoxIdle" : "HumanIdle", 0, 0);
                BounceUpdate(cond, ref newY);
            }
            else
            {
                PrepareUpdate(cond, ref newY);
            }

            rootBody.transform.localPosition = new Vector3(0, newY, 0);
        }

        private void BounceUpdate(Conductor cond, ref float newY)
        {
            double startBeat = (jumpBeat != double.MinValue) ? jumpBeat : 0;
            float normalizedBeat = cond.GetPositionFromBeat(startBeat, 1) % 1;
            float trampolinePos = 0f;

            if (normalizedBeat < 0.5f)
            {
                newY = upFunc(0, jumpHeightIdle, normalizedBeat * 2);
                trampolinePos = upFunc(0, 1, normalizedBeat * 2);
            }
            else
            {
                newY = downFunc(jumpHeightIdle, 0, (normalizedBeat - 0.5f) * 2);
                trampolinePos = downFunc(1, 0, (normalizedBeat - 0.5f) * 2);
            }

            trampolineAnim.DoNormalizedAnimation("Bounce", trampolinePos);
        }

        private void PrepareUpdate(Conductor cond, ref float newY)
        {
            float normalizedBeat = cond.GetPositionFromBeat(prepareBeat, 0.5f);

            if (normalizedBeat >= 0f && normalizedBeat <= 1f)
            {
                newY = upFunc(prepareHeight, 0, normalizedBeat);
                trampolineAnim.DoNormalizedAnimation("Prepare", normalizedBeat);
            }
            else if (normalizedBeat > 1f)
            {
                trampolineAnim.DoNormalizedAnimation("Prepare", 1);
                newY = 0f;
            }
        }

        public void Jump(double beat)
        {
            jumpBeat = beat;
            preparing = false;
            if (isBarely)
            {
                bodyAnim.Play("JumpBarely", 0, 0);
            }
            else bodyAnim.Play(isFox ? "JumpFox" : "JumpHuman", 0, 0);
            trampolineAnim.DoScaledAnimationAsync("Jump", 0.25f);
        }

        public void Prepare(double beat, bool inactive = false)
        {
            if (preparing) return;
            prepareBeat = beat;
            preparing = true;
            if (inactive)
            {
                if (isBarely)
                {
                    bodyAnim.DoNormalizedAnimation("PrepareBarely", 1);
                }
                else bodyAnim.DoNormalizedAnimation(isFox ? "Prepare" : "PrepareHuman", 1);
            }
            else
            {
                if (isBarely)
                {
                    bodyAnim.DoScaledAnimationAsync("PrepareBarely", 0.15f);
                }
                else bodyAnim.DoScaledAnimationAsync(isFox ? "Prepare" : "PrepareHuman", 0.15f);
            }
        }

        private bool isBarely = false;

        public void Transform(bool barely)
        {
            isBarely = barely;
            if (isBarely)
            {
                bodyAnim.DoScaledAnimationAsync("TransformBarely", 0.15f);
            }
            else bodyAnim.DoScaledAnimationAsync(isFox ? "TransformHuman" : "TransformFox", 0.15f);
            smokeParticle.SetAsyncScaling(0.5f);
            transformParticle.PlayScaledAsync(0.5f);
            isFox = !isFox;
        }

        public void SetTransform(bool fox)
        {
            isFox = fox;
        }
    }
}

