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
        private bool canKickRelease;
        private bool doFillStartSound = false;

        private PlayerActionEvent inputEvent;
        private PlayerActionEvent releaseEvent;
        [NonSerialized] public bool stopped;
        [SerializeField] private GameObject fallYan;
        [SerializeField] private GameObject fallYanRoll;
        [SerializeField] private Animator fish;
        [SerializeField] private Animator rollPlatform;
        [SerializeField] private GameObject rollPlatformLong;
        [SerializeField] private GameObject rollPlatformLong2;
        private bool playYanIsFalling;
        private double playYanFallBeat;
        private bool isFish;
        private bool isFinalBlock;
        private bool isEndEvent;
        private bool nextPlatformIsSameHeight;
        private bool isRollPlatform;

        public void StartInput(double beat, double hitBeat)
        {
            if (game == null) game = AgbNightWalk.instance;
            if (anim == null) anim = GetComponent<Animator>();
            if (hitBeat > game.endBeat + 1) 
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                return;
            }
            if (game.RollOnBeat(hitBeat - 1))
            {
                startBeat = beat;
                endBeat = hitBeat;
                ResetInput();
                return;
            }
            isRollPlatform = game.RollOnBeat(hitBeat);
            lastAdditionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat);
            additionalHeightInUnits = game.FindHeightUnitsAtBeat(hitBeat + (isRollPlatform ? 2 : 1));
            additionalHeight = lastAdditionalHeightInUnits * handler.heightAmount;
            nextPlatformIsSameHeight = lastAdditionalHeightInUnits == additionalHeightInUnits;
            isFinalBlock = hitBeat == game.endBeat + 1;
            platform.SetActive(nextPlatformIsSameHeight && !isFinalBlock);
            startBeat = beat;
            endBeat = hitBeat;
            isFish = game.FishOnBeat(endBeat);
            fish.gameObject.SetActive(isFish);
            isEndEvent = game.endBeat == endBeat;

            rollPlatform.transform.parent.gameObject.SetActive(isRollPlatform);
            rollPlatformLong.SetActive(nextPlatformIsSameHeight && !isFinalBlock && !isEndEvent);
            rollPlatformLong2.SetActive(nextPlatformIsSameHeight && !isFinalBlock && !isEndEvent);
            if (isEndEvent) 
            {
                if (isRollPlatform) rollPlatform.Play("EndIdle", 0, 0);
                else anim.Play("EndIdle", 0, 0);
            }
            if (isRollPlatform)
            {
                platform.SetActive(false);
                if (startBeat < endBeat)
                {
                    if (game.ShouldNotJumpOnBeat(endBeat) || isFish)
                    {
                        inputEvent = game.ScheduleUserInput(startBeat, endBeat - startBeat, AgbNightWalk.InputAction_AltDown, JustRollHold, RollMissHold, Empty);
                        if (nextPlatformIsSameHeight && !isFinalBlock && !isEndEvent)
                        {
                            BeatAction.New(this, new List<BeatAction.Action>()
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
                                    if (GameManager.instance.autoplay && !stopped && !isEndEvent)
                                    {
                                        game.playYan.Walk();
                                        anim.DoScaledAnimationAsync("Note", 0.5f);
                                        SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type);
                                    }
                                }),
                                new BeatAction.Action(endBeat + 1, delegate
                                {
                                    if (GameManager.instance.autoplay && !stopped && !isEndEvent)
                                    {
                                        rollPlatform.DoScaledAnimationAsync("Note", 0.5f);
                                        SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type);
                                    }
                                })
                            });
                        }
                        else
                        {
                            BeatAction.New(this, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(endBeat + 0.5, delegate
                                {
                                    if (GameManager.instance.autoplay && !stopped)
                                    {
                                        handler.StopAll();
                                        handler.DestroyPlatforms(endBeat + 2, endBeat - 3, endBeat + 6);
                                        SoundByte.PlayOneShotGame("nightWalkAgb/wot");
                                        game.playYan.Hide();
                                        fallYanRoll.SetActive(true);
                                        fallYanRoll.GetComponent<Animator>().DoScaledAnimationAsync("FallSmear", 0.5f);
                                    }
                                })
                            });
                        }
                    }
                    else
                    {
                        inputEvent = game.ScheduleInput(startBeat, endBeat - startBeat, AgbNightWalk.InputAction_AltDown, JustRollHold, RollMissHold, Empty);
                    }
                    canKick = true;
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(endBeat, delegate
                        {
                            if (!stopped)
                            {
                                SoundByte.PlayOneShotGame("nightWalkAgb/boxKick");
                                if (canKick)
                                {
                                    anim.Play("Kick", 0, 0);
                                }
                            }
                        }),
                    });
                    if (nextPlatformIsSameHeight && !isEndEvent)
                    {
                        canKickRelease = true;
                        BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(endBeat + 0.5, delegate
                            {
                                if (!stopped)
                                {
                                    SoundByte.PlayOneShotGame("nightWalkAgb/boxKick");
                                    if (canKickRelease)
                                    {
                                        rollPlatform.Play("Kick", 0, 0);
                                    }
                                }
                            }),
                        });
                    }
                }
            }
            else
            {
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
                        inputEvent = AgbNightWalk.instance.ScheduleUserInput(startBeat, endBeat - startBeat, Minigame.InputAction_BasicPress, isEndEvent ? JustEnd : Just, Miss, Empty);
                        if (nextPlatformIsSameHeight && !isFinalBlock)
                        {
                            BeatAction.New(this, new List<BeatAction.Action>()
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
                                    if (GameManager.instance.autoplay && !stopped && !isEndEvent)
                                    {
                                        anim.DoScaledAnimationAsync("Note", 0.5f);
                                        SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type);
                                    }
                                })
                            });
                        }
                        else
                        {
                            BeatAction.New(this, new List<BeatAction.Action>()
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
                                        fallYan.GetComponent<Animator>().DoScaledAnimationAsync("FallSmear", 0.5f);
                                    }
                                })
                            });
                        }
                    }
                    else if (!isFish)
                    {
                        inputEvent = AgbNightWalk.instance.ScheduleInput(startBeat, endBeat - startBeat, Minigame.InputAction_BasicPress, isEndEvent ? JustEnd : Just, Miss, Empty);
                    }
                    if (nextPlatformIsSameHeight && !isEndEvent)
                    {
                        canKick = true;
                        BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(endBeat, delegate
                            {
                                if (!stopped)
                                {
                                    SoundByte.PlayOneShotGame("nightWalkAgb/boxKick");
                                    if (canKick)
                                    {
                                        anim.Play("Kick", 0, 0);
                                    }
                                }
                            })
                        });
                    }

                }
            }
        }

        private void Awake()
        {
            game = AgbNightWalk.instance;
            anim = GetComponent<Animator>();
            Update();
        }

        private bool startGlowing;
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
                    if (fallYan.activeSelf) fallYan.transform.localPosition = new Vector3(0, newPlayYanY);
                    else if (fallYanRoll.activeSelf) fallYanRoll.transform.localPosition = new Vector3(fallYanRoll.transform.localPosition.x, newPlayYanY);
                }

                if (!startGlowing && isEndEvent && game.hitJumps >= game.requiredJumps && AgbNightWalk.hitJumpsPersist >= game.requiredJumpsP)
                {
                    if (isRollPlatform) rollPlatform.DoScaledAnimationAsync("EndGlow", 0.5f);
                    else anim.DoScaledAnimationAsync("EndGlow", 0.5f);
                    startGlowing = true;
                }
            }
        }

        public void Stop()
        {
            stopped = true;
            if (inputEvent != null) inputEvent.Disable();
            if (releaseEvent != null) releaseEvent.Disable();
        }

        public void Disappear(double beat)
        {
            anim.DoScaledAnimationAsync("Destroy", 0.5f);
            rollPlatform.DoScaledAnimationAsync("Destroy", 0.5f);
            SoundByte.PlayOneShotGame("nightWalkAgb/disappear");
            if (fallYan.activeSelf || fallYanRoll.activeSelf)
            {
                SoundByte.PlayOneShotGame("nightWalkAgb/fall");
                playYanIsFalling = true;
                playYanFallBeat = beat;
                Update();
            }
        }

        private void ResetInput(float multiplier = 0.5f)
        {
            double newStartBeat = endBeat + (handler.platformCount * multiplier);
            anim.Play("Idle", 0, 0);
            StartInput(newStartBeat, newStartBeat + (handler.platformCount * multiplier));
        }
        private void JustRollHold(PlayerActionEvent caller, float state)
        {
            canKick = false;

            if (caller.noAutoplay)
            {
                releaseEvent = game.ScheduleUserInput(startBeat, endBeat - startBeat + 0.5, AgbNightWalk.InputAction_AltUp, JustRollRelease, RollMissRelease, Empty);
            }
            else
            {
                releaseEvent = game.ScheduleInput(startBeat, endBeat - startBeat + 0.5, AgbNightWalk.InputAction_AltUp, JustRollRelease, RollMissRelease, Empty);
            }

            if (state >= 1f || state <= -1f)
            {
                anim.DoScaledAnimationAsync("FlowerBarely", 0.5f);
                return;
            }
            game.playYan.Roll(Conductor.instance.songPositionInBeats);
            SoundByte.PlayOneShot("games/nightWalkRvl/highJump5");
            anim.DoScaledAnimationAsync("Flower", 0.5f);
        }

        private void JustRollRelease(PlayerActionEvent caller, float state)
        {
            canKickRelease = false;
            double beat = Conductor.instance.songPositionInBeats;
            if (isEndEvent)
            {
                if (game.hitJumps >= game.requiredJumps && AgbNightWalk.hitJumpsPersist >= game.requiredJumpsP)
                {
                    rollPlatform.DoScaledAnimationAsync("EndPop", 0.5f);
                    handler.StopAll();
                    handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 1);
                    game.playYan.Float(Conductor.instance.songPositionInBeats);
                    handler.DevolveAll();
                    if (isFish)
                    {
                        BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + 1, delegate
                            {
                                game.ScoreMiss();
                                game.playYan.Shock();
                                fish.DoScaledAnimationAsync("Shock", 0.5f);
                            }),
                            new BeatAction.Action(caller.timer + caller.startBeat + 4, delegate
                            {
                                game.playYan.Fall(caller.timer + caller.startBeat + 4);
                                fish.DoScaledAnimationAsync("FishIdle", 0.5f);
                            })
                        });
                    }
                }
                else
                {
                    if (isFish)
                    {
                        game.ScoreMiss();
                        game.playYan.transform.localPosition = new Vector3(0, 2);
                        game.playYan.Shock(true);
                        fish.DoScaledAnimationAsync("Shock", 0.5f);
                        handler.StopAll();
                        handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 6);
                        BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(caller.timer + caller.startBeat + 4, delegate
                            {
                                game.playYan.Fall(caller.timer + caller.startBeat + 4);
                                fish.DoScaledAnimationAsync("FishIdle", 0.5f);
                            })
                        });
                    }
                    else
                    {
                        game.playYan.HighJump(beat, true, state >= 1f || state <= -1f);
                        handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat);
                        double missTime = 1.5 - Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat));
                        BeatAction.New(this, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + missTime, delegate
                            {
                                game.ScoreMiss();
                                handler.StopAll();
                            }),

                        });
                    }
                }
            }
            else
            {
                handler.RaiseHeight(beat, lastAdditionalHeightInUnits, additionalHeightInUnits);
                game.playYan.HighJump(beat, isFinalBlock, state >= 1f || state <= -1f);
                if (isFish)
                {
                    game.ScoreMiss();
                    game.playYan.transform.localPosition = new Vector3(0, 2);
                    game.playYan.Shock(true);
                    fish.DoScaledAnimationAsync("Shock", 0.5f);
                    handler.StopAll();
                    handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 6);
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.timer + caller.startBeat + 4, delegate
                        {
                            game.playYan.Fall(caller.timer + caller.startBeat + 4);
                            fish.DoScaledAnimationAsync("FishIdle", 0.5f);
                        })
                    });
                }
                else if (isFinalBlock)
                {
                    handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat);
                    double missTime2 = 1.5 - Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat));
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + missTime2, delegate
                        {
                            game.ScoreMiss();
                            handler.StopAll();
                        }),
                    });
                }
            }
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("nightWalkAgb/ng");
                if (!isEndEvent) rollPlatform.DoScaledAnimationAsync("UmbrellaBarely", 0.5f);
                return;
            }
            SoundByte.PlayOneShot("games/nightWalkRvl/highJump7");
            if (!isEndEvent) rollPlatform.DoScaledAnimationAsync("Umbrella", 0.5f);
            game.starHandler.Evolve(game.evolveAmount * 2);
            game.hitJumps += 2;
            AgbNightWalk.hitJumpsPersist += 2;
        }

        private void RollMissHold(PlayerActionEvent caller)
        {
            if (caller.noAutoplay)
            {
                releaseEvent = game.ScheduleUserInput(startBeat, endBeat - startBeat + 0.5, AgbNightWalk.InputAction_AltUp, JustRollRelease, RollMissRelease, Empty);
            }
            else
            {
                releaseEvent = game.ScheduleInput(startBeat, endBeat - startBeat + 0.5, AgbNightWalk.InputAction_AltUp, JustRollRelease, RollMissRelease, Empty);
            }
            releaseEvent.canHit = false;
            game.playYan.Walk();
            SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type, caller.timer + caller.startBeat + 0.5);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 0.5, delegate { anim.DoScaledAnimationAsync("Note", 0.5f); })
            });
        }

        private void RollMissRelease(PlayerActionEvent caller)
        {
            if (nextPlatformIsSameHeight && !isEndEvent)
            {
                game.playYan.Walk();
                SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type, caller.timer + caller.startBeat + 0.5);
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.timer + caller.startBeat + 0.5, delegate { rollPlatform.DoScaledAnimationAsync("Note", 0.5f); })
                });
            }
            else
            {
                handler.StopAll();
                handler.DestroyPlatforms(caller.timer + caller.startBeat + 1.5, endBeat - 2, endBeat + 6);
                SoundByte.PlayOneShotGame("nightWalkAgb/wot");
                game.playYan.Hide();
                fallYanRoll.SetActive(true);
                fallYanRoll.GetComponent<Animator>().DoScaledAnimationAsync("FallSmear", 0.5f);
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            canKick = false;
            double beat = Conductor.instance.songPositionInBeats;
            handler.RaiseHeight(beat, lastAdditionalHeightInUnits, additionalHeightInUnits);
            game.playYan.Jump(beat, isFinalBlock);
            if (isFish)
            {
                BeatAction.New(this, new List<BeatAction.Action>()
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
            else if (isFinalBlock)
            {
                handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat);
                double missTime = 1 - Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat));
                BeatAction.New(this, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + missTime, delegate
                    {
                        game.ScoreMiss();
                        handler.StopAll();
                    }),
 
                });
            }
            if (state >= 1 || state <= -1)
            {
                SoundByte.PlayOneShotGame("nightWalkAgb/ng");
                switch (type)
                {
                    case PlatformType.Flower:
                        anim.DoScaledAnimationAsync("FlowerBarely", 0.5f);
                        break;
                    case PlatformType.Lollipop:
                        anim.DoScaledAnimationAsync("LollipopBarely", 0.5f);
                        break;
                    case PlatformType.Umbrella:
                        anim.DoScaledAnimationAsync("UmbrellaBarely", 0.5f);
                        break;
                }
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
            game.starHandler.Evolve(game.evolveAmount);
            game.hitJumps++;
            AgbNightWalk.hitJumpsPersist++;
        }

        private void JustEnd(PlayerActionEvent caller, float state)
        {
            double beat = caller.timer + caller.startBeat;
            if (game.hitJumps >= game.requiredJumps && AgbNightWalk.hitJumpsPersist >= game.requiredJumpsP) 
            {
                anim.DoScaledAnimationAsync("EndPop", 0.5f);
                handler.StopAll();
                handler.DestroyPlatforms(caller.timer + caller.startBeat + 2, endBeat - 2, endBeat + 1);
                game.playYan.Float(Conductor.instance.songPositionInBeats);
                handler.DevolveAll();
                if (isFish)
                {
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 0.5, delegate
                        {
                            game.ScoreMiss();
                            game.playYan.Shock();
                            fish.DoScaledAnimationAsync("Shock", 0.5f);
                        }),
                        new BeatAction.Action(caller.timer + caller.startBeat + 4, delegate
                        {
                            game.playYan.Fall(caller.timer + caller.startBeat + 4);
                            fish.DoScaledAnimationAsync("FishIdle", 0.5f);
                        })
                    });
                }
            }
            else
            {
                handler.RaiseHeight(beat, lastAdditionalHeightInUnits, additionalHeightInUnits);
                game.playYan.Jump(beat);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (nextPlatformIsSameHeight)
            {
                game.playYan.Walk();
                SoundByte.PlayOneShotGame("nightWalkAgb/open" + (int)type, caller.timer + caller.startBeat + 0.5);
                BeatAction.New(this, new List<BeatAction.Action>()
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
                fallYan.GetComponent<Animator>().DoScaledAnimationAsync("FallSmear", 0.5f);
            }
        }
        
        private void Empty(PlayerActionEvent caller) { }
    }
}

