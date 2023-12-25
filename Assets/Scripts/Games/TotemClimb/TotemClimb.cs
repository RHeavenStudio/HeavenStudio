using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.Games.Scripts_TotemClimb;

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
                    defaultLength = 2f,
                    resizable = true
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

        public static TotemClimb instance;

        private void Awake()
        {
            instance = this;
        }

        public override void OnGameSwitch(double beat)
        {
            CalculateStartAndEndBeat(beat);
            HandleBopsOnStart(beat);
        }

        public override void OnPlay(double beat)
        {
            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat <= beat && x.datamodel is "gameManager/switchGame/totemClimb");
            double lastGameSwitchBeat = 0;
            if (allGameSwitches.Count > 0) lastGameSwitchBeat = allGameSwitches[^1].beat;

            CalculateStartAndEndBeat(lastGameSwitchBeat);
            HandleBopsOnStart(beat);
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
            _totemManager.InitBeats(_startBeat);

            BeatAction.New(this, new()
            {
                new(_startBeat - 1, delegate { _jumper.StartJumping(_startBeat - 1); })
            });

            var allStops = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "stop" }).FindAll(x => x.beat > _startBeat && x.beat < nextGameSwitchBeat);
            if (allStops.Count == 0) return;

            _endBeat = allStops[0].beat;
            _totemManager.InitBeats(_startBeat);
        }

        private void HandleBopsOnStart(double beat)
        {
            var e = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "bop" }).Find(x => x.beat < beat && x.beat + x.length > beat);
            if (e == null) return;

            Bop(e.beat, e.length, beat);
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

            _scrollTransform.localPosition = new Vector3(normalizedBeat * _scrollSpeedX, normalizedBeat * _scrollSpeedY);
            _cameraTransform.localPosition = new Vector3(_scrollTransform.localPosition.x * -2, _scrollTransform.localPosition.y * -2);
        }
    }
}

