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
        public GameObject Projectile_Root;

        public Animator anim;
        float startJumpTime = Single.MinValue;
        float jumpLength = 0;
        float jumpHeight = 0;
        bool jumpNg = false;

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
        bool throwNg = false;

        public bool canSleep = false;

        void Awake()
        {
            anim = Player.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.Pressed() && canJump && !PajamaParty.instance.IsExpectingInputNow())
            {
                Jukebox.PlayOneShot("miss");
                PlayerJump(cond.songPositionInBeats, true, false);
            }
            if (PlayerInput.AltPressed() && canCharge)
            {
                StartCharge();
            }
            if (PlayerInput.AltPressedUp() && charging && !PajamaParty.instance.IsExpectingInputNow())
            {
                Jukebox.PlayOneShot("miss");
                EndCharge(cond.songPositionInBeats, false, false);
            }

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
                anim.Play("MakoJump", -1, jumpPos);
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
                    if (jumpNg)
                        anim.Play("MakoCatchNg", -1, 0);
                    else if (jumpHeight != 4f)
                        anim.Play("MakoCatch", -1, 0);
                    else
                        anim.Play("MakoLand", -1, 0);
                    anim.speed = 1f / cond.pitchedSecPerBeat;
                    jumpNg = false;
                }
                startJumpTime = Single.MinValue;
                Player.transform.localPosition = new Vector3(0, 0);
                Shadow.transform.localScale = new Vector3(1.65f, 1f, 1f);
            }

            //thrown pillow logic
            jumpPos = cond.GetPositionFromBeat(startThrowTime, throwLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                if (throwType)
                {
                    hasThrown = true;
                    float yMul = jumpPos * 2f - 1f;
                    float yWeight = -(yMul*yMul) + 1f;
                    Projectile_Root.transform.localPosition = new Vector3(0, throwHeight * yWeight + 0.5f);
                }
                else
                {
                    Projectile.GetComponent<Animator>().Play("ThrowOut", -1, jumpPos);
                    Projectile.GetComponent<Animator>().speed = 0;
                }
                Projectile.transform.rotation = Quaternion.Euler(0, 0, Projectile.transform.rotation.eulerAngles.z - (360f * Time.deltaTime));
            }
            else
            {
                Projectile.GetComponent<Animator>().Play("NoPose", -1, 0);
                startThrowTime = Single.MinValue;
                Projectile_Root.transform.localPosition = new Vector3(0, 0);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (hasThrown)
                {
                    if (throwNg)
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
                    throwNg = false;

                    canCharge = true;
                    canJump = true;
                }
            }
        }

        public void ProjectileThrow(float beat, bool drop = false, bool ng = false)
        {
            throwNg = ng;
            Projectile.SetActive(true);
            startThrowTime = beat;
            if (drop)
            {
                throwType = false;
                Projectile.GetComponent<Animator>().Play("ThrowOut", -1, 0);
                Projectile.GetComponent<Animator>().speed = 0;
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 360f * UnityEngine.Random.Range(0f, 1f));
                throwLength = 0.5f;
            }
            else
            {
                throwType = true;
                throwHeight = ng ? 1.5f : 12f;
                throwLength = ng ? 1f : 4f;
            }
        }

        public void PlayerJump(float beat, bool pressout = false, bool ng = false)
        {
            startJumpTime = beat;
            canCharge = false;
            canJump = false;

            //temp
            jumpLength = (ng || pressout) ? 0.5f : 1f;
            jumpHeight = (ng || pressout) ? 2f : 4f;
            jumpNg = ng;
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
                anim.speed = (1f / cond.pitchedSecPerBeat) * 0.5f;
                BeatAction.New(Player, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(
                        beat + 0.5f,
                        delegate { 
                            anim.Play("MakoPickUp", -1, 0);
                            Jukebox.PlayOneShotGame("pajamaParty/jp/catch" + UnityEngine.Random.Range(0, 2)); //bruh
                            anim.speed = 1f / cond.pitchedSecPerBeat;
                            Projectile.SetActive(false);
                            canCharge = true;
                            canJump = true;
                        }
                    )
                });
            }
        }

        public void PlayerThrough(float beat)
        {
            var cond = Conductor.instance;
            anim.Play("MakoThrough", -1, 0);
            anim.speed = (1f / cond.pitchedSecPerBeat) * 0.5f;
            charging = false;
            canCharge = false;
            canJump = false;
            BeatAction.New(Player, new List<BeatAction.Action>()
            {
                new BeatAction.Action(
                    beat + 0.5f,
                    delegate { 
                        canCharge = true;
                        canJump = true;
                    }
                )
            });
        }

        // jumping cues (timings for both are the same)
            public void ScheduleJump(float beat)
            {
                PajamaParty.instance.ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, JumpJustOrNg, JumpThrough, JumpOut);
            }

            public void JumpJustOrNg(PlayerActionEvent caller, float state)
            {
                if (canJump)
                { 
                    var cond = Conductor.instance;
                    if (state <= -1f || state >= 1f)
                    {
                        Jukebox.PlayOneShot("miss");
                        PlayerJump(cond.songPositionInBeats, false, true);
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("pajamaParty/jumpJust");
                        PlayerJump(cond.songPositionInBeats, false, false);
                    }
                    caller.CanHit(false);
                }
            }

            public void JumpOut(PlayerActionEvent caller) {}

            public void JumpThrough(PlayerActionEvent caller)
            {
                if (canJump)
                {    
                    var cond = Conductor.instance;
                    PlayerThrough(cond.songPositionInBeats);
                }
            }
        //////

        // throw cue
            public void ScheduleThrow(float beat)
            {
                PajamaParty.instance.ScheduleInput(beat, 2f, InputType.STANDARD_ALT_DOWN, ChargeJustOrNg, ThrowThrough, JumpOut);
                PajamaParty.instance.ScheduleInput(beat, 3f, InputType.STANDARD_ALT_UP, ThrowJustOrNg, ThrowThrough, JumpOut);
            }

            public void ChargeJustOrNg(PlayerActionEvent caller, float state) {
                StartCharge();
                throwNg = (state <= -1f || state >= 1f);
                Jukebox.PlayOneShotGame("pajamaParty/jp/throw4");
            }

            public void ThrowJustOrNg(PlayerActionEvent caller, float state)
            {
                if (charging)
                { 
                    var cond = Conductor.instance;
                    if (state <= -1f || state >= 1f)
                    {
                        Jukebox.PlayOneShot("miss");
                        EndCharge(cond.songPositionInBeats, true, true);
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("pajamaParty/jp/throw5");
                        EndCharge(cond.songPositionInBeats, true, (throwNg || false));
                    }
                    caller.CanHit(false);
                }
            }

            public void ThrowThrough(PlayerActionEvent caller)
            {
                if (canCharge)
                {    
                    var cond = Conductor.instance;
                    PlayerThrough(cond.songPositionInBeats);
                }
            }
        //

        // sleep cue
            public void StartSleepSequence(float beat, bool alt)
            {
                if (hasJumped)
                {
                    hasJumped = false;
                    PajamaParty.instance.DoBedImpact();
                    jumpNg = false;
                }
                startJumpTime = Single.MinValue;
                Player.transform.localPosition = new Vector3(0, 0);
                Shadow.transform.localScale = new Vector3(1.65f, 1f, 1f);

                Projectile.GetComponent<Animator>().Play("NoPose", -1, 0);
                startThrowTime = Single.MinValue;
                Projectile_Root.transform.localPosition = new Vector3(0, 0);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (hasThrown)
                {
                    Projectile.SetActive(false);
                    hasThrown = false;
                }

                PajamaParty.instance.ScheduleInput(beat, 4f, InputType.STANDARD_DOWN, SleepJustOrNg, SleepThrough, SleepOut);

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
                        beat,
                        delegate { anim.Play("MakoSleep00", -1, 0); 
                            anim.speed = 1f / cond.pitchedSecPerBeat;
                        }
                    ),
                    new BeatAction.Action(
                        beat + 0.5f,
                        delegate { anim.Play("MakoSleep01", -1, 0); 
                            anim.speed = 1f;
                        }
                    ),
                    new BeatAction.Action(
                        beat + 1f,
                        delegate { 
                            canSleep = true;
                        }
                    ),
                    new BeatAction.Action(
                        beat + 3f,
                        delegate { 
                            if (canSleep)
                            {
                                anim.Play(alt ? "MakoReadySleep01" : "MakoReadySleep", -1, 0); 
                                anim.speed = 1f / cond.pitchedSecPerBeat;
                            }
                        }
                    ),
                    new BeatAction.Action(
                        beat + 8f,
                        delegate { 
                            canCharge = true;
                            canJump = true;
                        }
                    ),
                });
            }

            public void SleepJustOrNg(PlayerActionEvent caller, float state)
            {
                var cond = Conductor.instance;
                if (canSleep)
                {  
                    caller.CanHit(false);
                    canSleep = false;
                    if (state <= -1f || state >= 1f)
                    {
                        anim.Play("MakoSleepNg", -1, 0);
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("pajamaParty/siesta4");
                        anim.Play("MakoSleepJust", -1, 0); 
                        anim.speed = 1f / cond.pitchedSecPerBeat;

                        BeatAction.New(Player, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(
                                caller.startBeat + 7f,
                                delegate { 
                                    anim.Play("MakoAwake", -1, 0);
                                    anim.speed = 1f / cond.pitchedSecPerBeat;

                                    Jukebox.PlayOneShotGame("pajamaParty/siestaDone");
                                }
                            ),
                        });
                    }
                }
            }

            public void SleepThrough(PlayerActionEvent caller)
            {
                var cond = Conductor.instance;
                if (canSleep)
                {
                    anim.Play("MakoSleepThrough", -1, 0);
                    anim.speed = 1f / cond.pitchedSecPerBeat;
                    caller.CanHit(false);
                    canSleep = false;
                }
            }

            public void SleepOut(PlayerActionEvent caller)
            {
                var cond = Conductor.instance;
                if (canSleep)
                {
                    anim.Play("MakoSleepOut", -1, 0);
                    anim.speed = (1f / cond.pitchedSecPerBeat) * 0.5f;

                    Jukebox.PlayOneShotGame("pajamaParty/siestaBad");
                    caller.CanHit(false);
                    canSleep = false;
                }
            }
        //////
    }
}
