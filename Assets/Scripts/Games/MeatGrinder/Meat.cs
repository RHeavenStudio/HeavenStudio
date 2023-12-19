using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starpelly;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class Meat : MonoBehaviour
    {
        public double startBeat;
        public MeatType meatType;

        private string meatTypeStr;
        private bool isHit = false;

        // const float meatStart = 0;
        // const float meatEnd = 3.43f;

        [Header("Animators")]
        private Animator anim;
        private SpriteRenderer sr;
        [SerializeField] private Sprite[] meats;

        public enum MeatType
        {
            DarkMeat,
            LightMeat,
            BaconBall,
        }

        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
            anim = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
            anim.writeDefaultValuesOnDisable = false;
        }

        private void Start()
        {
            meatTypeStr = meatType.ToString();
            sr.sprite = meats[(int)meatType];
            Debug.Log(meats[(int)meatType]);
            Debug.Log(sr.sprite.name);

            game.ScheduleInput(startBeat, 1, MeatGrinder.InputAction_Press, Hit, Miss, Nothing);
        }

        private void Update()
        {
            // if (Input.GetKey(KeyCode.G)) { // Insane.
            //     anim.enabled = true;
            // }
            Debug.Log(sr.sprite.name);
            if (!isHit) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, 1);
                float newX = Mathf.LerpUnclamped(-14f, 1.7f, normalizedBeat);
                Debug.Log(newX);
                transform.position = new Vector3(newX, -1.2f);

                // anim.DoNormalizedAnimation("MeatThrown", normalizedBeat);
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            anim.enabled = true;
            isHit = true;
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync(meatTypeStr + "Hit", 0.5f);
            
            bool isBarely = state is >= 1f or <= -1f;

            game.bossAnnoyed = isBarely;
            SoundByte.PlayOneShotGame("meatGrinder/" + (isBarely ? "tink" : "meatHit"));
            game.TackAnim.DoScaledAnimationAsync("TackHit" + (isBarely ? "Barely" : "Success"), 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            anim.enabled = true;
            game.bossAnnoyed = true;
            SoundByte.PlayOneShotGame("meatGrinder/miss");

            game.TackAnim.DoScaledAnimationAsync("TackMiss" + meatTypeStr, 0.5f);
            game.TackAnim.SetBool("tackMeated", true);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);

            Destroy(gameObject);
        }

        private void Nothing(PlayerActionEvent caller) { }

        public void SetSprite() {
            sr.sprite = meats[(int)meatType];
        }

        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}
