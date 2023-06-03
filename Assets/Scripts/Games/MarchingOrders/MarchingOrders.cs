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
                            new Param("direction", MarchingOrders.Direction.Right, "Direction", "The direction for the cadets to face."),
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
                            new Param("direction", MarchingOrders.Direction.Right, "Direction", "The direction for the cadets to face."),
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
                        function = delegate {
                            var e = eventCaller.currentEntity;
                            MarchingOrders.instance.MoveConveyor(e.length, e["start"], e["direction"]);
                        },
                        parameters = new List<Param>()
                        {
                            new Param("start", true, "Start Moving", "Start moving the conveyor"),
                            new Param("direction", MarchingOrders.Direction.Right, "Direction", "Direction"),
                        }
                    },
                    new GameAction("background", "Background Colors")
                    {
                        function = delegate {
                            var e = eventCaller.currentEntity;
                            MarchingOrders.instance.BackgroundColorSet(e["preset"], e["colorFill"], e["colorTiles1"], e["colorTiles2"], e["colorTiles3"], e["colorPipes1"], e["colorPipes2"], e["colorPipes3"], e["colorConveyor1"], e["colorConveyor2"]);
                        },
                        defaultLength = 0.5f,
                        parameters = new List<Param>()
                        {
                            new Param("preset", MarchingOrders.BackgroundColor.Yellow, "Color", "Choose from a preset, or choose custom and set colors below"),
                            new Param("colorFill", new Color(0.259f, 0.353f, 0.404f), "Wall Color", "Sets the color of the wall"),
                            new Param("colorTiles1", new Color(1f, 0.76f, 0.52f), "Tile Outline Color", "Sets the color of the tile outline"),
                            new Param("colorTiles2", new Color(1f, 0.6f, 0.2f), "Tile Shading Color", "Sets the color of the tile shading"),
                            new Param("colorTiles3", new Color(1f, 0.675f, 0f), "Tile Fill Color", "Sets the color of the tile's main color"),
                            new Param("colorPipes1", new Color(), "Pipe Outline Color", "Sets the color of the pipes' outline"),
                            new Param("colorPipes2", new Color(), "Pipe Shading Color", "Sets the color of the pipes' shading"),
                            new Param("colorPipes3", new Color(), "Pipe Fill Color", "Sets the color of the pipes"),
                            new Param("colorConveyor1", new Color(), "Conveyor Fill Color", "Sets the color of the conveyer belt"),
                            new Param("colorConveyor2", new Color(), "Conveyor Trim Color", "Sets the conveyor's trim color"),
                        }
                    },
                    
                    new GameAction("forceMarching", "Force Marching")
                    {
                        preFunction = delegate { MarchingOrders.SargeMarch(eventCaller.currentEntity.beat - 2, true); },
                        resizable = true,
                    },
                    new GameAction("forceMarching", "Force Marching")
                    {
                        preFunction = delegate { MarchingOrders.wantMarch = eventCaller.currentEntity.beat - 1; },
                        preFunctionLength = 1,
                        resizable = true,
                    },
                    
                    // hidden in the editor but here cuz backwards compatibility
                    new GameAction("marching", "Start Marching (old)")
                    {
                        hidden = true,
                        preFunction = delegate { MarchingOrders.SargeMarch(eventCaller.currentEntity.beat - 2, true); },
                        resizable = true,
                    },
                    new GameAction("face turn", "Direction to Turn (old)")
                    {
                        hidden = true,
                        function = delegate {
                            var e = eventCaller.currentEntity;
                            MarchingOrders.instance.FaceTurn(e.beat, e["type"], e["type2"], false);
                        },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.Direction.Right, "Direction", "The direction the sergeant wants the cadets to face"),
                            new Param("type2", MarchingOrders.FaceTurnLength.Normal, "Length", "The duration of the turning event"),
                        }
                    },
                },
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
        [SerializeField] Animator[] CadetHeads = new Animator[3];
        [SerializeField] Animator CadetPlayer;
        [SerializeField] Animator CadetHeadPlayer;
        [SerializeField] ScrollObject[] ConveyorGo;
        [SerializeField] SpriteRenderer[] BackgroundRecolorable;
        [SerializeField] Material[] RecolorMats;

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
        public static float wantMarch = float.MinValue;
        

        public enum Direction
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
            Yellow,
            Blue,
            Custom,
        }

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
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                            new BeatAction.Action(march, delegate {
                                ScheduleInput(march, 1f, InputType.STANDARD_DOWN, MarchHit, GenericMiss, Empty);
                            }),
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

            if (ConveyorGo[0].AutoScroll && (ConveyorGo[1].gameObject.transform.position.x <= 0)) foreach (var scroll in ConveyorGo) scroll.AutoScroll = false;

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

            if (GameManager.instance.currentGame == "marchingOrders") {
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
        
        public void BackgroundColorSet(int preset, Color fill, Color tiles1, Color tiles2, Color tiles3, Color pipes1, Color pipes2, Color pipes3, Color conveyor1, Color conveyor2)
        {
            if (preset == 2) UpdateMaterialColor(fill, tiles1, tiles2, tiles3, pipes1, pipes2, pipes3, conveyor1, conveyor2);
            else {
                bool x = preset == 0;
                //UpdateMaterialColor();
            }
            
            //Pipes.color = pipesColor;
            //UpdateColor(pipes, floor, wall);
        }

        public void UpdateMaterialColor(Color fill, Color tiles1, Color tiles2, Color tiles3, Color pipes1, Color pipes2, Color pipes3, Color conveyor1, Color conveyor2)
        {
            BackgroundRecolorable[0].color = fill;

            RecolorMats[0].SetColor("_ColorAlpha", tiles3);
            RecolorMats[0].SetColor("_ColorBravo", tiles2);
            RecolorMats[0].SetColor("_ColorDelta", tiles1);
            
            RecolorMats[1].SetColor("_ColorAlpha", pipes1);
            RecolorMats[1].SetColor("_ColorBravo", pipes2);
            RecolorMats[1].SetColor("_ColorDelta", pipes3);

            RecolorMats[2].SetColor("_ColorBravo", conveyor1);
            RecolorMats[2].SetColor("_ColorDelta", conveyor2);

            BackgroundRecolorable[5].color = conveyor1;
        }

        /*
        red
        SetColor("_ColorAlpha", new Color(1, 0, 0, 1));
        green
        SetColor("_ColorBravo", new Color(1, 0, 0, 1));
        blue
        SetColor("_ColorDelta", new Color(1, 0, 0, 1));
        */

        public static void AttentionSound(float beat)
        {
            PlaySoundSequence("marchingOrders", "zentai", beat - 1);
        }
        
        public static void HaltSound(float beat)
        {
            PlaySoundSequence("marchingOrders", "tomare", beat);
        }

        public void MoveConveyor(float length, bool go, int direction)
        {
            foreach (var scroll in ConveyorGo) {
                scroll.SpeedMod = ((direction == 0 ? 4 : -4)/length)*(Conductor.instance.songBpm/100);
                scroll.AutoScroll = go;
            }
        }

        static void Empty(PlayerActionEvent caller) { }
    }
}

