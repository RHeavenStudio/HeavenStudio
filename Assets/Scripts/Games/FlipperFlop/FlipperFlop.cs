using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlFlipperFlopLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("flipperFlop", "Flipper-Flop \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class FlipperFlop : Minigame
    {

        public static FlipperFlop instance;

        void Awake()
        {
            instance = this;
        }
    }
}
