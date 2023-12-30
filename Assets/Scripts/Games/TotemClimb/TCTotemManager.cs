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
        private TotemClimb _game;

        private void Awake()
        {
            _game = TotemClimb.instance;
            _scrollTransform = transform.parent;
            _totemStartX = _totemTransform.localPosition.x;
            _totemStartY = _totemTransform.localPosition.y;

            _totems.Add(_totemTransform.GetComponent<TCTotem>());

            for (int i = 1; i < _totemAmount; i++)
            {
                Transform spawnedTotem = Instantiate(_totemTransform, transform);
                spawnedTotem.transform.localPosition = new Vector3(_totemStartX + (_xDistance * i), _totemStartY + (_yDistance * i));
                _totems.Add(spawnedTotem.GetComponent<TCTotem>());
            }
        }

        public void InitBeats(double startBeat)
        {
            for (int i = 0; i < _totems.Count; i++)
            {
                _totems[i].beat = startBeat + i;
                _totems[i].transform.gameObject.SetActive(_totems[i].beat - 1 < _game.EndBeat && !_game.IsTripleOrHighBeat(_totems[i].beat));
            }

            foreach (var e in _game._tripleEvents) 
            { 
                for (int i = 0; i < e.length; i += 2)
                {
                    double beat = e.beat + i;
                    Transform spawnedFrog = Instantiate(_frogTransform, transform);
                    spawnedFrog.transform.localPosition += new Vector3(_xDistance * (float)(beat - startBeat), _yDistance * (float)(beat - startBeat));
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

        private void Update()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistanceX = _totemStartX + (_xDistance * _totemIndex);

            if (currentScrollX >= currentDistanceX + (_xDistance * _totemAmount / 2))
            {
                var t = _totems[_totemIndex % _totemAmount];

                t.transform.localPosition = new Vector3(t.transform.localPosition.x + (_xDistance * _totemAmount), t.transform.localPosition.y + (_yDistance * _totemAmount));
                t.beat += _totemAmount;
                t.transform.gameObject.SetActive(t.beat - 1 < _game.EndBeat && !_game.IsTripleOrHighBeat(t.beat));
                _totemIndex++;
            }
        }
    }
}

