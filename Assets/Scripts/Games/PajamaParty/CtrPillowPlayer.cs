using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_PajamaParty
{
    public class CtrPillowPlayer : MonoBehaviour
    {
        const string HighSuffix = "_H";
        const string NrmSuffix = "";

        [Header("Objects")]
        [SerializeField] GameObject Player;
        [SerializeField] GameObject Shadow;
        [SerializeField] GameObject Projectile;
        [SerializeField] GameObject Projectile_Root;

        public Animator anim { get; private set; }
        string animSuffix => (PajamaParty.instance.HighState ? HighSuffix : NrmSuffix);

        double lastReportedBeat;
        double startJumpTime = double.MinValue;
        float jumpLength = 0;
        float jumpHeight = 0;
        bool jumpNg = false;

        bool canJump = true;
        bool hasJumped = false;

        private bool charging = false;
        private bool canCharge = true;

        private bool startedSleeping = false;

        double startThrowTime = double.MinValue;
        double throwLength = 0;
        float throwHeight = 0;

        bool throwType = true; // true = throw, false = dropped ("Out")
        bool hasThrown = false;
        bool throwNg = false;
        bool longSleep = false;

        public bool canSleep = false;
        public bool shouldBop = false;

        void Awake()
        {
            anim = Player.GetComponent<Animator>();
            longSleep = false;
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (PlayerInput.GetIsAction(PajamaParty.InputAction_BasicPress) && canJump && !PajamaParty.instance.IsExpectingInputNow(PajamaParty.InputAction_BasicPress))
            {
                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch
                    || (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !PajamaParty.instance.IsExpectingInputNow(PajamaParty.InputAction_AltStart)))
                {
                    SoundByte.PlayOneShot("miss");
                    PlayerJump(cond.songPositionInBeatsAsDouble, true, false);
                    PajamaParty.instance.ScoreMiss();
                }
            }
            if (PlayerInput.GetIsAction(PajamaParty.InputAction_AltStart) && canCharge)
            {
                StartCharge();
            }
            if (PlayerInput.GetIsAction(PajamaParty.InputAction_AltFinish)
                && charging && !PajamaParty.instance.IsExpectingInputNow(PajamaParty.InputAction_AltFinish))
            {
                SoundByte.PlayOneShot("miss");
                EndCharge(cond.songPositionInBeatsAsDouble, false, false);
                PajamaParty.instance.ScoreMiss();
            }
            if (PlayerInput.GetIsAction(PajamaParty.InputAction_TouchRelease)
                && charging && !PajamaParty.instance.IsExpectingInputNow(PajamaParty.InputAction_AltFinish))
            {
                SoundByte.PlayOneShot("miss");
                PlayerThrough(cond.songPositionInBeatsAsDouble);
            }

            // mako jumping logic
            float jumpPos = cond.GetPositionFromBeat(startJumpTime, jumpLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                Player.transform.localPosition = new Vector3(0, jumpHeight * yWeight);
                Shadow.transform.localScale = new Vector3((1f - yWeight * 0.2f) * 1.65f, (1f - yWeight * 0.2f), 1f);
                // handles the shirt lifting
                anim.DoScaledAnimation("MakoJump" + animSuffix, startJumpTime, jumpLength);
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
                        anim.DoScaledAnimationAsync("MakoCatchNg" + animSuffix);
                    else if (jumpHeight != 4f)
                        anim.DoScaledAnimationAsync("MakoCatch" + animSuffix);
                    else
                        anim.DoScaledAnimationAsync("MakoLand" + animSuffix);
                    jumpNg = false;
                }
                startJumpTime = double.MinValue;
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
                    float yWeight = -(yMul * yMul) + 1f;
                    Projectile_Root.transform.localPosition = new Vector3(0, throwHeight * yWeight + 0.5f);
                }
                else
                {
                    Projectile.GetComponent<Animator>().DoScaledAnimation("ThrowOut", startThrowTime, throwLength);
                }
                Projectile.transform.rotation = Quaternion.Euler(0, 0, Projectile.transform.rotation.eulerAngles.z - (360f * Time.deltaTime));
            }
            else
            {
                startThrowTime = double.MinValue;
                Projectile_Root.transform.localPosition = new Vector3(0, 0);
                if (hasThrown)
                {
                    Projectile.GetComponent<Animator>().Play("NoPose", -1, 0);
                    Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                    if (throwNg)
                    {
                        anim.DoUnscaledAnimation("MakoCatchNg" + animSuffix);
                    }
                    else
                    {
                        anim.DoUnscaledAnimation("MakoCatch" + animSuffix);
                    }
                    //TODO: change when locales are a thing
                    SoundByte.PlayOneShotGame("pajamaParty/catch" + UnityEngine.Random.Range(0, 2)); //bruh

                    Projectile.SetActive(false);
                    hasThrown = false;
                    throwNg = false;

                    canCharge = true;
                    canJump = true;
                }
            }
        }

        private void LateUpdate()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && anim.IsAnimationNotPlaying() && !hasThrown && !startedSleeping && canCharge && shouldBop)
            {
                anim.DoScaledAnimationAsync("MakoBeat" + animSuffix, 0.5f);
            }
        }

        public void ProjectileThrow(double beat, bool drop = false, bool ng = false)
        {
            throwNg = ng;
            Projectile.SetActive(true);
            startThrowTime = beat;
            if (drop)
            {
                throwType = false;
                throwLength = 0.5;
                Projectile.GetComponent<Animator>().DoScaledAnimation("ThrowOut", startThrowTime, throwLength);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 360f * UnityEngine.Random.Range(0f, 1f));
            }
            else
            {
                throwType = true;
                throwHeight = ng ? 1.5f : 14f;
                throwLength = ng ? 1 : 4;
            }
        }

        public void PlayerJump(double beat, bool pressout = false, bool ng = false)
        {
            startedSleeping = false;
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
            startedSleeping = false;
            canJump = false;
            anim.DoUnscaledAnimation("MakoReady" + animSuffix);
            charging = true;
        }

        public void EndCharge(double beat, bool hit = true, bool ng = false)
        {
            ProjectileThrow(beat, !hit, ng);
            var cond = Conductor.instance;
            charging = false;
            canCharge = false;
            var game = PajamaParty.instance;
            if (game.ExpectHigh)
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new
                    (
                        beat + 0.5,
                        delegate
                        {
                            game.ToggleHighState(hit && !ng, beat + 0.5);
                        }
                    ),
                    new
                    (
                        beat + 2,
                        delegate
                        {
                            if (hit && !ng)
                            {
                                anim.DoUnscaledAnimation("MakoThrow" + animSuffix, 1);
                                game.PrepareHighState();
                            }
                        }
                    )
                });
            }
            if (hit)
            {
                anim.DoUnscaledAnimation("MakoThrow" + animSuffix);
            }
            else
            {
                anim.DoScaledAnimationAsync("MakoThrowOut" + animSuffix, 0.5f);
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new
                    (
                        beat + 0.5,
                        delegate
                        {
                            anim.DoScaledAnimationAsync("MakoPickUp" + animSuffix);
                            SoundByte.PlayOneShotGame("pajamaParty/catch" + UnityEngine.Random.Range(0, 2)); //bruh
                            Projectile.SetActive(false);
                            canCharge = true;
                            canJump = true;
                        }
                    )
                });
            }
        }

        public void PlayerThrough(double beat)
        {
            var cond = Conductor.instance;
            anim.DoScaledAnimationAsync("MakoThrough" + animSuffix, 0.5f);
            charging = false;
            canCharge = false;
            canJump = false;
            BeatAction.New(this, new List<BeatAction.Action>()
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
        public bool CanJump()
        {
            return canJump;
        }

        public void ScheduleJump(double beat)
        {
            PajamaParty.instance.ScheduleInput(beat, 2f, PajamaParty.InputAction_BasicPress, JumpJustOrNg, JumpThrough, JumpOut, CanJump);
        }

        public void JumpJustOrNg(PlayerActionEvent caller, float state)
        {
            if (canJump)
            {
                var cond = Conductor.instance;
                if (state <= -1f || state >= 1f)
                {
                    SoundByte.PlayOneShot("miss");
                    PlayerJump(cond.songPositionInBeatsAsDouble, false, true);
                }
                else
                {
                    SoundByte.PlayOneShotGame("pajamaParty/jumpJust");
                    PlayerJump(cond.songPositionInBeatsAsDouble, false, false);
                }
                caller.CanHit(false);
            }
        }

        public void JumpOut(PlayerActionEvent caller) { }

        public void JumpThrough(PlayerActionEvent caller)
        {
            if (canJump)
            {
                var cond = Conductor.instance;
                PlayerThrough(cond.songPositionInBeatsAsDouble);
            }
        }
        //////

        // throw cue
        public void ScheduleThrow(double beat)
        {
            PajamaParty.instance.ScheduleInput(beat, 2f, PajamaParty.InputAction_AltStart, ChargeJustOrNg, ThrowThrough, JumpOut);
            PajamaParty.instance.ScheduleInput(beat, 3f, PajamaParty.InputAction_AltFinish, ThrowJustOrNg, ThrowThrough, JumpOut, CanThrow);
        }

        public void ChargeJustOrNg(PlayerActionEvent caller, float state)
        {
            StartCharge();
            throwNg = (state <= -1f || state >= 1f);
            SoundByte.PlayOneShotGame("pajamaParty/throw4");
        }

        public bool CanThrow()
        {
            return charging;
        }

        public void ThrowJustOrNg(PlayerActionEvent caller, float state)
        {
            var cond = Conductor.instance;
            if (state <= -1f || state >= 1f)
            {
                SoundByte.PlayOneShot("miss");
                throwNg = true;
                EndCharge(cond.songPositionInBeatsAsDouble, true, throwNg);
            }
            else
            {
                SoundByte.PlayOneShotGame("pajamaParty/throw5");
                EndCharge(cond.songPositionInBeatsAsDouble, true, throwNg);
            }
            caller.CanHit(false);
        }

        public void ThrowThrough(PlayerActionEvent caller)
        {
            if (canCharge)
            {
                var cond = Conductor.instance;
                PlayerThrough(cond.songPositionInBeatsAsDouble);
            }
        }
        //

        // sleep cue
        public void StartSleepSequence(double beat, bool alt, int action)
        {
            PajamaParty.instance.ScheduleInput(beat, 4f, PajamaParty.InputAction_BasicPress, SleepJustOrNg, SleepThrough, SleepOut, CanSleep);

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

                anim.DoScaledAnimationAsync("MakoLand" + animSuffix);
            }
            startJumpTime = double.MinValue;
            Player.transform.localPosition = new Vector3(0, 0);
            Shadow.transform.localScale = new Vector3(1.65f, 1f, 1f);

            Projectile.GetComponent<Animator>().Play("NoPose", -1, 0);
            startThrowTime = double.MinValue;
            Projectile_Root.transform.localPosition = new Vector3(0, 0);
            Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (hasThrown)
            {
                Projectile.SetActive(false);
                hasThrown = false;
            }

            if (action == (int)PajamaParty.SleepType.NoAwake)
            {
                longSleep = true;
            }

            BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(
                        beat,
                        delegate { anim.DoScaledAnimationAsync("MakoSleep00" + animSuffix); }
                    ),
                    new BeatAction.Action(
                        beat + 0.5f,
                        delegate { anim.DoUnscaledAnimation("MakoSleep01" + animSuffix); }
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
                                anim.DoScaledAnimationAsync((alt ? "MakoReadySleep01" : "MakoReadySleep") + animSuffix);
                        }
                    ),
                    new BeatAction.Action(
                        beat + (longSleep ? 4f : 8f),
                        delegate {
                            canCharge = true;
                            canJump = true;
                        }
                    ),
                });
        }

        public bool CanSleep()
        {
            return canSleep;
        }

        public void SleepJustOrNg(PlayerActionEvent caller, float state)
        {
            var cond = Conductor.instance;
            caller.CanHit(false);
            canSleep = false;
            if (state <= -1f || state >= 1f)
                anim.DoUnscaledAnimation("MakoSleepNg" + animSuffix);
            else
            {
                SoundByte.PlayOneShotGame("pajamaParty/siesta4");
                anim.DoScaledAnimationAsync("MakoSleepJust" + animSuffix);

                if (!longSleep)
                {
                    BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(
                                caller.startBeat + 7f,
                                delegate {
                                    anim.DoScaledAnimationAsync("MakoAwake" + animSuffix);
                                    SoundByte.PlayOneShotGame("pajamaParty/siestaDone");
                                }
                            ),
                        });
                }
                longSleep = false;
            }
        }

        public void SleepThrough(PlayerActionEvent caller)
        {
            var cond = Conductor.instance;
            if (canSleep)
            {
                anim.DoScaledAnimationAsync("MakoSleepThrough" + animSuffix, 1, 0);
                caller.CanHit(false);
                canSleep = false;
            }
        }

        public void SleepOut(PlayerActionEvent caller)
        {
            var cond = Conductor.instance;
            if (canSleep)
            {
                anim.DoScaledAnimationAsync("MakoSleepOut" + animSuffix, 0.5f);
                SoundByte.PlayOneShotGame("pajamaParty/siestaBad");
                caller.CanHit(false);
                canSleep = false;
            }
        }
        //////

        public void DoForcedHigh()
        {
            anim.Play("NoPose" + animSuffix, -1, 0);
        }
    }
}
