using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCPillarManager : MonoBehaviour
    {
        private const int PILLAR_AMOUNT_X = 12;
        private const int PILLAR_AMOUNT_Y = 4;

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

        private void Awake()
        {
            _scrollTransform = transform.parent;

            _pillarStartX = _pillarFirst.localPosition.x;
            _pillarDistanceX = _pillarSecond.localPosition.x - _pillarFirst.localPosition.x;
            _pillarStartY = _pillarFirst.localPosition.y;
            _pillarDistanceY = _pillarUp.localPosition.y - _pillarFirst.localPosition.y;

            for (int i = 0; i < PILLAR_AMOUNT_Y; i++) _pillars.Add(new());

            _pillars[0].Add(_pillarFirst);
            _pillars[0].Add(_pillarSecond);
            _pillars[1].Add(_pillarUp);

            for (int i = 0; i < PILLAR_AMOUNT_Y; i++)
            {
                for (int j = 0; j < PILLAR_AMOUNT_X; j++)
                {
                    if (_pillars.ElementAtOrDefault(i).ElementAtOrDefault(j) != null) continue;
                    Transform spawnedPillar = Instantiate(_pillarFirst, transform);
                    spawnedPillar.localPosition = new Vector3(_pillarStartX + (_pillarDistanceX * j), spawnedPillar.localPosition.y + (_pillarDistanceY * i));
                    _pillars[i].Add(spawnedPillar);
                }
            }
        }

        private void Update()
        {
            float currentScrollX = _scrollTransform.localPosition.x;
            float currentDistanceX = _pillarStartX + (_pillarDistanceX * _pillarIndexX);
            
            if (currentScrollX >= currentDistanceX + (_pillarDistanceX * PILLAR_AMOUNT_X / 2))
            {
                foreach (var pillarRow in _pillars)
                {
                    var p = pillarRow[_pillarIndexX % PILLAR_AMOUNT_X];

                    p.localPosition = new Vector3(p.localPosition.x + (_pillarDistanceX * PILLAR_AMOUNT_X), p.localPosition.y);
                }
                _pillarIndexX++;
            }
            
            float currentScrollY = _scrollTransform.localPosition.y;
            float currentDistanceY = _pillarStartY + (_pillarDistanceY * _pillarIndexY);

            if (currentScrollY >= currentDistanceY + (_pillarDistanceY * PILLAR_AMOUNT_Y / 2))
            {
                foreach (var p in _pillars[_pillarIndexY % PILLAR_AMOUNT_Y])
                {
                    p.localPosition = new Vector3(p.localPosition.x, p.localPosition.y + (_pillarDistanceY * PILLAR_AMOUNT_Y));    
                }
                _pillarIndexY++;
            }
        }
    }
}

