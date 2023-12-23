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
        private List<Transform> _totems = new();

        private int _totemIndex = 0;

        private void Awake()
        {
            _scrollTransform = transform.parent;
            _totemStartX = _totemTransform.localPosition.x;
            _totemStartY = _totemTransform.localPosition.y;

            _totems.Add(_totemTransform);

            for (int i = 1; i < _totemAmount; i++)
            {
                Transform spawnedTotem = Instantiate(_totemTransform, transform);
                spawnedTotem.transform.localPosition = new Vector3(_totemStartX + (_xDistance * i), _totemStartY + (_yDistance * i));
                _totems.Add(spawnedTotem);
            }
        }

        private void Update()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistanceX = _totemStartX + (_xDistance * _totemIndex);

            if (currentScrollX >= currentDistanceX + (_xDistance * _totemAmount / 2))
            {
                var t = _totems[_totemIndex % _totemAmount];

                t.localPosition = new Vector3(t.localPosition.x + (_xDistance * _totemAmount), t.localPosition.y + (_yDistance * _totemAmount));

                _totemIndex++;
            }
        }
    }
}

