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
        public bool cueBased;
        public string meatType;
        bool animCheck = false;
        


        [Header("Animators")]
        private Animator anim;

        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
            anim = GetComponent<Animator>();
        }

        private void Start() 
        {
            game.ScheduleInput(startBeat, cueLength, InputType.STANDARD_DOWN, Hit, Miss, Nothing);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(cueBased ? startBeat + 0.66f : cueLength + startBeat - 1 + 0.66f, delegate { anim.DoScaledAnimationAsync(meatType+"Thrown", 0.32f); }),
            });
        }

        private void Update()
        {
            if (GameManager.instance.currentGame != "meatGrinder") {
                GameObject.Destroy(gameObject);
            }

            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            }
            if (anim.IsAnimationNotPlaying() && animCheck) GameObject.Destroy(gameObject);
        }
        
        private void InputActions(bool annoyBoss, string whichSfx, string whichAnim)
        {
            game.bossAnnoyed = annoyBoss;
            Jukebox.PlayOneShotGame("meatGrinder/"+whichSfx);
            game.TackAnim.DoScaledAnimationAsync(whichAnim, 0.5f);
        } 

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync(meatType+"Hit", 0.5f);
            animCheck = true;

            if (state >= 1f || state <= -1f)
            {
                InputActions(true, "tink", "TackHitBarely");
            } else {
                InputActions(false, "meatHit", "TackHitSuccess");
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            GameObject.Destroy(gameObject);
            InputActions(true, "miss", "TackMiss"+meatType);
            game.BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);
            game.TackAnim.SetBool("tackMeated", true);
        }

        private void Nothing(PlayerActionEvent caller) 
        {
            
        }
    }
}
