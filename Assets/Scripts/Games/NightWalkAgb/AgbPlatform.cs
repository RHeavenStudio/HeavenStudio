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
        private double endBeat;
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
        private bool stopped;
        private Sound kickSound;
        [SerializeField] private GameObject fallYan;

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
                if (game.ShouldNotJumpOnBeat(endBeat))
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

                    }
                }
                else
                {
                    inputEvent = AgbNightWalk.instance.ScheduleInput(startBeat, endBeat - startBeat, InputType.STANDARD_DOWN, Just, Miss, Empty);
                }
                if (nextPlatformIsSameHeight)
                {
                    kickSound = SoundByte.PlayOneShotGame("nightWalkAgb/boxKick", endBeat);
                    canKick = true;
                    BeatAction.New(gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(endBeat, delegate { if (canKick && !stopped) anim.Play("Kick", 0, 0); })
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
            }
        }

        public void Stop()
        {
            stopped = true;
            if (inputEvent != null) inputEvent.Disable();
            if (kickSound != null) kickSound.Delete();
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
            handler.RaiseHeight(Conductor.instance.songPositionInBeats, lastAdditionalHeightInUnits, additionalHeightInUnits);
            game.playYan.Jump(Conductor.instance.songPositionInBeats);
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
                SoundByte.PlayOneShotGame("nightWalkAgb/wot");
                game.playYan.Hide();
                fallYan.SetActive(true);
            }
        }
        
        private void Empty(PlayerActionEvent caller) { }
    }
}

