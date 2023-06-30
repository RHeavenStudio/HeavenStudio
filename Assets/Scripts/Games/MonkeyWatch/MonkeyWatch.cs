using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMonkeyWatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("monkeyWatch", "Monkey Watch", "f0338d", false, false, new List<GameAction>()
            {
                new GameAction("monkeysAppear", "Monkeys Appear")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.instance.MonkeysAppear(e.beat, e.length);
                    },
                    defaultLength = 2f,
                    resizable = true
                },
                new GameAction("startClapping", "Start Clapping")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MonkeyWatch.instance.Clapping(e.beat);
                    },
                    defaultLength = 2f,
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.ClappingInactive(e.beat);
                    },
                },
                new GameAction("offbeatMonkeys", "Offbeat Monkeys")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cue", "Mute the offbeat monkeys's cue"),
                        new Param("custom", false, "Custom Cue", "Place the \"Custom Monkey\" block 2 beats after the start of this one to create a custom pink monkey cue."),
                    },
                    resizable = true,
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.WarnPinkMonkeys(e.beat, e.length, e["mute"]);
                    },
                },
                new GameAction("customMonkey", "Custom Monkey")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("sfx", MonkeyWatch.SfxTypes.First, "Which SFX", "Choose between the first and second \"ki\" sfx")
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.WarnPinkMonkeys(e.beat, e.length, e["mute"]);
                    },
                },
                new GameAction("monkeyModifiers", "Monkey Modifiers")
                {
                    //preFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.OnbeatSwitch(e.beat); },
                    defaultLength = 2f
                },
                new GameAction("balloonMonkey", "Balloon Monkey")
                {
                    //preFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.OnbeatSwitch(e.beat); },
                    defaultLength = 2f
                },
            }/*,
            new List<string>() {"rvl", "keep"},
            "rvlwatch", "en",
            new List<string>() {} */
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MonkeyWatch;
    public class MonkeyWatch : Minigame
    {
        [Header("Animators")]
        [SerializeField] MonkeyClicker monkeyPlayer;

        [Header("Objects")]
        [SerializeField] Transform watchHoleParent;
        [SerializeField] GameObject yellowMonkey;
        [SerializeField] GameObject pinkMonkey;

        // unserialized variables below
        public enum SfxTypes 
        {
            None,
            First,
            Second,
            Onbeat,
        }

        static List<double> queuedInputs = new();
        Dictionary<double, OffbeatMonkey> offbeatMonkeys = new Dictionary<double, OffbeatMonkey>();
        List<RiqEntity> customMonkeys = new List<RiqEntity>();
        List<GameObject> watchHoles = new List<GameObject>();

        public struct OffbeatMonkey
        {
            public double beat;
            public bool mute;
            public float length;
        }

        public struct CustomMonkey
        {
            public double beat;
            public int sfx;
        }

        static OffbeatMonkey wantOffbeat;
        static double wantClapping = double.MinValue;
        public static bool startedClapping;
        int lastMonkeyClapped;

        public static MonkeyWatch instance;

        void Awake()
        {
            instance = this;
            customMonkeys = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "customMonkey" });
            var offbeatEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offbeatMonkeys" });
            for (int i = 0; i < offbeatEvents.Count; i++)
            {
                if (offbeatEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    offbeatMonkeys.Add(
                        Mathf.Round((float)offbeatEvents[i].beat), 
                        new OffbeatMonkey{
                            mute = offbeatEvents[i]["mute"],
                            length = offbeatEvents[i].length,
                        }
                    );
                }
            }

            // put all the watch holes in a list
            for (int i = 0; i < watchHoleParent.childCount; i++) {
                watchHoles.Add(watchHoleParent.GetChild(i).gameObject);
            }
        }

        private void Start() 
        {
            GameCamera.additionalPosition = new Vector3(0, 19.548f, 0);
        }

        public override void OnGameSwitch(double beat)
        {
            if (wantOffbeat.length != 0)
            {
                double offBeat = wantOffbeat.beat;
                PinkMonkeys(offBeat, wantOffbeat.length, wantOffbeat.mute, customMonkeys.FindAll(x => x.beat >= offBeat + 2 && x.beat < offBeat + wantOffbeat.length));
                wantOffbeat = new OffbeatMonkey{
                    beat = 0,
                    length = 0,
                    mute = false,
                };
            }
            if (wantClapping != double.MinValue) {
                Clapping(wantClapping);
                wantClapping = double.MinValue;
            }
        }

        void OnDestroy()
        {
            if (!Conductor.instance.NotStopped()) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
                if (offbeatMonkeys.Count > 0) offbeatMonkeys.Clear();
                startedClapping = false;
            }
            
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public void Update()
        {
            
        }

        public void MonkeysAppear(double beat, float length)
        {

        }

        public static void ClappingInactive(double beat)
        {
            if (wantClapping == double.MinValue) return;
            wantClapping = beat;
            startedClapping = true;
        }

        public void Clapping(double beat, double inputCheck = double.MinValue)
        {
            if (offbeatMonkeys.ContainsKey(beat)) {
                var monkeys = customMonkeys.FindAll(x => x.beat >= beat + 2 && x.beat < beat + offbeatMonkeys[beat].length);
                PinkMonkeys(beat, offbeatMonkeys[beat].length, offbeatMonkeys[beat].mute, monkeys);
                PinkMonkeySFX(beat, offbeatMonkeys[beat].length, offbeatMonkeys[beat].mute, monkeys);
            } else if (offbeatMonkeys.ContainsKey(beat - 2)) {
                inputCheck = (beat - 2) + offbeatMonkeys[beat - 2].length;
            }

            if (inputCheck <= beat) ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustYellow, Miss, Nothing);

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 2, delegate { Clapping(beat + 2, inputCheck); }),
            });
        }

        public static void WarnPinkMonkeys(double beat, float length, bool mute)
        {
            wantOffbeat = new OffbeatMonkey{
                beat = beat,
                length = length,
                mute = mute,
            };
            ClappingInactive(beat);
            PinkMonkeySFX(beat, length, mute, EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offbeatMonkeys" }));
        }

        public static void PinkMonkeySFX(double beat, float length, bool mute, List<RiqEntity> monkeys)
        {
            var sfx = new List<MultiSound.Sound>();

            if (!mute) {
                sfx.AddRange(new MultiSound.Sound[] {
                    new MultiSound.Sound("monkeyWatch/voiceUki1",      beat       ),
                    new MultiSound.Sound("monkeyWatch/voiceUki1Echo1", beat + 0.25),
                    new MultiSound.Sound("monkeyWatch/voiceUki2",      beat + 1   ),
                    new MultiSound.Sound("monkeyWatch/voiceUki2Echo1", beat + 1.25),
                    new MultiSound.Sound("monkeyWatch/voiceUki3",      beat + 2   ),
                    new MultiSound.Sound("monkeyWatch/voiceUki3Echo1", beat + 2.25),
                });
            }

            if (monkeys == null) {
                for (int i = 2; i < length; i++) {
                    sfx.AddRange(new MultiSound.Sound[] {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}",      beat + i + 0.5 ),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}Echo1", beat + i + 0.75),
                    });
                }
            } else {
                for (int i = 0; i < monkeys.Count; i++) {
                    sfx.AddRange(new MultiSound.Sound[] {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}",      monkeys[i].beat       ),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{(i % 2 == 0 ? 1 : 2)}Echo1", monkeys[i].beat + 0.25),
                    });
                }
            }

            MultiSound.Play(sfx.ToArray(), forcePlay: true);
        }

        public void PinkMonkeys(double beat, float length, bool mute, List<RiqEntity> monkeys)
        {
            if (monkeys.Count == 0) {
                for (int i = 2; i < length; i++) {
                    ScheduleInput(beat, i + 0.5, InputType.STANDARD_DOWN, JustPink, Miss, Nothing);
                }
            } else {
                for (int i = 0; i < monkeys.Count; i++)
                {
                    ScheduleInput(beat, monkeys[i].beat - beat, InputType.STANDARD_DOWN, (monkeys[i]["sfx"] == 3 ? JustYellow : JustPink), Miss, Nothing);
                }
            }
        }

        void JustYellow(PlayerActionEvent caller, float state)
        {
            Just(state, true);
        }

        void JustPink(PlayerActionEvent caller, float state)
        {
            Just(state, false);
        }

        void Just(float state, bool isYellow)
        {
            lastMonkeyClapped++;
            string whichAnim = "PlayerClap";
            if (state >= 1f || state <= -1f) {
                SoundByte.PlayOneShot("miss");
                whichAnim += "Barely";
            } else {
                SoundByte.PlayOneShotGame(isYellow ? $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 5)}" : "monkeyWatch/clapOffbeat");
                if (isYellow)
                    monkeyPlayer.YellowStars.Play();
                else {
                    monkeyPlayer.PinkStars.Play();
                    whichAnim += "Big";
                }
            }
            monkeyPlayer.MonkeyAnim.DoScaledAnimationAsync(whichAnim, 0.5f);
            monkeyPlayer.ClickerAnim.DoScaledAnimationAsync("Click", 0.5f);
        }

        void Miss(PlayerActionEvent caller)
        {
            monkeyPlayer.ClickerAnim.DoScaledAnimationAsync("Click", 0.5f);
        }

        void Nothing(PlayerActionEvent caller) {}

        /* starting from the first monkey, these are the amount of monkeys clapped before the prepare animation switches.
        4 +
        8 +
        7 +
        8 +
        7 +
        8 +
        7 +
        7 +
        4
        */
    }
}
