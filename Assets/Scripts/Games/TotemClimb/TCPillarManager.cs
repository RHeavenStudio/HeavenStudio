using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCPillarManager : MonoBehaviour
    {
        private const int PILLAR_AMOUNT_X = 12;
        private const int PILLAR_AMOUNT_Y = 3;
        

        [Header("Components")]
        [SerializeField] private Transform _pillarFirst;
        [SerializeField] private Transform _pillarSecond;
        [SerializeField] private Transform _pillarUp;


        private List<List<Transform>> _pillars = new();
        private Transform _scrollTransform;

        private float _pillarDistanceX;
        private float _pillarStartX;

        private float _pillarDistanceY;
        private float _pillarStartY;

        private int _pillarIndexX = 0;
        private int _pillarIndexY = 0;

        private float _endDistance = float.MaxValue;
        private bool _hasReachedEnd = false;

        private void Awake()
        {
            gameObject.SetActive(!GetStartBeatParam());
            _scrollTransform = transform.parent;
            _endDistance = ((float)GetBeatDistance() * 1.45f) + _pillarStartY;

            _pillarStartX = _pillarFirst.localPosition.x;
            _pillarDistanceX = _pillarSecond.localPosition.x - _pillarFirst.localPosition.x;
            _pillarStartY = _pillarFirst.localPosition.y;
            _pillarDistanceY = _pillarUp.localPosition.y - _pillarFirst.localPosition.y;

            for (int i = 0; i < PILLAR_AMOUNT_Y; i++) _pillars.Add(new());

            _pillars[0].Add(_pillarFirst);
            _pillars[0].Add(_pillarSecond);
            _pillars[1].Add(_pillarUp);

            if (_pillarFirst.localPosition.y + _pillarDistanceY >= _endDistance)
            {
                _pillarFirst.GetChild(0).gameObject.SetActive(true);
                _pillarSecond.GetChild(0).gameObject.SetActive(true);
                _pillarUp.gameObject.SetActive(false);
            }
            else if (_pillarUp.localPosition.y + _pillarDistanceY >= _endDistance)
            {
                _pillarUp.GetChild(0).gameObject.SetActive(true);
            }

            for (int i = 0; i < PILLAR_AMOUNT_Y; i++)
            {
                if (_hasReachedEnd) break;
                for (int j = 0; j < PILLAR_AMOUNT_X; j++)
                {
                    if (_pillars.ElementAtOrDefault(i).ElementAtOrDefault(j) != null) continue;
                    Transform spawnedPillar = Instantiate(_pillarFirst, transform);
                    spawnedPillar.localPosition = new Vector3(_pillarStartX + (_pillarDistanceX * j), spawnedPillar.localPosition.y + (_pillarDistanceY * i));
                    _pillars[i].Add(spawnedPillar);

                    if (spawnedPillar.localPosition.y + _pillarDistanceY >= _endDistance)
                    {
                        spawnedPillar.GetChild(0).gameObject.SetActive(true);
                        _hasReachedEnd = true;
                    }
                }
            }
        }

        private void Update()
        {
            PillarUpdate();
        }

        private void PillarUpdate()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistanceX = _pillarStartX + (_pillarDistanceX * _pillarIndexX);

            if (currentScrollX >= currentDistanceX + (_pillarDistanceX * PILLAR_AMOUNT_X / 2))
            {
                foreach (var pillarRow in _pillars)
                {
                    if (pillarRow.Count <= _pillarIndexX % PILLAR_AMOUNT_X) continue;
                    var p = pillarRow[_pillarIndexX % PILLAR_AMOUNT_X];
                    if (p == null) continue;
                    p.localPosition = new Vector3(p.localPosition.x + (_pillarDistanceX * PILLAR_AMOUNT_X), p.localPosition.y);
                }
                _pillarIndexX++;
                PillarUpdate();
            }

            if (_hasReachedEnd) return;

            float currentScrollY = _scrollTransform.localPosition.y;
            float currentDistanceY = _pillarStartY + (_pillarDistanceY * _pillarIndexY) + (_pillarDistanceY * PILLAR_AMOUNT_Y / 2);

            if (currentScrollY >= currentDistanceY)
            {
                foreach (var p in _pillars[_pillarIndexY % PILLAR_AMOUNT_Y])
                {
                    if (p == null) continue;
                    p.localPosition = new Vector3(p.localPosition.x, p.localPosition.y + (_pillarDistanceY * PILLAR_AMOUNT_Y));

                    if (currentDistanceY + _pillarDistanceY >= _endDistance)
                    {
                        p.GetChild(0).gameObject.SetActive(true);
                        _hasReachedEnd = true;
                    }
                }

                _pillarIndexY++;
                PillarUpdate();
            }
        }

        private double GetBeatDistance()
        {
            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat <= Conductor.instance.songPositionInBeatsAsDouble && x.datamodel is "gameManager/switchGame/totemClimb");
            double lastGameSwitchBeat = 0;
            if (allGameSwitches.Count > 0) lastGameSwitchBeat = allGameSwitches[^1].beat;

            var nextGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > lastGameSwitchBeat && x.datamodel != "gameManager/switchGame/totemClimb");
            double nextGameSwitchBeat = double.MaxValue;
            if (nextGameSwitches.Count > 0)
            {
                nextGameSwitchBeat = nextGameSwitches[0].beat;
            }

            var allStarts = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "start" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (allStarts.Count == 0) return double.MaxValue;

            double startBeat = allStarts[0].beat;

            var allPillarEnds = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "above" }).FindAll(x => x.beat >= startBeat && x.beat < nextGameSwitchBeat);
            if (allPillarEnds.Count == 0) return double.MaxValue;
            return allPillarEnds[0].beat - startBeat;
        }

        private bool GetStartBeatParam()
        {
            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat <= Conductor.instance.songPositionInBeatsAsDouble && x.datamodel is "gameManager/switchGame/totemClimb");
            double lastGameSwitchBeat = 0;
            if (allGameSwitches.Count > 0) lastGameSwitchBeat = allGameSwitches[^1].beat;

            var nextGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > lastGameSwitchBeat && x.datamodel != "gameManager/switchGame/totemClimb");
            double nextGameSwitchBeat = double.MaxValue;
            if (nextGameSwitches.Count > 0)
            {
                nextGameSwitchBeat = nextGameSwitches[0].beat;
            }

            var allStarts = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "start" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (allStarts.Count == 0) return false;

            return allStarts[0]["hide"];
        }
    }
}

