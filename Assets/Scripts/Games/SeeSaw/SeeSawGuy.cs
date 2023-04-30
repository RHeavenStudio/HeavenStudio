using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System.Resources;
using System.Net;

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
            EndJumpIn,
            HighOutOut,
            HighOutIn,
            HighInOut,
            HighInIn
        }
        [SerializeField] bool see;
        public Animator anim;
        JumpState lastState;
        JumpState currentState;
        Path currentPath;
        Path cameraPath;
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
            cameraPath = game.GetPath("Camera");
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
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float height, startBeat);
                        if (height < heightLastFrame) anim.Play("Jump_OutOut_Fall", 0, 0);
                        heightLastFrame = height;
                        break;
                    case JumpState.StartJumpIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float heightIn, startBeat);
                        if (heightIn < heightLastFrame) anim.Play("Jump_InIn_Fall", 0, 0);
                        heightLastFrame = heightIn;
                        break;
                    case JumpState.OutOut:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1) anim.Play("Jump_OutOut_Fall", 0, 0);
                        break;
                    case JumpState.InIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f) anim.Play("Jump_InIn_Fall", 0, 0);
                        break;
                    case JumpState.InOut:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f) 
                        {
                            anim.Play("Jump_InOut_Tuck", 0, 0);
                            transform.rotation = Quaternion.Euler(0, 0, (see ? 1 : -1) * Mathf.Lerp(0, 360, cond.GetPositionFromBeat(startBeat + 0.5f, 0.75f)));
                        } 
                        break;
                    case JumpState.OutIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1f) 
                        {
                            anim.Play("Jump_OutIn_Tuck", 0, 0);
                            transform.rotation = Quaternion.Euler(0, 0, (see ? -1 : 1) * Mathf.Lerp(0, 360, cond.GetPositionFromBeat(startBeat + 1f, 1f)));
                        }
                        break;
                    case JumpState.EndJumpOut:
                    case JumpState.EndJumpIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        break;
                    case JumpState.HighOutOut:
                    case JumpState.HighOutIn:
                    case JumpState.HighInOut:
                    case JumpState.HighInIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (see) return;
                        GameCamera.additionalPosition = GetPathPositionFromBeat(cameraPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        break;
                }
            }
        }

        public void Land(LandType landType, bool getUpOut)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            GameCamera.additionalPosition = Vector3.zero;
            bool landedOut = false;
            switch (currentState)
            {
                default:
                    break;
                case JumpState.InOut:
                case JumpState.OutOut:
                case JumpState.StartJump:
                case JumpState.HighOutOut:
                case JumpState.HighInOut:
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
            if (landType is not LandType.Barely)
            {
                string getUpAnim = "GetUp_" + landOut + typeOfLanding;
                anim.DoScaledAnimationAsync(animName, 0.5f);
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeats + (getUpOut ? 1f : 0.5f), delegate { anim.DoScaledAnimationAsync(getUpAnim, 0.5f); })
                });
            }
            transform.position = landedOut ? landOutTrans.position : landInTrans.position;
            SetState(JumpState.None, 0);
        }

        public bool ShouldEndJumpOut()
        {
            switch (lastState)
            {
                default:
                    return false;
                case JumpState.InOut:
                case JumpState.OutOut:
                case JumpState.StartJump:
                    return true;
            }
        }

        public void SetState(JumpState state, float beat, bool miss = false, float height = 0)
        {
            lastState = currentState;
            currentState = state;
            startBeat = beat;
            heightLastFrame = 0;
            switch (currentState)
            {
                case JumpState.OutOut:
                    currentPath = game.GetPath(see ? "SeeJumpOutOut" : "SawJumpOutOut");
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutOut_Start", 0.5f);
                    break;
                case JumpState.StartJump:
                    currentPath = game.GetPath("SeeStartJump");
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutOut_Start", 0.5f);
                    break;
                case JumpState.InIn:
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    currentPath = game.GetPath(see ? "SeeJumpInIn" : "SawJumpInIn");
                    break;
                case JumpState.InOut:
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    currentPath = game.GetPath(see ? "SeeJumpInOut" : "SawJumpInOut");
                    break;
                case JumpState.StartJumpIn:
                    currentPath = game.GetPath("SeeStartJumpIn");
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    break;
                case JumpState.OutIn:
                    currentPath = game.GetPath(see ? "SeeJumpOutIn" : "SawJumpOutIn");
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutIn_Start", 0.5f);
                    break;
                case JumpState.EndJumpOut:
                    currentPath = game.GetPath("SeeEndJumpOut");
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutIn_Start", 0.5f);
                    break;
                case JumpState.EndJumpIn:
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutIn_Start", 0.5f);
                    currentPath = game.GetPath("SeeEndJumpIn");
                    break;
                case JumpState.HighOutOut:
                    currentPath = game.GetPath(see ? "SeeHighOutOut" : "SawHighOutOut");
                    currentPath.positions[0].height = Mathf.Lerp(12, 28, height);
                    cameraPath.positions[0].height = Mathf.Lerp(8, 24, height);
                    cameraPath.positions[0].duration = 2f;
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutOut_Start", 0.5f);
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate { anim.DoScaledAnimationAsync("Jump_OutOut_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighOutIn:
                    currentPath = game.GetPath(see ? "SeeHighOutIn" : "SawHighOutIn");
                    currentPath.positions[0].height = Mathf.Lerp(12, 28, height);
                    cameraPath.positions[0].height = Mathf.Lerp(8, 24, height);
                    cameraPath.positions[0].duration = 2f;
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutIn_Start", 0.5f);
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate { anim.DoScaledAnimationAsync("Jump_OutIn_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighInOut:
                    currentPath = game.GetPath(see ? "SeeHighInOut" : "SawHighInOut");
                    currentPath.positions[0].height = Mathf.Lerp(9, 20, height);
                    cameraPath.positions[0].height = Mathf.Lerp(5, 16, height);
                    cameraPath.positions[0].duration = 1f;
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5f, delegate { anim.DoScaledAnimationAsync("Jump_OutOut_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighInIn:
                    currentPath = game.GetPath(see ? "SeeHighInIn" : "SawHighInIn");
                    currentPath.positions[0].height = Mathf.Lerp(9, 20, height);
                    cameraPath.positions[0].height = Mathf.Lerp(5, 16, height);
                    cameraPath.positions[0].duration = 1f;
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5f, delegate { anim.DoScaledAnimationAsync("Jump_OutIn_Transform", 0.5f); })
                    });
                    break;
                default:
                    break;
            }
        }
    }

}
