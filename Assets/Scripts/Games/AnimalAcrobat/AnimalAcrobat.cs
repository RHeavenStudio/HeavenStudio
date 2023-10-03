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
        private static readonly int POOL_NUMBER = 4;
        private static readonly int EXPIRATION_TIMES = 2;

        [Header("Animal Prefabs")]
        [SerializeField] private AcrobatObstacle _elephant;
        [SerializeField] private AcrobatObstacle _giraffe, _monkeysLong, _monkeysShort;

        private List<AcrobatObstacle> _pooledElephants = new(), _pooledGiraffes = new(), _pooledMonkeysLong = new(), _pooledMonkeysShort = new();

        private enum AnimalType
        {
            Elephant,
            Giraffe,
            MonkeysLong,
            MonkeysShort
        }

        private struct QueuedAnimal
        {
            public AnimalType type;
            public double startBeat;
            public double length;
        }

        private List<QueuedAnimal> _queuedAnimals = new();
        private int _animalPoolIndex = 0;
        private float _animalSummatedDistance = 0;

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;
            AnimalPoolUpdate(cond);
        }

        private void AnimalPoolUpdate(Conductor cond)
        {
            if (_animalPoolIndex >= _queuedAnimals.Count) return;

            double bakeBeat = _queuedAnimals[_animalPoolIndex - EXPIRATION_TIMES].startBeat;
            if (cond.songPositionInBeatsAsDouble >= bakeBeat)
            {
                BakeNextAvailableAnimal();
            }
        }

        private void BakeNextAvailableAnimal()
        {
            if (_animalPoolIndex >= _queuedAnimals.Count) return;
            var currentAnimal = _queuedAnimals[_animalPoolIndex];
            List<AcrobatObstacle> pooledObstacles = currentAnimal.type switch
            {
                AnimalType.Elephant => _pooledElephants,
                AnimalType.Giraffe => _pooledGiraffes,
                AnimalType.MonkeysLong => _pooledMonkeysLong,
                AnimalType.MonkeysShort => _pooledMonkeysShort,
                _ => throw new System.NotImplementedException(),
            };

            var animal = pooledObstacles.Find(x => x.IsAvailableAtBeat(currentAnimal.startBeat));
            _animalSummatedDistance += (_animalPoolIndex == 0) ? animal.SpawnOffset : animal.SpawnDistance;
            animal.gameObject.SetActive(true);

            double expBeat = currentAnimal.startBeat + currentAnimal.length;

            for (int i = 0; i < EXPIRATION_TIMES; i++)
            {
                if (_animalPoolIndex + i + 1 >= _queuedAnimals.Count)
                {
                    expBeat = double.MaxValue;
                    break;
                }

                var seekedAnimal = _queuedAnimals[_animalPoolIndex + i + 1];
                expBeat += seekedAnimal.length;
            }

            animal.Init(currentAnimal.startBeat, expBeat);

            animal.transform.localPosition = new Vector3(_animalSummatedDistance, 0, 0);
            _animalSummatedDistance += animal.NextAnimalDistance;

            _animalPoolIndex++;
        }

        public override void OnGameSwitch(double beat)
        {
            GetAnimals(beat);
        }

        public override void OnPlay(double beat)
        {
            GetAnimals(beat);
        }

        private void GetAnimals(double beat)
        {
            for (int i = 0; i < POOL_NUMBER; i++)
            {
                _pooledElephants.Add(Instantiate(_elephant, transform));
                _pooledGiraffes.Add(Instantiate(_giraffe, transform));
                _pooledMonkeysLong.Add(Instantiate(_monkeysLong, transform));
                _pooledMonkeysShort.Add(Instantiate(_monkeysShort, transform));
            }

            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat);
            double nextGameSwitchBeat = double.MaxValue;
            if (allGameSwitches.Count > 0) nextGameSwitchBeat = allGameSwitches[0].beat;

            var allAnimals = EventCaller.GetAllInGameManagerList("animalAcrobat", new string[]
            {
                "elephant", "giraffe", "monkeys", "monkeyLong", "monkeyShort"
            }).FindAll(x => x.beat >= beat && x.beat < nextGameSwitchBeat);
            if (allAnimals.Count == 0) return;

            double goodBeat = allAnimals[0].beat;
            for (int i = 0; i < allAnimals.Count; i++)
            {
                var animal = allAnimals[i];
                if (animal.beat != goodBeat) continue;
                goodBeat += animal.length;

                if (animal.datamodel == "animalAcrobat/monkeys")
                {
                    _queuedAnimals.Add(new QueuedAnimal()
                    {
                        startBeat = animal.beat,
                        length = 5,
                        type = AnimalType.MonkeysLong,
                    });
                    _queuedAnimals.Add(new QueuedAnimal()
                    {
                        startBeat = animal.beat + 5,
                        length = 3,
                        type = AnimalType.MonkeysShort,
                    });
                    continue;
                }
                _queuedAnimals.Add(new QueuedAnimal()
                {
                    startBeat = animal.beat,
                    length = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => 4,
                        "animalAcrobat/giraffe" => 8,
                        "animalAcrobat/monkeyLong" => 5,
                        "animalAcrobat/monkeyShort" => 3,
                        _ => throw new System.NotImplementedException()
                    },
                    type = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => AnimalType.Elephant,
                        "animalAcrobat/giraffe" => AnimalType.Giraffe,
                        "animalAcrobat/monkeyLong" => AnimalType.MonkeysLong,
                        "animalAcrobat/monkeyShort" => AnimalType.MonkeysShort,
                        _ => throw new System.NotImplementedException()
                    },
                });
            }

            if (_queuedAnimals.Count > 0) 
            {
                _queuedAnimals.Sort((x, y) => x.startBeat.CompareTo(y.startBeat));
                for (int i = 0; i < POOL_NUMBER; i++)
                {
                    BakeNextAvailableAnimal();
                }
            }
        }
    }
}

