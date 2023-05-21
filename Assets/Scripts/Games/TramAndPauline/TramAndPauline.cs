using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTramLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tramAndPauline", "Tram & Pauline \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "adb5e7", true, false, new List<GameAction>()
            {

            }
            );
        }
    }
}
namespace HeavenStudio.Games
{
    public class TramAndPauline : Minigame
    {
        public static TramAndPauline instance;
        private void Awake()
        {
            instance = this;
        }
    }
}
