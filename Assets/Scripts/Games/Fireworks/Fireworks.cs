using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbFireworkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("fireworks", "Fireworks \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("firework", "Launch Firework")
                {
                    function = delegate { var e = eventCaller.currentEntity; Fireworks.instance.LaunchRocket(e.beat, e["type"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", Fireworks.FireworkType.Normal, "Firework Type", "Choose a firework")
                    },
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Fireworks;
    public class Fireworks : Minigame
    {
        //code is just copied from other minigame code, i will polish them later
        [Header("References")]
        public Animator Rocket;
        //public Animator Explosion;

        public static Fireworks instance;
        // Start is called before the first frame update
        public enum FireworkType
        {
            Normal,
            Quick,
            TamakoBomb
        }

        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            var currBeat = cond.songPositionInBeats;

            //if (PlayerInput.GetAnyDirection(PlayerInput.LEFT)) { }
        }

        public void LaunchRocket(float beat, int type)
        {

        }
    }
}
