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
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; MonkeyWatch.ClappingInactive(e.beat);},
                },
                new GameAction("offbeatMonkeys", "Offbeat Monkeys")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cue", "Mute the offbeat monkeys's cue"),
                        new Param("custom", false, "Custom Cue", "Place the \"Custom Monkey\" block 2 beats after the start of this one to create a custom purple monkey cue."),
                    },
                    resizable = true,
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.WarnPurpleMonkeys(e.beat, e.length, e["mute"]); },
                },
                new GameAction("customMonkey", "Custom Monkey")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("sfx", MonkeyWatch.SfxTypes.First, "Which SFX", "Choose between the first and second \"ki\" sfx")
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.WarnPurpleMonkeys(e.beat, e.length, e["mute"]); },
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
            },
            new List<string>() {"rvl", "keep"},
            "rvlwatch", "en",
            new List<string>() {}
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
        [SerializeField] GameObject purpleMonkey;

        // unserialized variables below
        public enum SfxTypes 
        {
            None,
            First,
            Second,
            Onbeat,
        }

        static List<double> queuedInputs = new();
        List<OffbeatMonkey> offbeatMonkeys = new List<OffbeatMonkey>();
        List<GameObject> watchHoles = new List<GameObject>();

        public struct OffbeatMonkey
        {
            public double beat;
            public bool mute;
            public float length;
            public CustomMonkey[] monkeys;
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
            var offbeatEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offbeatMonkeys" });
            var customEvents  = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "customMonkey" });
            for (int i = 0; i < offbeatEvents.Count; i++)
            {
                double offbeatBeat = offbeatEvents[i].beat;
                if (offbeatBeat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    var tempMonkeys = new List<CustomMonkey>();
                    if (offbeatEvents[i]["custom"]) {
                        for (int j = 0; j < customEvents.Count; j++)
                        {
                            if (customEvents[j].beat > offbeatBeat+2
                            && customEvents[j].beat < offbeatBeat+offbeatEvents[i].length)
                            {
                                tempMonkeys.Add(new CustomMonkey{
                                    beat = customEvents[j].beat,
                                    sfx = customEvents[j]["sfx"],
                                });
                            }
                        }
                    }
                    offbeatMonkeys.Add(new OffbeatMonkey{
                        beat = offbeatBeat,
                        mute = offbeatEvents[i]["mute"],
                        length = offbeatEvents[i].length,
                        monkeys = (tempMonkeys.Count == 0 ? null : tempMonkeys.ToArray()),
                    });
                }
            }

            // put all the watch holes in a list
            for (int i = 0; i < watchHoleParent.childCount; i++) {
                watchHoles.Add(watchHoleParent.GetChild(i).gameObject);
            }
        }

        private void Start() 
        {
            GameCamera.additionalPosition = new Vector3(0, 18.95f, 0);
        }

        public override void OnGameSwitch(double beat)
        {
            if (wantOffbeat.length != 0)
            {
                PurpleMonkeys(wantOffbeat.beat, wantOffbeat.length, wantOffbeat.mute);
                wantOffbeat = new OffbeatMonkey{
                    beat = 0,
                    length = 0,
                    mute = false,
                    monkeys = null,
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
            wantClapping = beat;
            startedClapping = true;
        }

        public void Clapping(double beat)
        {
            bool schedule = true;
            for (int i = 0; i < offbeatMonkeys.Count; i++)
            {
                if (beat >= offbeatMonkeys[i].beat+2 && beat < offbeatMonkeys[i].beat+offbeatMonkeys[i].length)
                    schedule = false;
            }
            if (schedule) ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustYellow, Miss, Nothing);
            
            for (int i = 0; i < offbeatMonkeys.Count; i++)
            {
                if (offbeatMonkeys[i].beat == beat) {
                    PurpleMonkeys(beat, offbeatMonkeys[i].length, offbeatMonkeys[i].mute, offbeatMonkeys[i].monkeys);
                    PurpleMonkeySFX(beat, offbeatMonkeys[i].length, offbeatMonkeys[i].mute, offbeatMonkeys[i].monkeys);
                }
            }

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 2, delegate { Clapping(beat + 2); }),
            });
        }

        public static void WarnPurpleMonkeys(double beat, float length, bool mute)
        {
            wantOffbeat = new OffbeatMonkey{
                beat = beat,
                length = length,
                mute = mute,
            };
            ClappingInactive(beat);
            PurpleMonkeySFX(beat, length, mute);
        }

        public static void PurpleMonkeySFX(double beat, float length, bool mute, CustomMonkey[] monkeys = null)
        {
            var tempMonkeys = new List<CustomMonkey>();
            if (monkeys == null) {
                for (int i = 2; i < length; i++) {
                    tempMonkeys.Add(new CustomMonkey{beat = beat + 0.5f + i, sfx = (i % 2 == 0 ? 1 : 2)});
                }
            }
            monkeys = tempMonkeys.ToArray();
            
            var sfx = new List<MultiSound.Sound>();

            for (int i = 0; i < monkeys.Length; i++)
            {
                sfx.AddRange(new MultiSound.Sound[] {
                    new MultiSound.Sound($"monkeyWatch/voiceKi{monkeys[i].sfx}",      monkeys[i].beat       ),
                    new MultiSound.Sound($"monkeyWatch/voiceKi{monkeys[i].sfx}Echo1", monkeys[i].beat + 0.25),
                });
            }
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
            MultiSound.Play(sfx.ToArray(), forcePlay: true);
        }

        public void PurpleMonkeys(double beat, float length, bool mute, CustomMonkey[] monkeys = null)
        {
            if (monkeys == null) 
                monkeys = new CustomMonkey[] {
                    new CustomMonkey{beat = beat + 2.5f, sfx = 1},
                    new CustomMonkey{beat = beat + 3.5f, sfx = 2},
                };

            for (int i = 0; i < monkeys.Length; i++)
            {
                ScheduleInput(beat, monkeys[i].beat - beat, InputType.STANDARD_DOWN, (monkeys[i].sfx == 3 ? JustYellow : JustPurple), Miss, Nothing);
            }
        }

        public void JustYellow(PlayerActionEvent caller, float state)
        {
            Just(state, true);
        }

        public void JustPurple(PlayerActionEvent caller, float state)
        {
            Just(state, false);
        }

        void Just(float state, bool isYellow)
        {
            lastMonkeyClapped++;
            monkeyPlayer.anim.DoScaledAnimationAsync("Clap", 0.5f);
            if (state >= 1f || state <= -1f) {
                SoundByte.PlayOneShot("miss");
            } else {
                SoundByte.PlayOneShotGame(isYellow ? $"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 5)}" : "monkeyWatch/clapOffbeat");
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}
