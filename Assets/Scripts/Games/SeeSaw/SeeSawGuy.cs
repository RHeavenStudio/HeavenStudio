using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SeeSaw
{
    public class SeeSawGuy : SuperCurveObject
    {
        public enum LandType
        {
            Big,
            Miss,
            Barely,
            Normal
        }
        public enum JumpState
        {
            None,
            StartJump,
            OutOut,
            InIn
        }
        [SerializeField] bool see;
        Animator anim;
        JumpState currentState;
        Path currentPath;
        SeeSaw game;
        float startBeat;
        float heightLastFrame;
        [SerializeField] Transform landOutTrans;
        [SerializeField] Transform landInTrans;

        private void Awake()
        {
            anim = transform.GetChild(0).GetComponent<Animator>();
            anim.Play(see ? "NeutralSee" : "NeutralSaw", 0, 0);
            game = SeeSaw.instance;
        }

        private void Update()
        {
            var cond = Conductor.instance;

            float currentBeat = cond.songPositionInBeats;

            if (cond.isPlaying && !cond.isPaused)
            {
                switch (currentState)
                {
                    default:
                        return;
                    case JumpState.StartJump:
                        currentPath = game.GetPath("SeeStartJump");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float height, startBeat);
                        if (height < heightLastFrame) anim.Play("Jump_OutOut_Fall", 0, 0);
                        heightLastFrame = height;
                        break;
                    case JumpState.OutOut:
                        currentPath = game.GetPath(see ? "SeeJumpOutOut" : "SawJumpOutOut");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat > startBeat + 1) anim.Play("Jump_OutOut_Fall", 0, 0);
                        break;
                    case JumpState.InIn:
                        currentPath = game.GetPath(see ? "SeeJumpInIn" : "SawJumpInIn");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat > startBeat + 0.5f) anim.Play("Jump_InIn_Fall", 0, 0);
                        break;
                }
            }
        }

        public void Land(bool landedOut, LandType landType)
        {
            string landOut = landedOut ? "Out" : "In";
            string typeOfLanding = "";
            switch (landType)
            {
                case LandType.Big:
                    typeOfLanding = "_Big";
                    break;
                case LandType.Miss:
                    typeOfLanding = "_Miss";
                    break;
                case LandType.Barely:
                    typeOfLanding = "_Barely";
                    break;
                default:
                    break;
            }
            string animName = "Land_" + landOut + typeOfLanding;
            anim.DoScaledAnimationAsync(animName, 0.5f);
            transform.position = landedOut ? landOutTrans.position : landInTrans.position;
            SetState(JumpState.None, 0);
        }



        public void SetState(JumpState state, float beat)
        {
            currentState = state;
            startBeat = beat;
            heightLastFrame = 0;
            switch (currentState)
            {
                case JumpState.OutOut:
                case JumpState.StartJump:
                    anim.DoScaledAnimationAsync("Jump_OutOut_Start", 0.5f);
                    break;
                case JumpState.InIn:
                    anim.DoScaledAnimationAsync("Jump_InIn_Start", 0.5f);
                    break;
                default:
                    break;
            }
        }
    }

}
