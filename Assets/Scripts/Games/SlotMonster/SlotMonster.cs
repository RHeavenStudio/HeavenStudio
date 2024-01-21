using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSlotMonsterLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("slotMonster", "Slot Monster", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("startInterval", "Start Interval")
                {

                }
            }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class SlotMonster : Minigame
    {
    }
}