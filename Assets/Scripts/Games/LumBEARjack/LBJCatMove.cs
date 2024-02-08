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

        private Vector3 _thisPosition;

        private Coroutine _currentMove;

        private void Awake()
        {
            _thisPosition = transform.position;
            if (_startAtOther) transform.position = _otherPoint.position;
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
                transform.position = inToScene ? _thisPosition : _otherPoint.position;
                yield break;
            }
            float normalized = Conductor.instance.GetPositionFromBeat(beat, length, false);
            while (normalized <= 1f)
            {
                normalized = Conductor.instance.GetPositionFromBeat(beat, length);
                Vector3 newPos = Vector3.Lerp(inToScene ? _otherPoint.position : _thisPosition, inToScene ? _thisPosition : _otherPoint.position, normalized);
                transform.position = newPos;
                yield return null;
            }
        }
    }
}


