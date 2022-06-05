using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_PajamaParty
{
    public class CtrPillowMonkey : MonoBehaviour
    {
        [Header("Objects")]
        public GameObject Monkey;
        public GameObject Shadow;
        public GameObject Projectile;

        public Animator anim;

        public int row;
        public int col;

        float startJumpTime = Single.MinValue;
        float jumpLength = 1f;
        float jumpHeight = 4f;
        int jumpAlt;

        private bool hasJumped = false;

        float startThrowTime = Single.MinValue;
        float throwLength = 4f;
        float throwHeight = 12f;
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
                float yWeight = -(yMul*yMul) + 1f;
                Monkey.transform.localPosition = new Vector3(0, jumpHeight * yWeight);
                Shadow.transform.localScale = new Vector3((1f-yWeight*0.2f) * 1.2f, (1f-yWeight*0.2f) * 0.8f, 1f);
                if (jumpAlt > 1)
                {
                    float t;
                    if (jumpAlt == 3)
                        t = 1f - jumpPos;
                    else
                        t = jumpPos;
                    Monkey.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(22.5f, -22.5f, t));
                    anim.Play("MonkeyJump0" + jumpAlt, -1, jumpPos);
                }
                else
                    anim.Play("MonkeyJump", -1, jumpPos);
                anim.speed = 0;
            }
            else
            {
                if (hasJumped)
                {
                    hasJumped = false;
                    PajamaParty.instance.DoBedImpact();
                    anim.Play("MonkeyLand", -1, 0);
                    anim.speed = 1f / cond.pitchedSecPerBeat;
                    Monkey.transform.rotation = Quaternion.Euler(0, 0, 0);
                    jumpAlt = 0;
                }
                startJumpTime = Single.MinValue;
                Monkey.transform.localPosition = new Vector3(0, 0);
                Shadow.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
            }

            //throwing logic
            jumpPos = cond.GetPositionFromBeat(startThrowTime, throwLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                hasThrown = true;
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                Projectile.transform.localPosition = new Vector3(0, throwHeight * yWeight + 1.5f);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, Projectile.transform.rotation.eulerAngles.z - (360f * Time.deltaTime));
            }
            else
            {
                startThrowTime = Single.MinValue;
                Projectile.transform.localPosition = new Vector3(0, 0);
                Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (hasThrown)
                {
                    anim.Play("MonkeyBeat", -1, 0);
                    anim.speed = 1f;
                    Projectile.SetActive(false);
                    hasThrown = false;
                }
            }
        }

        public void Jump(float beat, int alt = 1)
        {
            startJumpTime = beat;
            jumpAlt = 0;
            if (alt > 1)
            {
                jumpAlt = alt;
            }
        }

        public void Charge(float beat)
        {
            anim.Play("MonkeyReady", -1, 0);
            anim.speed = 1f;
        }

        public void Throw(float beat)
        {
            anim.Play("MonkeyThrow", -1, 0);
            anim.speed = 1f;
            startThrowTime = beat;
            Projectile.SetActive(true);
        }

        public void ReadySleep(float beat)
        {
            var cond = Conductor.instance;
            startThrowTime = Single.MinValue;
            Projectile.transform.localPosition = new Vector3(0, 0);
            Projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (hasThrown)
            {
                Projectile.SetActive(false);
                hasThrown = false;
            }

            startJumpTime = Single.MinValue;
            Monkey.transform.localPosition = new Vector3(0, 0);
            Shadow.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
            
            List<BeatAction.Action> seq = 
                new List<BeatAction.Action>()
                {
                    new BeatAction.Action( beat, delegate { 
                            anim.Play("MonkeySleep00", -1, 0);
                            anim.speed = 1f / cond.pitchedSecPerBeat;
                        }
                    ),
                    new BeatAction.Action( beat + 0.5f, delegate { 
                            anim.Play("MonkeySleep01", -1, 0);
                            anim.speed = 1f;
                        }
                    ),
                };
            
            if (col == 0 || col == 4)
            {
                seq.Add(new BeatAction.Action( beat + 1f, delegate { 
                        anim.Play("MonkeySleep02", -1, 0);
                        anim.speed = 1f / cond.pitchedSecPerBeat;
                    }
                ));
            }
            else if (col == 1 || col == 3)
            {
                seq.Add(new BeatAction.Action( beat + 1.5f, delegate { 
                        anim.Play("MonkeyReadySleep", -1, 0);
                        anim.speed = 1f / cond.pitchedSecPerBeat;
                    }
                ));
                seq.Add(new BeatAction.Action( beat + 2.5f, delegate { 
                        anim.Play("MonkeySleep02", -1, 0);
                        anim.speed = 1f / cond.pitchedSecPerBeat;
                    }
                ));
            }
            else
            {
                seq.Add(new BeatAction.Action( beat + 3f, delegate { 
                        anim.Play("MonkeyReadySleep", -1, 0);
                        anim.speed = 1f / cond.pitchedSecPerBeat;
                    }
                ));
                seq.Add(new BeatAction.Action( beat + 4f, delegate { 
                        anim.Play("MonkeySleep02", -1, 0);
                        anim.speed = 1f / cond.pitchedSecPerBeat;
                    }
                ));
            }
            seq.Add(new BeatAction.Action( beat + 7f, delegate { 
                    anim.Play("MonkeyAwake", -1, 0);
                    anim.speed = 1f / cond.pitchedSecPerBeat;
                }
            ));

            BeatAction.New(Monkey, seq);
        }
    }
}