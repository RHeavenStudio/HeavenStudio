using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbNightWalkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("nightWalkAgb", "Night Walk (GBA)", "FFFFFF", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class AgbNightWalk : Minigame
    {
        public static AgbNightWalk instance;

        private void Awake()
        {
            instance = this;
        }
    }
}


