using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;

namespace HeavenStudio.Games.Scripts_ForkLifter
{
    public class Pea : MonoBehaviour
    {
        public double startBeat;
        public int type;

        ForkLifter game;
        ForkLifterPlayer player;
        private Animator anim;


        private void Awake()
        {
            game = ForkLifter.instance;
            player = ForkLifterPlayer.instance;
            anim = GetComponent<Animator>();

            GetComponentInChildren<SpriteRenderer>().sprite = game.peaSprites[type];

            for (int i = 0; i < transform.GetChild(0).childCount; i++)
            {
                transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            }

            game.ScheduleInput(startBeat, 2f, ForkLifter.InputAction_BasicPress, Just, Miss, Out);
        }

        public void Hit()
        {
            player.Stab(this);

            if (player.currentPerfectPeasOnFork < 4)
            {
                GameObject pea = new GameObject();

                pea.transform.parent = player.perfect.transform;
                pea.transform.localScale = Vector2.one;
                pea.transform.localRotation = Quaternion.Euler(0, 0, 0);

                pea.transform.localPosition = Vector3.zero;

                float peaOffset = 0;

                if (player.currentPerfectPeasOnFork == 3) peaOffset = -0.15724f;

                for (int i = 0; i < player.perfect.transform.childCount; i++)
                {
                    player.perfect.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * player.currentPerfectPeasOnFork + peaOffset);
                }

                SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();

                psprite.sprite = game.peaHitSprites[type];
                psprite.sortingOrder = type switch {
                    0 => 101,
                    1 => 104,
                    2 => 103,
                    3 => 102,
                    _ => 20,
                };
            }

            GameObject hitFXo = new GameObject();
            hitFXo.transform.localPosition = new Vector3(1.9969f, -3.7026f);
            hitFXo.transform.localScale = new Vector3(3.142196f, 3.142196f);
            SpriteRenderer hfxs = hitFXo.AddComponent<SpriteRenderer>();
            hfxs.sprite = player.hitFX;
            hfxs.sortingOrder = 100;
            hfxs.DOColor(new Color(1, 1, 1, 0), 0.05f).OnComplete(delegate { Destroy(hitFXo); });

            player.FastEffectHit(type);

            SoundByte.PlayOneShotGame("forkLifter/stab");

            player.currentPerfectPeasOnFork++;

            player.topbun = type == 1;
            player.middleburger = type == 2;
            player.bottombun = type == 3;

            Destroy(this.gameObject);
        }

        public void Early()
        {
            player.Stab(null);

            GameObject pea = new GameObject();

            pea.transform.parent = player.early.transform;
            pea.transform.localScale = Vector2.one;

            pea.transform.localPosition = Vector3.zero;
            pea.transform.localRotation = Quaternion.Euler(0, 0, 90);

            for (int i = 0; i < player.early.transform.childCount; i++)
            {
                player.early.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * player.currentEarlyPeasOnFork);
            }

            SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();
            psprite.sprite = game.peaHitSprites[type];
            psprite.sortingOrder = 20;
            player.HitFXMiss(new Vector2(1.0424f, -4.032f), new Vector2(1.129612f, 1.129612f));
            player.HitFXMiss(new Vector2(0.771f, -3.016f), new Vector2(1.71701f, 1.71701f));
            player.HitFXMiss(new Vector2(2.598f, -2.956f), new Vector2(1.576043f, 1.576043f));
            player.HitFXMiss(new Vector2(2.551f, -3.609f), new Vector2(1.200788f, 1.200788f));

            player.FastEffectHit(type);

            SoundByte.PlayOneShot("miss");

            player.currentEarlyPeasOnFork++;

            Destroy(this.gameObject);
        }

        public void Late()
        {
            player.Stab(null);

            GameObject pea = new GameObject();
            pea.transform.parent = player.late.transform;
            pea.transform.localScale = Vector2.one;

            pea.transform.localPosition = Vector3.zero;
            pea.transform.localRotation = Quaternion.Euler(0, 0, 90);

            for (int i = 0; i < player.late.transform.childCount; i++)
            {
                player.late.transform.GetChild(i).transform.localPosition = new Vector3(0, (-1.67f - (0.15724f * i)) + 0.15724f * player.currentLatePeasOnFork);
            }

            SpriteRenderer psprite = pea.AddComponent<SpriteRenderer>();
            psprite.sprite = game.peaHitSprites[type];
            psprite.sortingOrder = 20;
            player.HitFXMiss(new Vector2(1.0424f, -4.032f), new Vector2(1.129612f, 1.129612f));
            player.HitFXMiss(new Vector2(0.771f, -3.016f), new Vector2(1.71701f, 1.71701f));
            player.HitFXMiss(new Vector2(2.598f, -2.956f), new Vector2(1.576043f, 1.576043f));
            player.HitFXMiss(new Vector2(2.551f, -3.609f), new Vector2(1.200788f, 1.200788f));

            player.FastEffectHit(type);

            SoundByte.PlayOneShot("miss");

            player.currentLatePeasOnFork++;
            Destroy(this.gameObject);
        }

        private void Update()
        {
            float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 2.45f);
            anim.Play("Flicked_Object", -1, normalizedBeatAnim);
            anim.speed = 0;
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f) 
            {
                Late();
            }
            else if (state <= -1f) 
            {
                Early();
            } 
            else 
            {
                Hit();
            }
        }

        private void Miss(PlayerActionEvent caller) 
        {
            SoundByte.PlayOneShotGame("forkLifter/disappointed");
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat+ 2.45f, delegate { 
                    Destroy(this.gameObject);
                }),
            });
        }

        private void Out(PlayerActionEvent caller) {}
    }
}