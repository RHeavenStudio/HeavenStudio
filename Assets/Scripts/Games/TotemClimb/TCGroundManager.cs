using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCGroundManager : MonoBehaviour
    {
        private const int GROUND_AMOUNT = 6;

        [Header("Components")]
        [SerializeField] private Transform _groundFirst;
        [SerializeField] private Transform _groundSecond;

        private List<Transform> _grounds = new();
        private Transform _scrollTransform;
        private float _groundDistance;
        private float _groundStart;

        private int _groundIndex = 0;

        private void Awake()
        {
            _scrollTransform = transform.parent;

            _groundStart = _groundFirst.localPosition.x;
            _groundDistance = _groundSecond.localPosition.x - _groundFirst.localPosition.x;

            _grounds.Add(_groundFirst);
            _grounds.Add(_groundSecond);

            for (int i = 2; i < GROUND_AMOUNT; i++)
            {
                Transform spawnedGround = Instantiate(_groundFirst, transform);
                spawnedGround.localPosition = new Vector3(_groundStart + (_groundDistance * i), spawnedGround.localPosition.y);
                _grounds.Add(spawnedGround);
            }
        }

        private void Update()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistance = _groundStart + (_groundDistance * _groundIndex);

            if (currentScrollX >= currentDistance + (_groundDistance * GROUND_AMOUNT / 2))
            {
                var g = _grounds[_groundIndex % GROUND_AMOUNT];

                g.localPosition = new Vector3(g.localPosition.x + (_groundDistance * GROUND_AMOUNT), g.localPosition.y);

                _groundIndex++;
            }
        }
    }
}

