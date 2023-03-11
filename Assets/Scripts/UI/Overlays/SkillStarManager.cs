using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Games;

namespace HeavenStudio.Common
{
    public class SkillStarManager : MonoBehaviour
    {
        public enum StarState
        {
            None,
            In,
            Collected,
            Out
        }
        public static SkillStarManager instance { get; private set; }
        [SerializeField] private Animator starAnim;
        [SerializeField] private ParticleSystem starParticle;

        public float StarTargetTime { get { return starStart + starLength; } }
        public bool IsEligible { get; private set; }

        float starStart = float.MaxValue;
        float starLength = float.MaxValue;
        StarState state = StarState.None;
        Conductor cond;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            cond = Conductor.instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (cond.songPositionInBeatsAsDouble > starStart && state == StarState.In)
            {
                double offset = cond.SecsToBeats(Minigame.AceStartTime()-1, cond.GetBpmAtBeat(StarTargetTime));
                if (cond.songPositionInBeatsAsDouble <= starStart + starLength + offset)
                    starAnim.DoScaledAnimation("StarIn", starStart, starLength + (float)offset);
                else
                    starAnim.Play("StarIn", -1, 1f);
                
                offset = cond.SecsToBeats(Minigame.AceEndTime()-1, cond.GetBpmAtBeat(StarTargetTime));
                if (cond.songPositionInBeatsAsDouble > starStart + starLength + offset)
                    KillStar();
            }
        }

        public void DoStarPreview()
        {
            if (starAnim == null) return;
            starAnim.Play("StarJust", -1, 0.5f);
            starAnim.speed = 0f;
        }

        public void ResetStarPreview()
        {
            if (starAnim == null) return;
            starAnim.Play("NoPose", -1, 0f);
            starAnim.speed = 1f;
        }

        public void Reset()
        {
            IsEligible = false;
            cond = Conductor.instance;
            state = StarState.None;
            starAnim.Play("NoPose", -1, 0f);
            starAnim.speed = 1f;
            starStart = float.MaxValue;
            starLength = float.MaxValue;
        }

        public void DoStarIn(float beat, float length)
        {
            if (!OverlaysManager.OverlaysEnabled) return;
            IsEligible = true;
            state = StarState.In;
            starStart = beat;
            starLength = length;

            TimingAccuracyDisplay.instance.StartStarFlash();
            
            starAnim.DoScaledAnimation("StarIn", beat, length);
        }

        public bool DoStarJust()
        {
            if (state == StarState.In && 
                cond.songPositionInBeatsAsDouble >= StarTargetTime + cond.SecsToBeats(Minigame.AceStartTime()-1, cond.GetBpmAtBeat(StarTargetTime)) &&
                cond.songPositionInBeatsAsDouble <= StarTargetTime + cond.SecsToBeats(Minigame.AceEndTime()-1, cond.GetBpmAtBeat(StarTargetTime))
            )
            {
                state = StarState.Collected;
                starAnim.Play("StarJust", -1, 0f);
                starParticle.Play();
                Jukebox.PlayOneShot("skillStar");

                TimingAccuracyDisplay.instance.StopStarFlash();
                return true;
            }
            return false;
        }

        public void KillStar()
        {
            if (state == StarState.In && cond.songPositionInBeatsAsDouble >= starStart + starLength*0.5f || !cond.isPlaying)
            {
                IsEligible = false;
                state = StarState.Out;
                starAnim.Play("NoPose", -1, 0f);

                TimingAccuracyDisplay.instance.StopStarFlash();
            }
        }
    }
}