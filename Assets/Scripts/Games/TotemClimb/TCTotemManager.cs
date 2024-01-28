using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCTotemManager : MonoBehaviour
    {
        [SerializeField] private Transform _totemTransform;
        [SerializeField] private Transform _frogTransform;
        [SerializeField] private Transform _endTotemTransform;
        [SerializeField] private Animator _endTotemAnimator;
        [SerializeField] private Transform _endJumperPoint;
        [SerializeField] private ParticleSystem _endParticleLeft;
        [SerializeField] private ParticleSystem _endParticleRight;
        [SerializeField] private TCDragon _dragon;
        [SerializeField] private float _xDistance;
        [SerializeField] private float _yDistance;
        [SerializeField] private int _totemAmount = 12;

        private Transform _scrollTransform;
        private float _totemStartX;
        private float _totemStartY;
        private List<TCTotem> _totems = new();
        private List<TCFrog> _frogs = new();
        private List<TCDragon> _dragons = new();

        private int _totemIndex = 0;

        private float _pillarEndDistance;

        private bool _usingEndTotem = false;

        public Transform EndJumperPoint => _endJumperPoint;
        public Animator EndTotemAnimator => _endTotemAnimator;

        private TotemClimb _game;

        private void Awake()
        {
            _game = TotemClimb.instance;
            _scrollTransform = transform.parent;
            _totemStartX = _totemTransform.localPosition.x;
            _totemStartY = _totemTransform.localPosition.y;

            var pillarEnd = GetPillarEndBeatDistance();

            _pillarEndDistance = pillarEnd;

            _totems.Add(_totemTransform.GetComponent<TCTotem>());

            for (int i = 1; i < _totemAmount; i++)
            {
                Transform spawnedTotem = Instantiate(_totemTransform, transform);
                spawnedTotem.transform.localPosition = new Vector3(_totemStartX + (_xDistance * i), _totemStartY + (_yDistance * i));
                _totems.Add(spawnedTotem.GetComponent<TCTotem>());
            }
        }

        public void InitBeats(double startBeat, double endBeat, bool useEndTotem)
        {
            _usingEndTotem = useEndTotem;
            for (int i = 0; i < _totems.Count; i++)
            {
                _totems[i].beat = startBeat + i;
                _totems[i].transform.gameObject.SetActive(_totems[i].beat - (_usingEndTotem ? 0 : 1) < _game.EndBeat && !_game.IsTripleOrHighBeat(_totems[i].beat));
            }

            bool startBeatParam = GetStartBeatParam();
            foreach (var e in _game._tripleEvents) 
            { 
                for (int i = 0; i < e.length; i += 2)
                {
                    double beat = e.beat + i;
                    Transform spawnedFrog = Instantiate(_frogTransform, transform);
                    spawnedFrog.transform.localPosition += new Vector3(_xDistance * (float)(beat - startBeat), _yDistance * (float)(beat - startBeat));
                    if (spawnedFrog.transform.localPosition.y >= _pillarEndDistance || startBeatParam) spawnedFrog.GetComponent<TCFrog>().SetHasWings();
                    spawnedFrog.gameObject.SetActive(true);
                    spawnedFrog.GetComponent<TCFrog>().beat = beat;
                    _frogs.Add(spawnedFrog.GetComponent<TCFrog>());
                }
            }

            foreach (var e in _game._highJumpEvents)
            {
                double beat = e.beat;
                TCDragon spawnedDragon = Instantiate(_dragon, transform);
                spawnedDragon.transform.localPosition += new Vector3(_xDistance * (float)(beat - startBeat), _yDistance * (float)(beat - startBeat));
                spawnedDragon.gameObject.SetActive(true);
                spawnedDragon.beat = beat;
                _dragons.Add(spawnedDragon);
            }

            if (useEndTotem)
            {
                _endTotemTransform.gameObject.SetActive(true);
                _endTotemTransform.localPosition += new Vector3(_xDistance * (float)(endBeat - startBeat), _yDistance * (float)(endBeat - startBeat));
            }
        }

        public void ActivateEndParticles()
        {
            _endParticleLeft.PlayScaledAsync(0.5f);
            _endParticleRight.PlayScaledAsync(0.5f);
        }

        public void BopTotemAtBeat(double beat)
        {
            var t = _totems.Find(x => x.beat == beat);
            if (t == null) return;

            t.Bop();
        }

        public Transform GetJumperPointAtBeat(double beat)
        {
            var t = _totems.Find(x => x.beat == beat);
            if (t == null)
            {
                Debug.Log($"Jumper Point unavaible at beat {beat}.");
                return null;
            }
            return t.JumperPoint;
        }

        public Transform GetJumperFrogPointAtBeat(double beat, int part)
        {
            var f = _frogs.Find(x => beat >= x.beat && beat < x.beat + 2);
            if (f == null)
            {
                Debug.Log($"Jumper Frog Point unavaible at beat {beat}.");
                return null;
            }
            switch (part)
            {
                case -1:
                    return f.JumperPointLeft;
                case 0:
                    return f.JumperPointMiddle;
                default:
                    return f.JumperPointRight;
            }
        }

        public void FallFrogAtBeat(double beat, int part)
        {
            var f = _frogs.Find(x => beat >= x.beat && beat < x.beat + 2);
            if (f == null)
            {
                Debug.Log($"Frog unavaible at beat {beat}.");
                return;
            }

            f.FallPiece(part);
        }

        public Transform GetHighJumperPointAtBeat(double beat)
        {
            var d = _dragons.Find(x => beat >= x.beat && beat < x.beat + 4);
            if (d == null)
            {
                Debug.Log($"Jumper Dragon Point unavaible at beat {beat}.");
                return null;
            }
            return d.JumperPoint;
        }

        public void HoldDragonAtBeat(double beat)
        {
            var d = _dragons.Find(x => beat >= x.beat && beat < x.beat + 4);
            if (d == null)
            {
                Debug.Log($"Dragon unavaible at beat {beat}.");
                return;
            }
            d.Hold();
        }

        public void ReleaseDragonAtBeat(double beat)
        {
            var d = _dragons.Find(x => beat >= x.beat && beat < x.beat + 4);
            if (d == null)
            {
                Debug.Log($"Dragon unavaible at beat {beat}.");
                return;
            }
            d.Release();
        }

        private void Update()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistanceX = _totemStartX + (_xDistance * _totemIndex);

            if (currentScrollX >= currentDistanceX + (_xDistance * _totemAmount / 2))
            {
                var t = _totems[_totemIndex % _totemAmount];

                t.transform.localPosition = new Vector3(t.transform.localPosition.x + (_xDistance * _totemAmount), t.transform.localPosition.y + (_yDistance * _totemAmount));
                t.beat += _totemAmount;
                t.transform.gameObject.SetActive(t.beat - (_usingEndTotem ? 0 : 1) < _game.EndBeat && !_game.IsTripleOrHighBeat(t.beat));
                _totemIndex++;
            }
        }

        private float GetPillarEndBeatDistance()
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
            if (allStarts.Count == 0) return float.MaxValue;

            double startBeat = allStarts[0].beat;

            var allPillarEnds = EventCaller.GetAllInGameManagerList("totemClimb", new string[] { "above" }).FindAll(x => x.beat >= startBeat && x.beat < nextGameSwitchBeat);
            if (allPillarEnds.Count == 0) return float.MaxValue;
            return (float)(allPillarEnds[0].beat - startBeat) * 1.45f;
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

