using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrFillbotsLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("fillbots", "Fillbots", "FFFFFF", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Fillbots : Minigame
    {
        public static Fillbots instance;

        private void Awake()
        {
            instance = this;
        }
    }
}

