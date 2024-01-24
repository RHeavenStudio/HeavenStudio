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
        private const int BACKGROUND_OBJECT_AMOUNT = 10;
        private const float CLOUD_MOVE_SPEED = 0.75f;

        [Header("Components")]
        [SerializeField] private Transform _pillarFirst;
        [SerializeField] private Transform _pillarSecond;
        [SerializeField] private Transform _pillarUp;
        [SerializeField] private Transform _backgroundObjectsParent;
        [SerializeField] private List<BackgroundScrollPair> _backgroundObjects;


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

        private bool _endDistanceSet = false;

        private void Awake()
        {
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

            if (_pillarFirst.localPosition.y >= _endDistance)
            {
                _pillarFirst.GetChild(0).gameObject.SetActive(true);
                _pillarSecond.GetChild(0).gameObject.SetActive(true);
                _pillarUp.gameObject.SetActive(false);
            }
            else if (_pillarUp.localPosition.y >= _endDistance)
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

                    if (spawnedPillar.localPosition.y >= _endDistance)
                    {
                        spawnedPillar.GetChild(0).gameObject.SetActive(true);
                        _hasReachedEnd = true;
                    }
                }
            }

            if (_endDistanceSet)
            {
                _backgroundObjectsParent.gameObject.SetActive(true);
                _backgroundObjectsParent.localPosition = new Vector3(0, _endDistance);

                foreach (var b in _backgroundObjects)
                {
                    b.InitClones(_backgroundObjectsParent);
                }
            }
        }

        private void Update()
        {
            PillarUpdate();
            if (!_endDistanceSet) return;
            foreach (var b in _backgroundObjects)
            {
                b.ScrollClones(_scrollTransform.localPosition.x);
            }
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

                    if (currentDistanceY >= _endDistance)
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
            _endDistanceSet = true;
            return allPillarEnds[0].beat - startBeat;
        }

        [System.Serializable]
        private class BackgroundScrollPair
        {
            public Transform first;
            public Transform second;
            public bool isCloud;

            private List<Transform> _objects = new();

            private int _index = 0;

            private float _startX;
            private float _xDistance;
            private float _moveX;

            private float GetDistance()
            {
                return second.localPosition.x - first.localPosition.x;
            }

            public void InitClones(Transform parent)
            {
                _xDistance = GetDistance();
                _startX = first.localPosition.x;
                _objects.Add(first);
                _objects.Add(second);

                for (int i = 0; i < BACKGROUND_OBJECT_AMOUNT; i++)
                {
                    Transform spawnedObject = Instantiate(first, parent);
                    spawnedObject.localPosition = new Vector3(second.localPosition.x + (_xDistance * (i + 1)), first.localPosition.y, first.localPosition.z);
                    _objects.Add(spawnedObject);
                }
            }

            public void ScrollClones(float currentScrollX)
            {
                if (isCloud)
                {
                    foreach (var b in _objects)
                    {
                        b.localPosition += new Vector3(-Time.deltaTime * CLOUD_MOVE_SPEED, 0);
                    }
                    _moveX -= Time.deltaTime * CLOUD_MOVE_SPEED;
                }
                float currentDistanceX = _startX + (_xDistance * _index) + (_xDistance * (BACKGROUND_OBJECT_AMOUNT + 2) / 2) + _moveX;

                if (currentScrollX >= currentDistanceX)
                {
                    var b = _objects[_index % BACKGROUND_OBJECT_AMOUNT];
                    b.localPosition += new Vector3(_xDistance * BACKGROUND_OBJECT_AMOUNT, 0);

                    _index++;
                    ScrollClones(currentScrollX);
                }
            }
        }
    }
}

