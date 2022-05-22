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

        float startJumpTime = Single.MinValue;
        float jumpLength = 1f;
        float jumpHeight = 4f;

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

        public void Jump(float beat)
        {
            startJumpTime = beat;
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
    }
}