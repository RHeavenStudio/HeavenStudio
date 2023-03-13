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
        const string sfxName = "munchyMonk/";
        
        [Header("References")]
        [SerializeField] Animator anim;
        [SerializeField] bool needSnap;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
            anim = GetComponent<Animator>();
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
            
            if (state >= 1f || state <= -1f) 
            {
                game.DumplingsAnim.DoScaledAnimationAsync("Barely", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"barely");
            } else {
                game.MonkAnim.DoScaledAnimationAsync("Eat", 0.4f);
                game.needBlush = true;
                Jukebox.PlayOneShotGame(sfxName+"gulp");
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
                new MultiSound.Sound(sfxName+"miss", game.lastReportedBeat),
            });
        }
    }
}
