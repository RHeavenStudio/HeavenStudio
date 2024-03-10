using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_PowerCalligraphy
{
    public class Writing : MonoBehaviour
    {
        // Declaring the same enum in another class is not beautiful.
        public enum CharacterType
        {
            re,
            comma,
            chikara,
            onore,
            sun,
            kokoro,
            face,
            face_kr,
            NONE,
        }

        public double targetBeat;
        public CharacterType characterType;
        public Animator paperAnim;
        public Animator fudePosAnim;
        public Animator fudeAnim;

        public Vector3 scrollSpeed;
        Vector3 scrollRate => scrollSpeed / (Conductor.instance.pitchedSecPerBeat * 2f);
        
        public bool onGoing = false;
        bool isFinish = false;
        int num;
        enum StrokeType {
            TOME = 0,
            HANE,
            HARAI,
        }
        StrokeType stroke;
        public int Stroke { get { return (int)stroke; }}

        Sound releaseSound = null;

        private PowerCalligraphy game;

        public void Init()
        {
            game = PowerCalligraphy.instance;
            Anim(0);
        }

        public void Play()
        {
            switch(characterType)
            {
                case CharacterType.re:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/reShout", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+2f, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(1);
                        }),
                        new BeatAction.Action(targetBeat+3f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+4f, delegate { Sweep(); stroke = StrokeType.HANE;}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(4, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;
                
                case CharacterType.comma:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/comma1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/comma2", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/comma2", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/comma3", targetBeat+4f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+2f, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+3f, delegate { fudeAnim.DoScaledAnimationAsync("fude-prepare", 0.5f);}),
                        new BeatAction.Action(targetBeat+4f, delegate { Anim(1);}),
                        new BeatAction.Action(targetBeat+5f, delegate { Halt(); stroke = StrokeType.TOME;}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(3, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.chikara:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+0.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+3f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(1);
                        }),
                        new BeatAction.Action(targetBeat+0.5f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+1f, delegate { Anim(3);}),
                        new BeatAction.Action(targetBeat+2f, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            Anim(4);
                        }),
                        new BeatAction.Action(targetBeat+2.5f, delegate { Anim(5);}),
                        new BeatAction.Action(targetBeat+3f, delegate { 
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(6);
                        }),
                        new BeatAction.Action(targetBeat+4f, delegate { Sweep(); stroke = StrokeType.HARAI;}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(8, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.onore:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1.5f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+4f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(1);
                        }),
                        new BeatAction.Action(targetBeat+1f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+1.5f, delegate { Anim(3);}),
                        new BeatAction.Action(targetBeat+2f, delegate { Anim(4);}),
                        new BeatAction.Action(targetBeat+3f, delegate { Anim(5);}),
                        new BeatAction.Action(targetBeat+4f, delegate
                        {
                            Anim(6);
                            Sweep(); stroke = StrokeType.HANE;
                        }),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(8, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+4f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.sun:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+0.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+1.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(1);
                        }),
                        new BeatAction.Action(targetBeat+0.5f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+1f, delegate { Anim(3);}),
                        new BeatAction.Action(targetBeat+1.5f, delegate { Anim(4);}),
                        new BeatAction.Action(targetBeat+2f, delegate { Sweep(); stroke = StrokeType.HANE; num = 1;}),
                        new BeatAction.Action(targetBeat+5f, delegate
                        {
                            Anim(6);
                            Halt(); stroke = StrokeType.TOME; num = 2;
                        }),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(8, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+2f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.kokoro:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+4.5f, volume:0.3f), // +Agb
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(1);
                        }),
                        new BeatAction.Action(targetBeat+0.5f, delegate 
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(2);
                        }),
                        new BeatAction.Action(targetBeat+1f, delegate
                        {
                            Anim(3);
                            Sweep(); stroke = StrokeType.HANE; num = 1;
                        }),
                        new BeatAction.Action(targetBeat+3.5f, delegate
                        { 
                            fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            Anim(5);
                        }),
                        new BeatAction.Action(targetBeat+4f, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            Anim(6);
                        }),
                        new BeatAction.Action(targetBeat+4.5f, delegate
                        {
                            fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            Anim(7);    
                        }),
                        new BeatAction.Action(targetBeat+5f, delegate { Halt(); stroke = StrokeType.TOME; num = 2;}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(9, "end");}),
                        new BeatAction.Action(targetBeat+7f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+1f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    game.ScheduleInput(targetBeat+5f, 1f, PowerCalligraphy.InputAction_BasicPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.face:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+3f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6.5f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7.25f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+7.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { Anim(1);}),
                        new BeatAction.Action(targetBeat+1f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+1.5f, delegate { Anim(3);}),
                        new BeatAction.Action(targetBeat+2f, delegate { Anim(4);}),
                        new BeatAction.Action(targetBeat+2.5f, delegate { Anim(5);}),
                        new BeatAction.Action(targetBeat+3f, delegate { Anim(6);}),
                        new BeatAction.Action(targetBeat+3.5f, delegate { Anim(7);}),
                        new BeatAction.Action(targetBeat+4f, delegate { Anim(8);}),
                        new BeatAction.Action(targetBeat+4.5f, delegate { Anim(9);}),
                        new BeatAction.Action(targetBeat+4.75f, delegate { Anim(10);}),
                        new BeatAction.Action(targetBeat+5f, delegate { Anim(11);}),
                        new BeatAction.Action(targetBeat+5.25f, delegate { Anim(12);}),
                        new BeatAction.Action(targetBeat+5.5f, delegate { Anim(13);}),
                        new BeatAction.Action(targetBeat+6f, delegate { Anim(14);}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(15);}),
                        new BeatAction.Action(targetBeat+7f, delegate { Anim(16);}),
                        new BeatAction.Action(targetBeat+7.25f, delegate { Anim(17);}),
                        new BeatAction.Action(targetBeat+7.5f, delegate { Anim(18);}),
                        new BeatAction.Action(targetBeat+7.75f, delegate { Anim(19);}),
                        new BeatAction.Action(targetBeat+8f, delegate
                        { 
                            Anim(20);
                            Sweep(); stroke = StrokeType.HARAI;
                        }),
                        new BeatAction.Action(targetBeat+10.5f, delegate { Anim(22, "end");}),
                        new BeatAction.Action(targetBeat+11f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+8f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;

                case CharacterType.face_kr:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+1f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+2f),     // korean
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+2.5f),     // korean
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+3.25f),    // korean
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+4.5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5f),
                        new MultiSound.Sound("powerCalligraphy/brush2", targetBeat+5.5f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6f),
                        new MultiSound.Sound("powerCalligraphy/brush1", targetBeat+6.5f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7f),
                        new MultiSound.Sound("powerCalligraphy/brush3", targetBeat+7.25f),
                        new MultiSound.Sound("powerCalligraphy/brushTap", targetBeat+7.5f),
                    });
                    BeatAction.New(this, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(targetBeat, delegate { Anim(1);}),
                        new BeatAction.Action(targetBeat+1f, delegate { Anim(2);}),
                        new BeatAction.Action(targetBeat+1.75f, delegate { Anim(3);}),
                        new BeatAction.Action(targetBeat+2f, delegate { Anim(4);}),
                        new BeatAction.Action(targetBeat+2.5f, delegate { Anim(5);}),
                        new BeatAction.Action(targetBeat+3.25f, delegate { Anim(6);}),
                        new BeatAction.Action(targetBeat+3.5f, delegate { Anim(7);}),
                        new BeatAction.Action(targetBeat+4f, delegate { Anim(8);}),
                        new BeatAction.Action(targetBeat+4.5f, delegate { Anim(9);}),
                        new BeatAction.Action(targetBeat+4.75f, delegate { Anim(10);}),
                        new BeatAction.Action(targetBeat+5f, delegate { Anim(11);}),
                        new BeatAction.Action(targetBeat+5.25f, delegate { Anim(12);}),
                        new BeatAction.Action(targetBeat+5.5f, delegate { Anim(13);}),
                        new BeatAction.Action(targetBeat+6f, delegate { Anim(14);}),
                        new BeatAction.Action(targetBeat+6.5f, delegate { Anim(15);}),
                        new BeatAction.Action(targetBeat+7f, delegate { Anim(16);}),
                        new BeatAction.Action(targetBeat+7.25f, delegate { Anim(17);}),
                        new BeatAction.Action(targetBeat+7.5f, delegate { Anim(18);}),
                        new BeatAction.Action(targetBeat+7.75f, delegate { Anim(19);}),
                        new BeatAction.Action(targetBeat+8f, delegate
                        { 
                            Anim(20);
                            Sweep(); stroke = StrokeType.HARAI;
                        }),
                        new BeatAction.Action(targetBeat+10.5f, delegate { Anim(22, "end");}),
                        new BeatAction.Action(targetBeat+11f, delegate { Finish();}),
                    });
                    game.ScheduleInput(targetBeat+8f, 2f, PowerCalligraphy.InputAction_FlickPress, writeSuccess, writeMiss, Empty, CanSuccess);
                    break;
            }
        }

        // TOME
        private void Halt()
        {
            onGoing = true;
            fudeAnim.Play("fude-halt");
            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseB1", forcePlay: true);
        }
        // HANE HARAI
        private void Sweep()
        {
            onGoing = true;
            fudeAnim.Play("fude-sweep");
            releaseSound = SoundByte.PlayOneShotGame("powerCalligraphy/releaseA1", forcePlay: true);
        }
        private void Finish()
        {
            isFinish = true;
            fudeAnim.Play("fude-none");
            paperAnim.enabled = false;
        }


        private void writeSuccess(PlayerActionEvent caller, float state)
        {
            if (state >= 1f)
                ProcessInput("late");
            else if (state <= -1f)
                ProcessInput("fast");
            else
                ProcessInput("just");
        }

        private void writeMiss(PlayerActionEvent caller)
        {
            if (onGoing)
                Miss();
        }

        private void Empty(PlayerActionEvent caller) { }

        bool CanSuccess()
        {
            return onGoing;
        }

        public void ProcessInput(string input)
        {
            onGoing = false;
            switch(characterType)
            {
                case CharacterType.re:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    Anim(3, input);
                    break;
                
                case CharacterType.comma:
                    switch (input) {
                        case "just":
                            fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                            break;
                        default:
                            fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                            break;
                    }
                    Anim(2, input);
                    break;

                case CharacterType.chikara:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    Anim(7, input);
                    break;

                case CharacterType.onore:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    Anim(7, input);
                    break;

                case CharacterType.sun:
                    if (num==1) {
                        fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                        Anim(5, input);
                    } else {
                        switch (input) {
                            case "just":
                                fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                                break;
                            default:
                                fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                                break;
                        }
                        Anim(7, input);
                    }
                    break;

                case CharacterType.kokoro:
                    if (num==1) {
                        fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                        Anim(4, input);
                    } else {
                        switch (input) {
                            case "just":
                                fudeAnim.DoScaledAnimationAsync("fude-tap", 0.5f);
                                break;
                            default:
                                fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                                break;
                        }
                        Anim(8, input);
                    }
                    break;

                case CharacterType.face:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    Anim(21, input);
                    break;

                case CharacterType.face_kr:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    Anim(21, input);
                    break;
            }
            
            switch (input)
            {
                case "just":
                    switch (stroke) {
                        case StrokeType.TOME:
                            SoundByte.PlayOneShotGame("powerCalligraphy/releaseB2");
                            break;
                            
                        case StrokeType.HANE:
                        case StrokeType.HARAI:
                            SoundByte.PlayOneShotGame("powerCalligraphy/releaseA2");
                            break;
                    }
                    break;
                    
                case "late":
                case "fast":
                    switch (stroke) {   // WIP
                        case StrokeType.TOME:
                            SoundByte.PlayOneShotGame("powerCalligraphy/8");
                            break;
                        case StrokeType.HANE:
                            SoundByte.PlayOneShotGame("powerCalligraphy/6");    
                            break;
                        case StrokeType.HARAI:
                            SoundByte.PlayOneShotGame("powerCalligraphy/9");
                            break;
                    }
                    break;
                default:
                    break;
            }

            // not work
            if (input == "fast" && releaseSound is not null)
            {
                releaseSound.Stop();
                releaseSound = null;
            }
        }
        
        public void Miss()
        {
            onGoing = false;
            SoundByte.PlayOneShotGame("powerCalligraphy/7");    // WIP
            switch(characterType)
            {
                case CharacterType.re:
                    fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;
                
                case CharacterType.comma:
                    fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                    fudePosAnim.DoScaledAnimationAsync("fudePos-comma02-miss", 0.5f);
                    break;

                case CharacterType.chikara:
                    fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;

                case CharacterType.onore:
                    fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;

                case CharacterType.sun:
                    if (num==1) {
                        fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    } else {
                        fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                        fudePosAnim.DoScaledAnimationAsync("fudePos-sun07-miss", 0.5f);
                    }
                    break;

                case CharacterType.kokoro:
                    if (num==1) {
                        fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    } else {
                        fudeAnim.DoScaledAnimationAsync("fude-none", 0.5f);
                        fudePosAnim.DoScaledAnimationAsync("fudePos-kokoro08-miss", 0.5f);
                    }
                    break;

                case CharacterType.face:
                    fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;

                case CharacterType.face_kr:
                    fudeAnim.DoScaledAnimationAsync("fude-sweep-end", 0.5f);
                    break;
            }
        }

        private void Anim(int num, string str = "")
        {
            string pattern = 
                characterType.ToString() 
                + num.ToString("D2") + ((str != "") ? "-" + str : str);

            fudePosAnim.DoScaledAnimationAsync("fudePos-" + pattern, 0.5f);
            paperAnim.DoScaledAnimationAsync("paper-" + pattern, 0.5f);
            
            
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (isFinish)
                {
                    double beat = cond.songPositionInBeats;
                    // Paper scroll.
                    var paperPos = transform.localPosition;
                    transform.localPosition = paperPos + (scrollRate * Time.deltaTime);
                    if (beat >= targetBeat + 24) Destroy(gameObject);
                }
            }
        }

        public void TheEnd()
        {
            fudePosAnim.Play("fudePos-end");
            paperAnim.Play("paper-end");
        }
    }
}