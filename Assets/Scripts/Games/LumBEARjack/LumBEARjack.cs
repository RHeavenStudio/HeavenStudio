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
                        LumBEARjack.instance.SpawnSmallObject(e.beat, e.length, (LumBEARjack.SmallType)e["type"], (LumBEARjack.HuhChoice)e["huh"], (LumBEARjack.CatPutChoice)e["cat"], e["bomb"]);
                    },
                    defaultLength = 3,
                    parameters = new()
                    {
                        new("type", LumBEARjack.SmallType.log, "Type", "", new()
                        {
                            new((x, _) => (LumBEARjack.SmallType)x == LumBEARjack.SmallType.barrel, new string[] { "bomb" })
                        }),
                        new("sound", true, "Cue Sound"),
                        new("huh", LumBEARjack.HuhChoice.ObjectSpecific, "Huh"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side"),
                        new("bomb", true, "Bomb")
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
                        LumBEARjack.instance.SpawnBigObject(e.beat, e.length, (LumBEARjack.BigType)e["type"], (LumBEARjack.CatPutChoice)e["cat"]);
                    },
                    defaultLength = 4,
                    parameters = new()
                    {
                        new("type", LumBEARjack.BigType.log, "Type"),
                        new("sound", true, "Cue Sound"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side")
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
                        LumBEARjack.instance.SpawnHugeObject(e.beat, e.length, (LumBEARjack.HugeType)e["type"], (LumBEARjack.CatPutChoice)e["cat"], e["zoom"], e["baby"]);
                    },
                    defaultLength = 6,
                    parameters = new()
                    {
                        new("type", LumBEARjack.HugeType.log, "Type", "", new()
                        {
                            new((x, _) => (LumBEARjack.HugeType)x == LumBEARjack.HugeType.peach, new string [] { "baby", "pBaby" })
                        }),
                        new("sound", true, "Cue Sound"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side"),
                        new("zoom", true, "Zoom-In"),
                        new("baby", true, "Baby"),
                        new("pBaby", true, "Persist Baby")
                    }
                },
                new("cats", "Cats Presence")
                {
                    resizable = true,
                    parameters = new()
                    {
                        new("main", LumBEARjack.MainCatChoice.Right, "Main Cats"),
                        new("bg", new EntityTypes.Integer(0, 12), "Background Cats"),
                        new("instant", false, "Instant"),
                        new("dance", true, "Dance")
                    }
                },
                new("sigh", "Rest")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.RestBear(e["instant"], (LumBEARjack.RestSoundChoice)e["sound"]);
                    },
                    defaultLength = 3,
                    parameters = new()
                    {
                        new("instant", false, "Instant"),
                        new("sound", LumBEARjack.RestSoundChoice.Random, "Sound")
                    }
                },
                new("snow", "Snow")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        LumBEARjack.instance.SetSnow(e["on"], e["instant"], e["wS"], e["pS"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new()
                    {
                        new("on", true, "Visible"),
                        new("instant", false, "Instant"),
                        new("pS", new EntityTypes.Float(30f, 200f, 30f), "Particle Strength"),
                        new("wS", new EntityTypes.Float(-25f, 25f, 1f), "Wind Strength")
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
                        LumBEARjack.instance.SpawnSmallObject(e.beat, e.length, (LumBEARjack.SmallType)e["type"], (LumBEARjack.HuhChoice)e["huh"], (LumBEARjack.CatPutChoice)e["cat"], e["bomb"]);
                    },
                    defaultLength = 3,
                    parameters = new()
                    {
                        new("type", LumBEARjack.SmallType.log, "Type", "", new()
                        {
                            new((x, _) => (LumBEARjack.SmallType)x == LumBEARjack.SmallType.barrel, new string[] { "bomb" })
                        }),
                        new("sound", true, "Cue Sound"),
                        new("huh", LumBEARjack.HuhChoice.ObjectSpecific, "Huh"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side"),
                        new("bomb", true, "Bomb")
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
                        LumBEARjack.instance.SpawnBigObject(e.beat, e.length, (LumBEARjack.BigType)e["type"], (LumBEARjack.CatPutChoice)e["cat"]);
                    },
                    defaultLength = 4,
                    parameters = new()
                    {
                        new("type", LumBEARjack.BigType.log, "Type"),
                        new("sound", true, "Cue Sound"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side")
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
                        LumBEARjack.instance.SpawnHugeObject(e.beat, e.length, (LumBEARjack.HugeType)e["type"], (LumBEARjack.CatPutChoice)e["cat"], e["zoom"], e["baby"]);
                    },
                    defaultLength = 6,
                    parameters = new()
                    {
                        new("type", LumBEARjack.HugeType.log, "Type", "", new()
                        {
                            new((x, _) => (LumBEARjack.HugeType)x == LumBEARjack.HugeType.peach, new string [] { "baby", "pBaby" })
                        }),
                        new("sound", true, "Cue Sound"),
                        new("cat", LumBEARjack.CatPutChoice.Alternate, "Side"),
                        new("zoom", true, "Zoom-In"),
                        new("baby", true, "Baby"),
                        new("pBaby", true, "Persist Baby")
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
        public enum RestSoundChoice
        {
            Random,
            restA,
            restB,
            NoSound
        }

        public enum SmallType
        {
            log,
            can,
            bat,
            broom,
            barrel,
            book
        }

        public enum BigType
        {
            log,
            bigBall
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

        public enum HuhChoice
        {
            ObjectSpecific,
            Off,
            On
        }

        public enum CatPutChoice
        {
            Alternate,
            Right,
            Left
        }

        public enum MainCatChoice
        {
            Right,
            Left,
            Both
        }

        [Header("Components")]
        [SerializeField] private LBJBear _bear;
        [SerializeField] private LBJBaby _baby;
        [SerializeField] private LBJSmallObject _smallObjectPrefab;
        [SerializeField] private LBJBigObject _bigObjectPrefab;
        [SerializeField] private LBJHugeObject _hugeObjectPrefab;
        [SerializeField] private Transform _cutObjectHolder;

        [SerializeField] private Animator _catRight;
        [SerializeField] private LBJCatMove _catRightMove;
        [SerializeField] private GameObject[] _catRightObjectsSmall = new GameObject[4];
        [SerializeField] private GameObject[] _catRightObjectsBig = new GameObject[1];
        [SerializeField] private GameObject[] _catRightObjectsHuge = new GameObject[3];

        [SerializeField] private Animator _catLeft;
        [SerializeField] private LBJCatMove _catLeftMove;
        [SerializeField] private GameObject[] _catLeftObjectsSmall = new GameObject[4];
        [SerializeField] private GameObject[] _catLeftObjectsBig = new GameObject[1];
        [SerializeField] private GameObject[] _catLeftObjectsHuge = new GameObject[3];

        [SerializeField] private Transform _particleHitPoint;
        [SerializeField] private Transform _particleCutPoint;

        [SerializeField] private LBJBGCat[] _bgCats = new LBJBGCat[12];

        [SerializeField] private LBJMissObject _missObjectRef;

        [SerializeField] private LBJBomb _bombRef;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _smallLogCutParticle;
        [SerializeField] private ParticleSystem _canCutParticle;
        [SerializeField] private ParticleSystem _batCutParticle;
        [SerializeField] private ParticleSystem _broomCutParticle;
        [SerializeField] private ParticleSystem _barrelCutParticle;
        [SerializeField] private ParticleSystem _bookCutParticle;

        [SerializeField] private ParticleSystem _bigLogHitParticle;
        [SerializeField] private ParticleSystem _bigLogCutParticle;
        [SerializeField] private ParticleSystem _bigBallCutParticle;

        [SerializeField] private ParticleSystem _hugeLogHitParticle;
        [SerializeField] private ParticleSystem _hugeLogCutParticle;
        [SerializeField] private ParticleSystem _freezerChipParticle;
        [SerializeField] private ParticleSystem _freezerBreakParticle;
        [SerializeField] private ParticleSystem _peachHitParticle;
        [SerializeField] private ParticleSystem _peachCutParticle;

        [Header("Snow")]
        [SerializeField] private ParticleSystem _snowParticle;
        [SerializeField] private WindZone _wind;

        [Header("Parameters")]
        [SerializeField] private double _catAnimationOffsetStart = -0.5;
        [SerializeField] private double _catAnimationOffsetEnd = 0.5;

        private int _babyIndex = 0;

        private List<double> _bearNoBopBeats = new();
        private Dictionary<double, CatPutChoice> _catPuts = new();
        private Dictionary<double, MainCatChoice> _mainCatPresences = new();
        private MainCatChoice _startMainCat = MainCatChoice.Right;

        private Dictionary<double, int> _bgCatPresences = new();

        public static LumBEARjack instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("lumbearjack", "bop", "auto", false);
            DisableAllCatRightObjects();
            DisableAllCatLeftObjects();
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

        public void SpawnSmallObject(double beat, double length, SmallType type, HuhChoice huh, CatPutChoice cat, bool bomb, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 3), delegate
                {
                    LBJSmallObject spawnedObject = Instantiate(_smallObjectPrefab, _cutObjectHolder);
                    spawnedObject.Init(_bear, beat, length, type, huh, ShouldBeRight(beat, cat), bomb, startUpBeat);
                })
            });
        }

        public void SpawnBigObject(double beat, double length, BigType type, CatPutChoice cat, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 4), delegate
                {
                    LBJBigObject spawnedObject = Instantiate(_bigObjectPrefab, _cutObjectHolder);
                    spawnedObject.Init(_bear, beat, length, type, ShouldBeRight(beat, cat), startUpBeat);
                })
            });
        }

        public void SpawnHugeObject(double beat, double length, HugeType type, CatPutChoice cat, bool zoom, bool baby, double startUpBeat = -1)
        {
            BeatAction.New(this, new()
            {
                new(beat + (length / 6), delegate
                {
                    LBJHugeObject spawnedObject = Instantiate(_hugeObjectPrefab, _cutObjectHolder);
                    spawnedObject.Init(_bear, beat, length, type, ShouldBeRight(beat, cat), zoom, baby, startUpBeat);
                })
            });
        }

        private bool ShouldBeRight(double beat, CatPutChoice cat)
        {
            switch (cat)
            {
                case CatPutChoice.Alternate:
                    bool right = _startMainCat != MainCatChoice.Left;
                    bool first = true;

                    foreach (var e in _catPuts)
                    {
                        if (e.Key > beat) break;
                        switch (e.Value)
                        {
                            case CatPutChoice.Alternate:
                                if (!first) right = !right;
                                if (CatPresenceAtBeat(beat) != MainCatChoice.Both) right = CatPresenceAtBeat(beat) != MainCatChoice.Left;
                                break;
                            case CatPutChoice.Right:
                                right = CatPresenceAtBeat(beat) != MainCatChoice.Left;
                                break;
                            case CatPutChoice.Left:
                                right = CatPresenceAtBeat(beat) == MainCatChoice.Right;
                                break;
                        }
                        first = false;
                    }
                    return right;
                case CatPutChoice.Left:
                    return CatPresenceAtBeat(beat) == MainCatChoice.Right;
                default:
                    return CatPresenceAtBeat(beat) != MainCatChoice.Left;
            }
        }

        #endregion

        #region StartUp Methods

        public override void OnGameSwitch(double beat)
        {
            HandleCatPresence(beat);
            PersistObjects(beat);
            HandleBops(beat);
            HandleCatAnimation(beat);
            HandleSnow(beat);
            HandleBaby(beat, false);
        }

        public override void OnPlay(double beat)
        {
            HandleCatPresence(beat);
            PersistObjects(beat);
            HandleBops(beat);
            HandleCatAnimation(beat);
            HandleSnow(beat);
            HandleBaby(beat, true);
        }

        private void HandleBaby(double beat, bool onPlay)
        {
            var allBaby = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "huge", "hugeS" }).FindAll(x => x["baby"] && (HugeType)x["type"] == HugeType.peach && x.beat + x.length - (x.length / 6) < beat);
            var lastGameSwitch = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat < beat && x.datamodel.Split(2) == "lumbearjack");

            double lastGameSwitchBeat = onPlay ? 0 : beat;
            if (lastGameSwitch != null) lastGameSwitchBeat = onPlay ? lastGameSwitch.beat : beat;
            foreach (var e in allBaby)
            {
                if (e["pBaby"] || e.beat >= lastGameSwitchBeat)
                {
                    ActivateBaby(e.beat + e.length - (e.length / 6), e.length / 6);
                }
            }
        }

        private void HandleSnow(double beat)
        {
            var lastSnow = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "snow" }).FindLast(x => x.beat < beat);
            if (lastSnow != null)
            {
                SetSnow(lastSnow["on"], true, lastSnow["wS"], lastSnow["pS"]);
            }
        }

        private void HandleCatPresence(double beat)
        {
            List<RiqEntity> allPresences = EventCaller.GetAllInGameManagerList("lumbearjack", new string[] { "cats" });

            foreach (var e in allPresences)
            {
                if (!_bgCatPresences.ContainsKey(e.beat)) _bgCatPresences.Add(e.beat, e["bg"]);
                if (!_mainCatPresences.ContainsKey(e.beat)) _mainCatPresences.Add(e.beat, (MainCatChoice)e["main"]);
            }

            _startMainCat = CatPresenceBeforeBeat(beat);

            switch (_startMainCat)
            {
                case MainCatChoice.Right:
                    ActivateCatVisualPresence(beat, 0, true, true, true);
                    ActivateCatVisualPresence(beat, 0, false, false, true);
                    break;
                case MainCatChoice.Left:
                    ActivateCatVisualPresence(beat, 0, false, true, true);
                    ActivateCatVisualPresence(beat, 0, true, false, true);
                    break;
                case MainCatChoice.Both:
                    ActivateCatVisualPresence(beat, 0, true, true, true);
                    ActivateCatVisualPresence(beat, 0, true, false, true);
                    break;
            }

            SetBGCats(beat, 0, BGCatPresenceBeforeBeat(beat), BGCatPresenceBeforeBeat(beat), true, true);

            var allPresencesAfterBeat = allPresences.FindAll(x => x.beat >= beat);
            List<BeatAction.Action> actions = new();
            foreach (var e in allPresencesAfterBeat)
            {
                double eventBeat = e.beat;
                double eventLength = e.length;
                bool instant = e["instant"];
                bool dance = e["dance"];

                int beforeBG = BGCatPresenceBeforeBeat(e.beat);
                int atBG = e["bg"];
                actions.Add(new(e.beat, delegate
                {
                    SetBGCats(eventBeat, eventLength, atBG, beforeBG, instant, dance);
                }));

                MainCatChoice beforeCats = CatPresenceBeforeBeat(e.beat);
                MainCatChoice atCats = (MainCatChoice)e["main"];
                if (beforeCats == atCats) continue;

                switch (atCats)
                {
                    case MainCatChoice.Right:
                        actions.Add(
                            new(e.beat, delegate
                            {
                                if (beforeCats == MainCatChoice.Both)
                                {
                                    ActivateCatVisualPresence(eventBeat, eventLength, true, true, true);
                                    ActivateCatVisualPresence(eventBeat, eventLength, false, false, instant);
                                    return;
                                }
                                ActivateCatVisualPresence(eventBeat, eventLength, true, true, instant);
                                ActivateCatVisualPresence(eventBeat, eventLength, false, false, instant);
                            })
                        );
                        break;
                    case MainCatChoice.Left:
                        actions.Add(
                            new(e.beat, delegate
                            {
                                if (beforeCats == MainCatChoice.Both)
                                {
                                    ActivateCatVisualPresence(eventBeat, eventLength, false, true, instant);
                                    ActivateCatVisualPresence(eventBeat, eventLength, true, false, true);
                                    return;
                                }
                                ActivateCatVisualPresence(eventBeat, eventLength, false, true, instant);
                                ActivateCatVisualPresence(eventBeat, eventLength, true, false, instant);
                            })
                        );
                        break;
                    case MainCatChoice.Both:
                        actions.Add(
                            new(e.beat, delegate
                            {
                                ActivateCatVisualPresence(eventBeat, eventLength, true, true, (beforeCats == MainCatChoice.Right) ? true : instant);
                                ActivateCatVisualPresence(eventBeat, eventLength, true, false, (beforeCats == MainCatChoice.Left) ? true : instant);
                            })
                        );
                        break;
                }
            }
            if (actions.Count > 0) BeatAction.New(this, actions);
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
                        SpawnSmallObject(e.beat, e.length, (SmallType)e["type"], (HuhChoice)e["huh"], (CatPutChoice)e["cat"], e["bomb"], beat);
                        break;
                    case "big":
                    case "bigS":
                        BigObjectSound(e.beat, e.length, (BigType)e["type"], beat);
                        SpawnBigObject(e.beat, e.length, (BigType)e["type"], (CatPutChoice)e["cat"], beat);
                        break;
                    case "huge":
                    case "hugeS":
                        HugeObjectSound(e.beat, e.length, (HugeType)e["type"], beat);
                        SpawnHugeObject(e.beat, e.length, (HugeType)e["type"], (CatPutChoice)e["cat"], e["zoom"], e["baby"], beat);
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
                        if (((SmallType)e["type"] != SmallType.log || (HuhChoice)e["huh"] == HuhChoice.On) && (HuhChoice)e["huh"] != HuhChoice.Off)
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
                float effectiveLength = e.length / (e.datamodel.Split(1) switch
                {
                    "small" => 3,
                    "smallS" => 3,
                    "big" => 4,
                    "bigS" => 4,
                    "huge" => 6,
                    "hugeS" => 6,
                    _ => 3
                });
                if ((e.beat + _catAnimationOffsetStart < beat && e.beat + effectiveLength + _catAnimationOffsetEnd > beat) || (e.beat >= beat && e.beat < nextGameSwitchBeat))
                    if (!_catPuts.ContainsKey(e.beat)) _catPuts.Add(e.beat, (CatPutChoice)e["cat"]);
            }

            var sortedCatPuts = _catPuts.OrderBy(x => x.Key);
            _catPuts = sortedCatPuts.ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var e in allEvents)
            {
                float effectiveLength = e.length / (e.datamodel.Split(1) switch
                {
                    "small" => 3,
                    "smallS" => 3,
                    "big" => 4,
                    "bigS" => 4,
                    "huge" => 6,
                    "hugeS" => 6,
                    _ => 3
                });

                if ((e.beat + _catAnimationOffsetStart < beat && e.beat + effectiveLength + _catAnimationOffsetEnd > beat) || (e.beat >= beat && e.beat < nextGameSwitchBeat))
                {
                    bool catRight = ShouldBeRight(e.beat, (CatPutChoice)e["cat"]);

                    switch (e.datamodel.Split(1))
                    {
                        case "small":
                        case "smallS":
                            CatPutObject(e.beat, e.length, (SmallType)e["type"], catRight);
                            break;
                        case "big":
                        case "bigS":
                            CatPutObject(e.beat, e.length, (BigType)e["type"], catRight);
                            break;
                        case "huge":
                        case "hugeS":
                            CatPutObject(e.beat, e.length, (HugeType)e["type"], catRight);
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
                    if (!_catRight.IsPlayingAnimationNames("CatGrab")) _catRight.DoScaledAnimationAsync("CatBop", 0.75f);
                    if (!_catLeft.IsPlayingAnimationNames("CatGrab")) _catLeft.DoScaledAnimationAsync("CatBop", 0.75f);
                    break;
                case WhoBops.Bear:
                    if (!_bearNoBopBeats.Contains(beat)) _bear.Bop();
                    break;
                case WhoBops.Cats:
                    if (!_catRight.IsPlayingAnimationNames("CatGrab")) _catRight.DoScaledAnimationAsync("CatBop", 0.75f);
                    if (!_catLeft.IsPlayingAnimationNames("CatGrab")) _catLeft.DoScaledAnimationAsync("CatBop", 0.75f);
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
                            if (!_catRight.IsPlayingAnimationNames("CatGrab")) _catRight.DoScaledAnimationAsync("CatBop", 0.75f);
                            if (!_catLeft.IsPlayingAnimationNames("CatGrab")) _catLeft.DoScaledAnimationAsync("CatBop", 0.75f);
                            break;
                        case WhoBops.Bear:
                            _bear.Bop();
                            break;
                        case WhoBops.Cats:
                            if (!_catRight.IsPlayingAnimationNames("CatGrab")) _catRight.DoScaledAnimationAsync("CatBop", 0.75f);
                            if (!_catLeft.IsPlayingAnimationNames("CatGrab")) _catLeft.DoScaledAnimationAsync("CatBop", 0.75f);
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
                    case SmallType.barrel:
                        sounds.Add(new("lumbearjack/smallLogPut", beat + (length / 3)));
                        break;
                    case SmallType.can:
                        sounds.Add(new("lumbearjack/canPut", beat + (length / 3)));
                        break;
                    case SmallType.broom:
                        sounds.Add(new("lumbearjack/broomPut", beat + (length / 3)));
                        break;
                    case SmallType.book:
                        sounds.Add(new("lumbearjack/bookPut", beat + (length / 3)));
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
                    case BigType.bigBall:
                        sounds.Add(new("lumbearjack/bigBallPut", beat + (length / 4)));
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
        private Coroutine _catLeftCoroutine;

        public void CatPutObject(double beat, double length, SmallType type, bool right)
        {
            GameObject objectToUse = right ? _catRightObjectsSmall[(int)type] : _catLeftObjectsSmall[(int)type];
            CatPutObjectExec(beat, length / 3, objectToUse, right);
        }

        public void CatPutObject(double beat, double length, BigType type, bool right)
        {
            GameObject objectToUse = right ? _catRightObjectsBig[(int)type] : _catLeftObjectsBig[(int)type];
            CatPutObjectExec(beat, length / 4, objectToUse, right);
        }

        public void CatPutObject(double beat, double length, HugeType type, bool right)
        {
            GameObject objectToUse = right ? _catRightObjectsHuge[(int)type] : _catLeftObjectsHuge[(int)type];
            CatPutObjectExec(beat, length / 6, objectToUse, right);
        }

        private void CatPutObjectExec(double beat, double effectiveLength, GameObject objectUsed, bool right)
        {
            BeatAction.New(this, new()
            {
                new(beat + (_catAnimationOffsetStart * effectiveLength), delegate
                {
                    if (right)
                    {
                        if (_catRightCoroutine != null) StopCoroutine(_catRightCoroutine);
                        DisableAllCatRightObjects();
                        _catRightCoroutine = StartCoroutine(CatPutObjectCo(beat, effectiveLength, _catRight));
                    }
                    else
                    {
                        if (_catLeftCoroutine != null) StopCoroutine(_catLeftCoroutine);
                        DisableAllCatLeftObjects();
                        _catLeftCoroutine = StartCoroutine(CatPutObjectCo(beat, effectiveLength, _catLeft));
                    }
                }),
                new(beat, delegate
                {
                    objectUsed.SetActive(true);
                }),
                new(beat + effectiveLength, delegate
                {
                    if (right) DisableAllCatRightObjects();
                    else DisableAllCatLeftObjects();
                })
            });
        }

        private IEnumerator CatPutObjectCo(double beat, double effectiveLength, Animator cat)
        {
            double s = _catAnimationOffsetStart * effectiveLength;
            double e = _catAnimationOffsetEnd * effectiveLength;

            float normalizedBeat = conductor.GetPositionFromBeat(beat + s, effectiveLength + e - s, false);
            cat.DoNormalizedAnimation("CatGrab", Mathf.Clamp01(normalizedBeat));

            while (normalizedBeat <= 1)
            {
                normalizedBeat = conductor.GetPositionFromBeat(beat + s, effectiveLength + e - s, false);

                cat.DoNormalizedAnimation("CatGrab", Mathf.Clamp01(normalizedBeat));

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

        private void DisableAllCatLeftObjects()
        {
            foreach (var g in _catLeftObjectsSmall)
            {
                if (g == null) continue;
                g.SetActive(false);
            }

            foreach (var g in _catLeftObjectsBig)
            {
                if (g == null) continue;
                g.SetActive(false);
            }

            foreach (var g in _catLeftObjectsHuge)
            {
                if (g == null) continue;
                g.SetActive(false);
            }
        }

        private void ActivateCatVisualPresence(double beat, double length, bool inToScene, bool right, bool instant = false)
        {
            LBJCatMove move = right ? _catRightMove : _catLeftMove;
            move.Move(beat, instant ? 0 : length, inToScene);
        }

        private void SetBGCats(double beat, double length, int bgCats, int beforeBgCats, bool instant, bool dance)
        {
            bgCats = Math.Clamp(bgCats, 0, 12);
            bgCats -= 1;
            beforeBgCats -= 1;
            Debug.Log(bgCats - beforeBgCats);
            
            for (int i = 0; i < _bgCats.Length; i++)
            {
                if (bgCats < beforeBgCats)
                {
                    _bgCats[i].Activate(beat, length, bgCats >= i, instant || !(i > bgCats && i <= beforeBgCats), dance, instant || i <= bgCats, instant);
                }
                else if (bgCats > beforeBgCats)
                {
                    _bgCats[i].Activate(beat, length, bgCats >= i, instant || !(i > beforeBgCats && i <= bgCats), dance, instant || i <= beforeBgCats, instant);
                }
                else
                {
                    _bgCats[i].Activate(beat, length, bgCats >= i, true, dance, true, instant);
                }
            }
        }

        private MainCatChoice CatPresenceAtBeat(double beat)
        {
            MainCatChoice cat = MainCatChoice.Right;
            foreach (var p in _mainCatPresences)
            {
                if (p.Key > beat) break;
                cat = p.Value;
            }
            return cat;
        }

        private MainCatChoice CatPresenceBeforeBeat(double beat)
        {
            MainCatChoice cat = MainCatChoice.Right;
            foreach (var p in _mainCatPresences)
            {
                if (p.Key >= beat) break;
                cat = p.Value;
            }
            return cat;
        }

        private int BGCatPresenceAtBeat(double beat)
        {
            int amount = 0;
            foreach (var p in _bgCatPresences)
            {
                if (p.Key > beat) break;
                amount = p.Value;
            }
            return amount;
        }

        private int BGCatPresenceBeforeBeat(double beat)
        {
            int amount = 0;
            foreach (var p in _bgCatPresences)
            {
                if (p.Key >= beat) break;
                amount = p.Value;
            }
            return amount;
        }

        #endregion

        #region Particles and Effects

        public void ActivateMissEffect(Transform objectToMove, SpriteRenderer objectSr)
        {
            LBJMissObject spawnedMiss = Instantiate(_missObjectRef, transform);
            spawnedMiss.gameObject.SetActive(true);
            spawnedMiss.Activate(objectToMove, objectSr);
        }

        public void ActivateBaby(double beat, float durationMult)
        {
            LBJBaby newBaby = Instantiate(_baby, transform);
            newBaby.Activate(beat, durationMult, _babyIndex);
            _babyIndex++;
        }

        public void SetSnow(bool isOn, bool instant, float windStrength, float particleStrength)
        {
            _snowParticle.gameObject.SetActive(isOn);
            if (!isOn)
            {
                _snowParticle.Stop();
                return;
            }

            var emm = _snowParticle.emission;
            var main = _snowParticle.main;

            emm.rateOverTimeMultiplier = particleStrength;
            main.prewarm = instant;

            _wind.windMain = 0.5f * windStrength;

            _snowParticle.Play();
        }

        public void DoSmallObjectEffect(SmallType type, bool bomb, double beat)
        {
            switch (type)
            {
                case SmallType.log:
                    ParticleSystem spawnedParticle = Instantiate(_smallLogCutParticle, _particleCutPoint);
                    spawnedParticle.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case SmallType.can:
                    ParticleSystem spawnedParticle1 = Instantiate(_canCutParticle, _particleCutPoint);
                    spawnedParticle1.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case SmallType.bat:
                    ParticleSystem spawnedParticle2 = Instantiate(_batCutParticle, _particleCutPoint);
                    spawnedParticle2.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case SmallType.broom:
                    ParticleSystem spawnedParticle3 = Instantiate(_broomCutParticle, _particleCutPoint);
                    spawnedParticle3.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case SmallType.barrel:
                    ParticleSystem spawnedParticle4 = Instantiate(_barrelCutParticle, _particleCutPoint);
                    spawnedParticle4.PlayScaledAsyncAllChildren(0.5f);

                    if (bomb)
                    {
                        LBJBomb spawnedBomb = Instantiate(_bombRef, transform);
                        spawnedBomb.gameObject.SetActive(true);
                        spawnedBomb.startBeat = beat;
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new("lumbearjack/bombCut", beat),
                            new("lumbearjack/bombBreak", beat + 4, 1, 0.2f)
                        });
                    }
                    break;
                case SmallType.book:
                    ParticleSystem spawnedParticle5 = Instantiate(_bookCutParticle, _particleCutPoint);
                    spawnedParticle5.PlayScaledAsyncAllChildren(0.5f);
                    break;
            }
        }

        public void DoBigObjectEffect(BigType type, bool hit)
        {
            switch (type)
            {
                case BigType.log:
                    ParticleSystem spawnedParticle = Instantiate(hit ? _bigLogHitParticle : _bigLogCutParticle, hit ? _particleHitPoint : _particleCutPoint);
                    spawnedParticle.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case BigType.bigBall:
                    if (hit) break;
                    ParticleSystem spawnedParticle2 = Instantiate(_bigBallCutParticle, _particleCutPoint);
                    spawnedParticle2.PlayScaledAsyncAllChildren(0.5f);
                    break;
            }
        }

        public void DoHugeObjectEffect(HugeType type, bool hit)
        {
            switch (type)
            {
                case HugeType.log:
                    ParticleSystem spawnedParticle = Instantiate(hit ? _hugeLogHitParticle : _hugeLogCutParticle, hit ? _particleHitPoint : _particleCutPoint);
                    spawnedParticle.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case HugeType.freezer:
                    ParticleSystem spawnedParticle1 = Instantiate(hit ? _freezerChipParticle : _freezerBreakParticle, hit ? _particleHitPoint : _particleCutPoint);
                    spawnedParticle1.PlayScaledAsyncAllChildren(0.5f);
                    break;
                case HugeType.peach:
                    ParticleSystem spawnedParticle2 = Instantiate(hit ? _peachHitParticle : _peachCutParticle, hit ? _particleHitPoint : _particleCutPoint);
                    spawnedParticle2.PlayScaledAsyncAllChildren(0.5f);
                    break;
            }
        }

        #endregion

        #region Misc

        public void RestBear(bool instant, RestSoundChoice sound)
        {
            _bear.Rest(instant, sound);
        }

        #endregion
    }
}