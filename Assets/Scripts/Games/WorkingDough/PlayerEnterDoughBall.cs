using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class PlayerEnterDoughBall : SuperCurveObject
    {
        private enum State
        {
            None,
            Entering,
            Hit,
            Barely,
            Miss,
            Weak
        }
        private State currentState;

        private double startBeat = double.MinValue;

        private bool big;

        private Path enterPath;
        private Path hitPath;
        private Path barelyPath;
        private Path missPath;
        private Path weakPath;

        private WorkingDough game;

        private PlayerActionEvent wrongInput;
        private PlayerActionEvent rightInput;

        [SerializeField] private GameObject gandw;

        private void Awake()
        {
            game = WorkingDough.instance;
        }

        public void Init(double beat, bool isBig, bool hasGandw)
        {
            startBeat = beat;
            big = isBig;
            enterPath = game.GetPath("PlayerEnter");
            hitPath = game.GetPath("PlayerHit");
            barelyPath = game.GetPath("PlayerBarely");
            missPath = game.GetPath("PlayerMiss");
            weakPath = game.GetPath("PlayerWeak");
            rightInput = game.ScheduleInput(beat, 1, isBig ? InputType.STANDARD_ALT_DOWN : InputType.STANDARD_DOWN, Just, Miss, Empty);
            wrongInput = game.ScheduleUserInput(beat, 1, isBig ? InputType.STANDARD_DOWN : InputType.STANDARD_ALT_DOWN, WrongInput, Empty, Empty);
            currentState = State.Entering;
            if (gandw != null) gandw.SetActive(hasGandw);
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (startBeat > double.MinValue)
                {
                    Vector3 pos = new Vector3();
                    double beat = cond.songPositionInBeats;
                    switch (currentState)
                    {
                        case State.None:
                            break;
                        case State.Entering:
                            pos = GetPathPositionFromBeat(enterPath, Math.Max(beat, startBeat), startBeat);
                            break;
                        case State.Hit:
                            pos = GetPathPositionFromBeat(hitPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Miss:
                            pos = GetPathPositionFromBeat(missPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Weak:
                            pos = GetPathPositionFromBeat(weakPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 1)
                            {
                                Destroy(gameObject);
                            }
                            break;
                        case State.Barely:
                            pos = GetPathPositionFromBeat(barelyPath, Math.Max(beat, startBeat), startBeat);
                            if (beat >= startBeat + 2)
                            {
                                Destroy(gameObject);
                            }
                            break;
                    }
                    if (startBeat <= beat) transform.position = pos;
                    else transform.position = new Vector3(-80, -80);
                }
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            wrongInput.Disable();
            double beat = Conductor.instance.songPositionInBeats;
            startBeat = beat;
            game.playerImpact.SetActive(true);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { game.playerImpact.SetActive(false); }),
            });
            if (state >= 1f || state <= -1f)
            {
                currentState = State.Barely;
                SoundByte.PlayOneShot("miss");
                if (big)
                {
                    SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                    game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("BigDoughJump", 0.5f);
                }
                else
                {
                    SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                    game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("SmallDoughJump", 0.5f);
                }
                Update();
                return;
            }
            currentState = State.Hit;
            if (big)
            {
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitBigPlayer");
                game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("BigDoughJump", 0.5f);
                game.backgroundAnimator.Play("BackgroundFlash", 0, 0);
            }
            else
            {
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                SoundByte.PlayOneShotGame("workingDough/hitSmallPlayer");
                game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("SmallDoughJump", 0.5f);
            }
            bool hasGandw = false;
            if (gandw != null) hasGandw = gandw.activeSelf;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.9f, delegate { game.arrowSRRightPlayer.sprite = game.redArrowSprite; }),
                new BeatAction.Action(beat + 1f, delegate { game.arrowSRRightPlayer.sprite = game.whiteArrowSprite; }),
                new BeatAction.Action(beat + 2f, delegate { game.SpawnBGBall(beat + 2f, big, hasGandw); }),
            });
            Update();
        }

        private void WrongInput(PlayerActionEvent caller, float state)
        {
            double beat = Conductor.instance.songPositionInBeats;
            rightInput.Disable();
            game.playerImpact.SetActive(true);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.1f, delegate { game.playerImpact.SetActive(false); }),
            });
            if (big)
            {
                currentState = State.Weak;
                startBeat = beat;
                game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("SmallDoughJump", 0.5f);
                SoundByte.PlayOneShotGame("workingDough/smallPlayer");
                SoundByte.PlayOneShotGame("workingDough/tooBig");
                Update();
            }
            else
            {
                GameObject.Instantiate(game.breakParticleEffect, game.breakParticleHolder);
                game.doughDudesPlayer.GetComponent<Animator>().DoScaledAnimationAsync("BigDoughJump", 0.5f);
                SoundByte.PlayOneShotGame("workingDough/bigPlayer");
                SoundByte.PlayOneShotGame("workingDough/tooSmall");
                Destroy(gameObject);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            double beat = caller.timer + caller.startBeat;
            currentState = State.Miss;
            startBeat = beat;
            Update();
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.25f, delegate { game.missImpact.SetActive(true); }),
                new BeatAction.Action(beat + 0.25f, delegate { SoundByte.PlayOneShotGame("workingDough/BallMiss"); }),
                new BeatAction.Action(beat + 0.35f, delegate { game.missImpact.SetActive(false); }),
            });
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}


