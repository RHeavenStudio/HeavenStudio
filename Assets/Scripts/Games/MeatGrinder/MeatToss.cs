using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class MeatToss : PlayerActionObject
    {
        public float startBeat;
        public bool isCue;
        public float cueLength;
        
        const string sfxName = "meatGrinder/";

        [Header("Animators")]
        
        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
        }

        private void Start() 
        {
            game.ScheduleInput(startBeat, cueLength, InputType.STANDARD_DOWN, Hit, Miss, Nothing);
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            };
        }
        private void Hit(PlayerActionEvent caller, float state)
        {
            GameObject.Destroy(gameObject);

            //GameObject MeatBall = Instantiate(game.MeatHitFab);

            if (state >= 1f || state <= -1f)
            {
                game.bossAnnoyed = true;
                Jukebox.PlayOneShotGame(sfxName+"tink");
                game.TackAnim.DoScaledAnimationAsync("TackHitBarely", 0.5f);
                return;
            } else {
                Jukebox.PlayOneShotGame(sfxName+"meatHit");
                game.TackAnim.DoScaledAnimationAsync("TackHitSuccess", 0.5f);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.bossAnnoyed = true;
            game.TackAnim.DoScaledAnimationAsync("TackMissDark", 0.5f);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);
        }

        private void Nothing(PlayerActionEvent caller) 
        {
            
        }
    }
}
