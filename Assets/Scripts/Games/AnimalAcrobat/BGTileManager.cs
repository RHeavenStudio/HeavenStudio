using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class BGTileManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _bgTileFirst;
        [SerializeField] private Transform _bgTileSecond;
        [SerializeField] private Transform _scroll;

        private float _tileDistance;
        private bool _putSecondInFront;
        private float _reachTileDistance;

        private void Awake()
        {
            _tileDistance = _bgTileSecond.transform.localPosition.x - _bgTileFirst.transform.localPosition.x;
            _reachTileDistance = _tileDistance;
        }

        private void LateUpdate()
        {
            if (_scroll.localPosition.x <= -_reachTileDistance)
            {
                var bgTile = _putSecondInFront ? _bgTileSecond : _bgTileFirst;

                bgTile.localPosition = new Vector3(bgTile.localPosition.x + (_tileDistance * 2), bgTile.localPosition.y);

                _putSecondInFront = !_putSecondInFront;
                _reachTileDistance += _tileDistance;
            }
        }
    }
}

