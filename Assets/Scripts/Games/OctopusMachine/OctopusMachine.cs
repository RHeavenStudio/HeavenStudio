using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
// using GhostlyGuy's Balls;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("OctopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Bop(e.beat, e["disableBop"], e[""], e["whichBop"]); 
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("bop", false, "Which Bop?", "Plays a sepcific bop type"),
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop?", "Plays a sepcific bop type"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("Expand", "Expand")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Expand(e.beat); 
                    },
                    defaultLength = 1f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Prepare(e.beat);
                    },
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Prepare(e.beat);
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("OctopusAnimation", "Octopus Animation")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.PlayAnimation(e.beat, e["keepWhich"], e["whichBop"]);
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("keepWhich", true, "Bop Like This?", "Keep bopping using the selected bop"),
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop?", "Plays a sepcific bop type"),
                    },
                    defaultLength = 0.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_OctopusMachine;
    public partial class OctopusMachine : Minigame
    {
        [Header("Animators")]
        public GameObject Octopus1;
        public GameObject Octopus2;
        public GameObject Octopus3;

        public bool isHappy;
        public bool isAngry;
        public bool isShocked;
        public bool isPreparing;
        public bool bopOn = true;
        private float lastReportedBeat = 0f;

        public static OctopusMachine instance;

        public enum Bops
        {
            Bop,
            Joyful,
            Upset,
            Shocked,
        }

        void Awake()
        {
            instance = this;
        }

        private void LateUpdate() 
        {
            
        }

        public void Prepare(float beat)
        {
            //AllAnimate("Prepare");
            isPreparing = true;
        }

        public void Expand(float beat)
        {
            Debug.Log("expand event rn");
        }

        public void Bop(float beat, float length, bool doesBop, bool autoBop)
        {
            bopOn = autoBop;
            if (doesBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            
                        })
                    });
                }
            }
        }

        public void PlayAnimation(float beat, bool keepBopping, int whichBop)
        {
            Octopus.instance.PlayAnimation(beat, keepBopping, whichBop);
        }
    }
}