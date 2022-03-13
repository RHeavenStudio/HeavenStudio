using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class Bomb : PlayerActionObject
    {
        private Animator anim;

        private float startBeat;
        private float hitBeat;
        private float missBeat;

        public bool kicked;
        private bool missed;
        private bool eligible;

        public GameObject Holder;
        public GameObject RotHolder;
        private Vector3 lastRot;

        public GameObject shadow;
        private float shadowY;

        [Header("Curves")]
        [SerializeField] private AnimationCurve outCurve;
        [SerializeField] private AnimationCurve shadowHitCurve;

        private void Start()
        {
            anim = GetComponent<Animator>();

            startBeat = Conductor.instance.songPositionInBeats;
            eligible = true;

            PlayerActionInit(this.gameObject, startBeat);
        }

        public override void OnAce()
        {
            Hit();
        }

        private void Update()
        {
            shadow.transform.localPosition = new Vector3(Holder.transform.localPosition.x, shadow.transform.localPosition.y);
            if (!kicked)
            {
                if (!missed)
                {
                    float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 1.25f);

                    anim.Play("BombOut", 0, normalizedBeatAnim);
                    anim.speed = 0;

                    float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 0.75f);

                    StateCheckNoList(normalizedBeat);

                    RotHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(0, -90, outCurve.Evaluate(normalizedBeatAnim)));
                    lastRot = RotHolder.transform.eulerAngles;

                    shadowY = shadow.transform.localPosition.y;

                    if (normalizedBeat > 1.5f)
                    {
                        eligible = false;
                        // explode animation
                        if (normalizedBeat > 4)
                            Destroy(this.gameObject);
                    }


                    if (PlayerInput.PressedUp())
                    {
                        if (state.perfect)
                        {
                            Hit();
                        }
                        else
                        {
                            Miss();
                        }
                    }
                }
                else
                {
                    float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(missBeat, 1f);
                    anim.Play("BombMiss", 0, normalizedBeatAnim);
                    anim.speed = 0;
                    RotHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastRot.z, lastRot.z - 180, normalizedBeatAnim));

                    if (normalizedBeatAnim > 2)
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
            else
            {
                float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(hitBeat, 3f);
                anim.Play("BombHit", 0, normalizedBeatAnim);
                anim.speed = 0;

                RotHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastRot.z, lastRot.z - 180, normalizedBeatAnim));

                shadow.transform.localPosition = new Vector3(shadow.transform.localPosition.x, Mathf.Lerp(shadowY, 0.881f, shadowHitCurve.Evaluate(normalizedBeatAnim)));
                shadow.transform.localScale = Holder.transform.localScale;

                if (normalizedBeatAnim > 1)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        public void Hit()
        {
            KarateJoe.instance.HitEffectF(new Vector3(0.9f, 2.0549f));

            Jukebox.PlayOneShotGame("karateman/bombKick");
            hitBeat = Conductor.instance.songPositionInBeats;
            kicked = true;
            RotHolder.transform.eulerAngles = lastRot;

            KarateJoe.instance.ResetKick();
            KarateJoe.instance.AnimPlay("Kick");
        }

        public void Miss()
        {
            missBeat = Conductor.instance.songPositionInBeats;
            missed = true;
            Jukebox.PlayOneShot("miss");

            KarateJoe.instance.ResetKick();
            KarateJoe.instance.AnimPlay("Kick");
        }
    }
}