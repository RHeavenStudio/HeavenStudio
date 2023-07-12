using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrAnimalAcrobatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("animalAcrobat", "Animal Acrobat", "FFFFFF", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class AnimalAcrobat : Minigame
    {
        public static AnimalAcrobat instance;

        private void Awake()
        {
            instance = this;
        }
    }
}

