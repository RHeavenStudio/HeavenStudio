using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.Games.Scripts_TotemClimb;
using Jukebox;
using System;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class TotemClimbLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("totemClimb", "Totem Climb", "FFFFFF", false, false, new()
            {
                new("start", "Start Jumping")
                {

                },
                new("triple", "Triple Jumping")
                {
                    preFunction = delegate { TotemClimb.TripleJumpSound(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); },
                    defaultLength = 2f,
                    resizable = true
                },
                new("high", "High Jump")
                {
                    preFunction = delegate
                    {
                        double beat = eventCaller.currentEntity.beat;

                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("count-ins/ready1", beat - 2f),
                            new MultiSound.Sound("count-ins/ready2", beat - 1f),
                        }, false, true);
                    },
                    defaultLength = 4f           
                },
                new("stop", "Stop Jumping")
                {

                },
                new("bop", "Bop")
                {
                    function = delegate { TotemClimb.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity.beat); },
                    resizable = true,
                    defaultLength = 4f
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class TotemClimb : Minigame
    {
        [Header("Components")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _scrollTransform;
        [SerializeField] private TCJumper _jumper;
        [SerializeField] private TCTotemManager _totemManager;

        [Header("Properties")]
        [SerializeField] private float _scrollSpeedX = 3.838f;
        [SerializeField] private float _scrollSpeedY = 1.45f;

        private double _startBeat = double.MaxValue;
        private double _endBeat = double.MaxValue;
        public double EndBeat => _endBeat;

        [NonSerialized] public List<RiqEntity> _tripleEvents = new();
        [NonSerialized] public List<RiqEntity> _highJumpEvents = new();

        public static TotemClimb instance;

        private void Awake()
        {
            instance = this;
        }

        public override void OnGameSwitch(double beat)
        {
            CalculateStartAndEndBeat(beat);
            GetHighJumpEvents();
            GetTripleEvents();
            HandleBopsOnStart(beat);
            _totemManager.InitBeats(_startBeat);
            _jumper.InitPath(_startBeat);
        }

        public override void OnPlay(double beat)
        {
            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat <= beat && x.datamodel is "gameManager/switchGame/totemClimb");
            double lastGameSwitchBeat = 0;
            if (allGameSwitches.Count > 0) lastGameSwitchBeat = allGameSwitches[^1].beat;

            CalculateStartAndEndBeat(lastGameSwitchBeat);
            GetHighJumpEvents();
            GetTripleEvents();
            HandleBopsOnStart(beat);
            _totemManager.InitBeats(_startBeat);
            _jumper.InitPath(_startBeat);
        }

        private void CalculateStartAndEndBeat(double beat)
        {
            var nextGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat && x.datamodel != "gameManager/switchGame/totemClimb");
            double nextGameSwitchBeat = double.MaxValue;
            if (nextGameSwitches.Count > 0)
            {
                nextGameSwitchBeat = nextGameSwitches[0].beat;
            }

            var allStarts = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "start" }).FindAll(x => x.beat >= beat && x.beat < nextGameSwitchBeat);
            if (allStarts.Count == 0) return;

            _startBeat = allStarts[0].beat;
            BeatAction.New(this, new()
            {
                new(_startBeat - 1, delegate { _jumper.StartJumping(_startBeat - 1); })
            });

            var allStops = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "stop" }).FindAll(x => x.beat > _startBeat && x.beat < nextGameSwitchBeat);
            if (allStops.Count == 0) return;

            _endBeat = allStops[0].beat;
        }

        private void HandleBopsOnStart(double beat)
        {
            var e = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "bop" }).Find(x => x.beat < beat && x.beat + x.length > beat);
            if (e == null) return;

            Bop(e.beat, e.length, beat);
        }

        private void GetHighJumpEvents()
        {
            var highs = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "high" }).FindAll(x => x.beat >= _startBeat && x.beat < _endBeat);
            if (highs.Count == 0) return;

            highs.Sort((x, y) => x.beat.CompareTo(y.beat));

            var tempHighs = new List<RiqEntity>();

            double goodAfterBeat = _startBeat;

            foreach (var h in highs)
            {
                if (h.beat >= goodAfterBeat && IsOnBeat(_startBeat, h.beat))
                {
                    tempHighs.Add(h);
                    goodAfterBeat = h.beat + 4;
                }
            }

            _highJumpEvents = tempHighs;
        }

        private void GetTripleEvents()
        {
            var triples = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "triple" }).FindAll(x => x.beat >= _startBeat && x.beat + x.length <= _endBeat);
            if (triples.Count == 0) return;

            triples.Sort((x, y) => x.beat.CompareTo(y.beat));

            var tempTriples = new List<RiqEntity>();

            double lastLengthBeat = _startBeat;

            foreach (var t in triples)
            {
                if (t.beat >= lastLengthBeat && IsOnBeat(_startBeat, t.beat))
                {
                    if (_highJumpEvents.Find(x => x.beat + 4f > t.beat && x.beat + 4 < t.beat + t.length + 4) != null) continue; 
                    tempTriples.Add(t);
                    lastLengthBeat = t.beat + t.length;
                }
            }

            _tripleEvents = tempTriples;
        }

        private void Update()
        {
            var cond = Conductor.instance;

            ScrollUpdate(cond);
        }

        public void BopTotemAtBeat(double beat)
        {
            _totemManager.BopTotemAtBeat(beat);
        }

        public Transform GetJumperPointAtBeat(double beat)
        {
            return _totemManager.GetJumperPointAtBeat(beat);
        }

        public Transform GetJumperFrogPointAtBeat(double beat, int part)
        {
            return _totemManager.GetJumperFrogPointAtBeat(beat, part);
        }

        public Transform GetDragonPointAtBeat(double beat)
        {
            return _totemManager.GetHighJumperPointAtBeat(beat);
        }

        public void HoldDragonAtBeat(double beat)
        {
            _totemManager.HoldDragonAtBeat(beat);
        }

        public void ReleaseDragonAtBeat(double beat)
        {
            _totemManager.ReleaseDragonAtBeat(beat);
        }

        public void FallFrogAtBeat(double beat, int part)
        {
            _totemManager.FallFrogAtBeat(beat, part);
        }

        public void Bop(double beat, float length, double callBeat)
        {
            List<BeatAction.Action> actions = new();

            for (int i = 0; i < length; i++)
            {
                double bopBeat = beat + i;
                if (bopBeat < callBeat) continue;
                actions.Add(new(bopBeat, delegate
                {
                    BopJumper(bopBeat);
                }));
            }

            if (actions.Count > 0) BeatAction.New(this, actions);
        }

        private void BopJumper(double beat)
        {
            if (beat >= _startBeat && beat < _endBeat) return;
            _jumper.Bop();
        } 

        private void ScrollUpdate(Conductor cond)
        {
            if (_startBeat == double.MaxValue) return;
            double beatDistance = _endBeat - _startBeat;
            float normalizedBeat = Mathf.Clamp(cond.GetPositionFromBeat(_startBeat, 1), 0f, (float)beatDistance);

            if (IsHighBeatBasedOnStart(normalizedBeat))
            {
                var h = GetHighJumpAtBeatBasedOnStart(normalizedBeat);
                if (h != null)
                {
                    double highBeat = h.beat - _startBeat;
                    if (normalizedBeat >= highBeat + 2)
                    {
                        normalizedBeat = Mathf.Clamp(normalizedBeat - 2 + (cond.GetPositionFromBeat(h.beat + 2, 2) * 2), (float)highBeat, (float)highBeat + 4);
                        Debug.Log(normalizedBeat);
                    }
                    else if (normalizedBeat >= highBeat)
                    {
                        normalizedBeat = Mathf.Clamp(normalizedBeat, 0f, (float)highBeat);
                    }
                }
            }

            _scrollTransform.localPosition = new Vector3(normalizedBeat * _scrollSpeedX, normalizedBeat * _scrollSpeedY);
            _cameraTransform.localPosition = new Vector3(_scrollTransform.localPosition.x * -2, _scrollTransform.localPosition.y * -2);
        }

        private bool IsOnBeat(double startBeat, double targetBeat)
        {
            return (targetBeat - startBeat) % 1 == 0;
        }

        public bool IsTripleBeat(double beat)
        {
            if (_tripleEvents.Count == 0) return false;
            return _tripleEvents.Find(x => beat >= x.beat && beat < x.beat + x.length) != null;
        }

        public bool IsHighBeat(double beat)
        {
            if (_highJumpEvents.Count == 0) return false;
            return _highJumpEvents.Find(x => beat >= x.beat && beat < x.beat + 4) != null;
        }

        public bool IsHighBeatBasedOnStart(double beat)
        {
            if (_highJumpEvents.Count == 0) return false;
            return _highJumpEvents.Find(x => beat >= x.beat - _startBeat && beat < x.beat - _startBeat + 4) != null;
        }

        public RiqEntity GetHighJumpAtBeatBasedOnStart(double beat)
        {
            if (_highJumpEvents.Count == 0) return null;
            return _highJumpEvents.Find(x => beat >= x.beat - _startBeat && beat < x.beat - _startBeat + 4);
        }

        public bool IsTripleOrHighBeat(double beat)
        {
            return IsHighBeat(beat) || IsTripleBeat(beat);
        }

        public static void TripleJumpSound(double beat, float length)
        {
            length = Mathf.Max(length, 2f);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new("totemClimb/beatchange", beat - 2),
                new("totemClimb/beatchange", beat - 1.5f),
                new("totemClimb/beatchange", beat - 1f),
                new("totemClimb/beatchange", beat + length - 2),
                new("totemClimb/beatchange", beat + length - 1),
            }, true, true);
        }
    }
}

