using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;

namespace HeavenStudio.Games.Scripts_ForkLifter
{
    public class Pea : PlayerActionObject
    {
        private Animator anim;

        public float startBeat;

        public int type;

        private void Start()
        {
            anim = GetComponent<Animator>();

            // SCHEDULING zoom sound so it lines up with when it meets the fork.
            var currentDspTime = AudioSettings.dspTime;
            var cond = Conductor.instance;
            var zoomStartTime = currentDspTime + (double)(cond.secPerBeat * 2 / cond.musicSource.pitch) - 0.317;
            Jukebox.PlayOneShotScheduledGame("forkLifter/zoomFast", (double)zoomStartTime);

            GetComponentInChildren<SpriteRenderer>().sprite = ForkLifter.instance.peaSprites[type];

            for (int i = 0; i < transform.GetChild(0).childCount; i++)
            {
                transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            }

            // PlayerActionInit(this.gameObject, startBeat, ForkLifter.instance.EligibleHits);

            isEligible = true;
        }

        public override void OnAce()
        {
            this.Hit();
        }

        public void Hit()
        {
            ForkLifterPlayer.instance.Stab(this);

            if (ForkLifterPlayer.instance.currentPerfectPeasOnFork < 4)
            {
                GameObject pea = new GameObject();

                pea.transform.parent = ForkLifterPlayer.instance.perfect.transform;
                pea.transform.localScale = Vector2.one;
                pea.transform.localRotation = Quaternion.Euler(0, 0, 0);

                pea.transform.localPosition = Vector3.zero;

                for (int i = 0; i < ForkLifterPlayer.instance.perfect.transform.childCount; i++)
                {
                    ForkLifterPlayer.instance.perfect.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * ForkLifterPlayer.instance.currentPerfectPeasOnFork);
                }

                SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();
                psprite.sprite = ForkLifter.instance.peaHitSprites[type];
                psprite.sortingOrder = 20;
                switch (type)
                {
                    case 0:
                        psprite.sortingOrder = 101;
                        break;
                    case 1:
                        psprite.sortingOrder = 104;
                        break;
                    case 2:
                        psprite.sortingOrder = 103;
                        break;
                    case 3:
                        psprite.sortingOrder = 102;
                        break;
                }
            }

            GameObject hitFXo = new GameObject();
            hitFXo.transform.localPosition = new Vector3(1.9969f, -3.7026f);
            hitFXo.transform.localScale = new Vector3(3.142196f, 3.142196f);
            SpriteRenderer hfxs = hitFXo.AddComponent<SpriteRenderer>();
            hfxs.sprite = ForkLifterPlayer.instance.hitFX;
            hfxs.sortingOrder = 100;
            hfxs.DOColor(new Color(1, 1, 1, 0), 0.05f).OnComplete(delegate { Destroy(hitFXo); });

            ForkLifterPlayer.instance.FastEffectHit(type);

            Jukebox.PlayOneShotGame("forkLifter/stab");

            ForkLifterPlayer.instance.currentPerfectPeasOnFork++;

            if (type == 1)
            {
                ForkLifterPlayer.instance.topbun = true;
            }
            else if (type == 2)
            {
                ForkLifterPlayer.instance.middleburger = true;
            }
            else if (type == 3)
            {
                ForkLifterPlayer.instance.bottombun = true;
            }

            Destroy(this.gameObject);
        }

        public void Early()
        {
            GameObject pea = new GameObject();

            pea.transform.parent = ForkLifterPlayer.instance.early.transform;
            pea.transform.localScale = Vector2.one;

            pea.transform.localPosition = Vector3.zero;
            pea.transform.localRotation = Quaternion.Euler(0, 0, 90);

            for (int i = 0; i < ForkLifterPlayer.instance.early.transform.childCount; i++)
            {
                ForkLifterPlayer.instance.early.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * ForkLifterPlayer.instance.currentEarlyPeasOnFork);
            }

            SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();
            psprite.sprite = ForkLifter.instance.peaHitSprites[type];
            psprite.sortingOrder = 20;
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(1.0424f, -4.032f), new Vector2(1.129612f, 1.129612f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(0.771f, -3.016f), new Vector2(1.71701f, 1.71701f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(2.598f, -2.956f), new Vector2(1.576043f, 1.576043f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(2.551f, -3.609f), new Vector2(1.200788f, 1.200788f));

            ForkLifterPlayer.instance.FastEffectHit(type);

            Jukebox.PlayOneShot("miss");

            ForkLifterPlayer.instance.currentEarlyPeasOnFork++;

            Destroy(this.gameObject);
        }

        public void Late()
        {
            GameObject pea = new GameObject();
            pea.transform.parent = ForkLifterPlayer.instance.late.transform;
            pea.transform.localScale = Vector2.one;

            pea.transform.localPosition = Vector3.zero;
            pea.transform.localRotation = Quaternion.Euler(0, 0, 90);

            for (int i = 0; i < ForkLifterPlayer.instance.late.transform.childCount; i++)
            {
                ForkLifterPlayer.instance.late.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * ForkLifterPlayer.instance.currentLatePeasOnFork);
            }

            SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();
            psprite.sprite = ForkLifter.instance.peaHitSprites[type];
            psprite.sortingOrder = 20;
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(1.0424f, -4.032f), new Vector2(1.129612f, 1.129612f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(0.771f, -3.016f), new Vector2(1.71701f, 1.71701f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(2.598f, -2.956f), new Vector2(1.576043f, 1.576043f));
            ForkLifterPlayer.instance.HitFXMiss(new Vector2(2.551f, -3.609f), new Vector2(1.200788f, 1.200788f));

            ForkLifterPlayer.instance.FastEffectHit(type);

            Jukebox.PlayOneShot("miss");

            ForkLifterPlayer.instance.currentLatePeasOnFork++;
            Destroy(this.gameObject);
        }

        private void Update()
        {
            float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 2.45f);
            anim.Play("Flicked_Object", -1, normalizedBeatAnim);
            anim.speed = 0;

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 2f);

            StateCheck(normalizedBeat);

            if (PlayerInput.Pressed() && ForkLifterPlayer.instance.hitOnFrame == 0)
            {
                if (state.perfect)
                {
                    Hit();
                }
                else if (state.early)
                {
                    Early();
                }
                else if (state.late)
                {
                    Late();
                }

                ForkLifterPlayer.instance.hitOnFrame++;
            }

            if (normalizedBeat > Minigame.EndTime())
            {
                Jukebox.PlayOneShot("audience/disappointed");
                Destroy(this.gameObject);
            }
        }
    }
}