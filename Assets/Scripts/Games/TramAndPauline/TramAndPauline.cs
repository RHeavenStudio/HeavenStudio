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
            return new Minigame("tramAndPauline", "Tram & Pauline", "adb5e7", false, false, new List<GameAction>()
            {
                new GameAction("pauline", "Pauline")
                {
                    defaultLength = 2f
                },
                new GameAction("tram", "Tram")
                {
                    defaultLength = 2f
                },
                new GameAction("shape", "Change Transformation")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("pauline", false, "Pauline is Human?"),
                        new Param("tram", false, "Tram is Human?")
                    }
                },
                new GameAction("curtains", "Curtains")
                {
                    defaultLength = 4f,
                    resizable = true
                }
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
