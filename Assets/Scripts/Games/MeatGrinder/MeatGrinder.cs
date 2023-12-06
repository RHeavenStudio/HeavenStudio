using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class pcoMeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("meatGrinder", "Meat Grinder", "501d18", false, false, new List<GameAction>()
            {
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
                new GameAction("MeatToss", "Meat Toss")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.instance.MeatToss(e.beat, e["bacon"]);
                    },
                    inactiveFunction = delegate {
                        MeatGrinder.QueueMeatToss(eventCaller.currentEntity);
                    },
                    defaultLength = 2f,
                    priority = 2,
                    parameters = new List<Param>()
                    {
                        new Param("bacon", false, "Bacon Ball", "Throw a bacon ball instead of the typical meat"),
                    },
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
                    preFunction = delegate { MeatGrinder.PrePassTurn(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1
                },
                new GameAction("expressions", "Tack Expressions")
                {
                    function = delegate { MeatGrinder.instance.TackExpression(eventCaller.currentEntity["expression"]); },
                    parameters = new List<Param>() {
                        new Param("expression", MeatGrinder.TackExpressions.Content, "Expression", "The expression Tack will display"),
                    }
                },
            }
            // ,
            // new List<string>() { "pco", "normal", "repeat" },
            // "pcomeat", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_MeatGrinder;

    public class MeatGrinder : Minigame
    {
        static List<QueuedInterval> queuedIntervals = new ();
        struct QueuedInterval
        {
            public double beat;
            public float length;
            public bool autoPassTurn;
        }

        static List<RiqEntity> queuedMeats = new();

        [Header("Objects")]
        public GameObject MeatBase;

        [Header("Animators")]
        public Animator BossAnim;
        public Animator TackAnim;

        [Header("Variables")]
        bool bossBop = true;
        public bool bossAnnoyed = false;
        const string sfxName = "meatGrinder/";

        public static MeatGrinder instance;

        public enum TackExpressions
        {
            Content,
            // Smug,
            Wonder,
        }

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("PcoMeatPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadAny, IA_TouchBasicPress, IA_BatonBasicPress);
        
        private void Awake()
        {
            instance = this;
            SetupBopRegion("meatGrinder", "bop", "bossBop");
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press))
            {
                TackAnim.DoScaledAnimationAsync("TackEmptyHit", 0.5f);
                TackAnim.SetBool("tackMeated", false);
                SoundByte.PlayOneShotGame(sfxName + "whiff");
                bossAnnoyed = false;
            }

            if (bossAnnoyed) BossAnim.SetBool("bossAnnoyed", true);

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns) {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (!BossAnim.IsPlayingAnimationNames("BossCall", "BossSignal") && BeatIsInBopRegion(beat))
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
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>() {
                        new BeatAction.Action(beat + i, delegate {
                            if (!BossAnim.IsPlayingAnimationNames("BossCall") && !BossAnim.IsPlayingAnimationNames("BossSignal")) {
                                BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
                            }
                        })
                    });
                }
            }
        }

        public static void PreInterval(double beat, float length, bool autoPassTurn)
        {
            SoundByte.PlayOneShotGame(sfxName + "startSignal", beat - 1, forcePlay: true);

            if (GameManager.instance.currentGame == "meatGrinder") {
                instance.StartInterval(beat, length, beat, autoPassTurn);
            } else {
                queuedIntervals.Add(new QueuedInterval() {
                    beat = beat,
                    length = length,
                    autoPassTurn = autoPassTurn
                });
            }
        }

        public void StartInterval(double beat, float length, double gameSwitchBeat, bool autoPassTurn)
        {
            List<BeatAction.Action> actions = new() {
                new(beat - 1, delegate { BossAnim.DoScaledAnimationFromBeatAsync("BossSignal", 0.5f, beat - 1); }),
            };

            var allCallEvents = GetRelevantMeatCallsBetweenBeat(beat, beat + length);
            allCallEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            for (int i = 0; i < allCallEvents.Count; i++)
            {
                double eventBeat = allCallEvents[i].beat;

                if (eventBeat >= gameSwitchBeat) {
                    actions.Add(new BeatAction.Action(eventBeat, delegate {
                        BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
                        SoundByte.PlayOneShotGame(sfxName + "signal");
                    }));
                }
            }

            BeatAction.New(this, actions);

            if (autoPassTurn)
            {
                PassTurn(beat + length, beat, length);
            }
        }

        public static void QueueMeatToss(RiqEntity entity)
        {
            queuedMeats.Add(entity);
        }

        public void MeatToss(double beat, bool bacon)
        {
            SoundByte.PlayOneShotGame(sfxName + "toss");

            Meat Meat = Instantiate(MeatBase, transform).GetComponent<Meat>();
            Meat.startBeat = beat;
            Meat.meatType = bacon ? Meat.MeatType.BaconBall : Meat.MeatType.DarkMeat;
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "meatGrinder") {
                instance.PassTurnStandalone(beat);
            } else {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurnStandalone(double beat)
        {
            var lastInterval = EventCaller.GetAllInGameManagerList("meatGrinder", new string[] { "StartInterval" }).FindLast(x => x.beat <= beat);
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
                meatCalls.Add(new BeatAction.Action(beat + relativeBeat - 1, delegate {
                    Meat Meat = Instantiate(MeatBase, transform).GetComponent<Meat>();
                    Meat.startBeat = beat + relativeBeat - 1;
                    Meat.meatType = Meat.MeatType.LightMeat;
                }));
            }
            BeatAction.New(this, meatCalls);
        }
    }
}