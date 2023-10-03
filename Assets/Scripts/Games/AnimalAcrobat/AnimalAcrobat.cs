using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
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
                new GameAction("elephant", "Elephant")
                {
                    defaultLength = 4
                },
                new GameAction("giraffe", "Giraffe")
                {
                    defaultLength = 8
                },
                new GameAction("monkeys", "Monkeys")
                {
                    defaultLength = 8
                },
                new GameAction("monkeyLong", "Monkey Line Standalone")
                {
                    defaultLength = 5
                },
                new GameAction("monkeyShort", "Monkey Standalone")
                {
                    defaultLength = 3
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_AnimalAcrobat;

    public class AnimalAcrobat : Minigame
    {
        [Header("Animal Prefabs")]
        [SerializeField] private AcrobatObstacle _elephant;
        [SerializeField] private AcrobatObstacle _giraffe, _monkeysLong, _monkeysShort;

        private void Awake()
        {
            Instantiate(_elephant, transform);
        }
    }
}

