using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_PajamaParty
{
    public class CtrPillowPlayer : MonoBehaviour
    {
        [Header("Objects")]
        public GameObject Player;
        public GameObject Shadow;

        public Animator anim;
        float startJumpTime = Single.MinValue;
        float jumpLength = 0;
        float jumpHeight = 0;

        private bool hasJumped = false;
        private bool canJump = true;

        private bool charging = false;
        private bool canCharge = true;

        void Awake()
        {
            anim = Player.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            // TESTING REMOVE FOR PROD
                if (PlayerInput.Pressed() && canJump)
                {
                    PlayerJump(cond.songPositionInBeats);
                }
                if (PlayerInput.AltPressed() && canCharge)
                {
                    StartCharge();
                }
                if (PlayerInput.AltPressedUp() && charging)
                {
                    EndCharge(cond.songPositionInBeats);
                }
            //

            float jumpPos = cond.GetPositionFromBeat(startJumpTime, jumpLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                Player.transform.localPosition = new Vector3(0, jumpHeight * yWeight + 0.25f);
                Shadow.transform.localScale = new Vector3((1f-yWeight*0.2f) * 1.65f, (1f-yWeight*0.2f), 1f);

                anim.Play("MakoJump", 0, jumpPos);
                anim.speed = 0;
            }
            else
            {
                if (hasJumped)
                {
                    canJump = true;
                    canCharge = true;
                    hasJumped = false;
                    PajamaParty.instance.DoBedImpact();
                    anim.Play("MakoLand", -1, 0);
                    anim.speed = 1f / cond.pitchedSecPerBeat;
                }
                startJumpTime = Single.MinValue;
                Player.transform.localPosition = new Vector3(0, 0);
                Shadow.transform.localScale = new Vector3(1.65f, 1f, 1f);
            }
        }

        public void PlayerJump(float beat)
        {
            startJumpTime = beat;
            canCharge = false;
            canJump = false;

            //temp
            jumpLength = 1f;
            jumpHeight = 4f;
        }

        public void StartCharge()
        {
            canJump = false;
            anim.Play("MakoReady");
            anim.speed = 1f;
            charging = true;
        }

        public void EndCharge(float beat, bool hit = false)
        {
            var cond = Conductor.instance;
            charging = false;
            canCharge = false;
            if (hit)
            {
                anim.Play("MakoThrow");
                anim.speed = 1f; 
                //TODO: spawn projectile

                BeatAction.New(Player, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(
                        beat + 4f,
                        delegate { 
                            anim.Play("MakoCatch", -1, 0); 
                            anim.speed = 1f; 
                            canCharge = true;
                            canJump = true;
                        }
                    ),
                });
            }
            else
            {
                anim.Play("MakoThrowOut");
                anim.speed = 1f / cond.pitchedSecPerBeat;
                BeatAction.New(Player, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(
                        beat + 0.5f,
                        delegate { 
                            anim.Play("MakoPickUp", -1, 0);
                            anim.speed = 1f / cond.pitchedSecPerBeat;
                        }
                    ),
                    new BeatAction.Action(
                        beat + 1f,
                        delegate { 
                            canCharge = true;
                            canJump = true;
                        }
                    ),
                });
            }
        }
    }
}
