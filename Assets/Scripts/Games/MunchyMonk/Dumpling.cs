using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MunchyMonk
{
    public class Dumpling : PlayerActionObject
    {
        public float startBeat;
        public float type;
        public string dumplingString;
        const string sfxName = "munchyMonk/";
        
        [Header("References")]
        [SerializeField] Animator anim;
        [SerializeField] bool needSnap;
        [SerializeField] float threeDelay;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
            anim = GetComponent<Animator>();
        }

        private void Start() 
        {
            if (type == 2f) {
                if (dumplingString == "two_2") game.ScheduleInput(startBeat, 1.5f, InputType.STANDARD_DOWN, Hit, Miss, Early);
                if (dumplingString == "two_1") game.ScheduleInput(startBeat, 2f, InputType.STANDARD_DOWN, Hit, Miss, Early);
            } else {
                game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early);
            }
        }

        private void Update()
        {
            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) || GameManager.instance.currentGame != "munchyMonk") {
                GameObject.Destroy(gameObject);
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.4f);
            
            if (state >= 1f || state <= -1f) 
            {
                game.DumplingsAnim.DoScaledAnimationAsync("Barely", 0.5f);
            } else {
                game.MonkAnim.DoScaledAnimationAsync("Eat", 0.4f);
                game.needBlush = true;
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound(sfxName+"gulp_hit", startBeat), 
                    new MultiSound.Sound(sfxName+"slap", startBeat), 
                });
                GameObject.Destroy(gameObject);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            game.DumplingsAnim.DoScaledAnimationAsync("Miss", 0.5f);
        }

        private void Early(PlayerActionEvent caller) 
        {
            game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
            game.DumplingsAnim.DoScaledAnimationAsync("HitMiss", 0.5f);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound(sfxName+"slap", game.lastReportedBeat),
                new MultiSound.Sound(sfxName+"slap_overlay", game.lastReportedBeat),
            });
        }
    }
}
