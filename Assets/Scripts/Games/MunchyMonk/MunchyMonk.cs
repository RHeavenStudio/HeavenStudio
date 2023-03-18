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
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("threeColor", new Color(0.34f, 0.77f, 0.36f, 1), "Color", "Change the color of the dumplings")
                    },
                    preFunctionLength = 0,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue(e.beat, e["threeColor"]); 
                    }
                },
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
                /*
                new GameAction("ScrollBackground", "Scroll Background")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.ScrollBG(e.beat, true); 
                    },
                    defaultLength = 0.5f,
                },
                */

                // hidden in the editor cuz drama and also yuck
                new GameAction("OneStretchable", "One (Stretchable)")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        //MunchyMonk.instance.OneGoCueStretchable(e.beat, e["oneColor"], e.length);
                    },
                    defaultLength = 4f,
                    resizable = true,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("oneColor", new Color(1, 1, 1, 1), "Color", "Change the color of the dumpling")
                    },
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
        public bool needBlush = false;
        public bool isStaring = false;
        private int inputsTilGrow;
        private bool forceGrow;
        private bool disableBaby;
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
            if (needBlush 
                && !MonkAnim.IsPlayingAnimationName("Eat")
                && !MonkAnim.IsPlayingAnimationName("Stare")
                && !isStaring) 
            {
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
            }
            
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"slap");
                isStaring = false;
            }

            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                MonkAnim.DoScaledAnimationAsync("Idle", 0.5f);
            }

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
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) 
                && !MonkAnim.IsPlayingAnimationName("Eat")
                && !MonkAnim.IsPlayingAnimationName("Blush")
                && !MonkAnim.IsPlayingAnimationName("Miss")
                && !MonkAnim.IsPlayingAnimationName("Sad")
                && !MonkAnim.IsPlayingAnimationName("Stare")
                && monkBop
                && !isStaring)
            {
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
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
            DumplingSprite.color = oneColor;
            DumplingSmear.color = oneColor;

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat     , delegate { OneGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { OneGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
            
            Dumpling DumplingClone = Instantiate(DumplingObj).GetComponent<Dumpling>();
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
                    TwoDumplingSprite1.color = twoColor;
                    TwoDumplingSprite2.color = twoColor;
                    TwoDumplingSmear1.color = twoColor;
                    TwoDumplingSmear2.color = twoColor;
                    
                    // first dumpling
                    Dumpling DumplingClone1 = Instantiate(TwoDumplingObj1).GetComponent<Dumpling>(); 
                    DumplingClone1.startBeat = beat-0.5f;
                    DumplingClone1.type = 2f;
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(TwoDumplingObj2).GetComponent<Dumpling>(); 
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
            DumplingSprite.color = threeColor;
            DumplingSmear.color = threeColor;
            
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                // first dumpling
                new BeatAction.Action(beat, delegate { 
                    Dumpling DumplingClone1 = Instantiate(DumplingObj).GetComponent<Dumpling>(); 
                    DumplingClone1.startBeat = beat;
                    DumplingClone1.type = 3f;

                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+0.5f, delegate { 
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                // second dumpling
                new BeatAction.Action(beat+1.25f, delegate { 
                    Dumpling DumplingClone2 = Instantiate(DumplingObj).GetComponent<Dumpling>(); 
                    DumplingClone2.startBeat = beat+1.25f;
                    DumplingClone2.type = 3.5f;
                    
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); }),
                new BeatAction.Action(beat+1.75f, delegate { 
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
                // third dumpling
                new BeatAction.Action(beat+2.25f, delegate { 
                    Dumpling DumplingClone3 = Instantiate(DumplingObj).GetComponent<Dumpling>(); 
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
                isStaring = true;
                break;
                case 2:
                MonkAnim.DoScaledAnimationAsync("Stare", 0.5f);
                isStaring = true;
                break;
            }
            
            if (whichAnim == 0) {
                MonkAnim.DoScaledAnimationAsync("Stare", 0.5f);
                isStaring = true;
            } else if (whichAnim == 1) {
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
            } else if (whichAnim == 2) {
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public void Modifiers(float beat, int inputsTilGrow, bool forceGrow, bool disableBaby)
        {
            instance.inputsTilGrow = inputsTilGrow;
            instance.forceGrow = forceGrow;
            instance.disableBaby = disableBaby;

            if (disableBaby) Baby.SetActive(false);
        }
    }
}