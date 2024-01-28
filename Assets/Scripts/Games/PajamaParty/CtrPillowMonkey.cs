using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_PajamaParty
{
    public class CtrPillowMonkey : MonoBehaviour
    {
        const string HighSuffix = "_H";
        const string NrmSuffix = "";

        [Header("Objects")]
        [SerializeField] GameObject Monkey;
        [SerializeField] GameObject Shadow;
        [SerializeField] GameObject Projectile;

        public Animator anim { get; private set; }
        string animSuffix => (PajamaParty.instance.HighState ? HighSuffix : NrmSuffix);

        public bool shouldBop = false;

        public int row;
        public int col;

        double lastReportedBeat;
        double startJumpTime = double.MinValue;
        float jumpLength = 1f;
        float jumpHeight = 4f;
        int jumpAlt;

        bool shouldntBop = false;
        bool hasJumped = false;

        double startThrowTime = double.MinValue;
        float throwLength = 4f;
        float throwHeight = 14f;

        private bool hasThrown = false;

        void Awake()
        {
            anim = Monkey.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            //jumping logic
            float jumpPos = cond.GetPositionFromBeat(startJumpTime, jumpLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasJumped = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                Monkey.transform.localPosition = new Vector3(0, jumpHeight * yWeight);
                Shadow.transform.localScale = new Vector3((1f - yWeight * 0.2f) * 1.2f, (1f - yWeight * 0.2f) * 0.8f, 1f);
                if (jumpAlt > 1)
                {
                    float t;
                    if (jumpAlt == 3)
                        t = 1f - jumpPos;
                    else
                        t = jumpPos;
                    Monkey.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(22.5f, -22.5f, t));
                    anim.DoScaledAnimation("MonkeyJump0" + jumpAlt + animSuffix, startJumpTime, jumpLength);
                }
                else
                    anim.DoScaledAnimation("MonkeyJump" + animSuffix, startJumpTime, jumpLength);
            }
            else
            {
                if (hasJumped)
                {
                    shouldntBop = false;
                    hasJumped = false;
                    PajamaParty.instance.DoBedImpact();
                    anim.DoScaledAnimationAsync("MonkeyLand" + animSuffix);
                    Monkey.transform.rotation = Quaternion.Euler(0, 0, 0);
                    jumpAlt = 0;
                }
                startJumpTime = double.MinValue;
                Monkey.transform.localPosition = new Vector3(0, 0);
                Shadow.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
            }

            //throwing logic
            jumpPos = cond.GetPositionFromBeat(startThrowTime, throwLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasThrown = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                Projectile.transform.localPosition = new Vector3(0, (throwHeight * yWeight) + 1.5f);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, Projectile.transform.rotation.eulerAngles.z - (360f * Time.deltaTime));
            }
            else
            {
                startThrowTime = double.MinValue;
                if (hasThrown)
                {
                    Projectile.transform.localPosition = new Vector3(0, 0);
                    Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                    anim.DoUnscaledAnimation("MonkeyBeat" + animSuffix);
                    Projectile.SetActive(false);
                    hasThrown = false;
                    shouldntBop = false;
                }
            }
        }

        private void LateUpdate()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && anim.IsAnimationNotPlaying() && !hasThrown && !shouldntBop && shouldBop)
            {
                anim.DoScaledAnimationAsync("MonkeyBeat" + animSuffix, 0.5f);
            }
        }

        public void Jump(double beat, int alt = 1)
        {
            startJumpTime = beat;
            jumpAlt = 0;
            if (alt > 1)
            {
                jumpAlt = alt;
            }
        }

        public void Charge(double beat)
        {
            shouldntBop = true;
            anim.DoUnscaledAnimation("MonkeyReady" + animSuffix);
        }

        public void Throw(double beat, bool highCheck)
        {
            anim.DoUnscaledAnimation("MonkeyThrow" + animSuffix);
            startThrowTime = beat;
            Projectile.SetActive(true);

            if (highCheck)
            {
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new
                    (
                        beat + 2,
                        delegate
                        {
                            anim.DoUnscaledAnimation("MonkeyThrow" + animSuffix, 1);
                        }
                    )
                });
            }
        }

        public void ReadySleep(double beat, int action)
        {
            shouldntBop = true;
            var cond = Conductor.instance;
            startThrowTime = double.MinValue;
            Projectile.transform.localPosition = new Vector3(0, 0);
            Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (hasThrown)
            {
                Projectile.SetActive(false);
                hasThrown = false;
            }

            startJumpTime = double.MinValue;
            Monkey.transform.localPosition = new Vector3(0, 0);
            Shadow.transform.localScale = new Vector3(1.2f, 0.8f, 1f);

            List<BeatAction.Action> seq =
                new List<BeatAction.Action>()
                {
                    new BeatAction.Action( beat, delegate { anim.DoScaledAnimationAsync("MonkeySleep00" + animSuffix); }),
                    new BeatAction.Action( beat + 0.5f, delegate { anim.DoUnscaledAnimation("MonkeySleep01" + animSuffix); }),
                };

            if (col == 0 || col == 4)
            {
                seq.Add(new BeatAction.Action(beat + 1f, delegate { anim.DoScaledAnimationAsync("MonkeySleep02" + animSuffix); }));
            }
            else if (col == 1 || col == 3)
            {
                seq.Add(new BeatAction.Action(beat + 1.5f, delegate { anim.DoScaledAnimationAsync("MonkeyReadySleep" + animSuffix); }));
                seq.Add(new BeatAction.Action(beat + 2.5f, delegate { anim.DoScaledAnimationAsync("MonkeySleep02" + animSuffix); }));
            }
            else
            {
                seq.Add(new BeatAction.Action(beat + 3f, delegate { anim.DoScaledAnimationAsync("MonkeyReadySleep" + animSuffix); }));
                seq.Add(new BeatAction.Action(beat + 4f, delegate { anim.DoScaledAnimationAsync("MonkeySleep02" + animSuffix); }));
            }

            if (action != (int)PajamaParty.SleepType.NoAwake)
                seq.Add(new BeatAction.Action(beat + 7f, delegate { anim.DoScaledAnimationAsync("MonkeyAwake" + animSuffix); }));

            BeatAction.New(this, seq);
        }

        public void DoForcedHigh()
        {
            anim.Play("NoPose" + animSuffix, -1, 0);
        }
    }
}