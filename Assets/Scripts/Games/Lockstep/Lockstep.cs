/* I do not know crap about Unity or C#
Almost none of this code is mine, but it's all fair game when the game you're stealing from
borrowed from other games */

using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrBackbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("lockstep", "Lockstep \n<color=#eb5454>[WIP]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Bop(e.beat, e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Reset Pose", "Resets to idle pose.")
                    },
                    defaultLength = 1f,
                },
                new GameAction("marching", "Stepping")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Lockstep.Marching(e.beat, e.length);},
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("offbeatSwitch", "Switch to Offbeat")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; Lockstep.OffbeatSwitch(e.beat); },
                    defaultLength = 3.5f
                },
                new GameAction("onbeatSwitch", "Switch to Onbeat")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; Lockstep.OnbeatSwitch(e.beat); },
                    defaultLength = 2f
                },
                new GameAction("hai", "Hai!")
                {
                    function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Hai(e.beat); },
                    defaultLength = 1f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Hai(e.beat);}
                },
                new GameAction("ho", "Ho!")
                {
                    function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Ho(e.beat); },
                    defaultLength = 1f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Ho(e.beat);}
                },
                new GameAction("set colours", "Set Background Colours")
                {
                    function = delegate {var e = eventCaller.currentEntity; Lockstep.instance.SetbackgroundColours(e["colorA"], e["colorB"]); },
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Lockstep.defaultBGColorOn, "Onbeat", "Select the color that appears for the onbeat."),
                        new Param("colorB", Lockstep.defaultBGColorOff, "Offbeat", "Select the color that appears for the offbeat."),
                    },
                    defaultLength = 0.5f,
                }
            });

        }
    }
}

namespace HeavenStudio.Games
{
   // using Scripts_Lockstep;
    public class Lockstep : Minigame
    {
        private static Color _defaultBGColorOn;
        public static Color defaultBGColorOn
        {
            get
            {
                ColorUtility.TryParseHtmlString("#f0338d", out _defaultBGColorOn);
                return _defaultBGColorOn;
            }
        }

        private static Color _defaultBGColorOff;
        public static Color defaultBGColorOff
        {
            get
            {
                ColorUtility.TryParseHtmlString("#BC318B", out _defaultBGColorOff);
                return _defaultBGColorOff;
            }
        }

        public Color currentBGOnColor;
        public Color currentBGOffColor;

        [Header("Components")]
        [SerializeField] Animator stepswitcherP;
        [SerializeField] Animator stepswitcher0;
        [SerializeField] Animator stepswitcher1;
        [SerializeField] SpriteRenderer background;


        [Header("Properties")]
        static List<float> queuedInputs = new List<float>();
        HowMissed currentMissStage;
        public enum HowMissed
        {
            NotMissed = 0,
            MissedOff = 1,
            MissedOn = 2
        }
        bool offColorActive;

        public static Lockstep instance;

