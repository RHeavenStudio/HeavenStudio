using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlManzaiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("manzai", "Manzai", "554899", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    //function = delegate {},
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Enable Bopping", "Whether to bop to the beat or not"),
                        new Param("auto", false, "Enable Bopping (Auto)", "Whether to bop to the beat or not automatically"),
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_DogNinja;
    public class Manzai : Minigame
    {
    }
}
