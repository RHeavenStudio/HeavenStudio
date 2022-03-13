using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class KarateJoe : MonoBehaviour
    {

        [Header("Components")]
        public Animator anim;
        public GameObject HitEffect;
        public GameObject BulbHit;
        [SerializeField] private SpriteRenderer head;
        [SerializeField] private Sprite[] heads;
        [SerializeField] private GameObject missEffect;
        [SerializeField] private SpriteRenderer[] shadows;

        [Header("Properties")]
        public bool hitBarrel = false;
        public Coroutine kickC;
        public Coroutine missC;
        private float barrelBeat;
        public bool inCombo;
        public bool hitCombo;
        private float comboBeat;
        public List<Pot> currentComboPots = new List<Pot>();
        private int comboPotIndex;
        private int currentComboHitInList;
        private int comboIndex;
        public float comboNormalizedBeat = 0;

        public static KarateJoe instance { get; set; }

        private void Awake()
        {
            instance = this;
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (inCombo)
            {
                comboNormalizedBeat = Conductor.instance.GetPositionFromBeat(comboBeat, 1);

                if (hitCombo)
                {
                    if (currentComboPots[comboPotIndex] == null) return;
                }
                else
                {
                    comboNormalizedBeat += 1;
                }

                if (comboNormalizedBeat >= 1 && comboIndex < 1)
                {
                    if (hitCombo)
                    {
                        currentComboPots[comboPotIndex].Hit();
                        HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                        comboPotIndex++;
                        Jukebox.PlayOneShotGame("karateman/comboHit1");
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("karateman/swingNoHit");
                    }
                    comboIndex++;
                    AnimPlay("PunchLeft");
                }
                else if (comboNormalizedBeat >= 1.25f && comboIndex < 2)
                {
                    if (hitCombo)
                    {
                        currentComboPots[comboPotIndex].Hit();
                        HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                        comboPotIndex++;
                        Jukebox.PlayOneShotGame("karateman/comboHit1");
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("karateman/swingNoHit_Alt");
                    }
                    comboIndex++;
                    AnimPlay("PunchRight");
                }
                else if (comboNormalizedBeat >= 1.5f && comboIndex < 3)
                {
                    if (hitCombo)
                    {
                        currentComboPots[comboPotIndex].Hit();
                        HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                        comboPotIndex++;
                        Jukebox.PlayOneShotGame("karateman/comboHit2");
                    }
                    comboIndex++;
                    AnimPlay("ComboCrouch");
                }
                else if (comboNormalizedBeat >= 1.75f && comboIndex < 4)
                {
                    if (hitCombo)
                    {
                        currentComboPots[comboPotIndex].Hit();
                        HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                        comboPotIndex++;
                        Jukebox.PlayOneShotGame("karateman/comboHit3");
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("karateman/comboMiss");
                    }
                    comboIndex++;
                    AnimPlay("ComboKick");
                }
                else if (comboNormalizedBeat >= 2f && comboIndex < 5)
                {
                    if (hitCombo)
                    {
                        currentComboPots[comboPotIndex].Hit();
                        HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                        comboPotIndex++;
                        Jukebox.PlayOneShotGame("karateman/comboHit3");
                    }
                    comboIndex++;
                    AnimPlay("ComboCrouchPunch");
                }
                else if (comboNormalizedBeat >= 2.05f)
                {
                    if (hitCombo)
                    {
                        if (PlayerInput.AltPressedUp())
                        {
                            // ComboPow(null);
                        }
                    }
                    else
                    {
                        // fail anim
                        AnimPlay("ComboMiss");
                        ResetCombo();
                    }

                    if (comboNormalizedBeat >= 3.5f && comboIndex < 69)
                    {
                        ResetCombo();
                        AnimPlay("ComboCrouchPunchToIdle");
                        comboIndex = 69;
                    }
                }
            }
            else
            {
                if (!inCombo)
                if (PlayerInput.AltPressed())
                {
                    Combo(null);
                }
            }

            if (!hitBarrel)
            {
                if (PlayerInput.Pressed() && !inCombo)
                {
                    Swing(null);
                }
            }

            for(int i=0;i < shadows.Length; i++)
            {
                shadows[i].color = KarateMan.instance.GetShadowColor();
            }
        }

        public void Combo(Pot p)
        {
            if (p == null)
            {
                comboBeat = Conductor.instance.songPositionInBeats;
                hitCombo = false;
            }
            else
            {
                comboBeat = p.createBeat;
                hitCombo = true;
            }

            inCombo = true;
        }

        public void ComboPow(Pot p, bool overrideState = false)
        {
            if (!hitCombo || !inCombo || !hitCombo && !inCombo) return;

            anim.Play("Pow", 0, 0);

            /*if (currentComboPots[comboPotIndex].state.perfect)
            {
                // BarrelDestroy(currentComboPots[comboPotIndex], true);
                HitEffectF(currentComboPots[comboPotIndex].Holder.transform.localPosition);
                Destroy(currentComboPots[comboPotIndex].gameObject);
                Jukebox.PlayOneShotGame("karateman/comboHit4");
            }
            else
            {
                Jukebox.PlayOneShot("miss");
                currentComboPots[comboPotIndex].Miss();
            }*/

            if (p != null)
            {
                if (p.state.perfect || overrideState)
                {
                    p.BarrelDestroy(true);
                    HitEffectF(p.Holder.transform.localPosition);
                    Destroy(p.gameObject);
                    Jukebox.PlayOneShotGame("karateman/comboHit4");
                }
                else if (p.state.notPerfect())
                {
                    p.Miss();
                }
            }

            ResetCombo();
        }

        private void ResetCombo()
        {
            SetHead(0);
            hitCombo = false;
            inCombo = false;
            comboPotIndex = 0;
            comboIndex = 0;
            currentComboHitInList = 0;
            currentComboPots.Clear();
        }

        public IEnumerator PrepareKick()
        {
            barrelBeat = Conductor.instance.songPositionInBeats;
            hitBarrel = true;
            yield return new WaitForSeconds(0.17f);
            AnimPlay("KickPrepare");
        }

        public void ResetKick()
        {
            if (kickC != null)
            {
                StopCoroutine(kickC);
            }
            hitBarrel = false;
        }

        public void Swing(Pot p)
        {
            SetHead(0);
            bool punchLeft = true;

            if (p == null)
            {
                Jukebox.PlayOneShotGame("karateman/swingNoHit");
            }
            else
            {
                if (p.type == 2 || p.type == 3 || p.type == 4 || p.type == 6 || p.type == 999)
                {
                    punchLeft = false;
                }
                else
                {
                    punchLeft = true;
                }

                if (p.type == 4)
                {
                    if (kickC != null) StopCoroutine(kickC);
                    kickC = StartCoroutine(PrepareKick());
                }

                if (!p.combo)
                    HitEffectF(HitEffect.transform.localPosition);
            }

            if (punchLeft)
                AnimPlay("PunchLeft");
            else
                AnimPlay("PunchRight");
        }

        public void HitEffectF(Vector3 pos)
        {
            GameObject hit = Instantiate(HitEffect);
            hit.transform.parent = HitEffect.transform.parent;
            hit.transform.localPosition = pos;
            hit.SetActive(true);
            Destroy(hit, 0.06f);
        }

        public void AnimPlay(string name)
        {
            anim.Play(name, 0, 0);
            anim.speed = 1;
        }

        public void SetHead(int index)
        {
            head.sprite = heads[index];
        }

        public IEnumerator Miss()
        {
            // I couldn't find the sound for this
            GameObject miss = Instantiate(missEffect, missEffect.transform.parent);
            miss.SetActive(true);
            SetHead(2);
            yield return new WaitForSeconds(0.08f);
            Destroy(miss);
            SetHead(0);
        }
    }
}