using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    using HeavenStudio.Games.Scripts_AnimalAcrobat;
    using HeavenStudio.Util;
    public class AnimalAcrobat : Minigame
    {
        [SerializeField] private float startCameraMoveLength = 3f;
        [Header("Elephant")]
        [SerializeField] private AcrobatObstacle elephantRef;
        [SerializeField] private float elephantDistance;
        [SerializeField] private float elephantStart = 0.004f;
        [SerializeField] private float elephantHangCameraMoveLength = 2f;
        [SerializeField] private float elephantHangMaxDip = 1f;
        [SerializeField] private float elephantReleaseMoveLength = 3f;
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

        private struct QueuedObstacleCameraMove
        {
            public double startBeat;
            public ObstacleType type;
            public float GetLength()
            {
                return type switch
                {
                    ObstacleType.Elephant => 4f,
                    ObstacleType.Giraffe => 8f,
                    ObstacleType.Monkeys => 5f,
                    ObstacleType.Monkey => 3f,
                    _ => throw new System.NotImplementedException(),
                };
            }
            
            public float GetHoldLength()
            {
                return type switch
                {
                    ObstacleType.Elephant => 2f,
                    ObstacleType.Giraffe => 4f,
                    ObstacleType.Monkeys => 3f,
                    ObstacleType.Monkey => 1f,
                    _ => throw new System.NotImplementedException(),
                };
            }
        }

        private int movesIndex = 0;
        private List<QueuedObstacleCameraMove> moves = new();
        private Vector3 lastCameraPos = new Vector3(0, 0, 0);
        private EasingFunction.Function funcInOut;
        private EasingFunction.Function funcIn;
        private EasingFunction.Function funcOut;

        public static AnimalAcrobat instance;

        private void Awake()
        {
            instance = this;
            funcInOut = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
            funcIn = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
            funcOut = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
        }

        private void LateUpdate()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                CameraMoveUpdate(cond);
            }
        }

        private void CameraMoveUpdate(Conductor cond)
        {
            if (movesIndex >= moves.Count) return;
            var currentMove = moves[movesIndex];
            double songBeat = cond.songPositionInBeats;
            if (cond.songPositionInBeats > currentMove.startBeat + currentMove.GetLength())
            {
                lastCameraPos = GameCamera.additionalPosition;
                movesIndex++;
                CameraMoveUpdate(cond);
                return;
            }

            if (movesIndex == 0 && songBeat >= currentMove.startBeat - 1 && songBeat < currentMove.startBeat)
            {
                float normalizedBeat = cond.GetPositionFromBeat(currentMove.startBeat - 1, 1);
                float newPosX = Mathf.Lerp(lastCameraPos.x, startCameraMoveLength, normalizedBeat);
                GameCamera.additionalPosition = new Vector3(newPosX, 0, 0);
            }
            else if (songBeat >= currentMove.startBeat && songBeat < currentMove.startBeat + currentMove.GetHoldLength())
            {
                float normalizedBeat = cond.GetPositionFromBeat(currentMove.startBeat, currentMove.GetHoldLength());
                float newPosX = 0;
                float newPosY = 0;
                switch (currentMove.type)
                {
                    case ObstacleType.Elephant:
                        if (movesIndex == 0) newPosX = funcInOut(lastCameraPos.x + startCameraMoveLength, lastCameraPos.x + startCameraMoveLength + elephantHangCameraMoveLength, normalizedBeat);
                        else newPosX = funcInOut(lastCameraPos.x, lastCameraPos.x + elephantHangCameraMoveLength, normalizedBeat);

                        if (normalizedBeat <= 0.5f)
                        {
                            float normalizedIn = cond.GetPositionFromBeat(currentMove.startBeat, 1);
                            newPosY = funcIn(0, -elephantHangMaxDip, normalizedIn);
                        }
                        else
                        {
                            float normalizedOut = cond.GetPositionFromBeat(currentMove.startBeat + 1, 1);
                            newPosY = funcOut(-elephantHangMaxDip, 0, normalizedOut);
                        }
                        break;
                    case ObstacleType.Giraffe:
                        break;
                    case ObstacleType.Monkeys:
                        break;
                    case ObstacleType.Monkey:
                        break;
                    default: break;
                }
                GameCamera.additionalPosition = new Vector3(newPosX, newPosY, 0);
            }
            else if (songBeat >= currentMove.startBeat + currentMove.GetHoldLength())
            {
                float normalizedBeat = cond.GetPositionFromBeat(currentMove.startBeat + currentMove.GetHoldLength(), currentMove.GetLength() - currentMove.GetHoldLength());
                float newPosX = 0;
                float newPosY = 0;
                switch (currentMove.type)
                {
                    case ObstacleType.Elephant:
                        if (movesIndex == 0) 
                        {
                            float baseValue = lastCameraPos.x + startCameraMoveLength + elephantHangCameraMoveLength;
                            newPosX = Mathf.Lerp(baseValue, baseValue + elephantReleaseMoveLength, normalizedBeat);
                        } 
                        else
                        {
                            float baseValue = lastCameraPos.x + elephantHangCameraMoveLength;
                            newPosX = Mathf.Lerp(baseValue, baseValue + elephantReleaseMoveLength, normalizedBeat);
                        }
                        break;
                    case ObstacleType.Giraffe:
                        break;
                    case ObstacleType.Monkeys:
                        break;
                    case ObstacleType.Monkey:
                        break;
                    default: break;
                }
                GameCamera.additionalPosition = new Vector3(newPosX, newPosY, 0);
            }
        }

        #region Spawn Animals
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
            double nextGameSwitchBeat = double.MaxValue;
            var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).FindAll(x => x.beat > gameSwitchBeat);
            if (allEnds.Count > 0) nextGameSwitchBeat = allEnds[0].beat;
            foreach (var animal in animalEvents)
            {
                if (animal.beat != nextBeat || animal.beat + animal.length <= gameSwitchBeat || animal.beat > nextGameSwitchBeat) continue;

                nextBeat += animal.length;
                switch (animal.datamodel)
                {
                    case "animalAcrobat/elephant":
                        startPos += elephantDistance;
                        AcrobatObstacle spawnedElephant = Instantiate(elephantRef, transform);
                        spawnedElephant.transform.position = new Vector3(startPos, 0, 0);
                        spawnedElephant.Init(animal.beat, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat,
                            type = ObstacleType.Elephant
                        });
                        break;
                    case "animalAcrobat/giraffe":
                        startPos += giraffeDistance;
                        AcrobatObstacle spawnedGiraffe = Instantiate(giraffeRef, transform);
                        spawnedGiraffe.transform.position = new Vector3(startPos, 0, 0);
                        spawnedGiraffe.Init(animal.beat, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat,
                            type = ObstacleType.Giraffe
                        });
                        break;
                    case "animalAcrobat/monkeys":
                        startPos += monkeysDistance;
                        AcrobatObstacle spawnedMonkeyLong = Instantiate(monkeysRef, transform);
                        spawnedMonkeyLong.transform.position = new Vector3(startPos, 0, 0);
                        spawnedMonkeyLong.Init(animal.beat, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat,
                            type = ObstacleType.Monkeys
                        });
                        startPos += oneMonkeyDistance;
                        AcrobatObstacle spawnedOneMonkeyS = Instantiate(oneMonkeyRef, transform);
                        spawnedOneMonkeyS.transform.position = new Vector3(startPos, 0, 0);
                        spawnedOneMonkeyS.Init(animal.beat + 5, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat + 5,
                            type = ObstacleType.Monkey
                        });
                        break;
                    case "animalAcrobat/monkeyLong":
                        startPos += monkeysDistance;
                        AcrobatObstacle spawnedMonkeys = Instantiate(monkeysRef, transform);
                        spawnedMonkeys.transform.position = new Vector3(startPos, 0, 0);
                        spawnedMonkeys.Init(animal.beat, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat,
                            type = ObstacleType.Monkeys
                        });
                        break;
                    case "animalAcrobat/monkeyShort":
                        startPos += oneMonkeyDistance;
                        AcrobatObstacle spawnedOneMonkey = Instantiate(oneMonkeyRef, transform);
                        spawnedOneMonkey.transform.position = new Vector3(startPos, 0, 0);
                        spawnedOneMonkey.Init(animal.beat, gameSwitchBeat);
                        moves.Add(new QueuedObstacleCameraMove
                        {
                            startBeat = animal.beat,
                            type = ObstacleType.Monkey
                        });
                        break;
                    default: break;
                }
            }
        }
        #endregion
    }
}

