using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_FanClub
{
    public class NtrIdolFan : PlayerActionObject
    {
        [Header("References")]
        [SerializeField] private GameObject motionRoot;
        [SerializeField] private GameObject headRoot;
        public Animator animator;
        public Animator headAnimator;
        public ParticleSystem fanClapEffect;
        public GameObject shadow;

        [Header("Properties")]
        [NonSerialized] public bool player = false;
        public float jumpStartTime = Single.MinValue;
        bool stopBeat = false;
        bool stopCharge = false;
        bool hasJumped = false;

        float clappingStartTime = Single.MinValue;

        public void AddHit(float beat, int type = 0)
        {
            if (player)
            {
                switch (type)
                {
                    case 0:
                        FanClub.instance.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, ClapJust, ClapThrough, Out);
                        break;
                    case 1:
                        FanClub.instance.ScheduleInput(beat, 1f, InputType.STANDARD_UP, JumpJust, JumpThrough, JumpOut);
                        break;
                    case 2:
                        FanClub.instance.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, ChargeClapJust, ClapThrough, Out);
                        break;
                    default:
                        FanClub.instance.ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, LongClapJust, ClapThrough, Out);
                        break;
                }
            }
        }

        public void ClapJust(PlayerActionEvent caller, float state)
        {
            bool auto = GameManager.instance.autoplay;
            ClapStart(true, false, auto ? 0.1f : 0f);
        }

        public void ChargeClapJust(PlayerActionEvent caller, float state)
        {
            bool auto = GameManager.instance.autoplay;
            ClapStart(true, true, auto ? 1f : 0f);
        }

        public void LongClapJust(PlayerActionEvent caller, float state)
        {
            bool auto = GameManager.instance.autoplay;
            ClapStart(true, false, auto ? 1f : 0f);
        }

        public void JumpJust(PlayerActionEvent caller, float state)
        {
            JumpStart(true);
        }

        public void ClapThrough(PlayerActionEvent caller) {
            FanClub.instance.AngerOnMiss();
        }

        public void JumpThrough(PlayerActionEvent caller) {
            FanClub.instance.AngerOnMiss();
        }

        public void Out(PlayerActionEvent caller) {}

        public void JumpOut(PlayerActionEvent caller) {
            var cond = Conductor.instance;
            if (stopCharge)
            {
                caller.CanHit(false);
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (player)
            {
                if (PlayerInput.Pressed())
                {
                    if (!FanClub.instance.IsExpectingInputNow(InputType.STANDARD_DOWN))
                    {
                        ClapStart(false);
                        FanClub.instance.ScoreMiss();
                    }
                }
                if (PlayerInput.Pressing())
                {
                    if (clappingStartTime != Single.MinValue && cond.songPositionInBeats > clappingStartTime + 2f && !stopCharge)
                    {
                        animator.speed = 1f;
                        animator.Play("FanClapCharge", -1, 0);
                        stopCharge = true;
                    }
                }
                if (PlayerInput.PressedUp())
                {
                    if (clappingStartTime != Single.MinValue && cond.songPositionInBeats > clappingStartTime + 2f && stopCharge && !FanClub.instance.IsExpectingInputNow(InputType.STANDARD_UP))
                    {
                        JumpStart(false);
                        FanClub.instance.ScoreMiss();
                    }
                    else
                    {
                        animator.speed = 1f;
                        animator.Play("FanFree", -1, 0);
                        stopBeat = false;
                        clappingStartTime = Single.MinValue;
                    }
                }
            }
            
            float jumpPos = cond.GetPositionFromBeat(jumpStartTime, 1f);
            if (cond.songPositionInBeats >= jumpStartTime && cond.songPositionInBeats < jumpStartTime + 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                motionRoot.transform.localPosition = new Vector3(0, 3f * yWeight);
                shadow.transform.localScale = new Vector3((1f-yWeight*0.8f) * 1.4f, (1f-yWeight*0.8f) * 1.4f, 1f);
                animator.DoScaledAnimation("FanJump", jumpStartTime);
            }
            else
            {
                motionRoot.transform.localPosition = new Vector3(0, 0);
                shadow.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
                if (hasJumped)
                {
                    Jukebox.PlayOneShotGame("fanClub/landing_impact", pitch: UnityEngine.Random.Range(0.95f, 1f), volume: 1f/4);
                    if (player)
                    {
                        animator.Play("FanPrepare", -1, 0);
                        stopBeat = false;
                    }
                }
                animator.speed = 1f;
                hasJumped = false;
            }
        }

        public void ClapStart(bool hit, bool doCharge = false, float autoplayRelease = 0f)
        {
            if (!hit)
            {
                FanClub.instance.AngerOnMiss();
            }

            var cond = Conductor.instance;
            hasJumped = false;
            stopBeat = true;
            jumpStartTime = -99f;
            animator.speed = 1f;
            animator.Play("FanClap", -1, 0);
            Jukebox.PlayOneShotGame("fanClub/play_clap");
            Jukebox.PlayOneShotGame("fanClub/crap_impact");
            clappingStartTime = cond.songPositionInBeats;

            if (doCharge)
                BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(cond.songPositionInBeats + 0.1f, delegate { 
                        if (PlayerInput.Pressing() || autoplayRelease > 0f)
                        {
                            animator.Play("FanClapCharge", -1, 0);
                            stopCharge = true;
                        }
                    }),
                });

            if (autoplayRelease > 0f && !doCharge)
            {
                BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(cond.songPositionInBeats + autoplayRelease, delegate { 
                        animator.Play("FanFree", -1, 0);
                        stopBeat = false;
                    }),
                });
                
            }
        }

        public void JumpStart(bool hit)
        {
            if (!hit)
            {
                FanClub.instance.AngerOnMiss();
            }

            var cond = Conductor.instance;
            animator.Play("FanJump", -1, 0);
            Jukebox.PlayOneShotGame("fanClub/play_jump");
            jumpStartTime = cond.songPositionInBeats;
            clappingStartTime = Single.MinValue;
            stopCharge = false;
        }

        public bool IsJumping()
        {
            var cond = Conductor.instance;
            return (cond.songPositionInBeats >= jumpStartTime && cond.songPositionInBeats < jumpStartTime + 1f);
        }

        public void Bop()
        {
            if (!stopBeat)
            {
                animator.speed = 1f;
                animator.Play("FanBeat");
            }
        }

        public void ClapParticle()
        {
            fanClapEffect.Play();
        }

        public void MakeAngry(bool flip = false)
        {
            headAnimator.Play("FanFaceAngry", -1, 0);
            if (flip)
            {
                headRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
    }
}
