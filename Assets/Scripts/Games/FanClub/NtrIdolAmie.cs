using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using static HeavenStudio.Games.FanClub;

namespace HeavenStudio.Games.Scripts_FanClub
{
    public class NtrIdolAmie : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] float stepDistance = 1f;
        [SerializeField] float startPostion = 0f;
        [SerializeField] float rootYPos = 0f;

        [Header("Objects")]
        [SerializeField] ParticleSystem clapEffect;
        [SerializeField] ParticleSystem winkEffect;
        [SerializeField] GameObject rootTransform;
        [SerializeField] GameObject shadow;

        [Header("References")]
        public Material coreMat;

        Animator anim;
        double startStepBeat = double.MaxValue;
        float stepLength = 16f;
        bool exiting = false;
        int currentAnim = 0;
        double startJumpTime = double.MinValue;
        bool hasJumped = false;

        const int StepCount = 8;
        const int AnimCount = StepCount * 2;

        Conductor cond;

        private void Update() {
            if (cond.songPositionInBeatsAsDouble >= startStepBeat + stepLength)
            {
                FinishEntrance(exiting);
                startStepBeat = double.MaxValue;
                currentAnim = 0;
            }
            else if (cond.songPositionInBeatsAsDouble >= startStepBeat)
            {
                currentAnim = (int)((cond.songPositionInBeatsAsDouble - startStepBeat) / (stepLength / AnimCount));
                double startAnimBeat = startStepBeat + (stepLength / AnimCount) * currentAnim;
                double endAnimBeat = startAnimBeat + (stepLength / AnimCount);
                float prog = (float)((cond.songPositionInBeatsAsDouble - startAnimBeat - 0.75) / (endAnimBeat - startAnimBeat));
                prog = Mathf.Clamp01(prog * 4);
                if (exiting)
                {
                    currentAnim = AnimCount - currentAnim;
                    prog = (float)((cond.songPositionInBeatsAsDouble - startAnimBeat) / (endAnimBeat - startAnimBeat));
                    prog = Mathf.Clamp01(prog * 4);
                }
                anim.DoScaledAnimation(currentAnim % 2 == 0 ? "WalkB" : "WalkA", startAnimBeat - (exiting ? 0.75f : 0), stepLength / AnimCount);
                if (exiting)
                    rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * currentAnim - stepDistance * prog, rootYPos);
                else
                    rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * currentAnim + stepDistance * prog, rootYPos);
            }

            if (startStepBeat == double.MaxValue)
            {
                //idol jumping physics
                float jumpPos = cond.GetPositionFromBeat(startJumpTime, 1f);
                float IDOL_SHADOW_SCALE = 1.18f;
                if (cond.songPositionInBeatsAsDouble >= startJumpTime && cond.songPositionInBeatsAsDouble < startJumpTime + 1f)
                {
                    hasJumped = true;
                    float yMul = jumpPos * 2f - 1f;
                    float yWeight = -(yMul*yMul) + 1f;
                    rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * AnimCount, rootYPos + (2f * yWeight + 0.25f));
                    shadow.transform.localScale = new Vector3((1f-yWeight*0.8f) * IDOL_SHADOW_SCALE, (1f-yWeight*0.8f) * IDOL_SHADOW_SCALE, 1f);

                    anim.DoScaledAnimation("Jump", startJumpTime, 1f);
                }
                else
                {
                    startJumpTime = float.MinValue;
                    rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * AnimCount, rootYPos);
                    shadow.transform.localScale = new Vector3(IDOL_SHADOW_SCALE, IDOL_SHADOW_SCALE, 1f);
                }
            }
            shadow.transform.localPosition = new Vector3(rootTransform.transform.localPosition.x, shadow.transform.localPosition.y);
        }

        public void ClapParticle()
        {
            if (clapEffect == null) return;
            clapEffect.Play();
        }

        public void WinkParticle()
        {
            if (winkEffect == null) return;
            winkEffect.Play();
        }

        public void Init()
        {
            gameObject.SetActive(false);
            rootTransform.SetActive(false);
            shadow.SetActive(false);

            cond = Conductor.instance;
            anim = GetComponent<Animator>();
        }

        public void StartEntrance(double beat, float length, bool exit) {
            gameObject.SetActive(true);
            rootTransform.SetActive(true);
            shadow.SetActive(true);

            startStepBeat = beat;
            stepLength = length;

            exiting = exit;
            if (exiting)
            {
                rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * AnimCount, rootYPos);
            }
            else
            {
                rootTransform.transform.localPosition = new Vector3(startPostion, rootYPos);
            }
        }

        public void FinishEntrance(bool exit) {
            exiting = exit;
            if (exiting)
            {
                rootTransform.transform.localPosition = new Vector3(startPostion, rootYPos);
                gameObject.SetActive(false);
                rootTransform.SetActive(false);
                shadow.SetActive(false);
            }
            else
            {
                rootTransform.transform.localPosition = new Vector3(startPostion + stepDistance * AnimCount, rootYPos);
                gameObject.SetActive(true);
                rootTransform.SetActive(true);
                shadow.SetActive(true);
            }
        }

        public void ToSpot(bool unspot = true)
        {
            if (unspot)
                coreMat.SetColor("_Color", new Color(1, 1, 1, 1));
            else
                coreMat.SetColor("_Color", new Color(117/255f, 177/255f, 209/255f, 1));
        }

        public void DoIdolJump(double beat)
        {
            if (startStepBeat != double.MaxValue) return;
            if (!gameObject.activeInHierarchy) return;
            startJumpTime = beat;

            //play anim
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                // new BeatAction.Action(beat,                     delegate { anim.Play("Jump", -1, 0); }),
                new BeatAction.Action(beat + 1f,                delegate { anim.Play("Land", -1, 0); }),
            });
        }

        public void PlayAnim(double beat, float length, int type)
        {
            if (startStepBeat != double.MaxValue) return;
            if (!gameObject.activeInHierarchy) return;
            startJumpTime = double.MinValue;
            // DisableResponse(beat, length + 0.5f);
            // DisableBop(beat, length + 0.5f);
            // DisableCall(beat, length + 0.5f);

            switch (type)
            {
                case (int) IdolAnimations.Bop:
                    anim.Play("Beat", -1, 0);
                    break;
                case (int) IdolAnimations.PeaceVocal:
                    anim.Play("Peace", -1, 0);
                    break;
                case (int) IdolAnimations.Peace:
                    anim.Play("Peace", -1, 0);
                    break;
                case (int) IdolAnimations.Clap:
                    anim.Play("Crap", -1, 0);
                    break;
                case (int) IdolAnimations.Jump:
                    DoIdolJump(beat);
                    break;
                case (int) IdolAnimations.Squat:
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { anim.Play("Squat0", -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { anim.Play("Squat1", -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Wink:
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat,             delegate { anim.Play("Wink0", -1, 0); }),
                        new BeatAction.Action(beat + length,    delegate { anim.Play("Wink1", -1, 0); }),
                    });
                    break;
                case (int) IdolAnimations.Dab:
                    anim.Play("Dab", -1, 0);
                    break;
                case (int) IdolAnimations.Call:
                case (int) IdolAnimations.Response:
                case (int) IdolAnimations.BigCall:
                default: 
                    break;
            }
        }

        public void PlayAnimState(string state)
        {
            if (startStepBeat != double.MaxValue) return;
            if (!gameObject.activeInHierarchy) return;
            anim.Play(state, -1, 0);
        }
    }
}