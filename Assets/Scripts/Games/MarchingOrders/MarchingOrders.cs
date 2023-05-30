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

using HeavenStudio.Common;
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
            return new Minigame("marchingOrders", "Marching Orders", "ffb108", false, false, new List<GameAction>()
                {
                    new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.BopAction(e.beat, e.length, e["bop"], e["autoBop"], e["clap"]); },
                        defaultLength = 1f,
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("bop", true, "Bop", "Should the cadets bop?"),
                            new Param("autoBop", false, "Bop (Auto)", "Should the cadets auto bop?"),
                            new Param("clap", false, "Clap", "Should the cadets clap instead of bop?"),
                        }
                    },
                    new GameAction("attention", "Attention...")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeAttention(e.beat); },
                        defaultLength = 2f,
                        preFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.AttentionSound(e.beat);}
                    },
                    new GameAction("march", "March!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.SargeMarch(e.beat, e["disableVoice"]); },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.SargeMarch(e.beat, e["disableVoice"]); },
                        defaultLength = 2f,
                        parameters = new List<Param>
                        {
                            new Param("disableVoice", false, "Disable Voice", "Disable the Drill Sergeant's call")
                        },
                    },
                    new GameAction("faceTurn", "Face Turn")
                    {
                        function = delegate {
                            var e = eventCaller.currentEntity;
                            MarchingOrders.instance.FaceTurn(e.beat, e["direction"], false, e["point"]);
                        },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("direction", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction for the cadets to face."),
                            new Param("point", false, "Point", "Point and face a direction instead of just facing a direction."),
                        }
                    },
                    new GameAction("faceTurnFast", "Fast Face Turn")
                    {
                        function = delegate { 
                            var e = eventCaller.currentEntity; 
                            MarchingOrders.instance.FaceTurn(e.beat, e["direction"], true, e["point"]); 
                        },
                        defaultLength = 3f,
                        parameters = new List<Param>()
                        {
                            new Param("direction", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction for the cadets to face."),
                            new Param("point", false, "Point", "Point and face a direction instead of just facing a direction."),
                        }
                    },
                    new GameAction("halt", "Halt!")
                    {
                        function = delegate { MarchingOrders.instance.Halt(eventCaller.currentEntity.beat); },
                        defaultLength = 2f,
                        inactiveFunction = delegate { MarchingOrders.HaltSound(eventCaller.currentEntity.beat);}
                    },
                    new GameAction("go", "Go!")
                    {
                        function = delegate { MarchingOrders.instance.MoveConveyor(eventCaller.currentEntity.length); },
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
                    
                    // hidden in the editor but here cuz backwards compatibility
                    new GameAction("marching", "Cadets March")
                    {
                        hidden = true,
                        preFunction = delegate { 
                            var e = eventCaller.currentEntity; 
                            MarchingOrders.SargeMarch(e.beat - 2, false); 
                        },
                        resizable = true,
                    },
                    new GameAction("face turn", "Direction to Turn")
                    {
                        
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.FaceTurn(e.beat, e["type"], e["type2"], false); },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction the sergeant wants the cadets to face"),
                            new Param("type2", MarchingOrders.FaceTurnLength.Normal, "Length", "The duration of the turning event"),
                            //new Param("toggle", false, "Point", "Do the pointing animation instead of just the head turn"),
                        }
                    },
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

        static List<float> queuedMarches = new List<float>();

        [Header("Animators")]
        [SerializeField] Animator Sarge;
        [SerializeField] Animator Steam;
        [SerializeField] Animator[] Cadets = new Animator[3];
        [SerializeField] Animator CadetPlayer;
        [SerializeField] Animator[] CadetHeads = new Animator[3];
        [SerializeField] Animator CadetHeadPlayer;
        [SerializeField] ScrollObject[] ConveyorGo;

        /*
        [Header("Background")]
        [SerializeField] GameObject BGMain1;
        [SerializeField] SpriteRenderer Background;
        [SerializeField] SpriteRenderer Pipes;
        [SerializeField] SpriteRenderer Floor;
        [SerializeField] SpriteRenderer Wall;
        [SerializeField] SpriteRenderer Conveyor;

        [Header("Color Map")]
        [SerializeField] static Color pipesColor;
        [SerializeField] static Color floorColor;
        [SerializeField] static Color wallColor;
        [SerializeField] static Color fillColor;
        */

        [Header("Variables")]
        bool goBop;
        bool shouldClap;
        bool keepMarching;
        private int marchOtherCount;
        private int marchPlayerCount;
        private float lastMissBeat;
        private float lastReportedBeat;
        static float wantMarch = float.MinValue;
        
        
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
        /*
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
        */

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (wantMarch != float.MinValue) {
                queuedMarches.Add(wantMarch);
                marchOtherCount =
                marchPlayerCount = 0;
                keepMarching = true;
                wantMarch = float.MinValue;
            }

            if (goBop && Conductor.instance.ReportBeat(ref lastReportedBeat)) {
                foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync(shouldClap ? "Clap" : "Bop", 0.5f);
                CadetPlayer.DoScaledAnimationAsync(shouldClap ? "Clap" : "Bop", 0.5f);
            }

            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                if (queuedMarches.Count > 0) {
                    foreach (var march in queuedMarches) {
                        ScheduleInput(march, 1f, InputType.STANDARD_DOWN, MarchHit, GenericMiss, Empty);
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                            new BeatAction.Action(march + 1, delegate {
                                marchOtherCount++;
                                foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync(marchOtherCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
                                Jukebox.PlayOneShotGame("marchingOrders/stepOther");
                                if (keepMarching) queuedMarches.Add(march + 1);
                            }),
                        });
                    }
                    queuedMarches.Clear();
                }
            }

            // input stuff below

            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN)) {
                Miss();
                marchPlayerCount++;
                CadetPlayer.DoScaledAnimationAsync((marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL"), 0.5f);
            }

            if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN)) {
                Miss();
                CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
            }

            if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.LEFT) && !IsExpectingInputNow(InputType.DIRECTION_LEFT_DOWN)) {
                Miss();
                CadetHeadPlayer.DoScaledAnimationAsync("FaceL", 0.5f);
            }

            if (PlayerInput.Pressed(true) && PlayerInput.GetSpecificDirection(PlayerInput.RIGHT) && !IsExpectingInputNow(InputType.DIRECTION_RIGHT_DOWN)) {
                Miss();
                CadetHeadPlayer.DoScaledAnimationAsync("FaceR", 0.5f);
            }
        }

        public void LeftSuccess(PlayerActionEvent caller, float state)
        {
            TurnSuccess(state, "L");
        }

        public void RightSuccess(PlayerActionEvent caller, float state)
        {
            TurnSuccess(state, "R");
        }

        public void LeftPointSuccess(PlayerActionEvent caller, float state)
        {
            TurnSuccess(state, "L", true);
        }

        public void RightPointSuccess(PlayerActionEvent caller, float state)
        {
            TurnSuccess(state, "R", true);
        }

        void TurnSuccess(float state, string dir, bool shouldPoint = false)
        {
            if (state <= -1f || state >= 1f) Jukebox.PlayOneShot("nearMiss");
            else Jukebox.PlayOneShotGame("marchingOrders/turnActionPlayer");

            CadetHeadPlayer.DoScaledAnimationAsync("Face"+dir, 0.5f);
            if (shouldPoint) CadetPlayer.DoScaledAnimationAsync("Point"+dir, 0.5f);
        }

        public void GenericMiss(PlayerActionEvent caller)
        {
            if (Conductor.instance.songPositionInBeats - lastMissBeat <= 1.1f) return;
            Miss();
        }

        public void Miss()
        {
            lastMissBeat = Conductor.instance.songPositionInBeats;
            Jukebox.PlayOneShot("miss");
            Sarge.DoScaledAnimationAsync("Anger", 0.5f);
            Steam.DoScaledAnimationAsync("Steam", 0.5f);
        }

        public void MarchHit(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f) Jukebox.PlayOneShot("nearMiss");
            else Jukebox.PlayOneShotGame("marchingOrders/stepPlayer", volume: 0.25f);
            marchPlayerCount++;
            CadetPlayer.DoScaledAnimationAsync(marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL", 0.5f);
        }

        public void HaltHit(PlayerActionEvent caller, float state)
        {
            if (state <= -1f || state >= 1f) Jukebox.PlayOneShot("nearMiss");
            else Jukebox.PlayOneShotGame("marchingOrders/stepPlayer", volume: 0.25f);
            
            CadetPlayer.DoScaledAnimationAsync("Halt", 0.5f);
        }

        public void BopAction(float beat, float length, bool shouldBop, bool autoBop, bool clap)
        {
            goBop = autoBop;
            shouldClap = clap;
            if (shouldBop) {
                for (int i = 0; i < length; i++) {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                        new BeatAction.Action(beat + i, delegate {
                            foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync(shouldClap ? "Clap" : "Bop", 0.5f);
                            CadetPlayer.DoScaledAnimationAsync(shouldClap ? "Clap" : "Bop", 0.5f);
                        })
                    });
                }
            }
        }

        public void SargeAttention(float beat)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 0.25f, delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
            });
        }
        
        public static void SargeMarch(float beat, bool noVoice)
        {
            MarchingOrders.wantMarch = beat + 1;

            if (!noVoice) PlaySoundSequence("marchingOrders", "susume", beat);

            if (GameManager.instance.currentGame != "marchingOrders") {
                
            } else {
                MarchingOrders.instance.PreMarch(beat);
                if (!noVoice) MarchingOrders.instance.Sarge.DoScaledAnimationAsync("Talk", 0.5f);
            }
        }

        public void PreMarch(float beat)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1f, delegate { 
                    foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync("MarchL", 0.5f);
                    CadetPlayer.DoScaledAnimationAsync("MarchL", 0.5f);
                }),
            });
        }
        
        public void Halt(float beat)
        {
            keepMarching = false;
            HaltSound(beat);            

            ScheduleInput(beat, 1f, InputType.STANDARD_ALT_DOWN, HaltHit, GenericMiss, Empty);
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(beat, delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + 1f, delegate { foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync("Halt", 0.5f);}),
            });
        }
        
        public void FaceTurn(float beat, int direction, bool isFast, bool shouldPoint)
        {
            // x is true if the direction is right
            bool x = (direction == 0);
            int turnLength = (isFast ? 0 : 1);

            ScheduleInput(beat, turnLength + 2f, x ? InputType.DIRECTION_RIGHT_DOWN : InputType.DIRECTION_LEFT_DOWN, x ? (shouldPoint ? RightPointSuccess : RightSuccess) : (shouldPoint ? LeftPointSuccess : LeftSuccess), GenericMiss, Empty);
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound($"marchingOrders/{(x ? "right" : "left")}FaceTurn1{(isFast ? "fast" : "")}", beat),
                new MultiSound.Sound($"marchingOrders/{(x ? "right" : "left")}FaceTurn2{(isFast ? "fast" : "")}", beat + 0.5f),
                new MultiSound.Sound($"marchingOrders/{(x ? "right" : "left")}FaceTurn3", beat + turnLength + 1f),
                new MultiSound.Sound("marchingOrders/turnAction", beat + turnLength + 2f),
            }, forcePlay: true);
            
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + turnLength + 2f,delegate {
                    if (shouldPoint) foreach (var cadet in Cadets) cadet.DoScaledAnimationAsync($"Point{(x ? "R" : "L")}", 0.5f);
                    foreach (var head in CadetHeads) head.DoScaledAnimationAsync($"Face{(x ? "R" : "L")}", 0.5f);
                })
            });
            
            BeatAction.New(gameObject, new List<BeatAction.Action>() 
            {
                new BeatAction.Action(beat, delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + turnLength + 1f, delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
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
        
        public static void HaltSound(float beat)
        {
            PlaySoundSequence("marchingOrders", "tomare", beat);
        }

        public void MoveConveyor(float length)
        {
            foreach (var scroll in ConveyorGo) {
                Debug.Log(scroll);
                scroll.SpeedMod = (4/length)*(Conductor.instance.songBpm/100);
                scroll.AutoScroll = !scroll.AutoScroll;
            }
        }

        static void Empty(PlayerActionEvent caller) { }
    }
}

