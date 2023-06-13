using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrSplashdownLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("splashdown", "Splashdown", "327BF5", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Splashdown : Minigame
    {
        public static Splashdown instance;

        private void Awake()
        {
            instance = this;
        }
    }
}

