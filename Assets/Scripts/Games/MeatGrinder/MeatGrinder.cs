using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class pcoMeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("meatGrinder", "Meat Grinder \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "f1492e", false, false, new List<GameAction>()
            {
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
                new GameAction("Meat", "Meat")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        MeatGrinder.instance.MeatCue(e.beat); 
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
                        MeatGrinder.CallInterval(e.beat);
                    },
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class MeatGrinder : Minigame
    {
        static List<QueuedMeatGrinderInput> queuedInputs = new List<QueuedMeatGrinderInput>();
        struct QueuedMeatGrinderInput
        {
            public bool hit;
            public float beatAwayFromStart;
        }

        [Header("Objects")]
        public GameObject Meat;

        [Header("Animators")]
        public Animator BossAnim;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 8f;
        bool bossBop = true;
        bool wantCall = false;
        int beatCaller = 0;
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
            if (wantCall) {
                BossAnim.Play("BossSignal", 0, 0);
                wantCall = false;
            }
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
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && BossAnim.IsAnimationNotPlaying())
            {
                BossAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
        }

        public void Bop(float beat, bool doesBop)
        {
            bossBop = doesBop;
        }

        public static void CallInterval(float beat)
        {
            //BossAnim.DoScaledAnimationAsync("BossSignal", 1f);
            MeatGrinder.instance.wantCall = true;

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(sfxName+"startSignal", beat - 1f),
            }, forcePlay: true);
        }

        public void StartInterval(float beat, float interval)
        {
            intervalStartBeat = beat;
            beatInterval = interval;
            if (!intervalStarted)
            {
                //misses = 0;
                //intervalStarted = true;
            }
        }

        public void MeatCue(float beat)
        {
            Jukebox.PlayOneShotGame(sfxName+"toss");

            //MeatToss Meat = Instantiate(Meat).GetComponent<MeatToss>();
            //Meat.startBeat = beat;
        }

        public void MeatCall(float beat) 
        {
            BossAnim.DoScaledAnimationAsync("BossCall", 0.5f);
            Jukebox.PlayOneShotGame(sfxName+"signal");
            
            queuedInputs.Add(new QueuedMeatGrinderInput()
            {
                //hit = hit,
                beatAwayFromStart = beat - intervalStartBeat,
            });
        }
    }

}