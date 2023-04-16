using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTossBoysLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tossBoys", "Toss Boys", "9cfff7", false, false, new List<GameAction>()
            {
                
            });
        }
    }
}
namespace HeavenStudio.Games
{
    public class TossBoys : Minigame
    {
        public static TossBoys instance;

        private void Awake()
        {
            instance = this;            
        }
    }
}

