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
                new("bop", "Bop")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.Bop(e.beat, e.length, (LumBEARjack.WhoBops)e["bop"]);
                    },
                    resizable = true,
                    parameters = new()
                    {
                        new("bop", LumBEARjack.WhoBops.Both, "Bop"),
                        new("auto", LumBEARjack.WhoBops.None, "Bop (Auto)")
                    }
                },
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
                new("sigh", "Rest")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.RestBear(e["instant"], e["sound"]);
                    },
                    defaultLength = 4,
                    parameters = new()
                    {
                        new("instant", false, "Instant"),
                        new("sound", true, "Sound")
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

        public enum WhoBops
        {
            Both = 0,
            Bear = 1,
            Cats = 2,
            None = 3
        }

        [Header("Components")]
        [SerializeField] private LBJBear _bear;
        [SerializeField] private LBJSmallObject _smallObjectPrefab;
        [SerializeField] private LBJBigObject _bigObjectPrefab;
        [SerializeField] private LBJHugeObject _hugeObjectPrefab;
        [SerializeField] private Transform _cutObjectHolder;
        [SerializeField] private Animator _catRight;
        [SerializeField] private GameObject[] _catRightObjectsSmall = new GameObject[4];
        [SerializeField] private GameObject[] _catRightObjectsBig = new GameObject[1];
        [SerializeField] private GameObject[] _catRightObjectsHuge = new GameObject[3];

        [Header("Parameters")]
        [SerializeField] private double _catAnimationOffsetStart = -0.5;
        [SerializeField] private double _catAnimationOffsetEnd = 0.5;

        private List<double> _bearNoBopBeats = new();

        public static LumBEARjack instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("lumbearjack", "bop", "auto", false);
            DisableAllCatRightObjects();
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
                    LBJSmallObject spawnedObject = Instantiate(_smallObjectPrefab, _cutObjectHolder);
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
                    LBJBigObject spawnedObject = Instantiate(_bigObjectPrefab, _cutObjectHolder);
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
                    LBJHugeObject spawnedObject = Instantiate(_hugeObjectPrefab, _cutObjectHolder);
                    spawnedObject.Init(_bear, beat, length, type, startUpBeat);
                })
            });
        }

        #endregion

        #region StartUp Methods

        public override void OnGameSwitch(double beat)
        {
            PersistObjects(beat);
            HandleBops(beat);
            HandleCatAnimation(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistObjects(beat);
            HandleBops(beat);
            HandleCatAnimation(beat);
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

        private void HandleBops(double beat)
        {
            List<RiqEntity> allCutEvents = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "small", "big", "huge", "smallS", "bigS", "hugeS" });

            for (int i = 0; i < allCutEvents.Count; i++)
            {
                var e = allCutEvents[i];

                switch (e.datamodel.Split(1))
                {
                    case "small":
                    case "smallS":
                        _bearNoBopBeats.Add(e.beat + (e.length / 3 * 2));
                        if ((SmallType)e["type"] != SmallType.log)
                        {
                            _bearNoBopBeats.Add(e.beat + e.length);
                            _bearNoBopBeats.Add(e.beat + e.length + 1);
                        }
                        break;
                    case "big":
                    case "bigS":
                        _bearNoBopBeats.Add(e.beat + (e.length / 4 * 2));
                        _bearNoBopBeats.Add(e.beat + (e.length / 4 * 3));
                        break;
                    case "huge":
                    case "hugeS":
                        _bearNoBopBeats.Add(e.beat + (e.length / 6 * 2));
                        _bearNoBopBeats.Add(e.beat + (e.length / 6 * 3));
                        _bearNoBopBeats.Add(e.beat + (e.length / 6 * 4));
                        _bearNoBopBeats.Add(e.beat + (e.length / 6 * 5));
                        break;
                }
            }

            List<RiqEntity> allEligibleBops = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "bop" }).FindAll(x => x.beat < beat && x.beat + x.length > beat);

            foreach (var e in allEligibleBops)
            {
                Bop(e.beat, e.length, (WhoBops)e["bop"], beat);
            }
        }

        private void HandleCatAnimation(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "small", "big", "huge", "smallS", "bigS", "hugeS" });
            var nextGameSwitch = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat > beat && x.datamodel.Split(2) != "lumbearjack");

            double nextGameSwitchBeat = double.MaxValue;
            if (nextGameSwitch != null) nextGameSwitchBeat = nextGameSwitch.beat;


            foreach (var e in allEvents)
            {
                if (e.beat + _catAnimationOffsetStart < beat || e.beat < nextGameSwitchBeat)
                {
                    switch (e.datamodel.Split(1))
                    {
                        case "small":
                        case "smallS":
                            CatPutObject(e.beat, e.length, (SmallType)e["type"]);
                            break;
                        case "big":
                        case "bigS":
                            CatPutObject(e.beat, e.length, (BigType)e["type"]);
                            break;
                        case "huge":
                        case "hugeS":
                            CatPutObject(e.beat, e.length, (HugeType)e["type"]);
                            break;
                    }
                } 
            }
        }

        #endregion

        #region Bop

        public override void OnLateBeatPulse(double beat)
        {
            switch ((WhoBops)BeatIsInBopRegionInt(beat))
            {
                case WhoBops.Both:
                    if (!_bearNoBopBeats.Contains(beat)) _bear.Bop();
                    if (_catRight.IsPlayingAnimationNames("CatGrab")) break;
                    _catRight.DoScaledAnimationAsync("CatBop", 0.5f);
                    break;
                case WhoBops.Bear:
                    if (!_bearNoBopBeats.Contains(beat)) _bear.Bop();
                    break;
                case WhoBops.Cats:
                    if (_catRight.IsPlayingAnimationNames("CatGrab")) break;
                    _catRight.DoScaledAnimationAsync("CatBop", 0.5f);
                    break;
                default:
                    break;
            }
        }

        public void Bop(double beat, float length, WhoBops who, double startUpBeat = -1)
        {
            if (who == WhoBops.None) return;

            List<BeatAction.Action> actions = new();

            for (int i = 0; i < length; i++)
            {
                if (beat + i < startUpBeat) continue;

                actions.Add(new(beat + i, delegate
                {
                    switch (who)
                    {
                        case WhoBops.Both:
                            _bear.Bop();
                            if (_catRight.IsPlayingAnimationNames("CatGrab")) break;
                            _catRight.DoScaledAnimationAsync("CatBop", 0.5f);
                            break;
                        case WhoBops.Bear:
                            _bear.Bop();
                            break;
                        case WhoBops.Cats:
                            if (_catRight.IsPlayingAnimationNames("CatGrab")) break;
                            _catRight.DoScaledAnimationAsync("CatBop", 0.5f);
                            break;
                        default:
                            break;
                    }
                }));
            }

            if (actions.Count > 0) BeatAction.New(this, actions);
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

        #region Cats

        private Coroutine _catRightCoroutine;

        public void CatPutObject(double beat, double length, SmallType type)
        {
            CatPutObjectExec(beat, length / 3, _catRightObjectsSmall[(int)type]);
        }

        public void CatPutObject(double beat, double length, BigType type)
        {
            CatPutObjectExec(beat, length / 4, _catRightObjectsBig[(int)type]);
        }

        public void CatPutObject(double beat, double length, HugeType type)
        {
            CatPutObjectExec(beat, length / 6, _catRightObjectsHuge[(int)type]);
        }

        private void CatPutObjectExec(double beat, double effectiveLength, GameObject objectUsed)
        {
            BeatAction.New(this, new()
            {
                new(beat + (_catAnimationOffsetStart * effectiveLength), delegate
                {
                    if (_catRightCoroutine != null) StopCoroutine(_catRightCoroutine);
                    DisableAllCatRightObjects();
                    _catRightCoroutine = StartCoroutine(CatPutObjectCo(beat, effectiveLength));
                }),
                new(beat, delegate
                {
                    objectUsed.SetActive(true);
                }),
                new(beat + effectiveLength, delegate
                {
                    DisableAllCatRightObjects();
                })
            });
        }

        private IEnumerator CatPutObjectCo(double beat, double effectiveLength)
        {
            double s = _catAnimationOffsetStart * effectiveLength;
            double e = _catAnimationOffsetEnd * effectiveLength;

            float normalizedBeat = conductor.GetPositionFromBeat(beat + s, effectiveLength + e - s, false);
            _catRight.DoNormalizedAnimation("CatGrab", Mathf.Clamp01(normalizedBeat));

            while (normalizedBeat <= 1)
            {
                normalizedBeat = conductor.GetPositionFromBeat(beat + s, effectiveLength + e - s, false);

                _catRight.DoNormalizedAnimation("CatGrab", Mathf.Clamp01(normalizedBeat));

                yield return null;
            }
        }

        private void DisableAllCatRightObjects()
        {
            foreach (var g in _catRightObjectsSmall)
            {
                if (g == null) continue;
                g.SetActive(false);
            }

            foreach (var g in _catRightObjectsBig)
            {
                if (g == null) continue;
                g.SetActive(false);
            }

            foreach (var g in _catRightObjectsHuge)
            {
                if (g == null) continue;
                g.SetActive(false);
            }
        }

        #endregion

        #region Misc

        public void RestBear(bool instant, bool sound)
        {
            _bear.Rest(instant, sound);
        }

        #endregion
    }
}