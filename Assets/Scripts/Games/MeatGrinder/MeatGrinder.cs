using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class pcoMeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("meatGrinder", "Meat Grinder", "501d18", false, false, new List<GameAction>()
            {
                new GameAction("MeatToss", "Meat Toss")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.MeatToss(e.beat); 
                    },
                    defaultLength = 2f,
                    priority = 2,
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    defaultLength = 4f,
                    resizable = true,
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.PreInterval(e.beat, e.length, e["auto"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn")
                    },
                    preFunctionLength = 1
                },
                new GameAction("MeatCall", "Meat Call")
                {
                    defaultLength = 0.5f,
                    priority = 2,
                    preFunctionLength = 1f,
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate
                    {
                        MeatGrinder.PrePassTurn(eventCaller.currentEntity.beat);
                    },
                    preFunctionLength = 1
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.Bop(e.beat, e.length, e["bop"], e["bossBop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Boss Bops?", "Does Boss bop?"),
                        new Param("bossBop", false, "Boss Bops? (Auto)", "Does Boss Auto bop?"),
                    },
                    resizable = true,
                    priority = 1,
                },
            },
            new List<string>() {"pco", "normal", "repeat"},
            "pcomeat", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_MeatGrinder;
    using UnityEngine.UIElements;

    public class MeatGrinder : Minigame
    {
        static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();
        struct QueuedInterval
        {
            public double beat;
            public float length;
            public bool autoPassTurn;
        }

        [Header("Objects")]
        public GameObject MeatBase;

        [Header("Animators")]
        public Animator BossAnim;
        public Animator TackAnim;

        [Header("Variables")]
        bool bossBop = true;
        public bool bossAnnoyed = false;
        private double lastReportedBeat = 0f;
        const string sfxName = "meatGrinder/";

        public static MeatGrinder instance;

        public enum MeatType
        {
            Dark,
            Light,
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update() 
        {
            if (PlayerInput.Pressed(true) && (!IsExpectingInputNow(InputType.STANDARD_DOWN) || !IsExpectingInputNow(InputType.DIRECTION_DOWN))) {
                TackAnim.DoScaledAnimationAsync("TackEmptyHit", 0.5f);
                TackAnim.SetBool("tackMeated", false);
                SoundByte.PlayOneShotGame(sfxName+"whiff");
                bossAnnoyed = false;
            }

            if (bossAnnoyed) BossAnim.SetBool("bossAnnoyed", true);

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)
                && !BossAnim.IsPlayingAnimationName("BossCall") 
                && !BossAnim.IsPlayingAnimationName("BossSignal")
                && bossBop)
            {
                BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals) StartInterval(interval.beat, interval.length, beat, interval.autoPassTurn);
                queuedIntervals.Clear();
            }
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("meatGrinder", new string[] { "StartInterval" }).FindLast(x => x.beat <= beat);
        }

        private List<RiqEntity> GetRelevantMeatCallsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("meatGrinder", new string[] { "MeatCall" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        public void Bop(double beat, double length, bool doesBop, bool autoBop)
        {
            bossBop = autoBop;
            if (doesBop) 
            {
                List<BeatAction.Action> bops = new();
                for (int i = 0; i < length; i++) 
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (!BossAnim.IsPlayingAnimationName("BossCall") && !BossAnim.IsPlayingAnimationName("BossSignal"))
                        {
                            BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
                        }
                    }));
                }
                BeatAction.New(gameObject, bops);
            }
        }

        public static void PreInterval(double beat, float length, bool autoPassTurn)
        {
            SoundByte.PlayOneShot("games/meatGrinder/startSignal", beat - 1);

            if (GameManager.instance.currentGame == "meatGrinder") 
            {
                instance.StartInterval(beat, length, beat, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    length = length,
                    autoPassTurn = autoPassTurn
                });
            }
        }

        public void StartInterval(double beat, float length, double gameSwitchBeat, bool autoPassTurn)
        {
            List<BeatAction.Action> actions = new()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    if (beat - 1 >= gameSwitchBeat) BossAnim.DoScaledAnimationAsync("BossSignal", 0.5f);
                }),
            };

            var allCallEvents = GetRelevantMeatCallsBetweenBeat(beat, beat + length);
            allCallEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            for (int i = 0; i < allCallEvents.Count; i++)
            {
                double eventBeat = allCallEvents[i].beat;

                if (eventBeat >= gameSwitchBeat)
                {
                    actions.Add(new BeatAction.Action(eventBeat, delegate
                    {
                        MeatCall();
                    }));
                }
            }

            BeatAction.New(gameObject, actions);

            if (autoPassTurn)
            {
                PassTurn(beat + length, beat, length);
            }
        }
        
        public void MeatToss(double beat)
        {
            SoundByte.PlayOneShotGame(sfxName+"toss");
            
            MeatToss Meat = Instantiate(MeatBase, gameObject.transform).GetComponent<MeatToss>();
            Meat.startBeat = beat;
            Meat.cueLength = 1f;
            Meat.cueBased = true;
            Meat.meatType = "DarkMeat";
        }

        public void MeatCall() 
        {
            BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
            SoundByte.PlayOneShotGame(sfxName+"signal");
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "meatGrinder")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurnStandalone(double beat)
        {
            var lastInterval = GetLastIntervalBeforeBeat(beat);
            if (lastInterval != null) PassTurn(beat, lastInterval.beat, lastInterval.length);
        }

        private void PassTurn(double beat, double intervalBeat, float intervalLength)
        {
            var allCallEvents = GetRelevantMeatCallsBetweenBeat(intervalBeat, intervalBeat + intervalLength);
            allCallEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            List<BeatAction.Action> meatCalls = new();
            for (int i = 0; i < allCallEvents.Count; i++)
            {
                double relativeBeat = allCallEvents[i].beat - intervalBeat;
                meatCalls.Add(new BeatAction.Action(beat + relativeBeat - 1, delegate
                {
                    MeatToss Meat = Instantiate(MeatBase, gameObject.transform).GetComponent<MeatToss>();
                    Meat.startBeat = beat;
                    Meat.cueLength = relativeBeat;
                    Meat.cueBased = false;
                    Meat.meatType = "LightMeat";
                }));
            }
            BeatAction.New(gameObject, meatCalls);
        }
    }
}