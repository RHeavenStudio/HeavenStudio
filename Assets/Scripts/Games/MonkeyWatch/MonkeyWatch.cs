using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

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
                    function = delegate {var e = eventCaller.currentEntity; MonkeyWatch.instance.MonkeysAppear(e.beat);},
                    defaultLength = 2f,
                    resizable = true
                },
                new GameAction("startClapping", "Start Clapping")
                {
                    function = delegate {var e = eventCaller.currentEntity; MonkeyWatch.instance.Clapping(e.beat);},
                    defaultLength = 2f,
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; MonkeyWatch.ClappingInactive(e.beat);},
                },
                new GameAction("offbeatMonkeys", "Offbeat Monkeys")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Cue", "Mute the offbeat monkeys's cue") 
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; MonkeyWatch.WarnPurpleMonkeys(e.beat, e["mute"]); },
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
    // using Scripts_MonkeyWatch;
    public class MonkeyWatch : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator monkeyPlayer;

        [Header("Properties")]
        static List<double> queuedInputs = new();
        List<OffbeatMonkey> offbeatMonkeys = new List<OffbeatMonkey>();
        public struct OffbeatMonkey
        {
            public double beat;
            public bool mute;
        }
        static double WantClapping = Double.MinValue;
        public enum HowMissed
        {
            NotMissed = 0,
            MissedOff = 1,
            MissedOn = 2
        }
        bool offColorActive;
        bool goBop;

        public static MonkeyWatch instance;

        void Awake()
        {
            instance = this;
            var tempEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offbeatMonkeys" });
            for (int i = 0; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    offbeatMonkeys.Add(new OffbeatMonkey{
                        beat = tempEvents[i].beat,
                        mute = tempEvents[i]["mute"],
                    });
                }
            }
        }

        void Start() 
        {

        }

        public override void OnGameSwitch(double beat)
        {
            if (WantClapping != double.MinValue)
            {
                Clapping(WantClapping);
                WantClapping = double.MinValue;
            }
        }

        void OnDestroy()
        {
            if (!Conductor.instance.NotStopped()) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
                if (offbeatMonkeys.Count > 0) offbeatMonkeys.Clear();
            }
            
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public void Update()
        {
            
        }

        public void MonkeysAppear(double beat)
        {

        }

        public static void ClappingInactive(double beat)
        {
            WantClapping = beat;
        }

        public void Clapping(double beat, bool schedule = true)
        {
            if (schedule) ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, JustYellow, Miss, Nothing);
            bool nextSchedule = true;
            for (int i = 0; i < offbeatMonkeys.Count; i++)
            {
                if (offbeatMonkeys[i].beat == beat) {
                    PurpleMonkeys(beat, offbeatMonkeys[i].mute);
                    //offbeatMonkeys.RemoveAt(i);
                    nextSchedule = false;
                }
            }
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 2, delegate { Clapping(beat + 2, nextSchedule); }),
            });
        }

        public static void WarnPurpleMonkeys(double beat, bool mute)
        {
            var sfx = new List<MultiSound.Sound>() {
                new MultiSound.Sound("monkeyWatch/voiceKi1",      beat + 2.5),
                new MultiSound.Sound("monkeyWatch/voiceKi1Echo1", beat + 2.75),
                new MultiSound.Sound("monkeyWatch/voiceKi2",      beat + 3.5),
                new MultiSound.Sound("monkeyWatch/voiceKi2Echo1", beat + 3.75),
            };
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

        public void PurpleMonkeys(double beat, bool mute)
        {
            WarnPurpleMonkeys(beat, mute);

            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, JustPurple, Miss, Nothing);
            ScheduleInput(beat, 3.5f, InputType.STANDARD_DOWN, JustPurple, Miss, Nothing);
        }

        public void JustYellow(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                SoundByte.PlayOneShotGame("miss");
            } else {
                SoundByte.PlayOneShotGame($"monkeyWatch/clapOnbeat{UnityEngine.Random.Range(1, 5)}");
            }
        }

        public void JustPurple(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                SoundByte.PlayOneShotGame("miss");
            } else {
                SoundByte.PlayOneShotGame("monkeyWatch/clapOffbeat");
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("miss");
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}
