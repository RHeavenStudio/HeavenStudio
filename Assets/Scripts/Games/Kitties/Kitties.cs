using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrTeppanLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("kitties", "Kitties! \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Cat Clap")
                {
                    function = delegate { Kitties.instance.Clap(eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle"], 
                        eventCaller.currentEntity.beat,  eventCaller.currentEntity["type"]); },

                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", Kitties.SpawnType.Straight, "Spawn", "The way in which the kitties will spawn"),
                        new Param("toggle", false, "Mice", "Replaces kitties as mice"),
                        new Param("toggle", false, "Invert Direction", "Inverts the direction they clap in"),
                    }
                },

                new GameAction("clapclose", "Up Close Clap")
                {
                    function = delegate { Kitties.instance.ClapClose(eventCaller.currentEntity.beat); },

                    defaultLength = 3,
                },

            });
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_Fireworks;
    public class Kitties : Minigame
    {
        public enum SpawnType
        {
            Straight,
            DiagonalDown,
            DiagonalUp,
        }

        public static Kitties instance;

        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Clap(bool isMice, bool isRightToLeft, float beat, int type = (int)SpawnType.Straight)
        { }

        public void ClapClose(float beat)
        { }
    }
}