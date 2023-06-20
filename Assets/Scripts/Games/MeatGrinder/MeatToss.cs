using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class MeatToss : MonoBehaviour
    {
        public double startBeat;
        public double cueLength;
        public bool cueBased;
        public string meatType;

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
            game.ScheduleInput(startBeat, cueLength, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Nothing);

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(cueBased ? startBeat + 0.66f : cueLength + startBeat - 1 + 0.66f, delegate { 
                    anim.DoScaledAnimationAsync(meatType+"Thrown", 0.32f); 
                }),
            });
        }

        private void Update()
        {
            if (anim.IsPlayingAnimationName("DarkIdle") || anim.IsPlayingAnimationName("LightIdle")) GameObject.Destroy(gameObject);
        }
        
        private void InputActions(bool annoyBoss, string whichSfx, string whichAnim)
        {
            game.bossAnnoyed = annoyBoss;
            SoundByte.PlayOneShotGame("meatGrinder/"+whichSfx);
            game.TackAnim.DoScaledAnimationAsync(whichAnim, 0.5f);
        } 

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.TackAnim.SetBool("tackMeated", false);
            anim.DoScaledAnimationAsync(meatType+"Hit", 0.5f);

            if (state >= 1f || state <= -1f) {
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

        private void Nothing(PlayerActionEvent caller) { }
    }
}
