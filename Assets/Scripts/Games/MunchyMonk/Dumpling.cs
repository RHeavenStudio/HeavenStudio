using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MunchyMonk
{
    public class Dumpling : PlayerActionObject
    {
        public Animator otherAnim;
        public float startBeat;
        public float type;
        const string sfxName = "munchyMonk/";
        
        [Header("References")]
        [SerializeField] Animator anim;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
        }

        private void Start() 
        {
            if (type == 1f || type == 3f) {
                game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early);
            } else if (type >= 3.5f) {
                game.ScheduleInput(startBeat, 0.75f, InputType.STANDARD_DOWN, Hit, Miss, Early);
            } else {
                game.ScheduleInput(startBeat, type == 2f ? 1.5f : 2f, InputType.STANDARD_DOWN, Hit, Miss, Early);
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
            Jukebox.PlayOneShotGame(sfxName+"slap");
            game.isStaring = false;
            
            if (state >= 1f || state <= -1f) 
            {
                game.MonkAnim.DoScaledAnimationAsync("Barely", 0.4f);
                anim.DoScaledAnimationAsync("HitHead", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"barely");
            } else {
                game.MonkAnim.DoScaledAnimationAsync("Eat", 0.4f);
                if (type == 2) otherAnim.DoScaledAnimationAsync("FollowHand", 0.5f);
                game.SmearAnim.DoScaledAnimationAsync("SmearAppear", 0.5f);
                game.needBlush = true;
                Jukebox.PlayOneShotGame(sfxName+"gulp");
                GameObject.Destroy(gameObject);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            anim.DoScaledAnimationAsync("FallOff", 0.5f);
        }

        private void Early(PlayerActionEvent caller) 
        {
            if (!(type <= 2f || type >= 2.5f)) {
                game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                game.MonkAnim.DoScaledAnimationAsync("Miss", 0.5f);
                anim.DoScaledAnimationAsync("HitMiss", 0.5f);
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"slap", game.lastReportedBeat),
                    new MultiSound.Sound(sfxName+"miss", game.lastReportedBeat),
                });
            }
        }
    }
}
