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
        private bool canDestroy;
        private int missCounter;
        bool delayEarly;
        
        [Header("References")]
        [SerializeField] Animator smearAnim;
        [SerializeField] Animator anim;

        private MunchyMonk game;

        private void Awake()
        {
            game = MunchyMonk.instance;
            
            delayEarly = game.twoTwoBuffer;
            missCounter = (type == 2.5f ? 2 : 1);
            
            if (game.twoTwoBuffer) BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(startBeat+0.2f, delegate { delayEarly = false; }),
            });
        }

        private void Start() 
        {
            if (type == 1f || type == 3f) {
                game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early);
            } else if (type > 3f) {
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

            if (canDestroy && anim.IsAnimationNotPlaying()) GameObject.Destroy(gameObject);
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (!canDestroy && missCounter !<= 0) {
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
                    if (type == 2) otherAnim.DoScaledAnimationAsync("FollowHand", 0.5f);
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
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (!canDestroy) {
                anim.DoScaledAnimationAsync("FallOff", 0.5f);
                canDestroy = true;
            }
        }

        private void Early(PlayerActionEvent caller) 
        { 
            Debug.Log(missCounter);
            if (!delayEarly && missCounter == 1) {
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
                missCounter--;
            }
        }
    }
}
