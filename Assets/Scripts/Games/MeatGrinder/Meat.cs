using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class Meat : MonoBehaviour
    {
        public double startBeat;
        public MeatType meatType;

        [Header("Animators")]
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;
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
            sr.sprite = meats[(int)meatType];
        }

        private void Start()
        {
            game.ScheduleInput(startBeat, 1, MeatGrinder.InputAction_Press, Hit, Miss, Nothing);
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat + 0.5, 1);
            anim.DoNormalizedAnimation("MeatThrown", normalizedBeat);
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync(meatType.ToString() + "Hit", 0.5f);
            
            bool isBarely = state is >= 1f or <= -1f;

            game.bossAnnoyed = isBarely;
            SoundByte.PlayOneShotGame("meatGrinder/" + (isBarely ? "tink" : "meatHit"));
            game.TackAnim.DoScaledAnimationAsync("TackHit" + (isBarely ? "Barely" : "Success"), 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.bossAnnoyed = true;
            SoundByte.PlayOneShotGame("meatGrinder/miss");

            game.TackAnim.DoScaledAnimationAsync("TackMiss" + meatType, 0.5f);
            game.TackAnim.SetBool("tackMeated", true);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);

            Destroy(gameObject);
        }

        private void Nothing(PlayerActionEvent caller) { }

        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}
