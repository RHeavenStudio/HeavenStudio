using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class SpaceballBall : PlayerActionObject
    {
        public float startBeat;
        public Animator anim;

        public bool high;

        private Minigame.Eligible e = new Minigame.Eligible();

        public GameObject Holder;
        public SpriteRenderer Sprite;

        public bool hit;
        public float hitBeat;
        public Vector3 hitPos;
        public float hitRot;
        public float randomEndPosX;

        private void Awake()
        {
            anim = GetComponent<Animator>();

            e.gameObject = this.gameObject;

            float rot = Random.Range(0, 360);
            Sprite.gameObject.transform.eulerAngles = new Vector3(0, 0, rot);

            isEligible = true;
        }

        private void Start() 
        {
            Spaceball.instance.ScheduleInput(startBeat, high ? 2f : 1f, InputType.STANDARD_DOWN, Just, Miss, Out);
        }

        private void Hit()
        {
            hit = true;
            hitBeat = Conductor.instance.songPositionInBeats;
            hitPos = Holder.transform.localPosition;
            hitRot = Holder.transform.eulerAngles.z;

            Jukebox.PlayOneShotGame("spaceball/hit");
            
            // jank fix for a bug with autoplay - freeform
            if (GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput)
            {
                Jukebox.PlayOneShotGame("spaceball/swing");
            }

            randomEndPosX = Random.Range(40f, 55f);

            anim.enabled = false;
            SpaceballPlayer.instance.Swing(this);
        }

        private void NearMiss()
        {
            Holder.transform.GetChild(0).gameObject.AddComponent<Rotate>().rotateSpeed = -55;

            enabled = false;
            anim.enabled = false;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(transform.up * 1100);
            rb.AddForce(transform.right * 400);
            rb.gravityScale = 9;

            Jukebox.PlayOneShot("miss");
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (hit)
            {
                float nba = cond.GetPositionFromBeat(hitBeat, 14);
                Holder.transform.localPosition = Vector3.Lerp(hitPos, new Vector3(randomEndPosX, 0f, -600f), nba);
                Holder.transform.eulerAngles = Vector3.Lerp(new Vector3(0, 0, hitRot), new Vector3(0, 0, -2260), nba);
            }
            else
            {
                float beatLength = 1f;
                if (high) beatLength = 2f;

                float normalizedBeatAnim = cond.GetPositionFromBeat(startBeat, beatLength + (float)cond.SecsToBeats(Minigame.EndTime()-1, cond.GetBpmAtBeat(startBeat + beatLength)));

                if (high)
                {
                    anim.Play("BallHigh", 0, normalizedBeatAnim);
                }
                else
                {
                    anim.Play("BallLow", 0, normalizedBeatAnim);
                }

                anim.speed = 0;
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                NearMiss();
                return; 
            }
            Hit();
        }

        private void Miss(PlayerActionEvent caller) 
        {
            Jukebox.PlayOneShotGame("spaceball/fall");
            Instantiate(Spaceball.instance.Dust, Spaceball.instance.Dust.transform.parent).SetActive(true);
            Destroy(this.gameObject);
        }

        private void Out(PlayerActionEvent caller) {}
    }
}
