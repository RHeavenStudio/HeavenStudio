using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;


namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class CtrLumBEARjackLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("lumbearjack", "LumBEARjack", "ffffff", false, false, new List<GameAction>()
            {
                new("small", "Small Object")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.SmallObjectSound(e.beat, e.length, (LumBEARjack.SmallType)e["type"]);
                    },
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.SpawnSmallObject(e.beat, e.length, (LumBEARjack.SmallType)e["type"]);
                    },
                    defaultLength = 3,
                    parameters = new()
                    {
                        new("type", LumBEARjack.SmallType.log, "Type")
                    }
                },
                new("big", "Big Object")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.BigObjectSound(e.beat, e.length, (LumBEARjack.BigType)e["type"]);
                    },
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.SpawnBigObject(e.beat, e.length, (LumBEARjack.BigType)e["type"]);
                    },
                    defaultLength = 4,
                    parameters = new()
                    {
                        new("type", LumBEARjack.BigType.log, "Type")
                    }
                },
                new("huge", "Huge Object")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.HugeObjectSound(e.beat, e.length, (LumBEARjack.HugeType)e["type"]);
                    },
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.SpawnHugeObject(e.beat, e.length, (LumBEARjack.HugeType)e["type"]);
                    },
                    defaultLength = 6,
                    parameters = new()
                    {
                        new("type", LumBEARjack.HugeType.log, "Type")
                    }
                }
            }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Games.Scripts_LumBEARjack;

    public class LumBEARjack : Minigame
    {
        public enum SmallType
        {
            log,
            can,
            bat,
            broom
        }

        public enum BigType
        {
            log
        }

        public enum HugeType
        {
            log,
            freezer,
            peach
        }

        [Header("Components")]
        [SerializeField] private LBJBear _bear;
        [SerializeField] private LBJSmallObject _smallObjectPrefab;
        [SerializeField] private LBJBigObject _bigObjectPrefab;
        [SerializeField] private LBJHugeObject _hugeObjectPrefab;

        public static LumBEARjack instance;

        private void Awake()
        {
            instance = this;
        }

        #region Update

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                _bear.SwingWhiff();
            }
        }

        #endregion

        #region Spawn Objects

        public void SpawnSmallObject(double beat, double length, SmallType type)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 3), delegate
                {
                    LBJSmallObject spawnedObject = Instantiate(_smallObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type);
                })
            });
        }

        public void SpawnBigObject(double beat, double length, BigType type)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 4), delegate
                {
                    LBJBigObject spawnedObject = Instantiate(_bigObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type);
                })
            });
        }

        public void SpawnHugeObject(double beat, double length, HugeType type)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 6), delegate
                {
                    LBJHugeObject spawnedObject = Instantiate(_hugeObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type);
                })
            });
        }

        #endregion

        #region PreSounds

        public static void SmallObjectSound(double beat, float length, SmallType type)
        {
            switch (type)
            {
                case SmallType.bat:
                case SmallType.log:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/smallLogPut", beat + (length / 3))
                    }, true, true);
                    break;
                case SmallType.can:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/canPut", beat + (length / 3))
                    }, true, true);
                    break;
                case SmallType.broom:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/broomPut", beat + (length / 3))
                    }, true, true);
                    break;
                default:
                    break;
            }
        }

        public static void BigObjectSound(double beat, float length, BigType type)
        {
            switch (type)
            {
                case BigType.log:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/bigLogPut", beat + (length / 4))
                    }, true, true);
                    break;
                default:
                    break;
            }
        }

        public static void HugeObjectSound(double beat, float length, HugeType type)
        {
            switch (type)
            {
                case HugeType.log:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/hugeLogPut", beat + (length / 6))
                    }, true, true);
                    break;
                case HugeType.freezer:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/freezerPut", beat + (length / 6))
                    }, true, true);
                    break;
                case HugeType.peach:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new("lumbearjack/readyVoice", beat),
                        new("lumbearjack/peachPut", beat + (length / 6))
                    }, true, true);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}