using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ForkLifter
{
    public class ForkLifterPlayer : MonoBehaviour
    {
        public static ForkLifterPlayer instance { get; set; }

        [Header("Objects")]
        public GameObject fork;
        public Sprite peaSprite;
        public Sprite hitFX;
        public Sprite hitFXG;
        public Sprite hitFXMiss;
        public Sprite hitFX2;
        public Transform early, perfect, late;

        [SerializeField]
        private BoxCollider2D col;

        private Animator anim;

        private int currentHitInList = 0;

        public int currentEarlyPeasOnFork;
        public int currentPerfectPeasOnFork;
        public int currentLatePeasOnFork;

        private bool isEating = false;

        // Burger shit

        public bool topbun, middleburger, bottombun;

        // -----------

        private void Awake()
        {
            instance = this;
            anim = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            if (PlayerInput.Pressed() && !ForkLifter.instance.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Stab(null);
            }

            if (ForkLifter.instance.EligibleHits.Count == 0)
            {
                currentHitInList = 0;
            }
        }

        public void Eat()
        {
            if (currentEarlyPeasOnFork != 0 || currentPerfectPeasOnFork != 0 || currentLatePeasOnFork != 0)
            {
                anim.Play("Player_Eat", 0, 0);
                isEating = true;
            }
        }

        public void EatConfirm()
        {
            if (topbun && middleburger && bottombun)
            {
                SoundByte.PlayOneShotGame("forkLifter/burger");
            }
            else
            {
                if (currentEarlyPeasOnFork > 0 || currentLatePeasOnFork > 0)
                {
                    SoundByte.PlayOneShotGame($"forkLifter/cough_{Random.Range(1, 3)}");
                }
                else
                {
                    SoundByte.PlayOneShotGame("forkLifter/gulp");
                }
            }

            RemoveObjFromFork();
        }

        public void RemoveObjFromFork()
        {
            for (int i = 0; i < early.transform.childCount; i++)
            {
                Destroy(early.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < perfect.transform.childCount; i++)
            {
                Destroy(perfect.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < late.transform.childCount; i++)
            {
                Destroy(late.transform.GetChild(i).gameObject);
            }
            currentEarlyPeasOnFork = 0;
            currentPerfectPeasOnFork = 0;
            currentLatePeasOnFork = 0;

            isEating = false;

            topbun = false; middleburger = false; bottombun = false;
        }

        public void Stab(Pea p)
        {
            if (isEating) return;

            if (p == null)
            {
                SoundByte.PlayOneShotGame("forkLifter/stabnohit");
            }

            anim.Play("Player_Stab", 0, 0);
        }

        public void FastEffectHit(int type)
        {
            GameObject hitFX2o = new GameObject();
            hitFX2o.transform.localPosition = new Vector3(0.11f, -2.15f);
            hitFX2o.transform.localScale = new Vector3(5.401058f, 1.742697f);
            hitFX2o.transform.localRotation = Quaternion.Euler(0, 0, -38.402f);
            SpriteRenderer hfx2s = hitFX2o.AddComponent<SpriteRenderer>();
            if (type == 2)
                hfx2s.sprite = hitFXG;
            else
                hfx2s.sprite = hitFX2;
            hfx2s.sortingOrder = -5;
            hfx2s.DOColor(new Color(1, 1, 1, 0), 0.07f).OnComplete(delegate { Destroy(hitFX2o); });
        }

        public void HitFXMiss(Vector2 pos, Vector2 size)
        {
            GameObject hitFXo = new GameObject();
            hitFXo.transform.localPosition = new Vector3(pos.x, pos.y);
            hitFXo.transform.localScale = new Vector3(size.x, size.y);
            SpriteRenderer hfxs = hitFXo.AddComponent<SpriteRenderer>();
            hfxs.sprite = hitFXMiss;
            hfxs.sortingOrder = 100;
            hfxs.DOColor(new Color(1, 1, 1, 0), 0.05f).OnComplete(delegate { Destroy(hitFXo); });
        }
    }
}