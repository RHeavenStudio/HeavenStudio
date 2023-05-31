using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static HeavenStudio.Games.SpaceDance;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbSpaceDanceLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceDance", "Space Dance", "0014d6", false, false, new List<GameAction>()
            {
                new GameAction("turn right", "Turn Right")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceDance.instance.DoTurnRight(e.beat, e["whoSpeaks"], e["gramps"]); },
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("whoSpeaks", SpaceDance.WhoSpeaks.Dancers, "Who Speaks?", "Who will say the voice line for the cue?"),
                        new Param("gramps", false, "Space Gramps Animations", "Will Space Gramps turn right?")
                    }
                },
                new GameAction("sit down", "Sit Down")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceDance.instance.DoSitDown(e.beat, e["whoSpeaks"], e["gramps"]); },
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("whoSpeaks", SpaceDance.WhoSpeaks.Dancers, "Who Speaks?", "Who will say the voice line for the cue?"),
                        new Param("gramps", false, "Space Gramps Animations", "Will Space Gramps turn right?")
                    }
                },
                new GameAction("punch", "Punch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceDance.instance.DoPunch(e.beat, e["whoSpeaks"], e["gramps"]); },
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("whoSpeaks", SpaceDance.WhoSpeaks.Dancers, "Who Speaks?", "Who will say the voice line for the cue?"),
                        new Param("gramps", false, "Space Gramps Animations", "Will Space Gramps turn right?")
                    }
                },
                new GameAction("shootingStar", "Shooting Star")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceDance.instance.UpdateShootingStar(e.beat, e.length, e["ease"]); },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the shooting of the stars use?")
                    }
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceDance.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["toggle"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", SpaceDance.defaultBGColor, "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", SpaceDance.defaultBGColor, "End Color", "The end color for the fade."),
                        new Param("toggle", false, "Instant", "Should the background instantly change color?")
                    }
                },
                new GameAction("bop", "Single Bop")
                {
                    function = delegate { SpaceDance.instance.Bop(); SpaceDance.instance.GrampsBop(eventCaller.currentEntity["gramps"]); },
                    parameters = new List<Param>()
                    {
                        new Param("gramps", false, "Gramps Bop", "Should Space Gramps bop with the dancers?")
                    }
                },
                new GameAction("bopToggle", "Bop Toggle")
                {
                    function = delegate { SpaceDance.instance.shouldBop = eventCaller.currentEntity["toggle"]; SpaceDance.instance.spaceGrampsShouldBop = eventCaller.currentEntity["gramps"]; },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Should bop?", "Should the dancers bop?"),
                        new Param("gramps", false, "Gramps Bop", "Should Space Gramps bop with the dancers?")
                    }
                },
                new GameAction("grampsAnims", "Space Gramps Animations")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceDance.instance.GrampsAnimations(e.beat, e["type"], e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Looping", "Should the animation loop?"),
                        new Param("type", SpaceDance.GrampsAnimationType.Talk, "Which animation?", "Which animation should space gramps do?")
                    }
                }
            },
            new List<string>() {"agb", "normal"},
            "agbspacedance", "jp",
            new List<string>() {"jp"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_SpaceDance;
    public class SpaceDance : Minigame
    {
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#0014D6", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        public enum WhoSpeaks
        {
            Dancers = 0,
            Gramps = 1,
            Both = 2
        }
        public enum GrampsAnimationType
        {
            Stand = 0,
            Talk = 1,
            Sniff = 2
        }
        Tween bgColorTween;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] Animator shootingStarAnim;
        public Animator DancerP;
        public Animator Dancer1;
        public Animator Dancer2;
        public Animator Dancer3;
        public Animator Gramps;
        public Animator Hit;
        public GameObject Player;
        public bool shouldBop = false;
        bool canBop = true;
        bool grampsCanBop = true;
        public bool spaceGrampsShouldBop = false;
        float shootingStarLength;
        float shootingStarStartBeat;
        EasingFunction.Ease lastEase;
        bool isShootingStar;
        bool grampsLoopingAnim;
        bool grampsSniffing;

        public GameEvent bop = new GameEvent();

        public static SpaceDance instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (shouldBop && canBop)
                    {
                        Bop();
                    }
                    if (spaceGrampsShouldBop && grampsCanBop)
                    {
                        GrampsBop();
                    }
                }
                if (isShootingStar)
                {
                    float normalizedBeat = cond.GetPositionFromBeat(shootingStarStartBeat, shootingStarLength);
                    if (normalizedBeat >= 0)
                    {
                        if (normalizedBeat > 1)
                        {
                            isShootingStar = false;
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                            float newAnimPos = func(0f, 1f, normalizedBeat);
                            shootingStarAnim.DoNormalizedAnimation("ShootingStar", newAnimPos);
                        }
                    }
                }
                if (!DancerP.IsPlayingAnimationName("PunchDo") && !DancerP.IsPlayingAnimationName("TurnRightDo") && !DancerP.IsPlayingAnimationName("SitDownDo"))
                {
                    if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                    {
                        Jukebox.PlayOneShotGame("spaceDance/inputBad");
                        DancerP.DoScaledAnimationAsync("PunchDo", 0.5f);
                        // Look at this later, sound effect has some weird clipping on it sometimes?? popping. like. fucking popopop idk why its doing that its fine theres no sample weirdness ughh
                    }
                    if (PlayerInput.GetSpecificDirectionDown(1) && !IsExpectingInputNow(InputType.DIRECTION_RIGHT_DOWN))
                    {
                        DancerP.DoScaledAnimationAsync("TurnRightDo", 0.5f);
                        Jukebox.PlayOneShotGame("spaceDance/inputBad");
                    }
                    if (PlayerInput.GetSpecificDirectionDown(2) && !IsExpectingInputNow(InputType.DIRECTION_DOWN_DOWN))
                    {
                        DancerP.DoScaledAnimationAsync("SitDownDo", 0.5f);
                        Jukebox.PlayOneShotGame("spaceDance/inputBad");
                    }
                }
            }
        }

        public void GrampsAnimations(float beat, int type, bool looping)
        {
            switch (type)
            {
                case (int)GrampsAnimationType.Stand:
                    Gramps.Play("GrampsStand", 0, 0);
                    grampsLoopingAnim = false;
                    grampsSniffing = false;
                    break;
                case (int)GrampsAnimationType.Talk:
                    if (looping)
                    {
                        grampsLoopingAnim = true;
                        grampsSniffing = false;
                        GrampsTalkLoop(beat);
                    }
                    else
                    {
                        grampsLoopingAnim = false;
                        grampsSniffing = false;
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                    break;
                case (int)GrampsAnimationType.Sniff:
                    if (looping)
                    {
                        grampsLoopingAnim = true;
                        grampsSniffing = true;
                        GrampsSniffLoop(beat);
                    }
                    else
                    {
                        grampsLoopingAnim = false;
                        grampsSniffing = false;
                        Gramps.DoScaledAnimationAsync("GrampsSniff", 0.5f);
                    }
                    break;
            }
        }

        void GrampsSniffLoop(float beat)
        {
            if (!grampsLoopingAnim || !grampsSniffing) return;
            spaceGrampsShouldBop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsSniff", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 3, delegate
                {
                    if (grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsSniff", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    if (grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsSniff", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 5.5f, delegate
                {
                    GrampsSniffLoop(beat + 5.5f);
                }),
            });
        }

        void GrampsTalkLoop(float beat)
        {
            if (!grampsLoopingAnim || grampsSniffing) return;
            spaceGrampsShouldBop = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.66666f , delegate
                {
                    if (!grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 1.33333f, delegate
                {
                    if (!grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 2f, delegate
                {
                    if (!grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 3f, delegate
                {
                    if (!grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    if (!grampsSniffing && grampsLoopingAnim)
                    {
                        Gramps.DoScaledAnimationAsync("GrampsTalk", 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 4f, delegate
                {
                    GrampsTalkLoop(beat + 4f);
                }),
            });
        }

        public void UpdateShootingStar(float beat, float length, int ease)
        {
            lastEase = (EasingFunction.Ease)ease;
            shootingStarLength = length;
            shootingStarStartBeat = beat;
            isShootingStar = true;
        }

        public void DoTurnRight(float beat, int whoSpeaks, bool grampsTurns)
        {
            canBop = false;
            if (grampsTurns) grampsCanBop = false;
            ScheduleInput(beat, 1f, InputType.DIRECTION_RIGHT_DOWN, JustRight, RightMiss, Empty);

            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("spaceDance/voicelessTurn", beat),
            };

            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Dancers:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerTurn", beat),
                        new MultiSound.Sound("spaceDance/dancerRight", beat + 1.0f, 1, 1, false, 0.007f),
                    });
                    break;
                case (int)WhoSpeaks.Gramps:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/otherTurn", beat),
                        new MultiSound.Sound("spaceDance/otherRight", beat + 1.0f, 1, 1, false, 0.007f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerTurn", beat),
                        new MultiSound.Sound("spaceDance/dancerRight", beat + 1.0f, 1, 1, false, 0.007f),
                        new MultiSound.Sound("spaceDance/otherTurn", beat),
                        new MultiSound.Sound("spaceDance/otherRight", beat + 1.0f, 1, 1, false, 0.007f),
                    });
                    break;
            }

            MultiSound.Play(soundsToPlay.ToArray());

            BeatAction.New(Player, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(beat,     delegate { DancerP.DoScaledAnimationAsync("TurnRightStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer1.DoScaledAnimationAsync("TurnRightStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer2.DoScaledAnimationAsync("TurnRightStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("TurnRightStart", 0.5f);
                    if (grampsTurns) Gramps.DoScaledAnimationAsync("GrampsTurnRightStart", 0.5f);
                }),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.DoScaledAnimationAsync("TurnRightDo", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.DoScaledAnimationAsync("TurnRightDo", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate 
                {
                    Dancer3.DoScaledAnimationAsync("TurnRightDo", 0.5f);
                    if (grampsTurns) Gramps.DoScaledAnimationAsync("GrampsTurnRightDo", 0.5f);
                }),
                new BeatAction.Action(beat + 1.99f,     delegate { canBop = true; grampsCanBop = true; }),
            });

        }

        public void DoSitDown(float beat, int whoSpeaks, bool grampsSits)
        {
            canBop = false;
            if (grampsSits) grampsCanBop = false;
            ScheduleInput(beat, 1f, InputType.DIRECTION_DOWN_DOWN, JustSit, SitMiss, Empty);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("spaceDance/voicelessSit", beat),
            };

            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Dancers:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerLets", beat, 1, 1, false, 0.07f),
                        new MultiSound.Sound("spaceDance/dancerSit", beat + 0.5f, 1, 1, false, 0.02f),
                        new MultiSound.Sound("spaceDance/dancerDown", beat + 1f, 1, 1, false, 0.006f),
                    });
                    break;
                case (int)WhoSpeaks.Gramps:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/otherLets", beat, 1, 1, false, 0.024f),
                        new MultiSound.Sound("spaceDance/otherSit", beat + 0.5f, 1, 1, false, 0.04f),
                        new MultiSound.Sound("spaceDance/otherDown", beat + 1f, 1, 1, false, 0.01f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerLets", beat, 1, 1, false, 0.07f),
                        new MultiSound.Sound("spaceDance/dancerSit", beat + 0.5f, 1, 1, false, 0.02f),
                        new MultiSound.Sound("spaceDance/dancerDown", beat + 1f, 1, 1, false, 0.006f),
                        new MultiSound.Sound("spaceDance/otherLets", beat, 1, 1, false, 0.024f),
                        new MultiSound.Sound("spaceDance/otherSit", beat + 0.5f, 1, 1, false, 0.04f),
                        new MultiSound.Sound("spaceDance/otherDown", beat + 1f, 1, 1, false, 0.01f),
                    });
                    break;
            }

            MultiSound.Play(soundsToPlay.ToArray());

            BeatAction.New(Player, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(beat,     delegate { DancerP.DoScaledAnimationAsync("SitDownStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer1.DoScaledAnimationAsync("SitDownStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer2.DoScaledAnimationAsync("SitDownStart", 0.5f);}),
                new BeatAction.Action(beat,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("SitDownStart", 0.5f);
                    if (grampsSits) Gramps.DoScaledAnimationAsync("GrampsSitDownStart", 0.5f);
                }),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.DoScaledAnimationAsync("SitDownDo", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.DoScaledAnimationAsync("SitDownDo", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("SitDownDo", 0.5f);
                    if (grampsSits) Gramps.DoScaledAnimationAsync("GrampsSitDownDo", 0.5f);
                }),
                new BeatAction.Action(beat + 1.99f,     delegate { canBop = true; grampsCanBop = true; }),
            });

        }

        public void DoPunch(float beat, int whoSpeaks, bool grampsPunches)
        {
            canBop = false;
            if (grampsPunches) grampsCanBop = false;
            ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, JustPunch, PunchMiss, Empty);
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("spaceDance/voicelessPunch", beat),
                new MultiSound.Sound("spaceDance/voicelessPunch", beat + 0.5f),
                new MultiSound.Sound("spaceDance/voicelessPunch", beat + 1f),
            };

            switch (whoSpeaks)
            {
                case (int)WhoSpeaks.Dancers:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerPa", beat),
                        new MultiSound.Sound("spaceDance/dancerPa", beat + 0.5f),
                        new MultiSound.Sound("spaceDance/dancerPa", beat + 1f),
                        new MultiSound.Sound("spaceDance/dancerPunch", beat + 1.5f),
                    });
                    break;
                case (int)WhoSpeaks.Gramps:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/otherPa", beat),
                        new MultiSound.Sound("spaceDance/otherPa", beat + 0.5f),
                        new MultiSound.Sound("spaceDance/otherPa", beat + 1f),
                        new MultiSound.Sound("spaceDance/otherPunch", beat + 1.5f),
                    });
                    break;
                case (int)WhoSpeaks.Both:
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("spaceDance/dancerPa", beat),
                        new MultiSound.Sound("spaceDance/dancerPa", beat + 0.5f),
                        new MultiSound.Sound("spaceDance/dancerPa", beat + 1f),
                        new MultiSound.Sound("spaceDance/dancerPunch", beat + 1.5f),
                        new MultiSound.Sound("spaceDance/otherPa", beat),
                        new MultiSound.Sound("spaceDance/otherPa", beat + 0.5f),
                        new MultiSound.Sound("spaceDance/otherPa", beat + 1f),
                        new MultiSound.Sound("spaceDance/otherPunch", beat + 1.5f),
                    });
                    break;
            }

            MultiSound.Play(soundsToPlay.ToArray());

            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { DancerP.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer1.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat,     delegate { Dancer2.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("PunchStartInner", 0.5f);
                    if (grampsPunches) Gramps.DoScaledAnimationAsync("GrampsPunchStartOdd", 0.5f);
                }),
                new BeatAction.Action(beat + 0.5f,     delegate { DancerP.DoScaledAnimationAsync("PunchStartOuter", 0.5f);}),
                new BeatAction.Action(beat + 0.5f,     delegate { Dancer1.DoScaledAnimationAsync("PunchStartOuter", 0.5f);}),
                new BeatAction.Action(beat + 0.5f,     delegate { Dancer2.DoScaledAnimationAsync("PunchStartOuter", 0.5f);}),
                new BeatAction.Action(beat + 0.5f,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("PunchStartOuter", 0.5f);
                    if (grampsPunches) Gramps.DoScaledAnimationAsync("GrampsPunchStartEven", 0.5f);
                }),
                new BeatAction.Action(beat + 1f,     delegate { DancerP.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer1.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Dancer2.DoScaledAnimationAsync("PunchStartInner", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("PunchStartInner", 0.5f);
                    if (grampsPunches) Gramps.DoScaledAnimationAsync("GrampsPunchStartOdd", 0.5f);
                }),
                new BeatAction.Action(beat + 1.5f,     delegate { Dancer1.DoScaledAnimationAsync("PunchDo", 0.5f);}),
                new BeatAction.Action(beat + 1.5f,     delegate { Dancer2.DoScaledAnimationAsync("PunchDo", 0.5f);}),
                new BeatAction.Action(beat + 1.5f,     delegate 
                { 
                    Dancer3.DoScaledAnimationAsync("PunchDo", 0.5f);
                    if (grampsPunches) Gramps.DoScaledAnimationAsync("GrampsPunchDo", 0.5f);
                }),
                });

        }

        public void Bop()
        {
            canBop = true;
            DancerP.DoScaledAnimationAsync("Bop", 0.5f);
            Dancer1.DoScaledAnimationAsync("Bop", 0.5f);
            Dancer2.DoScaledAnimationAsync("Bop", 0.5f);
            Dancer3.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void GrampsBop(bool forceBop = false)
        {
            if (spaceGrampsShouldBop || forceBop) 
            {
                grampsCanBop = true;
                Gramps.DoScaledAnimationAsync("GrampsBop", 0.5f);
            } 
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        public void JustRight(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("spaceDance/inputBad");
                DancerP.DoScaledAnimationAsync("TurnRightDo", 0.5f);
                Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
                return;
            }
            RightSuccess();
        }

        public void RightSuccess()
            {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.DoScaledAnimationAsync("TurnRightDo", 0.5f);
             }

        public void RightMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.DoScaledAnimationAsync("Ouch", 0.5f);
            Hit.Play("HitTurn", -1, 0);
            Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
        }

        public void JustSit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("spaceDance/inputBad");
                DancerP.DoScaledAnimationAsync("SitDownDo", 0.5f);
                Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
                return;
            }
            SitSuccess();
        }

        public void SitSuccess()
        {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.DoScaledAnimationAsync("SitDownDo", 0.5f);
        }

        public void SitMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.DoScaledAnimationAsync("Ouch", 0.5f);
            Hit.Play("HitSit", -1, 0);
            Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
        }

        public void JustPunch(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                Jukebox.PlayOneShotGame("spaceDance/inputBad");
                DancerP.DoScaledAnimationAsync("PunchDo", 0.5f);
                Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
                return;
            }
            PunchSuccess();
        }

        public void PunchSuccess()
            {
            Jukebox.PlayOneShotGame("spaceDance/inputGood");
            DancerP.DoScaledAnimationAsync("PunchDo", 0.5f);
             }

        public void PunchMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("spaceDance/inputBad2");
            DancerP.DoScaledAnimationAsync("Ouch", 0.5f);
            Hit.Play("HitPunch", -1, 0);
            Gramps.DoScaledAnimationAsync("GrampsOhFuck", 0.5f);
        }

        public void Empty(PlayerActionEvent caller) { }


    }
}