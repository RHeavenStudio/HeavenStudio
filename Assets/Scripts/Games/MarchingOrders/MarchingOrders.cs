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
using UnityEditor;
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
                        preFunction = delegate { 
                            var e = eventCaller.currentEntity; 
                            MarchingOrders.PreMarch(e.beat, e.length); 
                        },
                        defaultLength = 4f,
                        resizable = true,
                    },

                    new GameAction("attention", "Attention...")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeAttention(e.beat); },
                        defaultLength = 2f,
                        preFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.AttentionSound(e.beat);}
                    },

                    new GameAction("march", "March!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeMarch(e.beat, e["toggle"]); },
                        defaultLength = 2f,
                        parameters = new List<Param>
                        {
                            new Param("toggle", false, "Disable Voice", "Disable the Drill Sergeant's call")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.MarchSound(e.beat, e["toggle"]);}
                    },

                    new GameAction("halt", "Halt!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeHalt(e.beat); },
                        defaultLength = 2f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.HaltSound(e.beat);}
                    },

                    new GameAction("face turn", "Direction to Turn")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeFaceTurn(e.beat, e["type"], e["type2"], false); },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction the sergeant wants the cadets to face"),
                            new Param("type2", MarchingOrders.FaceTurnLength.Normal, "Length", "The duration of the turning event"),
                            //new Param("toggle", false, "Point", "Do the pointing animation instead of just the head turn")
                        }
                    },

                    /*new GameAction("background", "Set the Background") colors aren't implemented yet
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.BackgroundColorSet(e.beat, e["type"], e["type2"], e["colorDefault"], e["colorPipe"], e["colorFloor"], e["colorFill"]); },
                        defaultLength = 0.5f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.BackgroundColor.Blue, "Color", "The game Background Color"),
                            new Param("type2", MarchingOrders.BackgroundType.SingleColor, "Color Type", "The way the color is applied to the background"),
                            new Param("colorDefault", new Color(), "Wall Color", "Sets the color of the wall"),
                            new Param("colorPipe", new Color(), "Pipes Color", "Sets the color of the pipes"),
                            new Param("colorFloor", new Color(), "Floor Color", "Sets the color of the floor and conveyer belt"),
                            new Param("colorFill", new Color(), "Fill Color", "Sets the fill color")
                        }
                    },*/
                }, // this cause problems with the background
                new List<string>() { "agb", "normal" },
                "agbmarcher", "en",
                new List<string>() { "en", "jp" }
                );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MarchingOrders;
    public class MarchingOrders : Minigame
    {
        public static MarchingOrders instance;

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
        public SpriteRenderer Background;
        public SpriteRenderer Pipes;
        public SpriteRenderer Floor;
        public SpriteRenderer Wall;
        public SpriteRenderer Conveyor;

        [Header("Color Map")]
        public static Color pipesColor;
        public static Color floorColor;
        public static Color wallColor;
        public static Color fillColor;

        [Header("Game Events")]
        public GameEvent bop = new GameEvent();
        public GameEvent noBop = new GameEvent();
        public GameEvent marching = new GameEvent();

        private int marchOtherCount;
        private int marchPlayerCount;
        private int turnLength;
        private int background;
        private float steamTime;

        static float wantMarch = float.MaxValue;
        static float wantMarchLength = 0f;
        
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
            Custom,
        }
        public enum BackgroundType
        {
            SingleColor,
            DifferentColor
        }
        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        public void LeftSuccess(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f)
            {
                Jukebox.PlayOneShot("nearNiss");
            }
            else
                Jukebox.PlayOneShotGame("marchingOrders/turnActionPlayer");
            CadetHeadPlayer.DoScaledAnimationAsync("FaceL", 0.5f);
        }

        public void GenericMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShot("miss");
            Sarge.DoScaledAnimationAsync("Anger", 0.5f);
            Steam.DoScaledAnimationAsync("Steam", 0.5f);
        }

        public void LeftEmpty(PlayerActionEvent caller) {}

        public void RightSuccess(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f)
            {
                Jukebox.PlayOneShot("nearNiss");
            }
            else
                Jukebox.PlayOneShotGame("marchingOrders/turnActionPlayer");
            CadetHeadPlayer.DoScaledAnimationAsync("FaceR", 0.5f);
        }

        public void RightEmpty(PlayerActionEvent caller) {}

        public void MarchHit(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f)
            {
                Jukebox.PlayOneShot("nearNiss");
            }
            else
                Jukebox.PlayOneShotGame("marchingOrders/stepPlayer", volume: 0.25f);
            marchPlayerCount++;
            CadetPlayer.DoScaledAnimationAsync(marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
        }

        public void MarchEmpty(PlayerActionEvent caller) {}

        public void HaltHit(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f)
            {
                Jukebox.PlayOneShot("nearNiss");
            }
            else
                Jukebox.PlayOneShotGame("marchingOrders/stepPlayer", volume: 0.25f);
            CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
        }

        public void HaltEmpty(PlayerActionEvent caller) {}

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currBeat = cond.songPositionInBeats;

            if (cond.songPositionInBeatsAsDouble >= wantMarch)
            {
                PrepareMarch(wantMarch, wantMarchLength, true);
                wantMarch = float.MaxValue;
            }

            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1, true))
            {
                if (currBeat >= bop.startBeat && currBeat < bop.startBeat + bop.length)
                {
                    Cadet1.DoScaledAnimationAsync("Bop", 0.5f);
                    Cadet2.DoScaledAnimationAsync("Bop", 0.5f);
                    Cadet3.DoScaledAnimationAsync("Bop", 0.5f);
                    CadetPlayer.DoScaledAnimationAsync("Bop", 0.5f);
                }
            }

            if (!IsExpectingInputNow(InputType.STANDARD_DOWN))
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
            }
            if (!IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                if (PlayerInput.AltPressed())
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
                }
            }
            if (!IsExpectingInputNow(InputType.DIRECTION_LEFT_DOWN))
            {
                if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.LEFT))
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetHeadPlayer.DoScaledAnimationAsync("FaceL", 0.5f);
                }
            }
            if (!IsExpectingInputNow(InputType.DIRECTION_RIGHT_DOWN))
            {    
                if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.RIGHT))
                {
                    Jukebox.PlayOneShot("miss");
                    Sarge.DoScaledAnimationAsync("Anger", 0.5f);
                    Steam.DoScaledAnimationAsync("Steam", 0.5f);

                    CadetHeadPlayer.DoScaledAnimationAsync("FaceR", 0.5f);
                }
            }
        }

        public void BopAction(float beat, float length)
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public static void PreMarch(float beat, float length)
        {
            wantMarch = beat - 1;
            wantMarchLength = length;
        }

        public void PrepareMarch(float beat, float length = 0, bool first = false)
        {
            if (GameManager.instance.currentGame != "marchingOrders")
                return;
            if (first)
            {
                marching.length = length;
                marching.startBeat = beat + 1;

                marchOtherCount = 0;
                marchPlayerCount = 0;
            }
            else
            {
                marchOtherCount++;
                Cadet1.DoScaledAnimationAsync(marchOtherCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
                Cadet2.DoScaledAnimationAsync(marchOtherCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
                Cadet3.DoScaledAnimationAsync(marchOtherCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
            }

            if (beat + 1 < marching.startBeat + marching.length)
            {
                Debug.Log($"PrepareMarch next {beat + 1}, {marching.startBeat}, {marching.length}");
                BeatAction.New(gameObject, new List<BeatAction.Action>() 
                {
                    new BeatAction.Action(beat + 1f,     delegate {
                        PrepareMarch(beat + 1);
                        MultiSound.Play(new MultiSound.Sound[] {
                            new MultiSound.Sound("marchingOrders/stepOther", beat + 1),
                        }, true);
                    }),
                });
                ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, MarchHit, GenericMiss, MarchEmpty);
            }
        }

        public void SargeAttention(float beat)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
            {
               new BeatAction.Action(beat + 0.25f,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
            });
        }
        
        public void SargeMarch(float beat, bool noVoice)
        {
            marchOtherCount = 0;
            marchPlayerCount = 0;
            MarchSound(beat, noVoice);

            if (!noVoice)
                Sarge.DoScaledAnimationAsync("Talk", 0.5f);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { CadetPlayer.DoScaledAnimationAsync("MarchL", 0.5f);}),
            });
        }
        
        public void SargeHalt(float beat)
        {
            HaltSound(beat);            

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
            string fastTurn = "";
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
                    MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("marchingOrders/leftFaceTurn1" + fastTurn, beat),
                    new MultiSound.Sound("marchingOrders/leftFaceTurn2" + fastTurn, beat + 0.5f),
                    new MultiSound.Sound("marchingOrders/leftFaceTurn3", beat + turnLength + 1f),
                    new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                    }, forcePlay: true);
                    
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
                    MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("marchingOrders/rightFaceTurn1" + fastTurn, beat),
                    new MultiSound.Sound("marchingOrders/rightFaceTurn2" + fastTurn, beat + 0.5f),
                    new MultiSound.Sound("marchingOrders/rightFaceTurn3", beat + turnLength + 1f),
                    new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
                    }, forcePlay: true);
                    
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
        
        /*public void BackgroundColorSet(float beat, int type, int colorType, Color wall, Color pipes, Color floor, Color fill)
        {
            background = type;
            if (colorType == (int) MarchingOrders.BackgroundColor.Custom)
            { 
                pipesColor = pipes; 
                floorColor = floor;
                wallColor = wall;
                fillColor = fill;
            }
            Pipes.color = pipesColor;
            UpdateMaterialColour(pipes, floor, wall);
        }

        public static void UpdateMaterialColour(Color mainCol, Color highlightCol, Color objectCol)
        {
            pipesColor = mainCol;
            floorColor = highlightCol;
            wallColor = objectCol;
        }*/

        public static void AttentionSound(float beat)
        {
            PlaySoundSequence("marchingOrders", "zentai", beat - 1);
        }
        
        public static void MarchSound(float beat, bool noVoice)
        {
            if (!noVoice)
            {
                PlaySoundSequence("marchingOrders", "susume", beat);
            }
        }
        
        public static void HaltSound(float beat)
        {
            PlaySoundSequence("marchingOrders", "tomare", beat);
        }
    }
}

