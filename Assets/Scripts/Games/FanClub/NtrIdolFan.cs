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
        [NonSerialized] public bool hitValid = false;
        public float jumpStartTime = Single.MinValue;
        bool stopBeat = false;
        bool stopCharge = false;
        bool hasJumped = false;

        float clappingStartTime = 0f;

        public SortedList<float, int> upcomingHits;
        public float startBeat;
        public int type;
        public bool doCharge = false;
        private bool inputHit = false;
        private bool hasHit = false;

        public void Init()
        {
            if (player)
                upcomingHits = new SortedList<float, int>();    // beat, type
            
            inputHit = true;
            hasHit = true;
        }

        public override void OnAce()
        {
            Hit(true, type, true);
        }

        public void AddHit(float beat, int type)
        {
            inputHit = false;
            try
            {
                upcomingHits.Add(beat, type);
            }
            catch (ArgumentException)
            {}
        }

        public void Hit(bool _hit, int type = 0, bool fromAutoplay = false)
        {
            if (player && !hasHit)
            {
                if (type == 0)
                    ClapStart(_hit, true, doCharge, fromAutoplay);
                else if (type == 1)
                    JumpStart(_hit, true, fromAutoplay);

                hasHit = true;
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            // read cue queue and pop when needed
            if (hasHit)
            {
                if (upcomingHits?.Count > 0)
                {
                    var k = upcomingHits.Keys[0];
                    var v = upcomingHits[k];

                    startBeat = k;
                    type = v == 2 ? 0 : v;
                    doCharge = (v == 2);

                    // reset our shit to prepare for next hit
                    hasHit = false;
                    ResetState();

                    upcomingHits.Remove(k);
                }
                else if (Conductor.instance.GetPositionFromBeat(startBeat, 1) >= Minigame.EndTime())
                {
                    startBeat = Single.MinValue;
                    type = 0;
                    doCharge = false;
                    // DO NOT RESET, wait for future cues
                }
            }

            // no input?
            if (!hasHit && Conductor.instance.GetPositionFromBeat(startBeat, 1f) >= Minigame.EndTime())
            {
                FanClub.instance.AngerOnMiss();
                hasHit = true;
            }

            // dunno what this is for
            if (!inputHit && Conductor.instance.GetPositionFromBeat(startBeat, 1) >= Minigame.EndTime())
            {
                inputHit = true;
            }

            if (!hasHit)
            {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 1);
                StateCheck(normalizedBeat);
            }

            if (player)
            {
                if (PlayerInput.Pressed() && type == 0)
                {
                    if (state.perfect)
                    {
                        Hit(true);
                    } else if (state.notPerfect())
                    {
                        Hit(false);
                    }
                }
                if (PlayerInput.PressedUp() && type == 1)
                {
                    if (state.perfect)
                    {
                        Hit(true, type);
                    } else if (state.notPerfect())
                    {
                        Hit(false, type);
                    }
                }
                if (PlayerInput.Pressed())
                {
                    if (!hasHit || (upcomingHits?.Count == 0 && startBeat == Single.MinValue))
                        FanClub.instance.AngerOnMiss();

                    hasJumped = false;
                    stopBeat = true;
                    jumpStartTime = -99f;
                    animator.Play("FanClap", -1, 0);
                    Jukebox.PlayOneShotGame("fanClub/play_clap");
                    Jukebox.PlayOneShotGame("fanClub/crap_impact");
                    clappingStartTime = cond.songPositionInBeats;
                }
                if (PlayerInput.Pressing())
                {
                    if (cond.songPositionInBeats > clappingStartTime + 1.5f && !stopCharge)
                    {
                        animator.Play("FanClapCharge", -1, 0);
                        stopCharge = true;
                    }
                }
                if (PlayerInput.PressedUp())
                {
                    if (stopCharge)
                    {
                        if (!hasHit || (upcomingHits?.Count == 0 && startBeat == Single.MinValue))
                            FanClub.instance.AngerOnMiss();

                        animator.Play("FanJump", -1, 0);
                        Jukebox.PlayOneShotGame("fanClub/play_jump");
                        jumpStartTime = cond.songPositionInBeats;
                        stopCharge = false;
                    }
                    else
                    {
                        animator.Play("FanFree", -1, 0);
                        stopBeat = false;
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
                hasJumped = false;
            }
        }

        public void ClapStart(bool hit, bool force = false, bool doCharge = false, bool fromAutoplay = false)
        {
            var cond = Conductor.instance;
            if (hit)
            {
                if (doCharge)
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(cond.songPositionInBeats + 0.1f, delegate { 
                            if (PlayerInput.Pressing() || fromAutoplay)
                            {
                                animator.Play("FanClapCharge", -1, 0);
                                stopCharge = true;
                            }
                        }),
                    });
            }
            else
            {
                FanClub.instance.AngerOnMiss();
            }
            if (fromAutoplay || !force)
            {
                stopBeat = true;
                jumpStartTime = -99f;
                animator.Play("FanClap", -1, 0);
                Jukebox.PlayOneShotGame("fanClub/play_clap");
                Jukebox.PlayOneShotGame("fanClub/crap_impact");
                clappingStartTime = cond.songPositionInBeats;
            }
            if (fromAutoplay && !doCharge)
            {
                BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(cond.songPositionInBeats + 0.1f, delegate { 
                        animator.Play("FanFree", -1, 0);
                        stopBeat = false;
                    }),
                });
                
            }
        }

        public void JumpStart(bool hit, bool force = false, bool fromAutoplay = false)
        {
            var cond = Conductor.instance;
            if (hit)
            {}
            else
            {
                FanClub.instance.AngerOnMiss();
            }
            if (fromAutoplay || !force)
            {
                animator.Play("FanJump", -1, 0);
                Jukebox.PlayOneShotGame("fanClub/play_jump");
                jumpStartTime = cond.songPositionInBeats;
                stopCharge = false;
            }
        }

        public bool IsJumping()
        {
            var cond = Conductor.instance;
            return (cond.songPositionInBeats >= jumpStartTime && cond.songPositionInBeats < jumpStartTime + 1f);
        }

        public void Bop()
        {
            if (!stopBeat)
                animator.Play("FanBeat");
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
