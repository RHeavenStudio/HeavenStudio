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
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.Bop(e.beat, e.length); },
                        defaultLength = 1f,
                        resizable = true
                    },
                    
                    new GameAction("marching", "Cadets March")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.CadetsMarch(e.beat, e.length); },
                        defaultLength = 4f,
                        resizable = true
                    },
                    
                    new GameAction("attention", "Attention...")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeAttention(e.beat); },
                        defaultLength = 2.25f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; MarchingOrders.AttentionSound(e.beat);}
                    },
                    
                    new GameAction("march", "March!")
                    {
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeMarch(e.beat); },
                        defaultLength = 2f,
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
                        function = delegate { var e = eventCaller.currentEntity; MarchingOrders.instance.SargeFaceTurn(e.beat, e["type"], e["type2"]); },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("type", MarchingOrders.DirectionFaceTurn.Right, "Direction", "The direction sarge wants the cadets to face"),
                            new Param("type2", MarchingOrders.FaceTurnLength.Normal, "Length", "How fast or slow the event lasts"),
                        }
                    },
                });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_MarchingOrders;
    public class MarchingOrders : Minigame
    {
        //code is just copied from other minigame code, i will polish them later
        [Header("References")]
        public Animator Sarge;
        public Animator Steam;
        public Animator Cadet1;
        public Animator Cadet2;
        public Animator Cadet3;
        public Animator CadetPlayer;
        public Animator CadetHead1;
        public Animator CadetHead2;
        public Animator CadetHead3;
        public Animator CadetHeadPlayer;
        
        public GameObject Player;
        
        public GameEvent bop = new GameEvent();
        public GameEvent noBop = new GameEvent();
        public GameEvent marching = new GameEvent();
        
        private int marchOtherCount;
        private int marchPlayerCount;
        private int turnLength;
        
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
        
        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats >= bop.startBeat && cond.songPositionInBeats < bop.startBeat + bop.length)
                {
                    if (!(cond.songPositionInBeats >= noBop.startBeat && cond.songPositionInBeats < noBop.startBeat + noBop.length))
                        Cadet1.DoScaledAnimationAsync("Bop", 0.5f);
                        Cadet2.DoScaledAnimationAsync("Bop", 0.5f);
                        Cadet3.DoScaledAnimationAsync("Bop", 0.5f);
                        CadetPlayer.DoScaledAnimationAsync("Bop", 0.5f);
                }
            }
            
            if (cond.ReportBeat(ref marching.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats >= marching.startBeat && cond.songPositionInBeats < marching.startBeat + marching.length)
                {
                    marchOtherCount += 1;
                    var marchOtherAnim = (marchOtherCount % 2 != 0 ? "MarchR" : "MarchL");

                    Jukebox.PlayOneShotGame("marchingOrders/step1");
                    Cadet1.DoScaledAnimationAsync(marchOtherAnim, 0.5f);
                    Cadet2.DoScaledAnimationAsync(marchOtherAnim, 0.5f);
                    Cadet3.DoScaledAnimationAsync(marchOtherAnim, 0.5f);
                }
            }
            
            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {
            Jukebox.PlayOneShot("miss");
            Sarge.DoScaledAnimationAsync("Anger", 0.5f);
            Steam.DoScaledAnimationAsync("Steam", 0.5f);
            
            marchPlayerCount += 1;
                    var marchPlayerAnim = (marchPlayerCount % 2 != 0 ? "MarchR" : "MarchL");

                    Jukebox.PlayOneShotGame("marchingOrders/step1");
                    CadetPlayer.DoScaledAnimationAsync(marchPlayerAnim, 0.5f);
            }
        }
        
        public void Bop(float beat, float length)
        {
            bop.length = length;
            bop.startBeat = beat;
        }
        
        public void CadetsMarch(float beat, float length)
        {
            marching.length = length;
            marching.startBeat = beat;
        }
        
        public void SargeAttention(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/attention1", beat),
            new MultiSound.Sound("marchingOrders/attention2", beat + 0.25f),
            new MultiSound.Sound("marchingOrders/attention3", beat + 0.75f),
            }, forcePlay:true);
            
            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat + 0.25f,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                });
        }
        
        public void SargeMarch(float beat)
        {
            marchOtherCount = 0;
            marchPlayerCount = 0;
            
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/march1", beat),
            new MultiSound.Sound("marchingOrders/march2", beat + 1f),
            }, forcePlay:true);
            
            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("MarchL", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { CadetPlayer.DoScaledAnimationAsync("MarchL", 0.5f);}),
                });
        }
        
        public void SargeHalt(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/halt1", beat),
            new MultiSound.Sound("marchingOrders/halt2", beat + 1f),
            new MultiSound.Sound("marchingOrders/step1", beat + 1f),
            }, forcePlay:true);
            
            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet1.DoScaledAnimationAsync("Halt", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet2.DoScaledAnimationAsync("Halt", 0.5f);}),
                new BeatAction.Action(beat + 1f,     delegate { Cadet3.DoScaledAnimationAsync("Halt", 0.5f);}),
                });
        }
        
        public void SargeFaceTurn(float beat, int type, int type2)
        {
            switch (type2)
            {
                case (int) MarchingOrders.FaceTurnLength.Fast:
                    turnLength = 0;
                    break;
                default:
                    turnLength = 1;
                    break;
            }
            
            
            switch (type)
            {
                case (int) MarchingOrders.DirectionFaceTurn.Left:
                    //ScheduleInput(beat, turnLength + 2f, InputType.DIRECTION_RIGHT_DOWN, LeftSuccess, LeftMiss, LeftEmpty);
                    MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("marchingOrders/leftFaceTurn1", beat),
                    new MultiSound.Sound("marchingOrders/leftFaceTurn2", beat + 0.5f),
                    new MultiSound.Sound("marchingOrders/leftFaceTurn3", beat + turnLength + 1f),
                    new MultiSound.Sound("marchingOrders/leftFaceTurn4", beat + turnLength + 2f),
                    }, forcePlay:true);
                    
                        BeatAction.New(Player, new List<BeatAction.Action>() 
                            {
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead1.DoScaledAnimationAsync("FaceL", 0.5f);}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead2.DoScaledAnimationAsync("FaceL", 0.5f);}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead3.DoScaledAnimationAsync("FaceL", 0.5f);}),
                            });
                    break;
                default:
                    MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("marchingOrders/rightFaceTurn1", beat),
                    new MultiSound.Sound("marchingOrders/rightFaceTurn2", beat + 0.5f),
                    new MultiSound.Sound("marchingOrders/rightFaceTurn3", beat + turnLength + 1f),
                    new MultiSound.Sound("marchingOrders/rightFaceTurn4", beat + turnLength + 2f),
                    }, forcePlay:true);
                    
                        BeatAction.New(Player, new List<BeatAction.Action>() 
                            {
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead1.DoScaledAnimationAsync("FaceR", 0.5f);}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead2.DoScaledAnimationAsync("FaceR", 0.5f);}),
                            new BeatAction.Action(beat + turnLength + 2f,     delegate { CadetHead3.DoScaledAnimationAsync("FaceR", 0.5f);}),
                            });
                    break;
            }
            
            BeatAction.New(Player, new List<BeatAction.Action>() 
                {
                new BeatAction.Action(beat,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                new BeatAction.Action(beat + turnLength + 1f,     delegate { Sarge.DoScaledAnimationAsync("Talk", 0.5f);}),
                });
        }
        
        
        public static void AttentionSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/attention1", beat),
            new MultiSound.Sound("marchingOrders/attention2", beat + 0.25f),
            new MultiSound.Sound("marchingOrders/attention3", beat + 0.75f),
            }, forcePlay:true);
        }
        
        public static void MarchSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
            new MultiSound.Sound("marchingOrders/march1", beat),
            new MultiSound.Sound("marchingOrders/march2", beat + 1f),
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

