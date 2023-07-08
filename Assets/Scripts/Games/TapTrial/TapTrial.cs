using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("tapTrial", "Tap Trial", "94ffb5", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.Bop(e.beat, e.length, e["toggle"], e["toggle2"]); }, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Whether both will bop to the beat or not"),
                        new Param("toggle2", false, "Bop (Auto)", "Whether both will bop automatically to the beat or not")
                    }
                },
                new GameAction("tap", "Tap")
                {
                    function = delegate { TapTrial.instance.Tap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("double tap", "Double Tap")
                {
                    function = delegate { TapTrial.instance.DoubleTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2.0f
                },
                new GameAction("triple tap", "Triple Tap")
                {
                    function = delegate { TapTrial.instance.TripleTap(eventCaller.currentEntity.beat); }, 
                    defaultLength = 4.0f
                },
                new GameAction("jump tap prep", "Prepare Stance")
                {
                    function = delegate { TapTrial.instance.JumpPrepare(); }, 
                },
                new GameAction("jump tap", "Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, e["final"]); }, 
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("final", false, "Final")
                    }
                },
                new GameAction("scroll event", "Scroll Background")
                {
                    function = delegate {  }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Scroll FX", "Will scroll"),
                        new Param("flash", false, "Flash FX", "Will flash to white"),
                    }
                },
                new GameAction("giraffe events", "Giraffe Animations")
                {
                    function = delegate { }, 
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Enter?", "Giraffe will enter the scene"),
                        new Param("instant", false, "Instant", "Will the giraffe enter/exit instantly?")
                    }
                },
                // backwards-compatibility
                new GameAction("final jump tap", "Final Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, true); },
                    defaultLength = 2.0f,
                    hidden = true
                },
            },
            new List<string>() {"agb", "normal"},
            "agbtap", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_TapTrial;

    public class TapTrial : Minigame
    {
        [Header("Components")]
        [SerializeField] private TapTrialPlayer player;
        [SerializeField] private Animator monkeyL, monkeyR;
        [SerializeField] private ParticleSystem monkeyTapLL, monkeyTapLR, monkeyTapRL, monkeyTapRR;
        [SerializeField] private Transform rootPlayer, rootMonkeyL, rootMonkeyR;
        [Header("Values")]
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float monkeyJumpHeight = 3f;

        private GameEvent bop = new();
        private bool canBop = true;
        private bool shouldBop = true;

        private double jumpStartBeat = double.MinValue;

        public static TapTrial instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (shouldBop && cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    SingleBop();
                }

                float normalizedJumpBeat = cond.GetPositionFromBeat(jumpStartBeat, 1);

                if (normalizedJumpBeat >= 0 && normalizedJumpBeat <= 1)
                {
                    if (normalizedJumpBeat >= 0.5f)
                    {
                        float normalizedUp = cond.GetPositionFromBeat(jumpStartBeat, 0.5);
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                        float newPlayerY = func(0, jumpHeight, normalizedUp);
                        float newMonkeyY = func(0, monkeyJumpHeight, normalizedUp);
                        rootPlayer.localPosition = new Vector3(0, newPlayerY);
                        rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                        rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                    }
                    else
                    {
                        float normalizedDown = cond.GetPositionFromBeat(jumpStartBeat + 0.5, 0.5);
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                        float newPlayerY = func(jumpHeight, 0, normalizedDown);
                        float newMonkeyY = func(monkeyJumpHeight, 0, normalizedDown);
                        rootPlayer.localPosition = new Vector3(0, newPlayerY);
                        rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                        rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                    }
                }
                else
                {
                    rootPlayer.localPosition = Vector3.zero;
                    rootMonkeyL.localPosition = Vector3.zero;
                    rootMonkeyR.localPosition = Vector3.zero;
                }
            }
        }

        public void Bop(double beat, float length, bool bop, bool autoBop)
        {
            shouldBop = autoBop;
            if (bop)
            {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i  < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate { SingleBop(); }));
                }
                BeatAction.New(gameObject, actions);
            }
        }

        private void SingleBop()
        {
            if (!canBop) return;
            PlayMonkeyAnimationScaledAsync("Bop", 0.5f);
            player.Bop();
        }

        public void Tap(double beat)
        {
            canBop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("TapPrepare", 0.5f);
                    player.PrepareTap();
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("Tap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    canBop = true;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ook", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1),
            });

            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, JustTap, Empty, Empty);
        }

        public void DoubleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare", 0.5f);
                    player.PrepareTap(true);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                    canBop = true;
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ookook", beat),
                new MultiSound.Sound("tapTrial/ookook", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1.5),
            });

            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, JustDoubleTap, Empty, Empty);
            ScheduleInput(beat, 1.5, InputType.STANDARD_DOWN, JustDoubleTap, Empty, Empty);
        }

        public void TripleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.PrepareTripleTap();
                    PlayMonkeyAnimationScaledAsync("PostPrepare_1", 0.5f);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 2.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap_2", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 3, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ooki1", beat),
                new MultiSound.Sound("tapTrial/ooki2", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 3),
            });

            ScheduleInput(beat, 2, InputType.STANDARD_DOWN, JustTripleTap, Empty, Empty);
            ScheduleInput(beat, 2.5, InputType.STANDARD_DOWN, JustTripleTap, Empty, Empty);
            ScheduleInput(beat, 3, InputType.STANDARD_DOWN, JustTripleTap, Empty, Empty);
        }

        public void JumpPrepare()
        {
            canBop = false;
            player.PrepareJump();
            PlayMonkeyAnimationScaledAsync("JumpPrepare", 0.5f);
        }

        public void JumpTap(double beat, bool final)
        {
            canBop = false;
            jumpStartBeat = beat;
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.Jump(final);
                    PlayMonkeyAnimationScaledAsync("JumpTap", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync(final ? "FinalJumpTap" : "Jumpactualtap", 0.5f);
                    MonkeyParticles(true);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    canBop = final;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(final ? "tapTrial/jumptap2" : "tapTrial/jumptap1", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1),
            });

            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, final ? JustJumpTapFinal : JustJumpTap, final ? MissJumpFinal : MissJump, Empty);
        }

        private void JustJumpTap(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, false);
        }

        private void JustJumpTapFinal(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, true);
        }

        private void MissJump(PlayerActionEvent caller)
        {
            player.JumpTapMiss(false);
        }

        private void MissJumpFinal(PlayerActionEvent caller)
        {
            player.JumpTapMiss(true);
        }

        private void JustTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f);
        }

        private void JustDoubleTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f, true);
        }

        private void JustTripleTap(PlayerActionEvent caller, float state)
        {
            player.TripleTap(state < 1f && state > -1f);
        }

        private void Empty(PlayerActionEvent caller) { }

        private void PlayMonkeyAnimationScaledAsync(string name, float timeScale)
        {
            monkeyL.DoScaledAnimationAsync(name, timeScale);
            monkeyR.DoScaledAnimationAsync(name, timeScale);
        }

        private void MonkeyParticles(bool left)
        {
            ParticleSystem spawnedEffectL = Instantiate(left ? monkeyTapLL : monkeyTapLR, transform);
            spawnedEffectL.Play();

            ParticleSystem spawnedEffectR = Instantiate(left ? monkeyTapRL : monkeyTapRR, transform);
            spawnedEffectR.Play();
        }
    }
}