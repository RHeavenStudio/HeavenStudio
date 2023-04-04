using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MunchyMonk
{
    public class Dumpling : PlayerActionObject
    {
        public Color dumplingColor;
        public float startBeat;
        public int dumplingID;
        
        const string sfxName = "munchyMonk/";
        public bool canDestroy;
        private bool needSquish = true;
        private bool canSquish;
        
        [Header("References")]
        [SerializeField] Animator smearAnim;
        [SerializeField] SpriteRenderer smearSr;
        [SerializeField] Animator anim;
        [SerializeField] SpriteRenderer sr;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
        }

        private void Start() 
        {
            sr.color = dumplingColor;
            if (game.dumplings.Count > 1) anim.Play("IdleOnTop", 0, 0);
        }

        private void Update()
        {
            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) || GameManager.instance.currentGame != "munchyMonk") {
                GameObject.Destroy(gameObject);
            }

            if (game.dumplings.Count == 1) {
                canSquish = true;
            }

            if (game.dumplings.Count > 1 && needSquish && canSquish) anim.DoScaledAnimationAsync("Squish", 0.5f);
            if (anim.IsPlayingAnimationName("Squish")) needSquish = false;

            if (canDestroy && anim.IsAnimationNotPlaying()) GameObject.Destroy(gameObject);
        }

        public void HitFunction(float state)
        {
            smearSr.color = dumplingColor;
            game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
            Jukebox.PlayOneShotGame(sfxName+"slap");
            game.isStaring = false;
            
            if (state >= 1f || state <= -1f)
            {
                game.MonkAnim.DoScaledAnimationAsync("Barely", 0.5f);
                anim.DoScaledAnimationAsync("HitHead", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"barely");
                canDestroy = true;
                game.needBlush = false;
            } else {
                game.MonkAnim.DoScaledAnimationAsync("Eat", 0.4f);
                if (!needSquish) anim.DoScaledAnimationAsync("FollowHand", 0.5f);
                smearAnim.Play("SmearAppear", 0, 0);
                game.needBlush = true;
                Jukebox.PlayOneShotGame(sfxName+"gulp");
                if (game.forceGrow) game.growLevel++;
                game.howManyGulps++;
                for (int i = 1; i <= 4; i++)
                {
                    if (game.howManyGulps == game.inputsTilGrow*i) {
                        game.growLevel = i;
                    }
                }
                GameObject.Destroy(gameObject);
            }
        }

        public void MissFunction()
        {
            if (!canDestroy) {
                anim.DoScaledAnimationAsync("FallOff", 0.5f);
                canDestroy = true;
            }
        }

        public void EarlyFunction()
        {
            game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
            game.MonkAnim.DoScaledAnimationAsync("Miss", 0.5f);
            smearAnim.Play("SmearAppear", 0, 0);
            anim.DoScaledAnimationAsync("HitHead", 0.5f);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound(sfxName+"slap", game.lastReportedBeat),
                new MultiSound.Sound(sfxName+"miss", game.lastReportedBeat),
            });
            canDestroy = true;
            game.needBlush = false;
        }
    }
}
