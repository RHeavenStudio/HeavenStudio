using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDjLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("djSchool", "DJ School", "3fd0ff", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; DJSchool.instance.Bop(e.beat, e.length, e["toggle2"], e["toggle"]);  }, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Toggle if the the DJs should bop for the duration of this event."),
                        new Param("toggle", false, "Bop (Auto)", "Toggle if the DJs should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("and stop ooh", "And Stop!")
                {
                    function = delegate { var e = eventCaller.currentEntity; DJSchool.instance.AndStop(e.beat, e["toggle"]);  }, 
                    defaultLength = 2.5f, 
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DJSchool.WarnAndStop(e.beat, e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Ooh", "Toggle if DJ Yellow should say \"Ooh!\"")
                    }
                },
                new GameAction("break c'mon ooh", "Break, C'mon!")
                {
                    function = delegate { var e = eventCaller.currentEntity; DJSchool.instance.BreakCmon(e.beat, e["type"], e["toggle"]);  }, 
                    defaultLength = 3f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DJSchool.WarnBreakCmon(e.beat, e["type"], e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "Choose the type of voice for DJ Yellow to use."),
                        new Param("toggle", true, "Ooh", "Toggle if DJ Yellow should say \"Ooh!\"")
                    }
                },
                new GameAction("scratch-o hey", "Scratch-o")
                {
                    function = delegate { DJSchool.instance.ScratchoHey(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle2"]);  }, 
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "Choose the type of voice for DJ Yellow to use."),
                        new Param("toggle2", true, "Cheering", "Toggle if cheering should play if you successfully hit this cue."),
                        new Param("toggle", false, "Fast Hey", "Toggle if this cue should use the faster timing from Remix 4 (DS).")
                    }
                },
                new GameAction("dj voice lines", "DJ Yellow Banter")
                {
                    function = delegate { DJSchool.VoiceLines(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);  }, 
                    defaultLength = 2f,
                    inactiveFunction = delegate { DJSchool.VoiceLines(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);  },
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoiceLines.CheckItOut, "Voice Line", "Choose what DJ Yellow should say."),
                    }
                },
                new GameAction("sound FX", "Set Radio FX")
                {
                    function = delegate { DJSchool.SoundFX(eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Radio FX", "Toggle if holding down the record will trigger scratchy radio effects like in the original game.")
                    }
                },
                new GameAction("forceHold", "Force Hold")
                {
                    function = delegate {DJSchool.instance.ForceHold(); },
                    defaultLength = 0.5f
                }
            },
            new List<string>() {"ntr", "normal"},
            "ntrdj", "en",
            new List<string>(){}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DJSchool;

    public class DJSchool : Minigame
    {
        public enum DJVoice
        {
            Standard,
            Cool,
            Hyped
        }

        public enum DJVoiceLines
        {
            CheckItOut,
            LetsGo,
            OhYeah,
            OhYeahAlt,
            Yay
        }   

        [Header("Components")]
        [SerializeField] private Student student;
        [SerializeField] private GameObject djYellow;
        private Animator djYellowAnim;
        public DJYellow djYellowScript;

        [Header("Properties")]
        public GameEvent bop = new GameEvent();
        public bool djYellowHolding;
        public bool andStop;
        public double beatOfInstance;
        private bool djYellowBopLeft;
        public bool shouldBeHolding = false;
        public double smileBeat = double.MinValue;

        public static DJSchool instance { get; private set; }

        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrDjTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        private void Awake()
        {
            instance = this;
            djYellowAnim = djYellow.GetComponent<Animator>();
            djYellowScript = djYellow.GetComponent<DJYellow>();
            student.Init();
            SetupBopRegion("djSchool", "bop", "toggle");
        }

        //For inactive game purposes
        static double wantBreak = double.MinValue;
        static double wantAndStop = double.MinValue;
        static double wantDJVoiceLines = double.MinValue;

        public override void OnGameSwitch(double beat)
        {
            if (wantBreak != double.MinValue)
            {
                BreakCmon(wantBreak, 0, false, false);
                wantBreak = double.MinValue;
            }
            else if(wantAndStop != double.MinValue)
            {
                AndStop(wantAndStop, false, false);
                wantAndStop = double.MinValue;
            }
            else if(wantDJVoiceLines != double.MinValue)
            {
                VoiceLines(wantDJVoiceLines, 0);
                wantDJVoiceLines = double.MinValue;
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (!BeatIsInBopRegion(beat)) return;
            if (student.isHolding)
            {
                student.anim.DoScaledAnimationAsync("HoldBop", 0.5f);
            }
            else if (!student.swiping && student.anim.IsAnimationNotPlaying())
            {
                student.anim.DoScaledAnimationAsync("IdleBop", 0.5f);
            }

            var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
            if (yellowState.IsName("Hey"))
            {
                //PostScratchoFace();
            }
            if (!andStop && !djYellowHolding)
            {
                float normalizedSmileBeat = Conductor.instance.GetPositionFromBeat(smileBeat, 3f);
                if (normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f) djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Happy);
                else if (!djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed)) djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.NeutralLeft);
                djYellowScript.Reverse(djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed));
                if (djYellowBopLeft)
                {
                    djYellowAnim.DoScaledAnimationAsync("IdleBop2", 0.5f);
                }
                else
                {
                    djYellowAnim.DoScaledAnimationAsync("IdleBop", 0.5f);
                }
                djYellowBopLeft = !djYellowBopLeft;

            }
            else if (djYellowHolding)
            {
                djYellowAnim.DoScaledAnimationAsync("HoldBop", 0.5f);
            }
        }

        private void Update()
        {
            if(PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !student.isHolding) //Start hold miss
            {
                student.OnMissHoldForPlayerInput();
                student.isHolding = true;
                ScoreMiss();
            }
            else if(((PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_FlickRelease))
                || PlayerInput.GetIsAction(InputAction_TouchRelease))
                && student.isHolding) //Let go during hold
            {
                student.UnHold();
                shouldBeHolding = false;
            }
            else if(PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease) && student.isHolding) //Flick during hold
            {
                student.OnFlickSwipe();
                shouldBeHolding = false;
            }
            else if (!GameManager.instance.autoplay && shouldBeHolding && !PlayerInput.GetIsAction(InputAction_BasicPressing) && !IsExpectingInputNow(InputAction_FlickRelease))
            {
                student.UnHold();
                shouldBeHolding = false;
            }
        }

        public void ForceHold()
        {
            student.ForceHold();
            djYellow.GetComponent<Animator>().Play("Hold", -1, 1);
            djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Focused);
            djYellowHolding = true;
            shouldBeHolding = true;
        }

        public void Bop(double beat, float length, bool isBopping, bool autoBop)
        {
            if (isBopping)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if (student.isHolding)
                            {
                                student.anim.DoScaledAnimationAsync("HoldBop", 0.5f);
                            }
                            else if (!student.swiping && student.anim.IsAnimationNotPlaying())
                            {
                                student.anim.DoScaledAnimationAsync("IdleBop", 0.5f);
                            }

                            var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
                            if (yellowState.IsName("Hey"))
                            {
                                //PostScratchoFace();
                            }
                            if (!andStop && !djYellowHolding)
                            {
                                double normalizedSmileBeat = Conductor.instance.GetPositionFromBeat(smileBeat, 3f);
                                if (normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f) djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Happy);
                                else if (!djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed)) djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.NeutralLeft);
                                djYellowScript.Reverse((normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f) || djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed));
                                if (djYellowBopLeft)
                                {
                                    djYellowAnim.DoScaledAnimationAsync("IdleBop2", 0.5f);
                                }
                                else
                                {
                                    djYellowAnim.DoScaledAnimationAsync("IdleBop", 0.5f);
                                }
                                djYellowBopLeft = !djYellowBopLeft;

                            }
                            else if (djYellowHolding)
                            {
                                djYellowAnim.DoScaledAnimationAsync("HoldBop", 0.5f);
                            }
                        })
                    });
                }
            }
        }

        public void BreakCmon(double beat, int type, bool ooh, bool doSound = true)
        {
            if (djYellowHolding) return;

            string[] sounds = type switch {
                0 => new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" },
                1 => new string[] { "djSchool/breakCmonAlt1", "djSchool/breakCmonAlt2", "djSchool/oohAlt" },
                2 => new string[] { "djSchool/breakCmonLoud1", "djSchool/breakCmonLoud2", "djSchool/oohLoud" },
            };

            if (doSound)
            {
                List<MultiSound.Sound> sound = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound(sounds[0], beat),
                new MultiSound.Sound(sounds[1], beat + 1f, offset: 0.030f),
            };

            if (ooh) sound.Add(new MultiSound.Sound(sounds[2], beat + 2f));

            MultiSound.Play(sound.ToArray());
            }
            

            BeatAction.New(djYellowScript, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate 
                { 
                    djYellow.GetComponent<Animator>().DoScaledAnimationAsync("BreakCmon", 0.5f);
                    float normalizedSmileBeat = Conductor.instance.GetPositionFromBeat(smileBeat, 3f);
                    if (normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f)
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Happy);
                    }
                    else if (!djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed))
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.NeutralRight);
                    }
                    djYellowScript.Reverse();
                }),
                new BeatAction.Action(beat + 1f, delegate 
                { 
                    djYellow.GetComponent<Animator>().DoScaledAnimationAsync("BreakCmon", 0.5f);
                    float normalizedSmileBeat = Conductor.instance.GetPositionFromBeat(smileBeat, 3f);
                    if (normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f)
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Happy);
                    }
                    else if (!djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed))
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.NeutralRight);
                    }
                    djYellowScript.Reverse();
                }),
                new BeatAction.Action(beat + 2f, delegate 
                { 
                    djYellow.GetComponent<Animator>().DoScaledAnimationAsync("Hold", 0.5f); 
                    djYellowHolding = true;
                    djYellowScript.Reverse();
                }),
            });
            andStop = true;
            ScheduleInput(beat, 2f, InputAction_BasicPress, student.OnHitHold, student.OnMissHold, student.OnEmpty);
        }

        public void AndStop(double beat, bool ooh, bool doSound = true)
        {
            if (djYellowHolding) return;

            if (doSound)
            {
                List<MultiSound.Sound> sound = new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("djSchool/andStop1", beat),
                    new MultiSound.Sound("djSchool/andStop2", beat + .5f, offset: 0.1200f),
                };

                if (ooh) sound.Add(new MultiSound.Sound("djSchool/oohAlt", beat + 1.5f));

                MultiSound.Play(sound.ToArray());
            }
            

            BeatAction.New(djYellowScript, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate 
                {
                    djYellow.GetComponent<Animator>().DoScaledAnimationAsync("BreakCmon", 0.5f);
                    double normalizedSmileBeat = Conductor.instance.GetPositionFromBeat(smileBeat, 3f);
                    if (normalizedSmileBeat >= 0 && normalizedSmileBeat <= 1f)
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.Happy);
                    }
                    else if (!djYellowScript.HeadSpriteCheck(DJYellow.DJExpression.CrossEyed))
                    {
                        djYellowScript.ChangeHeadSprite(DJYellow.DJExpression.NeutralRight);
                    }
                    djYellowScript.Reverse();
                }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    djYellow.GetComponent<Animator>().Play("Hold", 0, 0);
                    djYellowHolding = true;
                    djYellowScript.Reverse();
                }),
            });
            andStop = true;

            ScheduleInput(beat, 1.5f, InputAction_BasicPress, student.OnHitHold, student.OnMissHold, student.OnEmpty);
        }

        public void ScratchoHey(double beat, int type, bool remix4, bool cheer)
        {
            string[] sounds = new string[] { };

            switch (type)
            {
                case 0:
                sounds = new string[] { "djSchool/scratchoHey1", "djSchool/scratchoHey2", "djSchool/scratchoHey3", "djSchool/scratchoHey4", "djSchool/hey" };
                break;
                case 1:
                sounds = new string[] { "djSchool/scratchoHeyAlt1", "djSchool/scratchoHeyAlt2", "djSchool/scratchoHeyAlt3", "djSchool/scratchoHeyAlt4", "djSchool/heyAlt" };
                break;
                default:
                sounds = new string[] { "djSchool/scratchoHeyLoud1", "djSchool/scratchoHeyLoud2", "djSchool/scratchoHeyLoud3", "djSchool/scratchoHeyLoud4", "djSchool/heyLoud" };
                break;
            }

            float timing = 0f;
            float beatOffset = 0f;
            float beatOffset2 = 0f;

            if (!remix4)
            {
                timing = 2f;
                beatOffset = 2f;
                beatOffset2 = 2.05f;
            }
            else
            {
                timing = 1.5f;
                beatOffset = 1.5f;
                beatOffset2 = 1.55f;
            }

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(sounds[0],   beat),
                new MultiSound.Sound(sounds[1], beat + .25f),
                new MultiSound.Sound(sounds[2], beat + .5f),
                new MultiSound.Sound(sounds[3], beat + 1f, offset: 0.0500f),
                new MultiSound.Sound(sounds[4], beat + beatOffset, offset: 0.070f),
            });


            BeatAction.New(djYellowScript, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { djYellow.GetComponent<Animator>().DoScaledAnimationAsync("Scratcho", 0.5f); }),
                new BeatAction.Action(beat + .5f, delegate { djYellow.GetComponent<Animator>().DoScaledAnimationAsync("Scratcho2", 0.5f); }),
                new BeatAction.Action(beat + 1f, delegate { djYellow.GetComponent<Animator>().DoScaledAnimationAsync("Scratcho", 0.5f); }),
                new BeatAction.Action(beat + beatOffset2, delegate
                {
                    djYellow.GetComponent<Animator>().DoScaledAnimationAsync("Hey", 0.5f);
                    djYellowHolding = false;
                }),
            });


            beatOfInstance = beat;

            if (cheer)
            {
                ScheduleInput(beat, timing, InputAction_FlickRelease, student.OnHitSwipeCheer, student.OnMissSwipe, student.OnEmpty);
            }
            else
            {
                ScheduleInput(beat, timing, InputAction_FlickRelease, student.OnHitSwipe, student.OnMissSwipe, student.OnEmpty);
            }
            andStop = false;




        }

        //void SetupCue(float beat, bool swipe)
        //{
        //    if (swipe)
        //        student.swipeBeat = beat;
        //    else
        //        student.holdBeat = beat;
            
        //    student.eligible = true;
        //    student.ResetState();
        //}

        public static void SoundFX(bool toggle)
        {
            Student.soundFX = toggle;
        }
        public static void VoiceLines(double beat, int type)
        {
            string[] sounds;
            var sound = new MultiSound.Sound[] { };
            switch (type)
            {
                case 0:
                    sounds = new string[] { "djSchool/checkItOut1", "djSchool/checkItOut2", "djSchool/checkItOut3" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .25f),
                        new MultiSound.Sound(sounds[2], beat + .5f),
                    };

                    MultiSound.Play(sound, forcePlay: true);
                    break;

                case 1:
                    sounds = new string[] { "djSchool/letsGo1", "djSchool/letsGo2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    MultiSound.Play(sound, forcePlay: true);
                    break;

                case 2:
                    sounds = new string[] { "djSchool/ohYeah1", "djSchool/ohYeah2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    MultiSound.Play(sound, forcePlay: true);
                    break;

                case 3:
                    sounds = new string[] { "djSchool/ohYeahAlt1", "djSchool/ohYeahAlt2", "djSchool/ohYeahAlt3" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                        new MultiSound.Sound(sounds[2], beat + 1f),
                    };

                    MultiSound.Play(sound, forcePlay: true);
                    break;

                case 4:
                    SoundByte.PlayOneShotGame("djSchool/yay", forcePlay: true);
                    break;
            }
        }

        #region Inactive Game Commands
        public static void WarnBreakCmon(double beat, int type, bool ooh)
        {
            string[] sounds = type switch {
                0 => new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" },
                1 => new string[] { "djSchool/breakCmonAlt1", "djSchool/breakCmonAlt2", "djSchool/oohAlt" },
                2 => new string[] { "djSchool/breakCmonLoud1", "djSchool/breakCmonLoud2", "djSchool/oohLoud" },
                _ => new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" },
            };

            List<MultiSound.Sound> sound = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound(sounds[0], beat),
                new MultiSound.Sound(sounds[1], beat + 1f, offset: 0.030f),
            };

            if (ooh) sound.Add(new MultiSound.Sound(sounds[2], beat + 2f));

            MultiSound.Play(sound.ToArray(), forcePlay: true);
            wantBreak = beat;
        }

        public static void WarnAndStop(double beat, bool ooh)
        {
            List<MultiSound.Sound> sound = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("djSchool/andStop1", beat),
                new MultiSound.Sound("djSchool/andStop2", beat + .5f, offset: 0.1200f),
            };

            if (ooh) sound.Add(new MultiSound.Sound("djSchool/oohAlt", beat + 1.5f));

            MultiSound.Play(sound.ToArray(), forcePlay: true);
            wantAndStop = beat;
        }
        #endregion
    }
}