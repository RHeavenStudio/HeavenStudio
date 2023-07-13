using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.Games.Scripts_AnimalAcrobat;
using Jukebox;

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
    public class AnimalAcrobat : Minigame
    {
        [Header("Elephant")]
        [SerializeField] private AcrobatObstacle elephantRef;
        [SerializeField] private float elephantDistance;
        [SerializeField] private float elephantStart = 0.004f;
        [Header("Giraffe")]
        [SerializeField] private AcrobatObstacle giraffeRef;
        [SerializeField] private float giraffeDistance;
        [SerializeField] private float giraffeStart;
        [Header("Monkeys")]
        [SerializeField] private AcrobatObstacle monkeysRef;
        [SerializeField] private float monkeysDistance;
        [SerializeField] private float monkeysStart;
        [Header("One Monkey")]
        [SerializeField] private AcrobatObstacle oneMonkeyRef;
        [SerializeField] private float oneMonkeyDistance;
        [SerializeField] private float oneMonkeyStart;

        public static AnimalAcrobat instance;

        private void Awake()
        {
            instance = this;
        }

        public override void OnPlay(double beat)
        {
            SpawnAnimals(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            SpawnAnimals(beat);
        }

        private void SpawnAnimals(double gameSwitchBeat)
        {
            var animalEvents = EventCaller.GetAllInGameManagerList("animalAcrobat", new string[] { "elephant", "giraffe", "monkeys", "monkeyLong", "monkeyShort"});
            if (animalEvents.Count == 0) return;
            animalEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            double nextBeat = animalEvents[0].beat;
            float startPos = animalEvents[0].datamodel switch
            {
                "animalAcrobat/elephant" => elephantStart - elephantDistance,
                "animalAcrobat/giraffe" => giraffeStart - giraffeDistance,
                "animalAcrobat/monkeys" => monkeysStart - monkeysDistance,
                "animalAcrobat/monkeyLong" => monkeysStart - monkeysDistance,
                "animalAcrobat/monkeyShort" => oneMonkeyStart - oneMonkeyDistance,
                _ => throw new System.NotImplementedException()
            }; 
            foreach (var animal in animalEvents)
            {
                if (animal.beat == nextBeat)
                {
                    nextBeat += animal.length;
                    switch (animal.datamodel)
                    {
                        case "animalAcrobat/elephant":
                            startPos += elephantDistance;
                            AcrobatObstacle spawnedElephant = Instantiate(elephantRef, transform);
                            spawnedElephant.transform.position = new Vector3(startPos, 0, 0);
                            spawnedElephant.Init(animal.beat, gameSwitchBeat);
                            break;
                        case "animalAcrobat/giraffe":
                            startPos += giraffeDistance;
                            AcrobatObstacle spawnedGiraffe = Instantiate(giraffeRef, transform);
                            spawnedGiraffe.transform.position = new Vector3(startPos, 0, 0);
                            spawnedGiraffe.Init(animal.beat, gameSwitchBeat);
                            break;
                        case "animalAcrobat/monkeys":
                            startPos += monkeysDistance;
                            AcrobatObstacle spawnedMonkeyLong = Instantiate(monkeysRef, transform);
                            spawnedMonkeyLong.transform.position = new Vector3(startPos, 0, 0);
                            spawnedMonkeyLong.Init(animal.beat, gameSwitchBeat);
                            startPos += oneMonkeyDistance;
                            AcrobatObstacle spawnedOneMonkeyS = Instantiate(oneMonkeyRef, transform);
                            spawnedOneMonkeyS.transform.position = new Vector3(startPos, 0, 0);
                            spawnedOneMonkeyS.Init(animal.beat, gameSwitchBeat);
                            break;
                        case "animalAcrobat/monkeyLong":
                            startPos += monkeysDistance;
                            AcrobatObstacle spawnedMonkeys = Instantiate(monkeysRef, transform);
                            spawnedMonkeys.transform.position = new Vector3(startPos, 0, 0);
                            spawnedMonkeys.Init(animal.beat, gameSwitchBeat);
                            break;
                        case "animalAcrobat/monkeyShort":
                            startPos += oneMonkeyDistance;
                            AcrobatObstacle spawnedOneMonkey = Instantiate(oneMonkeyRef, transform);
                            spawnedOneMonkey.transform.position = new Vector3(startPos, 0, 0);
                            spawnedOneMonkey.Init(animal.beat, gameSwitchBeat);
                            break;
                        default: break;
                    }
                }
            }
        }
    }
}

