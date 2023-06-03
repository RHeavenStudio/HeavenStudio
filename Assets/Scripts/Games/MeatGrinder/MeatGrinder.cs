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
                new GameAction("MeatCall", "Meat Call")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.MeatCall(e.beat); 
                    },
                    defaultLength = 0.5f,
                    priority = 2,
                    preFunctionLength = 1f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.PreInterval(e.beat, 4f);
                    },
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    defaultLength = 4f,
                    resizable = true,
                    priority = 1,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        MeatGrinder.PreInterval(e.beat, e.length);
                    },
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
                    priority = 4,
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
    using Scripts_MeatGrinder;
    public class MeatGrinder : Minigame
    {
        static List<float> queuedInputs = new List<float>();
        static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();
        struct QueuedInterval
        {
            public float beat;
            public float length;
        }

        [Header("Objects")]
        public GameObject MeatBase;

        [Header("Animators")]
        public Animator BossAnim;
        public Animator TackAnim;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        bool bossBop = true;
        bool hasSignaled;
        public float beatInterval = 4f;
        public bool bossAnnoyed = false;
        private float lastReportedBeat = 0f;
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

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
                if (queuedIntervals.Count > 0) queuedIntervals.Clear();
                intervalStarted = false;
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Update() 
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }

            if (!Conductor.instance.NotStopped()) {
                intervalStarted = false;
                beatInterval = 4f;
            }

            if (PlayerInput.Pressed(true) && !IsExpectingInputNow(InputType.STANDARD_DOWN)) {
                ScoreMiss();
                TackAnim.DoScaledAnimationAsync("TackEmptyHit", 0.5f);
                TackAnim.SetBool("tackMeated", false);
                Jukebox.PlayOneShotGame(sfxName+"whiff");
                if (bossAnnoyed) BossAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }

            if (bossAnnoyed) BossAnim.SetBool("bossAnnoyed", true);

            if (queuedIntervals.Count > 0) {
                foreach (var interval in queuedIntervals) { StartInterval(interval.beat, interval.length); }
                queuedIntervals.Clear();
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
            };
        }

        public void Bop(float beat, float length, bool doesBop, bool autoBop)
        {
            bossBop = autoBop;
            if (doesBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if (!BossAnim.IsPlayingAnimationName("BossCall") && !BossAnim.IsPlayingAnimationName("BossSignal"))
                            {
                                BossAnim.DoScaledAnimationAsync(bossAnnoyed ? "BossMiss" : "Bop", 0.5f);
                            };
                        })
                    });
                }
            }
        }

        public static void PreInterval(float beat, float length)
        {
            if (MeatGrinder.instance.intervalStarted || MeatGrinder.queuedIntervals.Count > 0) return;

            MeatGrinder.queuedIntervals.Add(new QueuedInterval() {
                beat = beat,
                length = length,
            });

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("meatGrinder/startSignal", beat - 1),
            }, forcePlay: true);

            if (GameManager.instance.currentGame == "meatGrinder") {
                BeatAction.New(MeatGrinder.instance.gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat - 1, delegate { 
                        MeatGrinder.instance.BossAnim.DoScaledAnimationAsync("BossSignal", 0.5f);
                    }),
                }); 
            }
        }

        public void StartInterval(float beat, float length)
        {
            if (MeatGrinder.instance.intervalStarted) return;

            intervalStartBeat = beat;
            intervalStarted = true;
            beatInterval = length;

            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + length - 0.33f, delegate { PassTurn(beat); }),
            });
        }
        
        public void MeatToss(float beat)
        {
            Jukebox.PlayOneShotGame(sfxName+"toss");
            
            MeatToss Meat = Instantiate(MeatBase, gameObject.transform).GetComponent<MeatToss>();
            Meat.startBeat = beat;
            Meat.cueLength = 1f;
            Meat.cueBased = true;
            Meat.meatType = "DarkMeat";
        }

        public void MeatCall(float beat) 
        {
            BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
            Jukebox.PlayOneShotGame(sfxName+"signal");
            
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }

            queuedInputs.Add(beat - intervalStartBeat);
        }

        public void PassTurn(float beat)
        {
            hasSignaled = false;
            intervalStarted = false;
            foreach (var input in queuedInputs)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(input + beatInterval , delegate { 
                        MeatToss Meat = Instantiate(MeatBase, gameObject.transform).GetComponent<MeatToss>();
                        Meat.startBeat = beat;
                        Meat.cueLength = beatInterval + input;
                        Meat.cueBased = false;
                        Meat.meatType = "LightMeat";
                    }),
                });
                
            }
            queuedInputs.Clear();
        }
    }
}