using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCTotemManager : MonoBehaviour
    {
        [SerializeField] private Transform _totemTransform;
        [SerializeField] private float _xDistance;
        [SerializeField] private float _yDistance;
        [SerializeField] private int _totemAmount = 12;

        private Transform _scrollTransform;
        private float _totemStartX;
        private float _totemStartY;
        private List<Totem> _totems = new();

        private int _totemIndex = 0;
        private TotemClimb _game;

        private class Totem
        {
            public double beat;
            public Transform transform;

            public Totem(Transform mTransform, double mBeat = 0)
            {
                transform = mTransform;
                beat = mBeat;
            }
        }

        private void Awake()
        {
            _game = TotemClimb.instance;
            _scrollTransform = transform.parent;
            _totemStartX = _totemTransform.localPosition.x;
            _totemStartY = _totemTransform.localPosition.y;

            _totems.Add(new(_totemTransform));

            for (int i = 1; i < _totemAmount; i++)
            {
                Transform spawnedTotem = Instantiate(_totemTransform, transform);
                spawnedTotem.transform.localPosition = new Vector3(_totemStartX + (_xDistance * i), _totemStartY + (_yDistance * i));
                _totems.Add(new(spawnedTotem));
            }
        }

        public void InitBeats(double startBeat)
        {
            for (int i = 0; i < _totems.Count; i++)
            {
                _totems[i].beat = startBeat + i;
                if (_totems[i].beat - 1 >= _game.EndBeat) _totems[i].transform.gameObject.SetActive(false);
            }
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
                if (t.beat - 1 >= _game.EndBeat) t.transform.gameObject.SetActive(false);
                _totemIndex++;
            }
        }
    }
}

