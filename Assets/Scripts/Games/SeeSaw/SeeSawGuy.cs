using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System.Resources;
using System.Net;
using System;

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
        [NonSerialized] public bool dead = false;
        public bool strum;
        public Animator anim;
        JumpState lastState;
        JumpState currentState;
        Path currentPath;
        Path cameraPath;
        SeeSaw game;
        float startBeat;
        float heightLastFrame;
        [NonSerialized] public bool canBop = true;
        [SerializeField] Transform landOutTrans;
        public Transform landInTrans;
        [SerializeField] Transform groundTrans;
        bool hasChangedAnimMidAir;
        [SerializeField] ParticleSystem deathParticle;

        [SerializeField] private Animator invertAnim;

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
                        if (height < heightLastFrame && !hasChangedAnimMidAir)
                        {
                            anim.Play("Jump_OutOut_Fall", 0, 0);
                            hasChangedAnimMidAir = true;
                        }

                        heightLastFrame = height;
                        break;
                    case JumpState.StartJumpIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float heightIn, startBeat);
                        if (heightIn < heightLastFrame && !hasChangedAnimMidAir)
                        {
                            anim.Play("Jump_InIn_Fall", 0, 0);
                            hasChangedAnimMidAir = true;
                        }

                        heightLastFrame = heightIn;
                        break;
                    case JumpState.OutOut:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1 && !hasChangedAnimMidAir)
                        {
                            anim.Play("Jump_OutOut_Fall", 0, 0);
                            hasChangedAnimMidAir = true;
                        }

                        break;
                    case JumpState.InIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f && !hasChangedAnimMidAir)
                        {
                            anim.Play("Jump_InIn_Fall", 0, 0);
                            hasChangedAnimMidAir = true;
                        }
                        break;
                    case JumpState.InOut:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 0.5f) 
                        {
                            if (!hasChangedAnimMidAir) anim.Play("Jump_InOut_Tuck", 0, 0);
                            hasChangedAnimMidAir = true;
                            transform.rotation = Quaternion.Euler(0, 0, (see ? 1 : -1) * Mathf.Lerp(0, 360, cond.GetPositionFromBeat(startBeat + 0.5f, 0.75f)));
                        } 
                        break;
                    case JumpState.OutIn:
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), startBeat);
                        if (currentBeat >= startBeat + 1f) 
                        {
                            if (!hasChangedAnimMidAir) anim.Play("Jump_OutIn_Tuck", 0, 0);
                            hasChangedAnimMidAir = true;
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
                        break;
                }
            }
        }

        private void LateUpdate()
        {
            var cond = Conductor.instance;

            float currentBeat = cond.songPositionInBeats;

            if (!see && game.cameraMove && cond.isPlaying && !cond.isPaused)
            {
                switch (currentState)
                {
                    default:
                        return;
                    case JumpState.HighOutOut:
                    case JumpState.HighOutIn:
                    case JumpState.HighInOut:
                    case JumpState.HighInIn:
                        float newCamY = Mathf.Max(GetPathPositionFromBeat(cameraPath, Mathf.Max(startBeat, currentBeat), startBeat).y, 0);
                        GameCamera.additionalPosition = new Vector3(0, newCamY, 0);
                        break;
                }
            }

        }

        public void Choke(float beat, float length)
        {
            if (!canBop || currentState != JumpState.None || dead) return;
            dead = true;
            anim.DoScaledAnimationAsync("Choke_" + (see ? "See" : "Saw") + "_Intro", 0.5f);
            Jukebox.PlayOneShotGame("seeSaw/explosion" + (see ? "Black" : "White"), beat + length);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length - 1, delegate { invertAnim.DoScaledAnimationAsync("Invert", 0.5f); }),
                new BeatAction.Action(beat + length, delegate { anim.DoScaledAnimationAsync("Explode", 0.5f); deathParticle.Play();})
            });
        }

        public void Bop()
        {
            if (!canBop || currentState != JumpState.None || dead) return;
            anim.DoScaledAnimationAsync("Bop" + (see ? "See" : "Saw") + (strum ? "_Strum" : ""), 0.5f);
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
            if (landType is LandType.Big && !see)
            {
                game.SpawnOrbs(!landedOut, Conductor.instance.songPositionInBeats);
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
            if (landType is not LandType.Barely)
            {
                string getUpAnim = "GetUp_" + landOut + typeOfLanding;
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
                case JumpState.HighOutOut:
                case JumpState.HighInOut:
                    return true;
            }
        }

        public void SetState(JumpState state, float beat, bool miss = false, float height = 0)
        {
            lastState = currentState;
            currentState = state;
            startBeat = beat;
            heightLastFrame = 0;
            hasChangedAnimMidAir = false;
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
                    cameraPath.positions[0].height = Mathf.Lerp(10, 26, height);
                    cameraPath.positions[0].duration = 2f;
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutOut_Start", 0.5f);
                    if (see) return;
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate { anim.DoScaledAnimationAsync("Jump_OutOut_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighOutIn:
                    currentPath = game.GetPath(see ? "SeeHighOutIn" : "SawHighOutIn");
                    currentPath.positions[0].height = Mathf.Lerp(12, 28, height);
                    cameraPath.positions[0].height = Mathf.Lerp(10, 26, height);
                    cameraPath.positions[0].duration = 2f;
                    anim.DoScaledAnimationAsync(miss ? "BadOut_SeeReact" : "Jump_OutIn_Start", 0.5f);
                    if (see) return;
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate { anim.DoScaledAnimationAsync("Jump_OutIn_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighInOut:
                    currentPath = game.GetPath(see ? "SeeHighInOut" : "SawHighInOut");
                    currentPath.positions[0].height = Mathf.Lerp(9, 20, height);
                    cameraPath.positions[0].height = Mathf.Lerp(7, 18, height);
                    cameraPath.positions[0].duration = 1f;
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    if (see) return;
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5f, delegate { anim.DoScaledAnimationAsync("Jump_OutOut_Transform", 0.5f); })
                    });
                    break;
                case JumpState.HighInIn:
                    currentPath = game.GetPath(see ? "SeeHighInIn" : "SawHighInIn");
                    currentPath.positions[0].height = Mathf.Lerp(9, 20, height);
                    cameraPath.positions[0].height = Mathf.Lerp(7, 18, height);
                    cameraPath.positions[0].duration = 1f;
                    anim.DoScaledAnimationAsync(miss ? "BadIn_SeeReact" : "Jump_InIn_Start", 0.5f);
                    if (see) return;
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
