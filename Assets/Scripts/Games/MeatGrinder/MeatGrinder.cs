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
                    function = delegate {var e = eventCaller.currentEntity; MeatGrinder.instance.Bop(e.beat, e["bossBop"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bossBop", false, "Boss Bops?", "Does Boss bop?"),
                    },
                    defaultLength = 0.5f,
                    priority = 4
                },
                new GameAction("Meat", "Meat")
                {
                    function = delegate {var e = eventCaller.currentEntity; MeatGrinder.instance.MeatCue(e.beat); },
                    defaultLength = 2f,
                    priority = 2
                },
                new GameAction("MeatCall", "Meat Call")
                {
                    function = delegate {var e = eventCaller.currentEntity; MeatGrinder.instance.MeatCall(e.beat); },
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; MeatGrinder.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true,
                    priority = 1
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
        private float lastReportedBeat = 0f;
        
        
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
            }
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && BossAnim.IsAnimationNotPlaying() && bossBop)
            {
                BossAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
        }

        public void Bop(float beat, bool doesBop)
        {
            bossBop = doesBop;
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
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }
        }

        public void MeatCall(float beat) 
        {
            queuedInputs.Add(new QueuedMeatGrinderInput()
            {
                //hit = hit,
                beatAwayFromStart = beat - intervalStartBeat,
            });
        }
    }

}