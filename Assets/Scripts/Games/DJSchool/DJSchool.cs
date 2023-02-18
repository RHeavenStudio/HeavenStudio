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
            return new Minigame("djSchool", "DJ School", "008c97", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { DJSchool.instance.Bop(eventCaller.currentEntity["toggle"]);  }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Whether both will bop to the beat or not")
                    }
                },
                new GameAction("and stop ooh", "And Stop!")
                {
                    function = delegate { var e = eventCaller.currentEntity; DJSchool.instance.AndStop(e.beat, e["toggle"]);  }, 
                    defaultLength = 2.5f, 
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DJSchool.WarnAndStop(e.beat, e["toggle"]);  },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Ooh", "Whether or not the \"ooh\" sound should be played")
                    }
                },
                new GameAction("break c'mon ooh", "Break, C'mon!")
                {
                    function = delegate { var e = eventCaller.currentEntity; DJSchool.instance.BreakCmon(e.beat, e["type"], e["toggle"]);  }, 
                    defaultLength = 3f,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DJSchool.WarnBreakCmon(e.beat, e["type"], e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "The voice line to play"),
                        new Param("toggle", true, "Ooh", "Whether or not the \"ooh\" sound should be played")
                    }
                },
                new GameAction("scratch-o hey", "Scratch-o")
                {
                    function = delegate { DJSchool.instance.ScratchoHey(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);  }, 
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "The voice line to play"),
                    }
                },
                new GameAction("scratch-o hey fast", "Scratch-o (Remix 4)")
                {
                    function = delegate { DJSchool.instance.ScratchoHey(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], true);  },
                    defaultLength = 2.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "The voice line to play"),
                    }
                },
                new GameAction("dj voice lines", "DJ Yellow Banter")
                {
                    function = delegate { DJSchool.instance.voiceLines(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);  }, 
                    defaultLength = 2f,
                    inactiveFunction = delegate { DJSchool.WarnDJVoiceLines(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"]);  },
                    parameters = new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoiceLines.CheckItOut, "Voice Lines", "The voice line to play"),
                    }
                },
                new GameAction("sound FX", "Scratchy Music")
                {
                    function = delegate { DJSchool.instance.soundFX(eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Radio FX", "Toggle on and off for Radio Effects")
                    }
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
        [SerializeField] private SpriteRenderer headSprite;
        [SerializeField] private Sprite[] headSprites;
        private float lastReportedBeat = 0f;

        [Header("Properties")]
        public GameEvent bop = new GameEvent();
        private bool djYellowHolding;
        public bool andStop;
        public bool goBop;
        public float beatOfInstance;

        public static DJSchool instance { get; private set; }

        private void Awake()
        {
            instance = this;
            djYellowAnim = djYellow.GetComponent<Animator>();
            student.Init();
            goBop = true;
        }

        //For inactive game purposes
        static float wantBreak = Single.MinValue;
        static float wantAndStop = Single.MinValue;
        static float wantDJVoiceLines = Single.MinValue;

        public override void OnGameSwitch(float beat)
        {
            if (wantBreak != Single.MinValue)
            {
                BreakCmon(wantBreak, 0, false, false);
                wantBreak = Single.MinValue;
            }
            else if(wantAndStop != Single.MinValue)
            {
                AndStop(wantAndStop, false, false);
                wantAndStop = Single.MinValue;
            }
            else if(wantDJVoiceLines != Single.MinValue)
            {
                voiceLines(wantDJVoiceLines, 0);
                wantDJVoiceLines = Single.MinValue;
            }
        }

        private void Update()
        {
            #region old script
            //var cond = Conductor.instance;

            //if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            //{
            //    if (cond.songPositionInBeats >= bop.startBeat && cond.songPositionInBeats < bop.startBeat + bop.length)
            //    {
            //        if (student.anim.IsAnimationNotPlaying())
            //        {
            //            if (student.isHolding)
            //            {
            //                student.anim.Play("HoldBop", 0, 0);
            //            }
            //            else
            //            {
            //                student.anim.Play("IdleBop", 0, 0);
            //            }
            //        }
            //        if (djYellowAnim.IsAnimationNotPlaying())
            //        {
            //            var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
            //            if (yellowState.IsName("Hey"))
            //            {
            //                PostScratchoFace();
            //            }

            //            if (djYellowHolding)
            //            {
            //                djYellowAnim.Play("HoldBop", 0, 0);
            //            }
            //            else
            //            {
            //                // todo: split between left and right bop based on beat
            //                djYellowAnim.Play("IdleBop", 0, 0);
            //            }
            //        }
            //    }
            //}
            #endregion

            if (Conductor.instance.ReportBeat(ref lastReportedBeat))
            {
                if (goBop)
                {
                    if (student.isHolding)
                    {
                        student.anim.Play("HoldBop", 0, 0);
                    }
                    else if (!student.swiping && student.anim.IsAnimationNotPlaying())
                    {
                        student.anim.Play("IdleBop", 0, 0);
                    }

                    var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
                    if (yellowState.IsName("Hey"))
                    {
                        PostScratchoFace();
                    }
                    if (!andStop && !djYellowHolding)
                    {
                        djYellowAnim.Play("IdleBop", 0, 0);

                    }
                    else if (djYellowHolding)
                    {
                        djYellowAnim.Play("HoldBop", 0, 0);
                    }
                }
                
            }
            else if (Conductor.instance.songPositionInBeats < lastReportedBeat)
            {
                lastReportedBeat = Mathf.Round(Conductor.instance.songPositionInBeats);
            }

            if(PlayerInput.Pressed() && !IsExpectingInputNow() && !student.isHolding) //Start hold miss
            {
                student.OnMissHoldForPlayerInput();
                student.isHolding = true;
                ScoreMiss();
            }
            else if(PlayerInput.PressedUp() && !IsExpectingInputNow() && student.isHolding) //Let go during hold
            {
                student.UnHold();
            }
            //else if(PlayerInput.PressedUp() && !IsExpectingInputNow() && !student.isHolding)
            //{
            //    student.OnMissSwipeForPlayerInput();
            //}
        }

        //public void Bop(float beat, float length)
        //{
        //    bop.startBeat = beat;
        //    bop.length = length;
        //}

        public void Bop(bool isBopping)
        {
            if (isBopping)
            {
                goBop = true;
            }
            else
            {
                goBop = false;
            }
        }

        public void BreakCmon(float beat, int type, bool ooh, bool doSound = true)
        {
            if (djYellowHolding) return;

            string[] sounds = new string[] { };

            switch (type)
            {
                case 0:
                    sounds = new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" };
                    break;
                case 1:
                    sounds = new string[] { "djSchool/breakCmonAlt1", "djSchool/breakCmonAlt2", "djSchool/oohAlt" };
                    break;
                case 2:
                    sounds = new string[] { "djSchool/breakCmonLoud1", "djSchool/breakCmonLoud2", "djSchool/oohLoud" };
                    break;
            }

            if (doSound)
            {
                var sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(sounds[0],   beat),
                    new MultiSound.Sound(sounds[1], beat + 1f, offset: 0.030f),
                    new MultiSound.Sound("", beat + 2f)
                };

                if (ooh)
                    sound[2] = new MultiSound.Sound(sounds[2], beat + 2f);

                MultiSound.Play(sound);
            }
            

            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); }),
                new BeatAction.Action(beat + 2f, delegate 
                { 
                    djYellow.GetComponent<Animator>().Play("Hold", 0, 0); 
                    djYellowHolding = true;
                }),
            });

            ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, student.OnHitHold, student.OnMissHold, student.OnEmpty);
        }

       
        public void AndStop(float beat, bool ooh, bool doSound = true)
        {
            if (djYellowHolding) return;

            if (doSound)
            {
                var sound = new MultiSound.Sound[]
                {
                    new MultiSound.Sound("djSchool/andStop1",   beat),
                    new MultiSound.Sound("djSchool/andStop2",   beat + .5f, offset: 0.1200f),
                    new MultiSound.Sound("", beat + 1.5f)
                };

                if (ooh)
                    sound[2] = new MultiSound.Sound("djSchool/oohAlt", beat + 1.5f);

                MultiSound.Play(sound);
            }
            

            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { djYellow.GetComponent<Animator>().Play("BreakCmon", 0, 0); }),
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    djYellow.GetComponent<Animator>().Play("Hold", 0, 0);
                    djYellowHolding = true;
                }),
            });
            andStop = true;

            ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, student.OnHitHold, student.OnMissHold, student.OnEmpty);
        }

        public void ScratchoHey(float beat, int type, bool remix4 = false)
        {
            string[] sounds = new string[] { };

            if (type == 0)
            {
                sounds = new string[] { "djSchool/scratchoHey1", "djSchool/scratchoHey2", "djSchool/scratchoHey3", "djSchool/scratchoHey4", "djSchool/hey" };
            }
            else if (type == 1)
            {
                sounds = new string[] { "djSchool/scratchoHeyAlt1", "djSchool/scratchoHeyAlt2", "djSchool/scratchoHeyAlt3", "djSchool/scratchoHeyAlt4", "djSchool/heyAlt" };
            }
            else if (type == 2)
            {
                sounds = new string[] { "djSchool/scratchoHeyLoud1", "djSchool/scratchoHeyLoud2", "djSchool/scratchoHeyLoud3", "djSchool/scratchoHeyLoud4", "djSchool/heyLoud" };
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


            BeatAction.New(djYellow, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { djYellow.GetComponent<Animator>().Play("Scratcho", 0, 0); }),
                new BeatAction.Action(beat + .5f, delegate { djYellow.GetComponent<Animator>().Play("Scratcho2", 0, 0); }),
                new BeatAction.Action(beat + 1f, delegate { djYellow.GetComponent<Animator>().Play("Scratcho", 0, 0); }),
                new BeatAction.Action(beat + beatOffset2, delegate
                {
                    djYellow.GetComponent<Animator>().Play("Hey", 0, 0);
                    djYellowHolding = false;
                }),
            });


            beatOfInstance = beat;

            ScheduleInput(beat, timing, InputType.STANDARD_UP, student.OnHitSwipe, student.OnMissSwipe, student.OnEmpty);
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

        public void SetDJYellowHead(int type, bool resetAfterBeats = false)
        {
            headSprite.sprite = headSprites[type];

            if (resetAfterBeats)
            {
                BeatAction.New(djYellow, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Mathf.Floor(Conductor.instance.songPositionInBeats) + 2f, delegate 
                    { 
                        var yellowState = djYellowAnim.GetCurrentAnimatorStateInfo(0);
                        if (yellowState.IsName("Idle")
                            || yellowState.IsName("IdleBop")
                            || yellowState.IsName("IdleBop2")
                            || yellowState.IsName("BreakCmon"))
                        {
                            SetDJYellowHead(0);
                        }
                    })
                });
            }
        }

        public void PostScratchoFace()
        {
            if (student.missed)
            {
                student.missed = false;
                SetDJYellowHead(3, true);
            }
            else
            {
                SetDJYellowHead(2, true);
            }
        }

        public void soundFX(bool toggle)
        {
            student.soundFX = toggle;
        }
        public void voiceLines(float beat, int type)
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

                    MultiSound.Play(sound);
                    break;

                case 1:
                    sounds = new string[] { "djSchool/letsGo1", "djSchool/letsGo2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    MultiSound.Play(sound);
                    break;

                case 2:
                    sounds = new string[] { "djSchool/ohYeah1", "djSchool/ohYeah2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    MultiSound.Play(sound);
                    break;

                case 3:
                    sounds = new string[] { "djSchool/ohYeahAlt1", "djSchool/ohYeahAlt2", "djSchool/ohYeahAlt3" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                        new MultiSound.Sound(sounds[2], beat + 1f),
                    };

                    MultiSound.Play(sound);
                    break;

                case 4:
                    Jukebox.PlayOneShotGame("djSchool/yay");
                    break;
            }
        }

        #region Inactive Game Commands
        public static void WarnBreakCmon(float beat, int type, bool ooh)
        {
            string[] sounds = new string[] { };
            switch (type)
            {
                case 0:
                    sounds = new string[] { "djSchool/breakCmon1", "djSchool/breakCmon2", "djSchool/ooh" };
                    break;
                case 1:
                    sounds = new string[] { "djSchool/breakCmonAlt1", "djSchool/breakCmonAlt2", "djSchool/oohAlt" };
                    break;
                case 2:
                    sounds = new string[] { "djSchool/breakCmonLoud1", "djSchool/breakCmonLoud2", "djSchool/oohLoud" };
                    break;
            }

            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound(sounds[0],   beat),
                new MultiSound.Sound(sounds[1], beat + 1f, offset: 0.030f),
                new MultiSound.Sound("", beat + 2f)
            };

            if (ooh)
                sound[2] = new MultiSound.Sound(sounds[2], beat + 2f);

            MultiSound.Play(sound, forcePlay: true);
            wantBreak = beat;
        }

        public static void WarnAndStop(float beat, bool ooh)
        {
            var sound = new MultiSound.Sound[]
            {
                new MultiSound.Sound("djSchool/andStop1", beat),
                new MultiSound.Sound("djSchool/andStop2", beat + .5f, offset: 0.1200f),
                new MultiSound.Sound("", beat + 1.5f)
            };

            if (ooh) 
                sound[2] = new MultiSound.Sound("djSchool/oohAlt", beat + 1.5f);


            MultiSound.Play(sound, forcePlay: true);
            wantAndStop = beat;
        }

        public static void WarnDJVoiceLines(float beat, int type)
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

                    
                    break;

                case 1:
                    sounds = new string[] { "djSchool/letsGo1", "djSchool/letsGo2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    
                    break;

                case 2:
                    sounds = new string[] { "djSchool/ohYeah1", "djSchool/ohYeah2" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                    };

                    
                    break;

                case 3:
                    sounds = new string[] { "djSchool/ohYeahAlt1", "djSchool/ohYeahAlt2", "djSchool/ohYeahAlt3" };
                    sound = new MultiSound.Sound[]
                    {
                        new MultiSound.Sound(sounds[0], beat),
                        new MultiSound.Sound(sounds[1], beat + .5f),
                        new MultiSound.Sound(sounds[2], beat + 1f),
                    };

                    
                    break;

                case 4:
                    Jukebox.PlayOneShotGame("djSchool/yay");
                    break;
            }
            MultiSound.Play(sound, forcePlay: true);
            wantDJVoiceLines = beat;
        }
        #endregion
    }
}