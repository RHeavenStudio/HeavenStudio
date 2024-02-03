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
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.SmallType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    }
                },
                new("big", "Big Object")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.BigType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    }
                },
                new("huge", "Huge Object")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.HugeType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    }
                },
                new("sigh", "Sigh")
                {
                    preFunction = delegate
                    {
                        SoundByte.PlayOneShotGame("lumbearjack/sigh" + (UnityEngine.Random.Range(1, 3) == 1 ? "A" : "B"), eventCaller.currentEntity.beat, 1, 1, false, true);
                    }
                },

                // Stretchable Objects

                new("smallS", "Small Object (Stretchable)")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.SmallType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    },
                    resizable = true
                },
                new("bigS", "Big Object (Stretchable)")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.BigType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    },
                    resizable = true
                },
                new("hugeS", "Huge Object (Stretchable)")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        if (!e["sound"]) return;
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
                        new("type", LumBEARjack.HugeType.log, "Type"),
                        new("sound", true, "Cue Sound")
                    },
                    resizable = true
                },
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

        public void SpawnSmallObject(double beat, double length, SmallType type, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 3), delegate
                {
                    LBJSmallObject spawnedObject = Instantiate(_smallObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type, startUpBeat);
                })
            });
        }

        public void SpawnBigObject(double beat, double length, BigType type, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 4), delegate
                {
                    LBJBigObject spawnedObject = Instantiate(_bigObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type, startUpBeat);
                })
            });
        }

        public void SpawnHugeObject(double beat, double length, HugeType type, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 6), delegate
                {
                    LBJHugeObject spawnedObject = Instantiate(_hugeObjectPrefab, transform);
                    spawnedObject.Init(_bear, beat, length, type, startUpBeat);
                })
            });
        }

        #endregion

        #region StartUp Methods

        public override void OnGameSwitch(double beat)
        {
            PersistObjects(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistObjects(beat);
        }

        private void PersistObjects(double beat)
        {
            List<RiqEntity> allEligibleEvents = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "small", "big", "huge", "smallS", "bigS", "hugeS" }).FindAll(x => x.beat < beat && x.beat + x.length > beat);

            for (int i = 0; i < allEligibleEvents.Count; i++)
            {
                var e = allEligibleEvents[i];

                switch (e.datamodel.Split(1))
                {
                    case "small":
                    case "smallS":
                        SmallObjectSound(e.beat, e.length, (SmallType)e["type"], beat);
                        SpawnSmallObject(e.beat, e.length, (SmallType)e["type"], beat);
                        break;
                    case "big":
                    case "bigS":
                        BigObjectSound(e.beat, e.length, (BigType)e["type"], beat);
                        SpawnBigObject(e.beat, e.length, (BigType)e["type"], beat);
                        break;
                    case "huge":
                    case "hugeS":
                        HugeObjectSound(e.beat, e.length, (HugeType)e["type"], beat);
                        SpawnHugeObject(e.beat, e.length, (HugeType)e["type"], beat);
                        break;
                }
            }
        }

        #endregion

        #region PreSounds

        public static void SmallObjectSound(double beat, float length, SmallType type, double startUpBeat = -1)
        {
            List<MultiSound.Sound> sounds = new();
            if (beat >= startUpBeat) sounds.Add(new("lumbearjack/readyVoice", beat));
            if (beat + (length / 3) >= startUpBeat)
            {
                switch (type)
                {
                    case SmallType.bat:
                    case SmallType.log:
                        sounds.Add(new("lumbearjack/smallLogPut", beat + (length / 3)));
                        break;
                    case SmallType.can:
                        sounds.Add(new("lumbearjack/canPut", beat + (length / 3)));
                        break;
                    case SmallType.broom:
                        sounds.Add(new("lumbearjack/broomPut", beat + (length / 3)));
                        break;
                    default:
                        break;
                }
            }

            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray(), true, true);
        }

        public static void BigObjectSound(double beat, float length, BigType type, double startUpBeat = -1)
        {
            List<MultiSound.Sound> sounds = new();
            if (beat >= startUpBeat) sounds.Add(new("lumbearjack/readyVoice", beat));
            if (beat + (length / 4) >= startUpBeat)
            {
                switch (type)
                {
                    case BigType.log:
                        sounds.Add(new("lumbearjack/bigLogPut", beat + (length / 4)));
                        break;
                    default:
                        break;
                }
            }

            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray(), true, true);
        }

        public static void HugeObjectSound(double beat, float length, HugeType type, double startUpBeat = -1)
        {
            List<MultiSound.Sound> sounds = new();

            if (beat >= startUpBeat) sounds.Add(new("lumbearjack/readyVoice", beat));
            if (beat + (length / 6) >= startUpBeat)
            {
                switch (type)
                {
                    case HugeType.log:
                        sounds.Add(new("lumbearjack/hugeLogPut", beat + (length / 6)));
                        break;
                    case HugeType.freezer:
                        sounds.Add(new("lumbearjack/freezerPut", beat + (length / 6)));
                        break;
                    case HugeType.peach:
                        sounds.Add(new("lumbearjack/peachPut", beat + (length / 6)));
                        break;
                    default:
                        break;
                }
            }

            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray(), true, true);
        }

        #endregion
    }
}