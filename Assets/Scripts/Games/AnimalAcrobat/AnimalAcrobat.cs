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
                new GameAction("bop", "Bop")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        AnimalAcrobat.instance.Bop(e.beat, e.length);
                    },
                    resizable = true
                },
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

        [Header("Components")]
        [SerializeField] private Transform _scroll;
        [SerializeField] private AcrobatPlayer _playerMonkey;

        [Header("Values")]
        [SerializeField] private float _jumpDistance = 8;
        [SerializeField] private float _jumpDistanceGiraffe = 16;
        [SerializeField] private float _jumpStartCameraDistance = 4;
        [SerializeField] private float _jumpStartDistance = -1;
        [SerializeField] private float _giraffeCameraZoom = 5;

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
            public float rotationDistance;
            public float rotationHeight;
            public Util.EasingFunction.Ease ease;

            public double GetHoldLengthFromType()
            {
                return type switch
                {
                    AnimalType.Elephant => 2,
                    AnimalType.Giraffe => 4,
                    AnimalType.MonkeysLong => 3,
                    AnimalType.MonkeysShort => 1,
                    _ => throw new System.NotImplementedException(),
                };
            }
        }

        private List<QueuedAnimal> _queuedAnimals = new();
        private int _animalPoolIndex = 0;
        private float _animalSummatedDistance = 0;
        private bool _lastAnimalWasGiraffe = false;
        private Util.EasingFunction.Function _funcEaseOut;

        private double _jumpBeat = double.MaxValue;

        public bool MonkeyMissed = false;

        public static AnimalAcrobat instance;

        private void Awake()
        {
            instance = this;
            _funcEaseOut = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuad);
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;
            AnimalPoolUpdate(cond);
            CameraUpdate(cond);
        }

        public void PlayerSetActive(bool active)
        {
            _playerMonkey.gameObject.SetActive(active);
        }

        public void PlayerJump(double beat, bool giraffe)
        {
            float startPoint = GetDistanceAtBeat(beat) - (giraffe ? _jumpDistanceGiraffe : _jumpDistance);

            PlayerSetActive(true);
            if (giraffe) _playerMonkey.JumpBetweenGiraffe(beat, startPoint);
            else _playerMonkey.JumpBetweenAnimals(beat, startPoint);
        }

        private float GetDistanceAtBeat(double beat)
        {
            var animals = _queuedAnimals.FindAll(x => x.startBeat <= beat);

            float result = (animals.Count == 0) ? 0 : _jumpStartCameraDistance;
            for (int i = 0; i < animals.Count; i++)
            {
                result += animals[i].rotationDistance;
                result += (animals[i].type == AnimalType.Giraffe) ? _jumpDistanceGiraffe : _jumpDistance;
            }
            return result;
        }

        public void Bop(double beat, float length)
        {
            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++)
            {
                if (beat + i >= _jumpBeat) break;
                actions.Add(new BeatAction.Action(beat + i, delegate
                {
                    _playerMonkey.Bop();
                }));
            }

            if (actions.Count > 0) BeatAction.New(this, actions);
        }

        private int _animalCameraIndex = 0;
        private float _lastCameraX = 0;
        private double _cameraHoldTime = 1;
        private bool _lastCameraAnimalWasGiraffe = false;

        private void CameraUpdate(Conductor cond)
        {
            if (_animalCameraIndex >= _queuedAnimals.Count || cond.songPositionInBeatsAsDouble < _queuedAnimals[0].startBeat - 1) return;

            var currentAnimal = _queuedAnimals[_animalCameraIndex];

            float distance = (_animalCameraIndex == 0) ? _jumpStartCameraDistance : (_lastCameraAnimalWasGiraffe ? _jumpDistanceGiraffe : _jumpDistance);

            float normalizedHold = cond.GetPositionFromBeat(currentAnimal.startBeat, currentAnimal.GetHoldLengthFromType());

            if (normalizedHold < 0)
            {
                float normalizedTravel = cond.GetPositionFromBeat(currentAnimal.startBeat - _cameraHoldTime, _cameraHoldTime);

                float newX = Mathf.Lerp(_lastCameraX, _lastCameraX + distance, normalizedTravel);
                float newZ = 0;
                if (_lastCameraAnimalWasGiraffe)
                {
                    if (normalizedTravel < 0.5)
                    {
                        float normalizedOut = cond.GetPositionFromBeat(currentAnimal.startBeat - _cameraHoldTime, 1);
                        newZ = _funcEaseOut(0, _giraffeCameraZoom, Mathf.Clamp01(normalizedOut));
                    }
                    else
                    {
                        float normalizedIn = Mathf.Clamp01(cond.GetPositionFromBeat(currentAnimal.startBeat - _cameraHoldTime + 3, 1));
                        newZ = _funcEaseOut(_giraffeCameraZoom, 0, normalizedIn);
                    }
                }

                _scroll.localPosition = new Vector3(-newX, 0, newZ);

                return;
            }
            if (normalizedHold >= 0 && normalizedHold <= 1)
            {
                var func = Util.EasingFunction.GetEasingFunction(currentAnimal.ease);

                float newX = func(_lastCameraX + distance, _lastCameraX + distance + currentAnimal.rotationDistance, normalizedHold);

                float angle = func(0, 180, normalizedHold);
                float newY = Mathf.Sin(angle * Mathf.Deg2Rad) * currentAnimal.rotationHeight;

                _scroll.localPosition = new Vector3(-newX, -newY, 0);
                return;
            }
            _cameraHoldTime = (currentAnimal.type == AnimalType.Giraffe) ? 4 : 2;
            _lastCameraX += distance + currentAnimal.rotationDistance;
            _animalCameraIndex++;
            _lastCameraAnimalWasGiraffe = currentAnimal.type == AnimalType.Giraffe;
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
            if (_animalPoolIndex == 0) _animalSummatedDistance = _jumpStartDistance;
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

            animal.Init(currentAnimal.startBeat, expBeat, _lastAnimalWasGiraffe);

            _animalSummatedDistance += (animal.GetRotationDistance() * 0.5f);

            animal.transform.localPosition = new Vector3(_animalSummatedDistance, 0, 0);
            
            _animalSummatedDistance += (animal.GetRotationDistance() * 0.5f) + ((currentAnimal.type == AnimalType.Giraffe) ? _jumpDistanceGiraffe : _jumpDistance);
            _lastAnimalWasGiraffe = currentAnimal.type == AnimalType.Giraffe;

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
                _pooledElephants.Add(Instantiate(_elephant, _scroll));
                _pooledGiraffes.Add(Instantiate(_giraffe, _scroll));
                _pooledMonkeysLong.Add(Instantiate(_monkeysLong, _scroll));
                _pooledMonkeysShort.Add(Instantiate(_monkeysShort, _scroll));
            }

            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat);
            double nextGameSwitchBeat = double.MaxValue;
            if (allGameSwitches.Count > 0) nextGameSwitchBeat = allGameSwitches[0].beat;

            var allAnimals = EventCaller.GetAllInGameManagerList("animalAcrobat", new string[]
            {
                "elephant", "giraffe", "monkeys", "monkeyLong", "monkeyShort"
            }).FindAll(x => x.beat >= beat && x.beat < nextGameSwitchBeat);
            if (allAnimals.Count == 0) return;
            allAnimals.Sort((x, y) => x.beat.CompareTo(y.beat));

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
                        rotationDistance = _monkeysLong.GetRotationDistance(),
                        rotationHeight = _monkeysLong.GetRotationHeight(),
                        ease = _monkeysLong.Ease
                    });
                    _queuedAnimals.Add(new QueuedAnimal()
                    {
                        startBeat = animal.beat + 5,
                        length = 3,
                        type = AnimalType.MonkeysShort,
                        rotationDistance = _monkeysShort.GetRotationDistance(),
                        rotationHeight = _monkeysShort.GetRotationHeight(),
                        ease = _monkeysShort.Ease
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
                    rotationDistance = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => _elephant.GetRotationDistance(),
                        "animalAcrobat/giraffe" => _giraffe.GetRotationDistance(),
                        "animalAcrobat/monkeyLong" => _monkeysLong.GetRotationDistance(),
                        "animalAcrobat/monkeyShort" => _monkeysShort.GetRotationDistance(),
                        _ => throw new System.NotImplementedException()
                    },
                    rotationHeight = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => _elephant.GetRotationHeight(),
                        "animalAcrobat/giraffe" => _giraffe.GetRotationHeight(),
                        "animalAcrobat/monkeyLong" => _monkeysLong.GetRotationHeight(),
                        "animalAcrobat/monkeyShort" => _monkeysShort.GetRotationHeight(),
                        _ => throw new System.NotImplementedException()
                    },
                    ease = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => _elephant.Ease,
                        "animalAcrobat/giraffe" => _giraffe.Ease,
                        "animalAcrobat/monkeyLong" => _monkeysLong.Ease,
                        "animalAcrobat/monkeyShort" => _monkeysShort.Ease,
                        _ => throw new System.NotImplementedException()
                    }
                });
            }

            if (_queuedAnimals.Count > 0) 
            {
                for (int i = 0; i < POOL_NUMBER - 1; i++)
                {
                    BakeNextAvailableAnimal();
                }

                _jumpBeat = _queuedAnimals[0].startBeat - 1;
                if (_queuedAnimals[0].startBeat - 1 >= beat) SoundByte.PlayOneShotGame("animalAcrobat/start", _jumpBeat);
                _playerMonkey.InitialJump(_jumpBeat);
            }
        }
    }
}

