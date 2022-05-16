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

        void Awake()
        {
            anim = Player.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            // TESTING REMOVE FOR PROD
            if (PlayerInput.Pressed())
            {
                PlayerJump(cond.songPositionInBeats);
            }

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

            //temp
            jumpLength = 1f;
            jumpHeight = 4f;
        }
    }
}
