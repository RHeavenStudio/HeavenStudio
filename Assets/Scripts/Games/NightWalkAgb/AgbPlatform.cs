using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatform : MonoBehaviour
    {
        public enum PlatformType
        {
            Flower = 1,
            Lollipop = 2,
            Umbrella = 3
        }
        private double startBeat;
        [NonSerialized] public double endBeat;
        [NonSerialized] public AgbPlatformHandler handler;
        private Animator anim;

        private AgbNightWalk game;

        private PlatformType type = PlatformType.Flower;

        private float additionalHeight = 0f;
        private int additionalHeightInUnits = 0;
        private int lastAdditionalHeightInUnits = 0;

        [SerializeField] private GameObject platform;
        private bool canKick;
        private bool doFillStartSound = false;

        private PlayerActionEvent inputEvent;
        [NonSerialized] public bool stopped;
        private Sound kickSound;
        [SerializeField] private GameObject fallYan;
        [SerializeField] private Animator fish;
        private bool playYanIsFalling;
        private double playYanFallBeat;
        private bool isFish;

        public void StartInput(double beat, double hitBeat)
        {
            if (game == null) game = AgbNightWalk.instance;
            lastAdditionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat);
            additionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat + 1);
            additionalHeight = lastAdditionalHeightInUnits * handler.heightAmount;
            bool nextPlatformIsSameHeight = lastAdditionalHeightInUnits == additionalHeightInUnits;
            platform.SetActive(nextPlatformIsSameHeight);
            startBeat = beat;
            endBeat = hitBeat;
            isFish = game.FishOnBeat(endBeat);
            fish.gameObject.SetActive(isFish);
            if (game.platformTypes.ContainsKey(hitBeat))
            {
                if (game.platformTypes[hitBeat].platformType == AgbNightWalk.PlatformType.Lollipop)
                {
                    type = PlatformType.Lollipop;
                }
                else
                {
                    type = PlatformType.Umbrella;
                }
                doFillStartSound = false;
            }
            else
            {
                type = PlatformType.Flower;
                if (game.platformTypes.ContainsKey(hitBeat + 1)) 
                { 
                    doFillStartSound = game.platformTypes[hitBeat + 1].fillType != AgbNightWalk.FillType.None;
                }
            }
            if (startBeat < endBeat)
            {
                if (game.ShouldNotJumpOnBeat(endBeat) || isFish)
                {
                    inputEvent = AgbNightWalk.instance.ScheduleUserInput(startBeat, endBeat - startBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
                    if (nextPlatformIsSameHeight)
                    {
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(endBeat, delegate
                            {
                                if (GameManager.instance.autoplay && !stopped)
                                {
                                    game.playYan.Walk();
                                }
                            }),
                            new BeatAction.Action(endBeat + 0.5, delegate
                            {
                                if (GameManager.instance.autoplay && !stopped)
                                {
                                    anim.DoScaledAnimationAsync("Note", 0.5f);
                                    SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type);
                                }
                            })
                        });
                    }
                    else
                    {
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(endBeat, delegate
                            {
                                if (GameManager.instance.autoplay && !stopped)
                                {
                                    handler.StopAll();
                                    handler.DestroyPlatforms(endBeat + 2, endBeat - 3, endBeat + 6);
                                    SoundByte.PlayOneShotGame("nightWalkAgb/wot");
                                    game.playYan.Hide();
                                    fallYan.SetActive(true);
                                }
                            })
                        });
                    }
                }
                else if (!isFish)
                {
                    inputEvent = AgbNightWalk.instance.ScheduleInput(startBeat, endBeat - startBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
                }
                if (nextPlatformIsSameHeight)
                {
                    canKick = true;
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(endBeat, delegate 
                        { 
                            if (canKick && !stopped)
                            {
                                SoundByte.PlayOneShotGame("nightWalkAgb/boxKick");
                                anim.Play("Kick", 0, 0);
                            } 
                        })
                    });
                }

            }
        }

        private void Awake()
        {
            game = AgbNightWalk.instance;
            anim = GetComponent<Animator>();
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (!stopped)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(startBeat, endBeat - startBeat);

                    float newPosX = Mathf.LerpUnclamped(handler.playerXPos + (float)((endBeat - startBeat) * handler.platformDistance), handler.playerXPos, normalizedBeat);

                    transform.localPosition = new Vector3(newPosX, handler.defaultYPos + additionalHeight);

                    if (cond.songPositionInBeats > endBeat + (handler.platformCount * 0.5f))
                    {
                        ResetInput();
                    }
                }
                if (playYanIsFalling)
                {
                    float normalizedFallBeat = cond.GetPositionFromBeat(playYanFallBeat, 2);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                    float newPlayYanY = func(0, -12, normalizedFallBeat);
                    fallYan.transform.localPosition = new Vector3(0, newPlayYanY);
                }
            }
        }

        public void Stop()
        {
            stopped = true;
            if (inputEvent != null) inputEvent.Disable();
        }

        public void Disappear(double beat)
        {
            anim.DoScaledAnimationAsync("Destroy", 0.5f);
            SoundByte.PlayOneShotGame("nightWalkAgb/disappear");
            if (fallYan.activeSelf)
            {
                SoundByte.PlayOneShotGame("nightWalkAgb/fall");
                playYanIsFalling = true;
                playYanFallBeat = beat;
                Update();
            }
        }

        private void ResetInput()
        {
            double newStartBeat = endBeat + (handler.platformCount * 0.5f);
            anim.Play("Idle", 0, 0);
            StartInput(newStartBeat, newStartBeat + (handler.platformCount * 0.5f));
        }
        private void Just(PlayerActionEvent caller, float state)
        {
            canKick = false;
            double beat = Conductor.instance.songPositionInBeats;
            handler.RaiseHeight(beat, lastAdditionalHeightInUnits, additionalHeightInUnits);
            game.playYan.Jump(beat);
            if (isFish)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.5, delegate
                    {
                        game.ScoreMiss();
                        game.playYan.Shock();
                        fish.DoScaledAnimationAsync("Shock", 0.5f);
                        handler.StopAll();
                        handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 6);
                    }),
                    new BeatAction.Action(caller.timer + caller.startBeat + 4, delegate
                    {
                        game.playYan.Fall(caller.timer + caller.startBeat + 4);
                        fish.DoScaledAnimationAsync("FishIdle", 0.5f);
                    })
                });
            }
            if (state >= 1 || state <= -1)
            {
                return;
            }
            if (doFillStartSound) SoundByte.PlayOneShotGame("nightWalkAgb/fillStart");
            else SoundByte.PlayOneShotGame("nightWalkAgb/jump" + (int)type);
            switch (type)
            {
                case PlatformType.Flower:
                    anim.DoScaledAnimationAsync("Flower", 0.5f);
                    break;
                case PlatformType.Lollipop:
                    anim.DoScaledAnimationAsync("Lollipop", 0.5f);
                    break;
                case PlatformType.Umbrella:
                    anim.DoScaledAnimationAsync("Umbrella", 0.5f);
                    break;
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (platform.activeSelf)
            {
                game.playYan.Walk();
                SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type, caller.timer + caller.startBeat + 0.5);
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.timer + caller.startBeat + 0.5, delegate { anim.DoScaledAnimationAsync("Note", 0.5f); })
                });
            }
            else
            {
                handler.StopAll();
                handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 6);
                SoundByte.PlayOneShotGame("nightWalkAgb/wot");
                game.playYan.Hide();
                fallYan.SetActive(true);
            }
        }
        
        private void Empty(PlayerActionEvent caller) { }
    }
}

