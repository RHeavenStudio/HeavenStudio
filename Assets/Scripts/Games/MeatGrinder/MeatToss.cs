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
        const string sfxName = "meatGrinder/";
        float flyPos;


        [Header("Animators")]
        private Animator anim;

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
                new BeatAction.Action(startBeat + 0.5f, delegate { anim.DoScaledAnimationAsync("DarkMeatThrown", 0.3f); }),
            });
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            }
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
