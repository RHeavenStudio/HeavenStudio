using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class ntrMunchyMonkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("munchyMonk", "Munchy Monk", "b9fffc", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.Bop(e.beat, e["monkBop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("monkBop", false, "Bop", "Does the Monk bop?"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("MonkMove", "Monk Move")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.MonkMove(e.beat, e.length, e["instant"], e["whichSide"]); 
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "", "Instantly move to the middle or to the right"),
                    }
                },
                new GameAction("One", "One")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreOneGoCue(e.beat, e["oneColor"]); 
                    },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("oneColor", new Color(1, 1, 1, 1), "Color", "Change the color of the dumpling")
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreOneGoCue(e.beat, e["oneColor"]); 
                    }
                },
                new GameAction("TwoTwo", "Two Two")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("twoColor", new Color(1, 0.51f, 0.45f, 1), "Color", "Change the color of the dumplings")
                    },
                    preFunctionLength = 0.5f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreTwoTwoCue(e.beat, e["twoColor"]);
                    },
                },
                new GameAction("Three", "Three")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue(e.beat, e["threeColor"]); 
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("threeColor", new Color(0.34f, 0.77f, 0.36f, 1), "Color", "Change the color of the dumplings")
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue(e.beat, e["threeColor"]); 
                    }
                },
                new GameAction("Modifiers", "Modifiers")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.Modifiers(e.beat, e["inputsTil"], e["forceGrow"], e["disableBaby"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("inputsTil", new EntityTypes.Integer(0, 50, 10), "How Many 'til Growth?", "How many dumplings are needed to grow the stache?"),
                        new Param("forceGrow", false, "Next Will Grow?", "Will the next input increment stache growth?"),
                        new Param("disableBaby", false, "Disable Baby?", "Make baby active or not"),
                    },
                },
                new GameAction("MonkAnimation", "Monk Animations")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.PlayMonkAnim(e.beat, e["whichAnim"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("whichAnim", MunchyMonk.WhichMonkAnim.Stare, "Which Animation", "Which animation will the Monk play?"),
                    }
                },
                // note: make the bg not scroll by default
                new GameAction("ScrollBackground", "Scroll Background")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.ScrollBG(e.beat, e["instant"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant?", "Will the scrolling happen immediately?"),
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MunchyMonk;
    public class MunchyMonk : Minigame
    {
        static List<QueuedOne> queuedOnes = new List<QueuedOne>();
        struct QueuedOne
        {
            public float beat;
            public Color color;
        }
        
        static List<QueuedTwoTwo> queuedTwoTwos = new List<QueuedTwoTwo>();
        struct QueuedTwoTwo
        {
            public float beat;
            public Color color;
        }

        static List<QueuedThree> queuedThrees = new List<QueuedThree>();
        struct QueuedThree
        {
            public float beat;
            public Color color;
        }

        public enum WhichMonkAnim
        {
            Stare,
            Blush,
            Bop,
        }
        
        [Header("Objects")]
        [SerializeField] GameObject Baby;
        [SerializeField] GameObject BrowHolder;
        [SerializeField] GameObject StacheHolder;
        [SerializeField] GameObject DumplingObj;
        [SerializeField] GameObject TwoDumplingObj1;
        [SerializeField] GameObject TwoDumplingObj2;
        [SerializeField] SpriteRenderer DumplingSprite;
        [SerializeField] SpriteRenderer TwoDumplingSprite1;
        [SerializeField] SpriteRenderer TwoDumplingSprite2;
        [SerializeField] SpriteRenderer DumplingSmear;
        [SerializeField] SpriteRenderer TwoDumplingSmear1;
        [SerializeField] SpriteRenderer TwoDumplingSmear2;
        [SerializeField] Transform MMParent;

        [Header("Animators")]
        [SerializeField] Animator OneGiverAnim;
        [SerializeField] Animator TwoGiverAnim;
        [SerializeField] Animator ThreeGiverAnim;
        [SerializeField] Animator BrowAnim;
        [SerializeField] Animator StacheAnim;
        public Animator MonkAnim;
        public Animator MonkArmsAnim;
        public Animator DumplingAnim;
        public Animator TwoDumpling1Anim;
        public Animator TwoDumpling2Anim;
        public Animator SmearAnim;

        [Header("Variables")]
        public float lastReportedBeat = 0f;
        public bool monkBop = true;
        public bool needBlush;
        public bool isStaring;
        public bool firstTwoMissed;
        public bool forceGrow;
        public int growLevel = 0;
        private bool disableBaby;
        private int inputsTilGrow;
        float scrollModifier = 0f;
        const string sfxName = "munchyMonk/";

        public static MunchyMonk instance;
        
        private void Awake()
        {
            instance = this;
            if (disableBaby) Baby.SetActive(false);
        }

        private void Update() 
        {
            // input stuff
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"slap");
                isStaring = false;
            }

            // blushes when done eating but not when staring
            if (needBlush 
                && !MonkAnim.IsPlayingAnimationName("Eat")
                && !MonkAnim.IsPlayingAnimationName("Stare")
                && !MonkAnim.IsPlayingAnimationName("Barely")
                && !MonkAnim.IsPlayingAnimationName("Miss")
                && !isStaring) 
            {
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
            }

            // sets hair stuff active when it needs to be
            if (growLevel == 4) BrowHolder.SetActive(true);
            if (growLevel > 0) StacheHolder.SetActive(true);

            // resets the monk when game is paused
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                MonkAnim.DoScaledAnimationAsync("Idle", 0.5f);
            }

            // scrolling stuff
            //if (needScroll) {
            //    Tile += new Vector2(2 * Time.deltaTime, 0);
            //    NormalizedX += 0.5f * Time.deltaTime;
            //}

            // cue queuing stuff
            if (queuedOnes.Count > 0) {
                foreach (var dumpling in queuedOnes) { OneGoCue(dumpling.beat, dumpling.color); }
                queuedOnes.Clear();
            }

            if (queuedTwoTwos.Count > 0) {
                foreach (var dumpling in queuedTwoTwos) { TwoTwoCue(dumpling.beat, dumpling.color); }
                queuedTwoTwos.Clear();
            }

            if (queuedThrees.Count > 0) {
                foreach (var dumpling in queuedThrees) { ThreeGoCue(dumpling.beat, dumpling.color); }
                queuedThrees.Clear();
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)) {
                if ((MonkAnim.IsAnimationNotPlaying() || MonkAnim.IsPlayingAnimationName("Bop") || MonkAnim.IsPlayingAnimationName("Idle"))
                    && monkBop
                    && !isStaring){
                    MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
                }
                // commented this out cuz it makes a warning every beat but im not fixing it cuz i need to fix it on my munchy monk branch
                //if (BrowAnim.IsPlayingAnimationName("Bop") && growLevel == 4) BrowAnim.DoScaledAnimationAsync("Bop", 0.5f);
                //if (StacheAnim.IsPlayingAnimationName("Bop"+growLevel)) StacheAnim.DoScaledAnimationAsync("Bop"+growLevel, 0.5f);
            }
        }

        public void Bop(float beat, bool doesBop)
        {
            monkBop = doesBop;
        }

        public static void PreOneGoCue(float beat, Color oneColor)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"one_1", beat),
                    new MultiSound.Sound(sfxName+"one_2", beat + 1f),
            }, forcePlay: true);

            queuedOnes.Add(new QueuedOne() 
                { beat = beat, color = oneColor, });
        }

        public void OneGoCue(float beat, Color oneColor)
        {
            DumplingSprite.color =
            DumplingSmear.color = oneColor;

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat     , delegate { OneGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { OneGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
            
            Dumpling DumplingClone = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>();
            DumplingClone.startBeat = beat;
        }

        public static void PreTwoTwoCue(float beat, Color twoColor)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound(sfxName+"two_1", beat - 0.5f),
                new MultiSound.Sound(sfxName+"two_2", beat), 
                new MultiSound.Sound(sfxName+"two_3", beat + 1f),
                new MultiSound.Sound(sfxName+"two_4", beat + 1.5f),
            }, forcePlay: true);
            
            queuedTwoTwos.Add(new QueuedTwoTwo() 
                { beat = beat, color = twoColor, });
        }

        public void TwoTwoCue(float beat, Color twoColor)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat-0.5f, delegate { 
                    // lol
                    TwoDumplingSmear1.color =
                    TwoDumplingSmear2.color =
                    TwoDumplingSprite1.color =
                    TwoDumplingSprite2.color = twoColor;
                    
                    // first dumpling
                    Dumpling DumplingClone1 = Instantiate(TwoDumplingObj1, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone1.startBeat = beat-0.5f;
                    DumplingClone1.type = 2f;
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(TwoDumplingObj2, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone2.startBeat = beat-0.5f; 
                    DumplingClone2.type = 2.5f;
                    DumplingClone1.otherAnim = DumplingClone2.gameObject.GetComponent<Animator>();

                    TwoGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat, delegate { 
                    TwoGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
        }

        public static void PreThreeGoCue(float beat, Color threeColor)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"three_1", beat),
                    new MultiSound.Sound(sfxName+"three_2", beat + 1f),
                    new MultiSound.Sound(sfxName+"three_3", beat + 2f),
                    new MultiSound.Sound(sfxName+"three_4", beat + 3f),
                }, forcePlay: true);
            
            queuedThrees.Add(new QueuedThree() 
                { beat = beat, color = threeColor, });
        }

        public void ThreeGoCue(float beat, Color threeColor)
        {
            DumplingSprite.color =
            DumplingSmear.color = threeColor;
            
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                // first dumpling
                new BeatAction.Action(beat, delegate { 
                    Dumpling DumplingClone1 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone1.startBeat = beat;
                    DumplingClone1.type = 3f;

                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { 
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                // second dumpling
                new BeatAction.Action(beat+1.25f, delegate { 
                    Dumpling DumplingClone2 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone2.startBeat = beat+1.25f;
                    DumplingClone2.type = 3.5f;
                    
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+1.75f, delegate { 
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                // third dumpling
                new BeatAction.Action(beat+2.25f, delegate { 
                    Dumpling DumplingClone3 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone3.startBeat = beat+2.25f;
                    DumplingClone3.type = 4f;

                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+2.75f, delegate { 
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
        }

        public void PlayMonkAnim(float beat, int whichAnim)
        {
            switch (whichAnim)
            {
                case 0:
                MonkAnim.DoScaledAnimationAsync("Stare", 0.5f);
                isStaring = true;
                break;
                case 1:
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
                break;
                case 2:
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
                break;
            }
        }

        public void MonkMove(float beat, float length, bool isInstant, int whichSide)
        {
            if (isInstant) {
                
            } else {
                if (whichSide == 0) {
                    
                } else {

                }
            }
        }

        public void Modifiers(float beat, int inputsTilGrow, bool forceGrow, bool disableBaby)
        {
            instance.inputsTilGrow = inputsTilGrow;
            instance.forceGrow = forceGrow;
            instance.disableBaby = disableBaby;

            if (disableBaby) Baby.SetActive(false);
        }

        public void ScrollBG(float beat, bool isInstant)
        {

        }
    }
}