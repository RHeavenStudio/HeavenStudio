using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbGhostLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("sneakySpirits", "Sneaky Spirits \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_SneakySpirits;
    public class SneakySpirits : Minigame
    {
        // Start is called before the first frame update
        void Awake()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
