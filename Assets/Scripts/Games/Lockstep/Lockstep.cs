/* I do not know crap about Unity or C#
Almost none of this code is mine, but it's all fair game when the game you're stealing from
borrowed from other games */
//Don't worry Raffy everyone starts somewhere - Rasmus

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
            return new Minigame("lockstep", "Lockstep \n<color=#eb5454>[WIP]</color>", "f0338d", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; Lockstep.instance.Bop(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Should the stepswitchers bop?"),
                        new Param("toggle2", false, "Bop (Auto)", "Should the stepswitchers auto bop?"),
                    },
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
                new GameAction("set colours", "Set Colours")
                {
                    function = delegate {var e = eventCaller.currentEntity; Lockstep.instance.SetBackgroundColours(e["colorA"], e["colorB"], e["objColA"], e["objColB"], e["objColC"]); },
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Lockstep.defaultBGColorOn, "Background Onbeat", "Select the color that appears for the onbeat."),
                        new Param("colorB", Lockstep.defaultBGColorOff, "Background Offbeat", "Select the color that appears for the offbeat."),
                        new Param("objColA", Lockstep.stepperOut, "Stepper Outline", "Select the color used for the outline of the stepswitchers."),
                        new Param("objColB", Lockstep.stepperDark, "Stepper Dark", "Select the color that appears for the dark side of the stepwitchers."),
                        new Param("objColC", Lockstep.stepperLight, "Stepper Light", "Select the color that appears for the light side of the stepwitchers."),
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

        private static Color _stepperDark;
        public static Color stepperDark
        {
            get
            {
                ColorUtility.TryParseHtmlString("#737373", out _stepperDark);
                return _stepperDark;
            }
        }

        private static Color _stepperLight;
        public static Color stepperLight
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _stepperLight);
                return _stepperLight;
            }
        }

        private static Color _stepperOut;
        public static Color stepperOut
        {
            get
            {
                ColorUtility.TryParseHtmlString("#9A2760", out _stepperOut);
                return _stepperOut;
            }
        }

        public Color currentBGOnColor;
        public Color currentBGOffColor;

        [Header("Components")]
        [SerializeField] Animator stepswitcherPlayer;
        [SerializeField] Animator stepswitcherLeft;
        [SerializeField] Animator stepswitcherRight;

        // master stepper dictates what sprite the slave steppers use
        [SerializeField] Animator masterStepperAnim;
        [SerializeField] SpriteRenderer masterStepperSprite;

        // slave steppers copy the sprite of the master stepper
        [SerializeField] SpriteRenderer[] slaveSteppers;

        // rendertextures update when the slave steppers change sprites
        [SerializeField] CustomRenderTexture[] renderTextures;

        [SerializeField] SpriteRenderer background;
        [SerializeField] Material stepperMaterial;

        [Header("Properties")]
        static List<float> queuedInputs = new List<float>();
        Sprite masterSprite;
        HowMissed currentMissStage;
        bool lessSteppers = false;
        public enum HowMissed
        {
            NotMissed = 0,
            MissedOff = 1,
            MissedOn = 2
        }
        bool offColorActive;
        bool goBop;
        public GameEvent bop = new GameEvent();

        public static Lockstep instance;

        void Awake()
        {
            instance = this;
            currentBGOnColor = defaultBGColorOn;
            currentBGOffColor = defaultBGColorOff;
        }

        void Start() {
            stepperMaterial.SetColor("_ColorAlpha", stepperOut);
            stepperMaterial.SetColor("_ColorBravo", stepperDark);
            stepperMaterial.SetColor("_ColorDelta", stepperLight);

            masterSprite = masterStepperSprite.sprite;
            stepswitcherLeft.gameObject.SetActive(lessSteppers);
            stepswitcherRight.gameObject.SetActive(lessSteppers);
            masterStepperAnim.gameObject.SetActive(!lessSteppers);

            UpdateAndRenderSlaves();
        }

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
        }

        void UpdateAndRenderSlaves()
        {
            foreach (var stepper in slaveSteppers)
            {
                stepper.sprite = masterSprite;
            }
        }

        void PlayStepperAnim(string animName, bool player, float timescale = 1f, float startpos = 0f, int layer = -1)
        {
            if (player) stepswitcherPlayer.DoScaledAnimationAsync(animName, timescale, startpos, layer);
            if (lessSteppers)
            {
                stepswitcherLeft.DoScaledAnimationAsync(animName, timescale, startpos, layer);
                stepswitcherRight.DoScaledAnimationAsync(animName, timescale, startpos, layer);
            }
            else
            {
                masterStepperAnim.DoScaledAnimationAsync(animName, timescale, startpos, layer);
            }
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (goBop)
                    {
                        PlayStepperAnim("Bop", true, 0.5f);
                    }
                }
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
                    double beatAnimCheck = cond.songPositionInBeatsAsDouble - 0.25;
                    if (beatAnimCheck % 1.0 >= 0.5)
                    {
                        stepswitcherPlayer.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                    }
                    else
                    {
                        stepswitcherPlayer.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
                    }
                    Jukebox.PlayOneShotGame("lockstep/miss");
                    ScoreMiss();
                }
            }
            if (masterSprite != masterStepperSprite.sprite)
            {
                masterSprite = masterStepperSprite.sprite;
                UpdateAndRenderSlaves();
            }
        }

        public void Bop(float beat, float length, bool shouldBop, bool autoBop)
        {
            goBop = autoBop;
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            PlayStepperAnim("Bop", true, 0.5f);
                        })
                    });
                }
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
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    Lockstep.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, Lockstep.instance.Just, Lockstep.instance.Miss, Lockstep.instance.Nothing);
                    actions.Add(new BeatAction.Action(beat + i, delegate { Lockstep.instance.EvaluateMarch(); }));
                }
                BeatAction.New(instance.gameObject, actions);
            }
            else
            {
                for (int i = 0; i < length; i++)
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
                PlayStepperAnim("OffbeatMarch", false, 0.5f);
            }
            else
            {
                PlayStepperAnim("OnbeatMarch", false, 0.5f);
            }
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            currentMissStage = HowMissed.NotMissed;
            var cond = Conductor.instance;
            if (state >= 1f || state <= -1f)
            {
                double beatAnimCheck = cond.songPositionInBeatsAsDouble - 0.25;
                if (beatAnimCheck % 1.0 >= 0.5)
                {
                    Jukebox.PlayOneShotGame("lockstep/tink");
                    stepswitcherPlayer.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
                }
                else
                {
                    Jukebox.PlayOneShotGame("lockstep/tink");
                    stepswitcherPlayer.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
                }
                return;
            }
            Success(cond.songPositionInBeatsAsDouble);
        }

        public void Success(double beat)
        {
            double beatAnimCheck = beat - 0.25;
            if (beatAnimCheck % 1.0 >= 0.5)
            {
                Jukebox.PlayOneShotGame($"lockstep/marchOnbeat{UnityEngine.Random.Range(1, 3)}");
                stepswitcherPlayer.DoScaledAnimationAsync("OnbeatMarch", 0.5f);
            }
            else
            {
                Jukebox.PlayOneShotGame($"lockstep/marchOffbeat{UnityEngine.Random.Range(1, 3)}");
                stepswitcherPlayer.DoScaledAnimationAsync("OffbeatMarch", 0.5f);
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            var beatAnimCheck = Math.Round(caller.startBeat * 2);
            
            if (beatAnimCheck % 2 != 0 && currentMissStage != HowMissed.MissedOff)
            {
                stepswitcherPlayer.Play("OffbeatMiss", 0, 0);
                Jukebox.PlayOneShotGame("lockstep/wayOff");
                currentMissStage = HowMissed.MissedOff;
            }
            else if (beatAnimCheck % 2 == 0 && currentMissStage != HowMissed.MissedOn)
            {
                stepswitcherPlayer.Play("OnbeatMiss", 0, 0);
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

        public void SetBackgroundColours(Color onColor, Color offColor, Color outlineColor, Color darkColor, Color lightColor)
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

            stepperMaterial.SetColor("_ColorAlpha", outlineColor);
            stepperMaterial.SetColor("_ColorBravo", darkColor);
            stepperMaterial.SetColor("_ColorDelta", lightColor);
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}
