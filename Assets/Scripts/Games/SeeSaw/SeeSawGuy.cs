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
            StartJump
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
                    case JumpState.None:
                        return;
                    case JumpState.StartJump:
                        currentPath = game.GetPath("SeeStartJump");
                        transform.position = GetPathPositionFromBeat(currentPath, Mathf.Max(startBeat, currentBeat), out float height, startBeat);
                        if (height < heightLastFrame) anim.Play("Jump_OutOut_Fall", 0, 0);
                        heightLastFrame = height;
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
            switch (currentState)
            {
                case JumpState.StartJump:
                    anim.DoScaledAnimationAsync("Jump_OutOut_Start", 0.5f);
                    break;
                default:
                    break;
            }
        }
    }

}
