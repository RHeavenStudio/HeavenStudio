using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class TotemClimbLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("totemClimb", "Totem Climb", "FFFFFF", false, false, new()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class TotemClimb : Minigame
    {
        public static TotemClimb instance;

        private void Awake()
        {
            instance = this;
        }
    }
}

