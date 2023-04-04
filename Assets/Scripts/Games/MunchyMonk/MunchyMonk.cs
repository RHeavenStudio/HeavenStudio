using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        MunchyMonk.instance.Bop(e.beat, e["bop"], e["autoBop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Monk Bops?", "Does the monk bop?"),
                        new Param("autoBop", false, "Monk Bops? (Auto)", "Does the monk auto bop?"),
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
                        new Param("instant", false, "Instant?", "Instantly move to the middle or to the right"),
                        new Param("whichSide", MunchyMonk.WhichSide.Right, "Starting Side", "Start on the right or the left"),
                    },
                },
                new GameAction("One", "One")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("oneColor", new Color(1, 1, 1, 1), "Color", "Change the color of the dumpling")
                    },
                    preFunctionLength = 0,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreOneGoCue(e.beat, e["oneColor"]); 
                    },
                },
                new GameAction("TwoTwo", "Two Two")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("twoColor1", new Color(1, 0.51f, 0.45f, 1), "1st Dumpling Color", "Change the color of the first dumpling"),
                        new Param("twoColor2", new Color(1, 0.51f, 0.45f, 1), "2nd Dumpling Color", "Change the color of the second dumpling"),
                    },
                    preFunctionLength = 0.5f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreTwoTwoCue(e.beat, e["twoColor1"], e["twoColor2"]);
                    },
                },
                new GameAction("Three", "Three")
                {
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("threeColor1", new Color(0.34f, 0.77f, 0.36f, 1), "1st Dumpling Color", "Change the color of the first dumpling"),
                        new Param("threeColor2", new Color(0.34f, 0.77f, 0.36f, 1), "2nd Dumpling Color", "Change the color of the second dumpling"),
                        new Param("threeColor3", new Color(0.34f, 0.77f, 0.36f, 1), "3rd Dumpling Color", "Change the color of the third dumpling"),
                    },
                    preFunctionLength = 0,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue(e.beat, e["threeColor1"], e["threeColor2"], e["threeColor3"]); 
                    },
                },
                new GameAction("Modifiers", "Modifiers")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.Modifiers(e.beat, e["inputsTil"], e["resetLevel"], e["setLevel"], e["disableBaby"], e["shouldBlush"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("inputsTil", new EntityTypes.Integer(0, 50, 10), "How Many 'til Growth?", "How many dumplings are needed to grow the stache?"),
                        new Param("resetLevel", false, "Remove Hair", "Instantly remove all hair"),
                        new Param("setLevel", new EntityTypes.Integer(0, 4, 0), "Set Growth Level", "Instantly grow hair"),
                        new Param("disableBaby", false, "Disable Baby?", "Make baby active or not"),
                        new Param("shouldBlush", true, "Should Monk Blush?", "Makes the Monk blush or not after eating"),
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
        static List<QueuedDumpling> queuedOnes = new List<QueuedDumpling>();
        static List<QueuedDumpling> queuedTwoTwos = new List<QueuedDumpling>();
        static List<QueuedDumpling> queuedThrees = new List<QueuedDumpling>();
        struct QueuedDumpling
        {
            public float beat;
            public Color color1;
            public Color color2;
            public Color color3;
        }

        public List<Dumplings> dumplings = new List<Dumplings>();
        public struct Dumplings
        {
            public Dumpling dumpling;
            public int dumplingListIterate;
            public float dumplingListBeat;
        }

        public enum WhichMonkAnim
        {
            Stare,
            Blush,
        }

        public enum WhichSide
        {
            Right,
            Left,
        }
        
        [Header("Objects")]
        [SerializeField] GameObject Baby;
        [SerializeField] GameObject BrowHolder;
        [SerializeField] GameObject StacheHolder;
        [SerializeField] GameObject DumplingObj;
        [SerializeField] SpriteRenderer DumplingSmear;
        [SerializeField] Transform MonkHolderTrans;
        [SerializeField] Transform MMParent;

        [Header("Animators")]
        [SerializeField] Animator OneGiverAnim;
        [SerializeField] Animator TwoGiverAnim;
        [SerializeField] Animator ThreeGiverAnim;
        [SerializeField] Animator BrowAnim;
        [SerializeField] Animator StacheAnim;
        [SerializeField] Animator MonkHolderAnim;
        public Animator MonkAnim;
        public Animator MonkArmsAnim;

        [Header("Variables")]
        public float lastReportedBeat = 0f;
        public bool needBlush;
        public bool isStaring;
        public bool forceGrow;
        public int growLevel = 0;
        public int howManyGulps;
        public int inputsTilGrow = 10;
        public int dumplingIterate;
        private bool monkBop = true;
        private bool noBlush;
        private bool disableBaby;
        float scrollModifier = 0f;
        const string sfxName = "munchyMonk/";

        public static MunchyMonk instance;
        
        private void Awake()
        {
            instance = this;
            Baby.SetActive(!disableBaby);
        }

        private void Update() 
        {
            // input stuff
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN) && (dumplings.Count == 0))
            {
                MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"slap");
                isStaring = false;
            } else {

            }

            // blushes when done eating but not when staring
            if (needBlush 
                && !MonkAnim.IsPlayingAnimationName("Eat")
                && !MonkAnim.IsPlayingAnimationName("Stare")
                && !MonkAnim.IsPlayingAnimationName("Barely")
                && !MonkAnim.IsPlayingAnimationName("Miss")
                && !isStaring
                && !noBlush) 
            {
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
            }

            // sets hair stuff active when it needs to be
            if (growLevel == 4) BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(lastReportedBeat + 0.5f, delegate { 
                    BrowHolder.SetActive(true); 
                    BrowAnim.Play("Idle", 0, 0); }),
            });
            if (growLevel > 0) BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(lastReportedBeat + 0.5f, delegate { 
                    StacheHolder.SetActive(true); 
                    BrowAnim.Play("Idle"+growLevel, 0, 0); }),
            });

            // resets the monk when game is paused
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                MonkAnim.DoScaledAnimationAsync("Idle", 0.5f);
            }

            // temporary scrolling stuff (?)
            //if (needScroll) {
            //    Tile += new Vector2(2 * Time.deltaTime, 0);
            //    NormalizedX += 0.5f * Time.deltaTime;
            //}

            // cue queuing stuff
            if (queuedOnes.Count > 0) {
                foreach (var dumpling in queuedOnes) { OneGoCue(dumpling.beat, dumpling.color1); }
                queuedOnes.Clear();
            }

            if (queuedTwoTwos.Count > 0) {
                foreach (var dumpling in queuedTwoTwos) { TwoTwoCue(dumpling.beat, dumpling.color1, dumpling.color2); }
                queuedTwoTwos.Clear();
            }

            if (queuedThrees.Count > 0) {
                foreach (var dumpling in queuedThrees) { ThreeGoCue(dumpling.beat, dumpling.color1, dumpling.color2, dumpling.color3); }
                queuedThrees.Clear();
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)) {
                if ((MonkAnim.IsAnimationNotPlaying() || MonkAnim.IsPlayingAnimationName("Bop") || MonkAnim.IsPlayingAnimationName("Idle"))
                && monkBop
                && !isStaring) 
                {
                    MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
                }
                if (growLevel == 4) BrowAnim.DoScaledAnimationAsync("Bop", 0.5f);
                if (growLevel > 0) StacheAnim.DoScaledAnimationAsync("Bop"+growLevel, 0.5f);
            }
        }

        public void Bop(float beat, bool bop, bool autoBop)
        {
            monkBop = autoBop;
            if (bop) {
                needBlush = false;
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
                if (growLevel == 4) BrowAnim.DoScaledAnimationAsync("Bop", 0.5f);
                StacheAnim.DoScaledAnimationAsync("Bop"+growLevel, 0.5f);
            }
        }

        public void InputFunctions(int whichVar, float state = 0)
        {
            List<float> dumplingBeats = new List<float>();

            foreach (var dumplingList in dumplings) {
                dumplingBeats.Add(dumplingList.dumplingListBeat);
            }

            float max = dumplingBeats.Max();

            foreach (var item in dumplings)
            {
                if (max == item.dumplingListBeat) {
                    if (!item.dumpling.canDestroy) {
                        if (whichVar == 1) {
                            item.dumpling.HitFunction(state);
                        } else {
                            item.dumpling.MissFunction();
                        }
                        dumplings.RemoveAt(dumplings.Count-1);
                        break;
                    }
                }
            }
        }

        public void Hit(PlayerActionEvent caller, float state)
        {
            InputFunctions(1, state);
        }

        public void Miss(PlayerActionEvent caller)
        {
            InputFunctions(2);
        }

        public void Early(PlayerActionEvent caller) { }

        public static void PreOneGoCue(float beat, Color firstColor)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"one_1", beat),
                    new MultiSound.Sound(sfxName+"one_2", beat + 1f),
            }, forcePlay: true);

            queuedOnes.Add(new QueuedDumpling() 
                { beat = beat, color1 = firstColor, });
        }

        public void OneGoCue(float beat, Color firstColor)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { 
                    OneGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f);
                    // dumpling
                    Dumpling DumplingClone = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>();
                    DumplingClone.dumplingColor = firstColor;
                    DumplingClone.startBeat = beat;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone, dumplingListIterate = dumplingIterate, dumplingListBeat = beat, });
                    dumplingIterate++;
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early); 
                }),
                new BeatAction.Action(beat+0.5f, delegate { 
                    OneGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); 
                }),
            });
        }

        public static void PreTwoTwoCue(float beat, Color firstColor, Color secondColor)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound(sfxName+"two_1", beat - 0.5f),
                new MultiSound.Sound(sfxName+"two_2", beat), 
                new MultiSound.Sound(sfxName+"two_3", beat + 1f),
                new MultiSound.Sound(sfxName+"two_4", beat + 1.5f),
            }, forcePlay: true);
            
            queuedTwoTwos.Add(new QueuedDumpling() { 
                beat = beat,
                color1 = firstColor,
                color2 = secondColor,
            });
        }

        public void TwoTwoCue(float beat, Color firstColor, Color secondColor)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat-0.5f, delegate { 
                    TwoGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); 
                    // first dumpling
                    Dumpling DumplingClone1 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>();
                    DumplingClone1.dumplingColor = firstColor;
                    DumplingClone1.dumplingID = dumplingIterate;
                    DumplingClone1.startBeat = beat-0.5f;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone1, dumplingListIterate = dumplingIterate, dumplingListBeat = beat-0.5f, });
                    dumplingIterate++;
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early);
                    //DumplingClone1.otherAnim = DumplingClone2.gameObject.GetComponent<Animator>();
                }),
                new BeatAction.Action(beat, delegate { 
                    TwoGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f);
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>();
                    DumplingClone2.dumplingColor = secondColor;
                    DumplingClone2.dumplingID = dumplingIterate;
                    DumplingClone2.startBeat = beat-0.5f;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone2, dumplingListIterate = dumplingIterate, dumplingListBeat = beat, });
                    dumplingIterate++;
                    ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, Hit, Miss, Early);
                }),
            });
        }

        public static void PreThreeGoCue(float beat, Color firstColor, Color secondColor, Color thirdColor)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxName+"three_1", beat),
                    new MultiSound.Sound(sfxName+"three_2", beat + 1f),
                    new MultiSound.Sound(sfxName+"three_3", beat + 2f),
                    new MultiSound.Sound(sfxName+"three_4", beat + 3f),
                }, forcePlay: true);
            
            queuedThrees.Add(new QueuedDumpling() { 
                beat = beat,
                color1 = firstColor,
                color2 = secondColor,
                color3 = thirdColor,
            });
        }

        public void ThreeGoCue(float beat, Color firstColor, Color secondColor, Color thirdColor)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { 
                    // first in
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); 
                    // first dumpling
                    Dumpling DumplingClone1 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>();
                    DumplingClone1.dumplingColor = firstColor;
                    DumplingClone1.startBeat = beat;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone1, dumplingListIterate = dumplingIterate, dumplingListBeat = beat, });
                    dumplingIterate++;
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+0.5f, delegate { 
                    // first out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),

                new BeatAction.Action(beat+1.25f, delegate { 
                    // second in
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); 
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone2.dumplingColor = secondColor;
                    DumplingClone2.startBeat = beat+1.25f;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone2, dumplingListIterate = dumplingIterate, dumplingListBeat = beat+1.25f, });
                    dumplingIterate++;
                    ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+1.75f, delegate { 
                    // second out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),

                new BeatAction.Action(beat+2.25f, delegate { 
                    // third in
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f);
                    // third dumpling
                    Dumpling DumplingClone3 = Instantiate(DumplingObj, MMParent).GetComponent<Dumpling>(); 
                    DumplingClone3.dumplingColor = thirdColor;
                    DumplingClone3.startBeat = beat+2.25f;
                    dumplings.Add(new Dumplings() 
                        { dumpling = DumplingClone3, dumplingListIterate = dumplingIterate, dumplingListBeat = beat+2.25f, });
                    dumplingIterate++;
                    ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+2.75f, delegate {
                    // third out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
        }

        public void PlayMonkAnim(float beat, int whichAnim)
        {
            string anim = "";
            switch (whichAnim)
            {
                case 0:
                anim = "Stare";
                isStaring = true;
                break;
                case 1:
                anim = "Blush";
                needBlush = false;
                break;
                case 2:
                anim = "Bop";
                break;
            }

            MonkAnim.DoScaledAnimationAsync(anim, 0.5f);
        }

        public void MonkMove(float beat, float length, bool isInstant, int whichSide)
        {
            string whichAnim = isInstant ? "Idle" : "Go";
            whichAnim += (whichSide == 0 ? "Left" : "Right");

            MonkHolderAnim.DoScaledAnimationAsync(whichAnim, !isInstant ? length : 0.5f);
        }

        public void Modifiers(float beat, int inputsTilGrow, bool resetLevel, int setLevel, bool disableBaby, bool shouldBlush)
        {
            instance.noBlush = !shouldBlush;
            instance.inputsTilGrow = inputsTilGrow;
            instance.disableBaby = disableBaby;

            if (setLevel != 0) {
                growLevel = setLevel;
            }
            if (resetLevel) growLevel = 0;

            Baby.SetActive(!disableBaby);
        }

        public void ScrollBG(float beat, bool isInstant)
        {

        }
    }
}