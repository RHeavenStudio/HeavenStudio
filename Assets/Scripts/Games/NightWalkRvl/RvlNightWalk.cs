using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

// namespace HeavenStudio.Games.Loaders
// {
//     using static Minigames;

//     public static class RvlNightWalkLoader
//     {
//         public static Minigame AddGame(EventCaller eventCaller)
//         {
//             return new Minigame("nightWalkRvl", "Night Walk (Wii)", "FFFFFF", false, false, new List<GameAction>()
//             {

//             });
//         }
//     }
// }

namespace HeavenStudio.Games
{
    public class RvlNightWalk : Minigame
    {
        public static RvlNightWalk instance;

        private void Awake()
        {
            instance = this;
        }
    }
}
