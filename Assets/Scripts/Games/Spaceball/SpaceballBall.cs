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

        private void Start()
        {
            anim = GetComponent<Animator>();

            e.gameObject = this.gameObject;

            float rot = Random.Range(0, 360);
            Sprite.gameObject.transform.eulerAngles = new Vector3(0, 0, rot);


            // PlayerActionInit(this.gameObject, startBeat, Spaceball.instance.EligibleHits);

            isEligible = true;
        }

        public override void OnAce()
        {
            this.Hit();
        }

        private void Hit()
        {
            hit = true;
            hitBeat = Conductor.instance.songPositionInBeats;
            hitPos = Holder.transform.localPosition;
            hitRot = Holder.transform.eulerAngles.z;

            Jukebox.PlayOneShotGame("spaceball/hit");

            randomEndPosX = Random.Range(40f, 55f);

            anim.enabled = false;
            SpaceballPlayer.instance.Swing(this);
        }

        private void Miss()
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
            if (hit)
            {
                float nba = Conductor.instance.GetPositionFromBeat(hitBeat, 14);
                Holder.transform.localPosition = Vector3.Lerp(hitPos, new Vector3(randomEndPosX, 0f, -600f), nba);
                Holder.transform.eulerAngles = Vector3.Lerp(new Vector3(0, 0, hitRot), new Vector3(0, 0, -2260), nba);
            }
            else
            {
                float beatLength = 1f;
                if (high) beatLength = 2f;

                float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, beatLength + 0.15f);
                // print(normalizedBeatAnim + " " + Time.frameCount);

                if (high)
                {
                    anim.Play("BallHigh", 0, normalizedBeatAnim);
                }
                else
                {
                    anim.Play("BallLow", 0, normalizedBeatAnim);
                }

                anim.speed = 0;

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, beatLength);

                StateCheck(normalizedBeat);

                if (PlayerInput.Pressed())
                {
                    if (state.perfect)
                    {
                        Hit();
                    }
                    else if (state.notPerfect())
                    {
                        Miss();
                    }
                }

                // too lazy to make a proper fix for this
                float endTime = 1.2f;
                if (high) endTime = 1.1f;

                if (normalizedBeat > endTime)
                {
                    Jukebox.PlayOneShotGame("spaceball/fall");
                    Instantiate(Spaceball.instance.Dust, Spaceball.instance.Dust.transform.parent).SetActive(true);
                    Destroy(this.gameObject);
                }
            }
        }
    }

}