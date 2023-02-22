using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class MeatToss : PlayerActionObject
    {
        public float startBeat;
        public float cueLength;
        public int meatType;
        const string sfxName = "meatGrinder/";
        float flyPos;
        bool animCheck = false;
        


        [Header("Animators")]
        private Animator anim;

        [Header("GameObjects")]
        public GameObject MeatBall;

        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
            anim = GetComponent<Animator>();
            
            flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+1.1f;
        }

        private void Start() 
        {
            game.ScheduleInput(startBeat, cueLength, InputType.STANDARD_DOWN, Hit, Miss, Nothing);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 0.58f, delegate { anim.DoScaledAnimationAsync("DarkMeatThrown", 0.32f); }),
            });
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            }
            if (anim.IsAnimationNotPlaying() && animCheck) GameObject.Destroy(gameObject);
        }
        private void Hit(PlayerActionEvent caller, float state)
        {
            //GameObject.Destroy(gameObject);
            
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync("DarkMeatHit", 0.5f);
            animCheck = true;

            if (state >= 1f || state <= -1f)
            {
                game.bossAnnoyed = true;
                Jukebox.PlayOneShotGame(sfxName+"tink");
                game.TackAnim.DoScaledAnimationAsync("TackHitBarely", 0.5f);
                return;
            } else {
                game.bossAnnoyed = false;
                Jukebox.PlayOneShotGame(sfxName+"meatHit");
                game.TackAnim.DoScaledAnimationAsync("TackHitSuccess", 0.5f);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            GameObject.Destroy(gameObject);
            game.bossAnnoyed = true;
            Jukebox.PlayOneShotGame(sfxName+"miss");
            game.TackAnim.DoScaledAnimationAsync("TackMissDark", 0.5f);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);
        }

        private void Nothing(PlayerActionEvent caller) 
        {
            
        }
    }
}
