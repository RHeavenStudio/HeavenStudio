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
            StartJumpIn,
            OutOut,
            InIn,
            InOut,
            OutIn,
            EndJumpOut,
            EndJumpIn
        }
        [SerializeField] bool see;
        public Animator anim;
        JumpState currentState;
        Path currentPath;
        SeeSaw game;
        float startBeat;
        float heightLastFrame;
        [SerializeField] Transform landOutTrans;
        public Transform landInTrans;
        [SerializeField] Transform groundTrans;

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
                    case JumpState.StartJumpIn:
                        currentPath = game.GetPath("SeeStartJumpIn");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float heightIn, startBeat);
                        if (heightIn < heightLastFrame) anim.Play("Jump_InIn_Fall", 0, 0);
                        heightLastFrame = heightIn;
                        break;
                    case JumpState.OutOut:
                        currentPath = game.GetPath(see ? "SeeJumpOutOut" : "SawJumpOutOut");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1) anim.Play("Jump_OutOut_Fall", 0, 0);
                        break;
                    case JumpState.InIn:
                        currentPath = game.GetPath(see ? "SeeJumpInIn" : "SawJumpInIn");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f) anim.Play("Jump_InIn_Fall", 0, 0);
                        break;
                    case JumpState.InOut:
                        currentPath = game.GetPath(see ? "SeeJumpInOut" : "SawJumpInOut");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f) 
                        {
                            anim.Play("Jump_InOut_Tuck", 0, 0);
                            transform.rotation = Quaternion.Euler(0, 0, (see ? 1 : -1) * Mathf.Lerp(0, 360, cond.GetPositionFromBeat(startBeat + 0.5f, 0.75f)));
                        } 
                        break;
                    case JumpState.OutIn:
                        currentPath = game.GetPath(see ? "SeeJumpOutIn" : "SawJumpOutIn");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1f) 
                        {
                            anim.Play("Jump_OutIn_Tuck", 0, 0);
                            transform.rotation = Quaternion.Euler(0, 0, (see ? -1 : 1) * Mathf.Lerp(0, 360, cond.GetPositionFromBeat(startBeat + 1f, 1f)));
                        }
                        break;
                    case JumpState.EndJumpOut:
                        currentPath = game.GetPath("SeeEndJumpOut");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        break;
                    case JumpState.EndJumpIn:
                        currentPath = game.GetPath("SeeEndJumpIn");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        break;
                }
            }
        }

        public void Land(LandType landType)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            bool landedOut = false;
            switch (currentState)
            {
                default:
                    break;
                case JumpState.InOut:
                case JumpState.OutOut:
                case JumpState.StartJump:
                    landedOut = true;
                    break;
                case JumpState.EndJumpOut:
                case JumpState.EndJumpIn:
                    anim.Play("NeutralSee", 0, 0);
                    transform.position = groundTrans.position;
                    SetState(JumpState.None, 0);
                    return;
            }
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
                case JumpState.InOut:
                case JumpState.StartJumpIn:
                    anim.DoScaledAnimationAsync("Jump_InIn_Start", 0.5f);
                    break;
                case JumpState.OutIn:
                case JumpState.EndJumpOut:
                case JumpState.EndJumpIn:
                    anim.DoScaledAnimationAsync("Jump_OutIn_Start", 0.5f);
                    break;
                default:
                    break;
            }
        }
    }

}