        void Awake()
        {
            instance = this;
            currentBGOnColor = defaultBGColorOn;
            currentBGOffColor = defaultBGColorOff;
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs)
                    {
                        ScheduleInput(cond.songPositionInBeats, input - cond.songPositionInBeats, InputType.STANDARD_DOWN, Just, Miss, Nothing);
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(input, delegate { EvaluateMarch(); }),
                        });
                    }
                    queuedInputs.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    currentMissStage = HowMissed.NotMissed;
                    var beatAnimCheck = Math.Round(cond.songPositionInBeats * 2);
                    var stepPlayerAnim = (beatAnimCheck % 2 != 0 ? "OffbeatMarch" : "OnbeatMarch");
                    Jukebox.PlayOneShotGame("lockstep/miss");
                    stepswitcherP.DoScaledAnimationAsync(stepPlayerAnim, 0.5f);
                    ScoreMiss();
                }
            }

        }

        public void Bop(float beat, bool reset)
        {
            if (reset)
            {
                stepswitcher0.DoScaledAnimationAsync("BopReset", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("BopReset", 0.5f);
                stepswitcherP.DoScaledAnimationAsync("BopReset", 0.5f);

            }
            else
            {
                stepswitcher0.DoScaledAnimationAsync("Bop", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("Bop", 0.5f);
                stepswitcherP.DoScaledAnimationAsync("Bop", 0.5f);

            }
        }

        public void Hai(float beat)
        {
            Jukebox.PlayOneShotGame("lockstep/switch1");
        }

        public void Ho(float beat)
        {
            Jukebox.PlayOneShotGame("lockstep/switch4");
        }

        public static void OnbeatSwitch(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("lockstep/switch5", beat),
                new MultiSound.Sound("lockstep/switch6", beat + 0.5f),
                new MultiSound.Sound("lockstep/switch5", beat + 1f),
                new MultiSound.Sound("lockstep/switch6", beat + 1.5f)
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(false); }),
                new BeatAction.Action(beat + 0.5f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(true); }),
                new BeatAction.Action(beat + 1f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(false); }),
                new BeatAction.Action(beat + 1.5f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(true); }),
                new BeatAction.Action(beat + 2f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(false); }),
            });
        }

        public static void OffbeatSwitch(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("lockstep/switch1", beat),
                new MultiSound.Sound("lockstep/switch1", beat + 1f),
                new MultiSound.Sound("lockstep/switch1", beat + 2f),
                new MultiSound.Sound("lockstep/switch2", beat + 3f),
                new MultiSound.Sound("lockstep/switch3", beat + 3.5f),
            }, forcePlay: true);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(true); }),
                new BeatAction.Action(beat + 1f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(false); }),
                new BeatAction.Action(beat + 2f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(true); }),
                new BeatAction.Action(beat + 3f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(false); }),
                new BeatAction.Action(beat + 3.5f, delegate { if (GameManager.instance.currentGame == "lockstep") Lockstep.instance.ChangeBeatBackGroundColour(true); }),
            });
        }

        public static void Marching(float beat, float length)
        {
            if (GameManager.instance.currentGame == "lockstep")
            {
                for (int i = 0; i < length + 1; i++)
                {
                    Lockstep.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, Lockstep.instance.Just, Lockstep.instance.Miss, Lockstep.instance.Nothing);
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate { Lockstep.instance.EvaluateMarch(); }),
                    });
                }
            }
            else
            {
                for (int i = 0; i < length + 1; i++)
                {
                    queuedInputs.Add(beat + i);
                }
            }
        }

        public void EvaluateMarch()
        {
            var cond = Conductor.instance;
            var beatAnimCheck = Math.Round(cond.songPositionInBeats * 2);
            if (beatAnimCheck % 2 != 0)
            {
                stepswitcher0.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
            }
            else
            {
                stepswitcher0.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                stepswitcher1.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            }
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            currentMissStage = HowMissed.NotMissed;
            if (state >= 1f || state <= -1f)
            {
                var cond = Conductor.instance;
                var beatAnimCheck = Math.Round(cond.songPositionInBeats * 2);
                if (beatAnimCheck % 2 != 0)
                {
                    Jukebox.PlayOneShotGame("lockstep/tink");
                    stepswitcherP.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
                }
                else
                {
                    Jukebox.PlayOneShotGame("lockstep/tink");
                    stepswitcherP.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                }
                return;
            }
            Success();
        }

        public void Success()
        {
            var cond = Conductor.instance;
            var beatAnimCheck = Math.Round(cond.songPositionInBeats * 2);
            if (beatAnimCheck % 2 != 0)
            {
                Jukebox.PlayOneShotGame($"lockstep/marchOffbeat{UnityEngine.Random.Range(1, 3)}");
                stepswitcherP.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
            }
            else
            {
                Jukebox.PlayOneShotGame($"lockstep/marchOnbeat{UnityEngine.Random.Range(1, 3)}");
                stepswitcherP.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            var cond = Conductor.instance;
            var beatAnimCheck = Math.Round(cond.songPositionInBeats * 2);
            
            if (beatAnimCheck % 2 != 0 && currentMissStage != HowMissed.MissedOff)
            {
                stepswitcherP.Play("OffbeatMiss", 0, 0);
                Jukebox.PlayOneShotGame("lockstep/wayOff");
                currentMissStage = HowMissed.MissedOff;
            }
            else if (beatAnimCheck % 2 == 0 && currentMissStage != HowMissed.MissedOn)
            {
                stepswitcherP.Play("OnbeatMiss", 0, 0);
                Jukebox.PlayOneShotGame("lockstep/wayOff");
                currentMissStage = HowMissed.MissedOn;
            }
        }

        public void ChangeBeatBackGroundColour(bool off)
        {
            if (off)
            {
                background.color = currentBGOffColor;
                offColorActive = true;
            }
            else
            {
                background.color = currentBGOnColor;
                offColorActive = false;
            }
        }

        public void SetbackgroundColours(Color onColor, Color offColor)
        {
            currentBGOnColor = onColor;
            currentBGOffColor = offColor;

            if (offColorActive)
            {
                background.color = currentBGOffColor;
            }
            else
            {
                background.color = currentBGOnColor;
            }
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}
