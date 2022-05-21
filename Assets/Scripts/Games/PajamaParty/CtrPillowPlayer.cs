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
        public GameObject Projectile;

        public Animator anim;
        float startJumpTime = Single.MinValue;
        float jumpLength = 0;
        float jumpHeight = 0;

        private bool hasJumped = false;
        private bool canJump = true;

        private bool charging = false;
        private bool canCharge = true;

        float startThrowTime = Single.MinValue;
        float throwLength = 0;
        float throwHeight = 0;
        // true = throw, false = dropped ("Out")
        bool throwType = true;
        bool hasThrown = false;

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

            // mako jumping logic
            float jumpPos = cond.GetPositionFromBeat(startJumpTime, jumpLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                Player.transform.localPosition = new Vector3(0, jumpHeight * yWeight);
                Shadow.transform.localScale = new Vector3((1f-yWeight*0.2f) * 1.65f, (1f-yWeight*0.2f), 1f);
                // handles the shirt lifting
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

            //thrown pillow logic
            jumpPos = cond.GetPositionFromBeat(startThrowTime, throwLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasThrown = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                Projectile.transform.localPosition = new Vector3(0, throwHeight * yWeight + 0.5f);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, Projectile.transform.rotation.eulerAngles.z - (360f * Time.deltaTime));
            }
            else
            {
                startThrowTime = Single.MinValue;
                Projectile.transform.localPosition = new Vector3(0, 0);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (hasThrown)
                {
                    if (throwLength == 1f)    //ew hardcoded change later
                    {
                        anim.Play("MakoCatchNg", -1, 0);
                    }
                    else
                    {
                        anim.Play("MakoCatch", -1, 0);
                    }
                    //TODO: change when locales are a thing
                    Jukebox.PlayOneShotGame("pajamaParty/jp/catch" + UnityEngine.Random.Range(0, 2)); //bruh
                    
                    anim.speed = 1f;
                    Projectile.SetActive(false);
                    hasThrown = false;


                    canCharge = true;
                    canJump = true;
                }
            }
        }

        public void ProjectileThrow(float beat, bool drop = false, bool ng = false)
        {
            if (drop)
            {
                // fuckkkk have to animate the pillow bouncing
            }
            else
            {
                Projectile.SetActive(true);
                startThrowTime = beat;
                throwHeight = ng ? 1.5f : 12f;
                throwLength = ng ? 1f : 4f;
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

        public void EndCharge(float beat, bool hit = true, bool ng = false)
        {
            ProjectileThrow(beat, !hit, ng);
            var cond = Conductor.instance;
            charging = false;
            canCharge = false;
            if (hit)
            {
                anim.Play("MakoThrow");
                anim.speed = 1f; 
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
                            
                            canCharge = true;
                            canJump = true;
                        }
                    )
                });
            }
        }

        public void StartSleepSequence(float beat)
        {
            var cond = Conductor.instance;
            charging = false;
            canCharge = false;
            canJump = false;
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

            BeatAction.New(Player, new List<BeatAction.Action>()
            {
                new BeatAction.Action(
                    beat + 8f,
                    delegate { 
                        canCharge = true;
                        canJump = true;
                    }
                ),
            });
        }
    }
}
