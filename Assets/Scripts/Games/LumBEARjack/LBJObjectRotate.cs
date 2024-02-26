using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJObjectRotate : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float _rotationLeft = 22f;
        [SerializeField] private float _rotationRight = -22f;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;

        [Header("References")]
        [SerializeField] private Transform _pivotLeft;
        [SerializeField] private Transform _pivotRight;
        [SerializeField] private Transform _pivotSingle;
        [SerializeField] private Transform[] _objectsToMove;


        public void Move(double beat, double length, bool right)
        {
            float normalized = Conductor.instance.GetPositionFromBeat(beat - length, length * 2, false);
            if (!right) normalized = 1 - normalized;
            var func = EasingFunction.GetEasingFunction(_ease);

            if (normalized <= 0.5f)
            {
                foreach (var o in _objectsToMove)
                {
                    if (o.parent != _pivotRight) o.SetParent(_pivotRight, true);
                }
                float newRotation = Mathf.Min(func(_rotationRight, -_rotationRight, normalized), 0);
                _pivotRight.localEulerAngles = new Vector3(0, 0, newRotation);
            }
            else
            {
                foreach (var o in _objectsToMove)
                {
                    if (o.parent != _pivotLeft) o.SetParent(_pivotLeft, true);
                }
                float newRotation = Mathf.Max(func(-_rotationLeft, _rotationLeft, normalized), 0);
                _pivotLeft.localEulerAngles = new Vector3(0, 0, newRotation);
            }
        }

        
        public void SingleMove(double beat, double length, bool right)
        {
            float normalized = Conductor.instance.GetPositionFromBeat(beat - length, length * 2, false);
            if (!right) normalized = 1 - normalized;
            var func = EasingFunction.GetEasingFunction(_ease);
            foreach (var o in _objectsToMove)
            {
                if (o.parent != _pivotSingle) o.SetParent(_pivotSingle, true);
            }
            float newRotation = func(_rotationRight, _rotationLeft, normalized);
            _pivotSingle.localEulerAngles = new Vector3(0, 0, newRotation);
        }
    }
}

