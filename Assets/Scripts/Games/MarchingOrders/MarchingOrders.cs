//notes:
//  BEFORE NEW PROPS
// - minenice will also use this to test out randomly named parameters so coding has to rest until the new props update [DONE]
// - see fan club for separate prefabs (cadets) [DONE]
// - temporarily take sounds from rhre, wait until someone records the full code, including misses, or record it myself (unlikely) [IN PROGRESS]
//  AFTER NEW PROPS
// - testmod marching orders using speed
// - see space soccer, mr upbeat, tunnel for keep-the-beat codes
// - figure how to do custom bg changes when the upscaled textures are finished (see karate man, launch party once it releases)
// - will use a textbox without going through the visual options but i wonder how..?? (see first contact if ever textboxes are implemented in said minigame)
//  AFTER FEATURE COMPLETION
// - delete all notes once the minigame is considered feature-complete

using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbMarcherLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("marchingOrders", "Marching Orders \n<color=#eb5454>[WIP]</color>", "00A43B", false, false, new List<GameAction>()
                {
                    new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.BopAction(e.beat, e.length); },
                        defaultLength = 1f,
                        resizable = true
                    },
                    
                    new GameAction("marching", "Cadets March")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.MarchAction(e.beat, e.length, e["toggle"]); },
                        defaultLength = 4f,
                        resizable = true,
                        parameters = new List<Param>
                        {
                            new Param("toggle", false, "Auto March", "When enabled, will march automatically like at the end of Marchers")
                        }
                    },
                    
                    new GameAction("attention", "Attention...")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeAttention(e.beat); },
                        defaultLength = 2f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.AttentionSound(e.beat);}
                    },
                    
                    new GameAction("march", "March!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeMarch(e.beat, e["toggle"]); },
                        defaultLength = 2f,
                        parameters = new List<Param>
                        {
                            new Param("toggle", false, "Disable Voice", "Remove the sarge from saying 'MARCH' ")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.MarchSound(e.beat);}
                    },
                    
                    new GameAction("halt", "Halt!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeHalt(e.beat); },
                        defaultLength = 2f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.HaltSound(e.beat);}
                    },
                    
                    new GameAction("face turn", "Direction to Turn")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeFaceTurn(e.beat, e["type"], e["type2"], e["toggle"]); },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction sarge wants the cadets to face"),
                            new Param("type2", MarchingOrders.FaceTurnLength.Normal, "Length", "How fast or slow the event lasts"),
                            new Param("toggle", false, "Point", "Do the pointing animation instead of just the head turn")
                        }
                    },

                    new GameAction("background", "Set the Background")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.BackgroundColorSet(e.beat, e["type"]); },
                        defaultLength = 2f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.BackgroundColor.Blue, "Color", "The background color of Marching Orders"),
                        }
                    },

                    new GameAction("game mod", "Game Modifiers")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.GameplayMod(e.beat, e["toggle"]); },
                        defaultLength = 1f,
                        parameters = new List<Param>()
                        {
                            new Param("toggle", false, "Female Commandress", "Makes the Commander the Rabbit Girl"),
                        }
                    }
                },
                new List<string>() { "agb", "normal" },
                "agbmarcher", "jp", "ver0",
                new List<string>() { "en", "jp" },
                new List<string>() {}
                );
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_MarchingOrders;
    public class MarchingOrders : Minigame
    {
        //code is just copied from other minigame code, i will polish them later
        [Header("Sarge")]
        public Animator Sarge;
        public Animator Steam;

        [Header("Cadets")]
        public Animator Cadet1;
        public Animator Cadet2;
        public Animator Cadet3;
        public Animator CadetPlayer;
        public Animator CadetHead1;
        public Animator CadetHead2;
        public Animator CadetHead3;
        public Animator CadetHeadPlayer;

        [Header("Background")]
        public GameObject BGMain1;
        public GameObject BGMain2;
        public SpriteRenderer Background;
        
        public GameEvent bop = new GameEvent();
        public GameEvent noBop = new GameEvent();
        public GameEvent marching = new GameEvent();

        private string path;
        private int marchOtherCount;
        private int marchPlayerCount;
        private int turnLength;
        private int background;
        // private bool marchSuru;
        // private bool beatSuru;
        private bool autoMarch;
        private bool usagiVoice;
        private float marchTsugi;
        private float beatTsugi;
        private float steamTime;

        private string fastTurn;
        
        public static MarchingOrders instance;
        
        public enum DirectionFaceTurn
        {
            Right,
            Left,
        }
        public enum FaceTurnLength
        {
            Normal,
            Fast,
        }
        public enum BackgroundColor
        {
            Blue,
            Yellow,
            // custom
        }
        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        public void LeftSuccess(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("marchingOrders/turnActionPlayer");
            CadetHeadPlayer.DoScaledAnimationAsync("FaceL", 0.5f);
        }

        public void GenericMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShot("miss");
            Sarge.DoScaledAnimationAsync("Anger", 0.5f);
            Steam.DoScaledAnimationAsync("Steam", 0.5f);
        }

        public void LeftEmpty(PlayerActionEvent caller)
        {
            
        }

        public void RightSuccess(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("marchingOrders/turnActionPlayer");
            CadetHeadPlayer.DoScaledAnimationAsync("FaceR", 0.5f);
        }

        public void RightEmpty(PlayerActionEvent caller)
        {

        }

        public void MarchHit(PlayerActionEvent caller, float beat)
        {
            marchPlayerCount++;

            Jukebox.PlayOneShotGame("marchingOrders/step1", volume: 0.25f);
            CadetPlayer.DoScaledAnimationAsync(marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
        }

        public void MarchEmpty(PlayerActionEvent caller)
        {
            
        }

        public void HaltHit(PlayerActionEvent caller, float beat)
        {
            Jukebox.PlayOneShotGame("marchingOrders/step1", volume: 0.25f);
            CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
        }

        public void HaltEmpty(PlayerActionEvent caller)
        {

        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currBeat = cond.songPositionInBeats;

            if (cond.ReportBeat(ref marching.lastReportedBeat, marching.startBeat % 1))
            {
                if (currBeat >= marching.startBeat && currBeat < marching.startBeat + marching.length)
                {
                    CadetsMarch(marchTsugi + (int)currBeat, marching.length);
                }
                //else
                //marchSuru = false;
            }
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (currBeat >= bop.startBeat && currBeat < bop.startBeat + bop.length)
                {
                    Bop(beatTsugi + (int)currBeat, bop.length);
                }
            }
            if (!IsExpectingInputNow())
            {
                if (PlayerInput.Pressed())
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    marchPlayerCount++;
                    var marchPlayerAnim = (marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL");

                    CadetPlayer.DoScaledAnimationAsync(marchPlayerAnim, 0.5f);
                }
                if (PlayerInput.AltPressed())
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
                }
                if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.LEFT))
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetHeadPlayer.DoScaledAnimationAsync("FaceL", 0.5f);
                }
                if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.RIGHT))
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetHeadPlayer.DoScaledAnimationAsync("FaceR", 0.5f);
                }
            }
            switch (background)
            {
                case (int) MarchingOrders.BackgroundColor.Yellow:
                    BGMain1.SetActive(false);
                    BGMain2.SetActive(true);
                    break;
                default:
                    BGMain1.SetActive(true);
                    BGMain2.SetActive(false);
                    break;
            }

            if (usagiVoice)
                path = "usagiOnna/";
            else
                path = null;
        }

        public void Bop(float beat, float length)
        {
            // beatSuru = true;
            beatTsugi += 0f;

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Cadet1.DoScaledAnimationAsync("Bop", 0.5f); }),
                new BeatAction.Action(beat, delegate { Cadet2.DoScaledAnimationAsync("Bop", 0.5f); }),
                new BeatAction.Action(beat, delegate { Cadet3.DoScaledAnimationAsync("Bop", 0.5f); }),
                new BeatAction.Action(beat, delegate { CadetPlayer.DoScaledAnimationAsync("Bop", 0.5f); }),
            });
        }

        public void BopAction(float beat, float length)
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public void CadetsMarch(float beat, float length)
        {
            marchOtherCount += 1;
            // marchSuru = true;
            marchTsugi += 0f;
            var marchOtherAnim = (marchOtherCount % 2 != 0 ? "MarchR" : "MarchL");

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Cadet1.DoScaledAnimationAsync(marchOtherAnim, 0.5f); }),
                new BeatAction.Action(beat, delegate { Cadet2.DoScaledAnimationAsync(marchOtherAnim, 0.5f); }),
                new BeatAction.Action(beat, delegate { Cadet3.DoScaledAnimationAsync(marchOtherAnim, 0.5f); }),
                new BeatAction.Action(beat, delegate { 
                    if (autoMarch) {CadetPlayer.DoScaledAnimationAsync(marchOtherAnim, 0.5f); 
                        Jukebox.PlayOneShotGame("marchingOrders/step1"); }
                    else ScheduleInput(beat - 1f, 1f, InputType.STANDARD_DOWN, MarchHit, GenericMiss, MarchEmpty);})
            });
            Jukebox.PlayOneShotGame("marchingOrders/step1", volume: 0.75f);
        }
        
        public void MarchAction(float beat, float length, bool auto)
        {
            marching.length = length;
            marching.startBeat = beat;
            autoMarch = auto;
        }

        public void SargeAttention(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/" + path + "attention1", beat),
            new MultiSound.Sound("marchingOrders/" + path + "attention2", beat + 0.5f),
            }, forcePlay:true);
            
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat + 0.25f,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                });
        }
        
        public void SargeMarch(float beat, bool noVoice)
        {
            marchOtherCount = 0;
            marchPlayerCount = 0;

            if (!noVoice)
            {
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("marchingOrders/" + path + "march1", beat),
                    new MultiSound.Sound("marchingOrders/" + path + "march2", beat + (usagiVoice ? 0.15f : 0.25f)),
                    new MultiSound.Sound("marchingOrders/" + path + "march3", beat + (usagiVoice ? 0.25f : 0.5f)),
                    new MultiSound.Sound("marchingOrders/marchStart", beat + 1f),
                }, forcePlay: true);
            }

            if (!noVoice)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { CadetPlayer.DoScaledAnimationAsync("MarchL", 0.5f);}),
                });
            }
            else
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { CadetPlayer.DoScaledAnimationAsync("MarchL", 0.5f);}),
                });
            }
        }
        
        public void SargeHalt(float beat)
        {
            if (!usagiVoice)
            {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("marchingOrders/halt1", beat),
                new MultiSound.Sound("marchingOrders/halt2", beat + 1f),
                new MultiSound.Sound("marchingOrders/step1", beat + 1f),
                }, forcePlay: true);
            } 
            else
            {
                MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("marchingOrders/usagiOnna/halt1", beat),
                new MultiSound.Sound("marchingOrders/usagiOnna/halt2", beat + 0.2f),
                new MultiSound.Sound("marchingOrders/usagiOnna/halt3", beat + 0.4f),
                new MultiSound.Sound("marchingOrders/halt2", beat + 1f),
                new MultiSound.Sound("marchingOrders/step1", beat + 1f, volume: 0.75f),
                }, forcePlay: true);
            }

            ScheduleInput(beat, 1f, InputType.STANDARD_ALT_DOWN, HaltHit, GenericMiss, HaltEmpty);
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("Halt", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("Halt", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("Halt", 0.5f);}),
                });
        }
        
        public void SargeFaceTurn(float beat, int type, int type2, bool toggle)
        {
            switch (type2)
            {
                case (int) MarchingOrders.FaceTurnLength.Fast:
                    turnLength = 0;
                    fastTurn = "fast";
                    break;
                default:
                    turnLength = 1;
                    fastTurn = "";
                    break;
            }
            
             
            switch (type)
            {
                case (int) MarchingOrders.DirectionFaceTurn.Left:
                    ScheduleInput(beat, turnLength + 2f, InputType.DIRECTION_LEFT_DOWN, LeftSuccess, GenericMiss, LeftEmpty);
                    if (!usagiVoice)
                    {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("marchingOrders/leftFaceTurn1" + fastTurn, beat),
                        new MultiSound.Sound("marchingOrders/leftFaceTurn2" + fastTurn, beat + 0.6f),
                        new MultiSound.Sound("marchingOrders/leftFaceTurn3", beat + turnLength + 1f),
                        new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                        }, forcePlay: true);
                    }
                    else
                    {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("marchingOrders/usagiOnna/leftFaceTurn1" + fastTurn, beat),
                        new MultiSound.Sound("marchingOrders/usagiOnna/leftFaceTurn2" + fastTurn, beat + 0.4f),
                        new MultiSound.Sound("marchingOrders/usagiOnna/leftFaceTurn3" + fastTurn, beat + 0.6f),
                        new MultiSound.Sound("marchingOrders/usagiOnna/leftFaceTurn4", beat + turnLength + 1f),
                        new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                        }, forcePlay: true);
                    }
                    
                        BeatAction.New(gameObject, new List<BeatAction.Action>() 
                            {
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead1.DoScaledAnimationAsync("FaceL", 0.5f);
                                else Cadet1.DoScaledAnimationAsync("PointL"); }),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead2.DoScaledAnimationAsync("FaceL", 0.5f);
                                else Cadet2.DoScaledAnimationAsync("PointL");}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead3.DoScaledAnimationAsync("FaceL", 0.5f);
                                else Cadet3.DoScaledAnimationAsync("PointL");}),
                            });
                    break;
                default:
                    ScheduleInput(beat, turnLength + 2f, InputType.DIRECTION_RIGHT_DOWN, RightSuccess, GenericMiss, RightEmpty);
                    if (!usagiVoice)
                    {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("marchingOrders/rightFaceTurn1" + fastTurn, beat),
                        new MultiSound.Sound("marchingOrders/rightFaceTurn2" + fastTurn, beat + 0.6f),
                        new MultiSound.Sound("marchingOrders/rightFaceTurn3", beat + turnLength + 1f),
                        new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                        }, forcePlay: true);
                    }
                    else
                    {
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("marchingOrders/usagiOnna/rightFaceTurn1" + fastTurn, beat),
                        new MultiSound.Sound("marchingOrders/usagiOnna/rightFaceTurn2" + fastTurn, beat + 0.4f),
                        new MultiSound.Sound("marchingOrders/usagiOnna/rightFaceTurn3" + fastTurn, beat + 0.6f),
                        new MultiSound.Sound("marchingOrders/usagiOnna/rightFaceTurn4", beat + turnLength + 1f),
                        new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                        }, forcePlay: true);
                    }
                    
                        BeatAction.New(gameObject, new List<BeatAction.Action>() 
                            {
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead1.DoScaledAnimationAsync("FaceR", 0.5f);
                                else Cadet1.DoScaledAnimationAsync("PointR");}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead2.DoScaledAnimationAsync("FaceR", 0.5f);
                                else Cadet2.DoScaledAnimationAsync("PointR");}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { if (!toggle) CadetHead3.DoScaledAnimationAsync("FaceR", 0.5f);
                                else Cadet3.DoScaledAnimationAsync("PointR");}),
                            });
                    break;
            }
            
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + turnLength + 1f,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                });
        }
        
        public void BackgroundColorSet(float beat, int type)
        {
            background = type;
        }

        public void GameplayMod(float beat, bool femCom)
        {
            if (femCom)
                usagiVoice = true;
        }
        public static void AttentionSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/attention1", beat),
            new MultiSound.Sound("marchingOrders/attention2", beat + 0.5f),
            }, forcePlay:true);
        }
        
        public static void MarchSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/march1", beat),
            new MultiSound.Sound("marchingOrders/march2", beat + 0.25f),
            new MultiSound.Sound("marchingOrders/march3", beat + 0.5f),
            new MultiSound.Sound("marchingOrders/marchStart", beat + 1f),
            }, forcePlay:true);
        }
        
        public static void HaltSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/halt1", beat),
            new MultiSound.Sound("marchingOrders/halt2", beat + 1f),
            }, forcePlay:true);
        }
    }
}

