using HeavenStudio.Util;
using HeavenStudio.Common;
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
                        MunchyMonk.instance.Bop((float) e.beat, e["bop"], e["autoBop"]); 
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
                        MunchyMonk.instance.MonkMove((float) e.beat, e.length, e["goToSide"], e["ease"]); 
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("goToSide", MunchyMonk.WhichSide.Right, "Go to Which Side?", "Which side the Monk will move to"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the movement have?"),
                    },
                },
                new GameAction("One", "One")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreOneGoCue((float) e.beat, e["oneColor"]); 
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreOneGoCue((float) e.beat, e["oneColor"]); 
                    },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("oneColor", new Color(1, 1, 1, 1), "Color", "Change the color of the dumpling")
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
                        MunchyMonk.PreTwoTwoCue((float) e.beat, e["twoColor1"], e["twoColor2"]);
                    },
                },
                new GameAction("Three", "Three")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue((float) e.beat, e["threeColor1"], e["threeColor2"], e["threeColor3"]); 
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.PreThreeGoCue((float) e.beat, e["threeColor1"], e["threeColor2"], e["threeColor3"]); 
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("threeColor1", new Color(0.34f, 0.77f, 0.36f, 1), "1st Dumpling Color", "Change the color of the first dumpling"),
                        new Param("threeColor2", new Color(0.34f, 0.77f, 0.36f, 1), "2nd Dumpling Color", "Change the color of the second dumpling"),
                        new Param("threeColor3", new Color(0.34f, 0.77f, 0.36f, 1), "3rd Dumpling Color", "Change the color of the third dumpling"),
                    },
                },
                new GameAction("Modifiers", "Modifiers")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.Modifiers((float) e.beat, e["inputsTil"], e["resetLevel"], e["setLevel"], e["disableBaby"], e["shouldBlush"]); 
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
                        MunchyMonk.instance.PlayMonkAnim((float) e.beat, e["whichAnim"], e["vineBoom"]);
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.PlayMonkAnimInactive(e["vineBoom"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("whichAnim", MunchyMonk.WhichMonkAnim.Stare, "Which Animation", "Which animation will the Monk play?"),
                        new Param("vineBoom", false, "Vine Boom", "Just guess with this one."),
                    }
                },
                new GameAction("ScrollBackground", "Scroll Background")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MunchyMonk.instance.ScrollBG((float) e.beat, e.length, e["scrollSpeed"], e["ease"]); 
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("scrollSpeed", new EntityTypes.Float(0, 20, 5), "Scroll Speed", "What should the scroll speed be?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the scroll ramp up have?"),
                    }
                },
            },
            new List<string>() {"ntr", "normal"},
            "ntrshugyo", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MunchyMonk;
    public class MunchyMonk : Minigame
    {
        public List<Dumpling> dumplings = new List<Dumpling>();
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
        [SerializeField] Sprite[] dumplingSprites;
        public float lastReportedBeat = 0f;
        public bool needBlush;
        public bool isStaring;
        bool monkBop = true;

        // these variables are static so that they can be set outside of the game/stay the same between game switches
        static public int howManyGulps;
        static public int growLevel = 0;
        static public int inputsTilGrow = 10;
        static bool noBlush;
        static bool disableBaby;

        // the variables for scroll
        bool scrollRampUp;
        float scrollBeat;
        float scrollLength;
        float scrollMod;
        static float scrollModCurrent = 0;
        EasingFunction.Ease scrollEase;

        // the variables for the monk moving 
        bool isMoving;
        float movingStartBeat;
        float movingLength;
        string moveAnim;
        EasingFunction.Ease lastEase;
        ScrollObject[] scrollObjects;
        const string sfxName = "munchyMonk/";

        public static MunchyMonk instance;
        
        private void Awake()
        {
            instance = this;
            Baby.SetActive(!disableBaby);
        }

        private void Start() 
        {
            scrollObjects = FindObjectsByType<ScrollObject>(FindObjectsSortMode.None);
            foreach (var obj in scrollObjects) obj.SpeedMod = scrollModCurrent;
        }

        private void OnDestroy() 
        {
            // reset static variables
            if (queuedOnes.Count > 0) queuedOnes.Clear();
            if (queuedTwoTwos.Count > 0) queuedThrees.Clear();
            if (queuedThrees.Count > 0) queuedThrees.Clear();

            howManyGulps = 0;
            growLevel = 0;
            inputsTilGrow = 10;
            noBlush = false;
            disableBaby = false;

            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Update() 
        {
            // input stuff
            if (PlayerInput.Pressed(true) && (!IsExpectingInputNow(InputType.STANDARD_DOWN) || !IsExpectingInputNow(InputType.DIRECTION_DOWN))) {
                MonkArmsAnim.DoScaledAnimationAsync("WristSlap", 0.5f);
                SoundByte.PlayOneShotGame(sfxName+"slap");
                isStaring = false;
                // early input stuff
                if (dumplings.Count != 0) InputFunctions(3);
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
            if (growLevel > 0) {
                StacheHolder.SetActive(true);
                if (growLevel == 4) BrowHolder.SetActive(true);
            }

            // resets the monk when game is stopped
            if (!Conductor.instance.NotStopped()) MonkAnim.DoScaledAnimationAsync("Idle", 0.5f);

            if (isMoving) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                MonkHolderAnim.DoNormalizedAnimation(moveAnim, newPos);
                if (normalizedBeat >= 1f) isMoving = false;
            }

            if (scrollRampUp) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(scrollBeat, scrollLength);
                EasingFunction.Function func = EasingFunction.GetEasingFunction(scrollEase);
                float newPos = func(scrollModCurrent, scrollMod, normalizedBeat);
                if (normalizedBeat >= 1f) {
                    scrollRampUp = false;
                    scrollModCurrent = scrollMod;
                }
                
                foreach (var obj in scrollObjects) obj.SpeedMod = newPos;
            }

            // cue queuing stuff
            if (queuedOnes.Count > 0) {
                foreach (var dumpling in queuedOnes) OneGoCue(dumpling.beat, dumpling.color1);
                queuedOnes.Clear();
            }

            if (queuedTwoTwos.Count > 0) {
                foreach (var dumpling in queuedTwoTwos) TwoTwoCue(dumpling.beat, dumpling.color1, dumpling.color2);
                queuedTwoTwos.Clear();
            }

            if (queuedThrees.Count > 0) {
                foreach (var dumpling in queuedThrees) ThreeGoCue(dumpling.beat, dumpling.color1, dumpling.color2, dumpling.color3);
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

                if (!MonkAnim.IsPlayingAnimationName("Blush") || !MonkAnim.IsPlayingAnimationName("Stare")) {
                    if (growLevel == 4) BrowAnim.DoScaledAnimationAsync("Bop", 0.5f);
                    if (growLevel > 0) StacheAnim.DoScaledAnimationAsync($"Bop{growLevel}", 0.5f);
                }
            }
        }

        public void Bop(float beat, bool bop, bool autoBop)
        {
            monkBop = autoBop;
            if (bop) {
                needBlush = false;
                MonkAnim.DoScaledAnimationAsync("Bop", 0.5f);
                if (growLevel == 4) BrowAnim.DoScaledAnimationAsync("Bop", 0.5f);
                if (growLevel > 0) StacheAnim.DoScaledAnimationAsync($"Bop{growLevel}", 0.5f);
            }
        }

        public void InputFunctions(int whichVar, float state = 0)
        {
            switch (whichVar)
            {
                case 1:
                dumplings[dumplings.Count-1].HitFunction(state);
                break;
                case 2:
                dumplings[dumplings.Count-1].MissFunction();
                break;
                case 3:
                dumplings[dumplings.Count-1].EarlyFunction();
                break;
            }
            dumplings.RemoveAt(dumplings.Count-1);
        }

        public void Hit(PlayerActionEvent caller, float state)
        {
            if (dumplings.Count > 0) InputFunctions(1, state);
        }

        public void Miss(PlayerActionEvent caller)
        {
            if (dumplings.Count > 0) InputFunctions(2);
        }

        public void Early(PlayerActionEvent caller) { }

        public static void PreOneGoCue(float beat, Color firstColor)
        {
            PlaySoundSequence("munchyMonk", "one_go", beat);

            queuedOnes.Add(new QueuedDumpling() 
                { beat = beat, color1 = firstColor, });
        }

        public void OneGoCue(float beat, Color firstColor)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { 
                    OneGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f);
                    // dumpling
                    Dumpling DumplingClone = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>();
                    DumplingClone.dumplingColor = firstColor;
                    DumplingClone.startBeat = beat;
                    DumplingClone.sr.sprite = dumplingSprites[0];
                    dumplings.Add(DumplingClone);
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early); 
                }),
                new BeatAction.Action(beat+0.5f, delegate { 
                    OneGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); 
                }),
            });
        }

        public static void PreTwoTwoCue(float beat, Color firstColor, Color secondColor)
        {
            PlaySoundSequence("munchyMonk", "two_go", beat);

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
                    Dumpling DumplingClone1 = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>();
                    DumplingClone1.dumplingColor = firstColor;
                    DumplingClone1.startBeat = beat-0.5f;
                    DumplingClone1.sr.sprite = dumplingSprites[1];
                    dumplings.Add(DumplingClone1);
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early);
                    //DumplingClone1.otherAnim = DumplingClone2.gameObject.GetComponent<Animator>();
                }),
                new BeatAction.Action(beat, delegate { 
                    TwoGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f);
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>();
                    DumplingClone2.dumplingColor = secondColor;
                    DumplingClone2.startBeat = beat-0.5f;
                    DumplingClone2.sr.sprite = dumplingSprites[2];
                    dumplings.Add(DumplingClone2);
                    ScheduleInput(beat, 1.5f, InputType.STANDARD_DOWN, Hit, Miss, Early);
                }),
            });
        }

        public static void PreThreeGoCue(float beat, Color firstColor, Color secondColor, Color thirdColor)
        {
            PlaySoundSequence("munchyMonk", "three_go", beat);
            
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
                    Dumpling DumplingClone1 = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>();
                    DumplingClone1.dumplingColor = firstColor;
                    DumplingClone1.startBeat = beat;
                    DumplingClone1.sr.sprite = dumplingSprites[3];
                    dumplings.Add(DumplingClone1);
                    ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+0.5f, delegate { 
                    // first out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),

                new BeatAction.Action(beat+1.3f, delegate { 
                    // second in
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f); 
                    // second dumpling
                    Dumpling DumplingClone2 = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>(); 
                    DumplingClone2.dumplingColor = secondColor;
                    DumplingClone2.startBeat = beat+1.3f;
                    DumplingClone2.sr.sprite = dumplingSprites[4];
                    dumplings.Add(DumplingClone2);
                    ScheduleInput(beat, 2f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+1.75f, delegate { 
                    // second out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),

                new BeatAction.Action(beat+2.3f, delegate { 
                    // third in
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveIn", 0.5f);
                    // third dumpling
                    Dumpling DumplingClone3 = Instantiate(DumplingObj, gameObject.transform).GetComponent<Dumpling>(); 
                    DumplingClone3.dumplingColor = thirdColor;
                    DumplingClone3.startBeat = beat+2.3f;
                    DumplingClone3.sr.sprite = dumplingSprites[5];
                    dumplings.Add(DumplingClone3);
                    ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, Hit, Miss, Early); }),

                new BeatAction.Action(beat+2.75f, delegate {
                    // third out
                    ThreeGiverAnim.DoScaledAnimationAsync("GiveOut", 0.5f); }),
            });
        }

        public void PlayMonkAnim(float beat, int whichAnim, bool vineBoom)
        {
            switch (whichAnim)
            {
                case 1:
                MonkAnim.DoScaledAnimationAsync("Blush", 0.5f);
                needBlush = false;
                break;
                default:
                MonkAnim.DoScaledAnimationAsync("Stare", 0.5f);
                isStaring = true;
                break;
            }
            
            // it's in zeo's video; no reason not to include it :)
            if (vineBoom) SoundByte.PlayOneShotGame("fanClub/arisa_dab", forcePlay: true);
        }

        public void PlayMonkAnimInactive(bool vineBoom)
        {
            if (vineBoom) Jukebox.PlayOneShotGame("fanClub/arisa_dab", forcePlay: true);
        }

        public void MonkMove(float beat, float length, int goToSide, int ease)
        {
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = (goToSide == 0 ? "GoRight" : "GoLeft");
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
        }

        public static void Modifiers(float beat, int inputsTilGrow, bool resetLevel, int setLevel, bool disableBaby, bool shouldBlush)
        {
            if (MunchyMonk.inputsTilGrow != inputsTilGrow) MunchyMonk.howManyGulps = inputsTilGrow * MunchyMonk.growLevel;
            if (setLevel != 0) MunchyMonk.growLevel = setLevel;
            if (resetLevel) {
                MunchyMonk.growLevel = 0;
                MunchyMonk.howManyGulps = 0;
            }
            

            MunchyMonk.noBlush = !shouldBlush;
            MunchyMonk.inputsTilGrow = inputsTilGrow;
            MunchyMonk.disableBaby = disableBaby;

            if (GameManager.instance.currentGame == "munchyMonk") 
                MunchyMonk.instance.Baby.SetActive(!disableBaby);
        }

        public void ScrollBG(float beat, float length, float scrollSpeed, int ease)
        {
            scrollBeat = beat;
            scrollLength = length;
            scrollMod = scrollSpeed;
            scrollRampUp = true;
            scrollEase = (EasingFunction.Ease)ease;
        }
    }
}