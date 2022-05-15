using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrPillowLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("pajamaParty", "Pajama Party \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class PajamaParty : Minigame
    {
        //game scene
        public static PajamaParty instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            
        }
    }
}
