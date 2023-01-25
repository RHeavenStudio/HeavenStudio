using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRingsideLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ringside", "Ringside \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "WUTRU3", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Ringside : Minigame
    {

    }
}
