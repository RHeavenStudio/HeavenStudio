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
            return new Minigame("meatGrinder", "Meat Grinder \n<color=#eb5454>[WIP]</color>", "f1492e", false, false, new List<GameAction>()
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
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.StartInterval(e.beat, e.length); 
                    },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 1,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.CallInterval(e.beat, e.length);
                    },
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.Bop(e.beat, e["bossBop"]); 
                    },
                    parameters = new List<Param>()
                    {
                        new Param("bossBop", false, "Boss Bops?", "Does Boss bop?"),
                    },
                    defaultLength = 0.5f,
                    priority = 4,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MeatGrinder;
    public class MeatGrinder : Minigame
    {
        static List<QueuedMeatInput> queuedInputs = new List<QueuedMeatInput>();
        struct QueuedMeatInput
        {
            public float beatAwayFromStart;
        }

        [Header("Objects")]
        public GameObject MeatBase;
        public GameObject MeatFall;

        [Header("Animators")]
        public Animator BossAnim;
        public Animator TackAnim;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 4f;
        float misses;
        bool bossBop = true;
        public bool bossAnnoyed = false;
        private float lastReportedBeat = 0f;
        const string sfxName = "meatGrinder/";

        public static MeatGrinder instance;
        
        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
        }

        private void Update() 
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused && intervalStarted)
            {
                intervalStarted = false;
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                ScoreMiss();
                bossAnnoyed = false;

                TackAnim.DoScaledAnimationAsync("TackEmptyHit", 0.5f);
                Jukebox.PlayOneShotGame(sfxName+"whiff");
            }

            if (bossAnnoyed) BossAnim.SetBool("bossAnnoyed", true);
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)
                && !BossAnim.IsPlayingAnimationName("BossCall") 
                && !BossAnim.IsPlayingAnimationName("BossSignal")
                && bossBop)
            {
                if (!bossAnnoyed) {
                    BossAnim.DoScaledAnimationAsync("Bop", 0.5f);
                } else {
                    BossAnim.DoScaledAnimationAsync("BossMiss", 0.5f);
                }
            };
        }

        public void Bop(float beat, bool doesBop)
        {
            bossBop = doesBop;
        }

        public static void CallInterval(float beat, float interval)
        {
            MeatGrinder.instance.beatInterval = interval;

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate { instance.BossAnim.DoScaledAnimationAsync("BossSignal", 0.5f); }),
            });

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound(sfxName+"startSignal", beat - 1f),
            }, forcePlay: true);
        }

        public void StartInterval(float beat, float interval)
        {
            intervalStartBeat = beat;
            if (!intervalStarted)
            {
                misses = 0;
                intervalStarted = true;
            }

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + interval - 1, delegate { PassTurn(beat); }),
            });
        }

        public void MeatToss(float beat)
        {
            Jukebox.PlayOneShotGame(sfxName+"toss");
            
            MeatToss Meat = Instantiate(MeatBase).GetComponent<MeatToss>();
            Meat.startBeat = beat;
            Meat.cueLength = 1f;

            Debug.Log("does this shit even work");

            /*
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.8f, delegate { Instantiate(MeatBase); }),
            });
            */
        }

        public void MeatCall(float beat) 
        {
            BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
            Jukebox.PlayOneShotGame(sfxName+"signal");
            
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }

            queuedInputs.Add(new QueuedMeatInput()
            {
                beatAwayFromStart = beat - intervalStartBeat,
            });
        }

        public void PassTurn(float beat)
        {
            intervalStarted = false;
            foreach (var input in queuedInputs)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate { 
                    MeatToss Meat = Instantiate(MeatBase).GetComponent<MeatToss>();
                    Meat.startBeat = beat;
                    Meat.cueLength = beatInterval + input.beatAwayFromStart; 
                }),
            });
                
            }
            queuedInputs.Clear();
        }
    }
}