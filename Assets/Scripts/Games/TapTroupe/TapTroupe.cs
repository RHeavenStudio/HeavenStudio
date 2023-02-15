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
            return new Minigame("tapTroupe", "Tap Troupe \n<color=#eb5454>[WIP]</color>", "TAPTAP", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {TapTroupe.instance.Bop(); },
                    defaultLength = 1f
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_TapTroupe;
    public class TapTroupe : Minigame
    {
        [Header("Components")]
        [SerializeField] TapTroupeTapper playerTapper;
        [SerializeField] TapTroupeCorner playerCorner;
        [SerializeField] List<TapTroupeTapper> npcTappers = new List<TapTroupeTapper>();
        [SerializeField] List<TapTroupeCorner> npcCorners = new List<TapTroupeCorner>();
        [Header("Properties")]
        private static List<float> queuedInputs = new List<float>();

        public static TapTroupe instance;

        void Awake()
        {
            instance = this;
        }

        public void Bop()
        {
            playerTapper.Bop();
            playerCorner.Bop();
            foreach (var tapper in npcTappers)
            {
                tapper.Bop();
            }
            foreach (var corner in npcCorners)
            {
                corner.Bop();
            }
        }
    }
}
