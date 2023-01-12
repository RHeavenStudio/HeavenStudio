using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlWorkingDoughLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("workingDough", "Working Dough \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "090909", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_WorkingDough;
    public class WorkingDough : Minigame
    {
        public static WorkingDough instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {

        }
    }
}
