using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTapTroupeLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tapTroupe", "Tap Troupe \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "TAPTAP", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class TapTroupe : Minigame
    {
        public static TapTroupe instance;

        void Awake()
        {
            instance = this;
        }
    }
}
