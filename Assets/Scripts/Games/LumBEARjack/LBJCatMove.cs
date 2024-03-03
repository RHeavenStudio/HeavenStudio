using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJCatMove : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _otherPoint;

        [Header("Properties")]
        [SerializeField] private bool _startAtOther;
        [SerializeField] private bool _usePoint = true;
        [SerializeField] private float _slideOffset = 0f;

        private Vector3 _thisPosition;
        private Vector3 _otherPosition;

        private Coroutine _currentMove;

        private void Awake()
        {
            _thisPosition = transform.localPosition;
            if (_usePoint)
            {
                _otherPosition = _otherPoint.localPosition;
            }
            else
            {
                _otherPosition = _thisPosition + new Vector3(_slideOffset / 2, 0);
            }
            if (_startAtOther) transform.localPosition = _otherPosition;
        }

        public void Move(double beat, double length, bool inToScene)
        {
            if (_currentMove != null) StopCoroutine(_currentMove);
            _currentMove = StartCoroutine(MoveCo(beat, length, inToScene));
        }

        private IEnumerator MoveCo(double beat, double length, bool inToScene)
        {
            if (length <= 0)
            {
                transform.localPosition = inToScene ? _thisPosition : _otherPosition;
                yield break;
            }
            float normalized = Conductor.instance.GetPositionFromBeat(beat, length, false);
            while (normalized <= 1f)
            {
                normalized = Conductor.instance.GetPositionFromBeat(beat, length);
                Vector3 newPos = Vector3.Lerp(inToScene ? _otherPosition : _thisPosition, inToScene ? _thisPosition : _otherPosition, normalized);
                transform.localPosition = newPos;
                yield return null;
            }
        }
    }
}


