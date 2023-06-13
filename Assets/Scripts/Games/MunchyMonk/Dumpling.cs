using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_MunchyMonk
{
    public class Dumpling : MonoBehaviour
    {
        public Color dumplingColor;
        public double startBeat;
        
        const string sfxName = "munchyMonk/";
        public bool canDestroy;
        
        [Header("References")]
        [SerializeField] Animator smearAnim;
        [SerializeField] Animator anim;
        [SerializeField] SpriteRenderer smearSr;
        public SpriteRenderer sr;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start() 
        {
            sr.color = dumplingColor;
            if (game.dumplings.Count > 1) {
                anim.Play("IdleOnTop", 0, 0);
                game.dumplings[0].anim.DoScaledAnimationAsync("Squish", 0.5f);
            }
        }

        private void Update()
        {
            if (canDestroy && anim.IsAnimationNotPlaying()) GameObject.Destroy(gameObject);
        }

        public void HitFunction(float state)
        {
            smearSr.color = dumplingColor;
            game.MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
            SoundByte.PlayOneShotGame(sfxName+"slap");
            game.isStaring = false;
            
            if (state >= 1f || state <= -1f)
            {
                game.MonkAnim.DoScaledAnimationAsync("Barely", 0.5f);
                anim.DoScaledAnimationAsync("HitHead", 0.5f);
                SoundByte.PlayOneShotGame(sfxName+"barely");
                canDestroy = true;
                game.needBlush = false;
            } else {
                game.MonkAnim.DoScaledAnimationAsync("Eat", 0.4f);
                game.dumplings[0].anim.DoScaledAnimationAsync("FollowHand", 0.5f);
                smearAnim.Play("SmearAppear", 0, 0);
                game.needBlush = true;
                SoundByte.PlayOneShotGame(sfxName+"gulp");
                MunchyMonk.howManyGulps++;
                for (int i = 1; i <= 4; i++)
                {
                    if (MunchyMonk.howManyGulps == MunchyMonk.inputsTilGrow*i) {
                        MunchyMonk.growLevel = i;
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
                new MultiSound.Sound(sfxName+"slap", Conductor.instance.songPositionInBeatsAsDouble),
                new MultiSound.Sound(sfxName+"miss", Conductor.instance.songPositionInBeatsAsDouble),
            });
            canDestroy = true;
            game.needBlush = false;
        }
    }
}
