using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using UnityEngine.Events;
using System;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class AcrobatObstacle : MonoBehaviour
    {
        public Action<double, bool> OnInit;

        [Header("Values")]
        [SerializeField] private float _fullRotRange;
        [SerializeField] private double _holdLength;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;
        [SerializeField] private double _holdPadding = 0.5;
        [SerializeField] private double _holdPaddingStart = 0;

        private double _startBeat;
        private double _expirationBeat = -1;
        private float _halfRotRange;
        private EasingFunction.Function _func;

        public EasingFunction.Ease Ease => _ease;

        [Header("Components")]
        [SerializeField] private Transform _rotateRoot;
        [SerializeField] private Transform _gripPoint;

        private void Awake()
        {
            _halfRotRange = _fullRotRange * 0.5f;
            _func = EasingFunction.GetEasingFunction(_ease);
        }

        public void Init(double beat, double expirationBeat, bool behindGiraffe)
        {
            _startBeat = beat;
            _expirationBeat = expirationBeat;
            OnInit(beat, behindGiraffe);
            Update();
        }

        public float GetRotationDistance()
        {
            float result = Mathf.Cos((_fullRotRange + 180) * Mathf.Deg2Rad) * GetRotationHeight();
            return Mathf.Abs(result * 2);
        }

        public float GetRotationHeight()
        {
            return _gripPoint.localPosition.y;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;

            float normalNoMod = Mathf.Abs(cond.GetPositionFromBeat(_startBeat - _holdPaddingStart, _holdLength + _holdPadding + _holdPaddingStart));

            float normalizedBeat = normalNoMod % 1;
            
            if (Mathf.Floor(normalNoMod) % 2 != 0)
            {
                normalizedBeat = 1 - normalizedBeat;
            }

            float newAngleZ = _func(-_halfRotRange, _halfRotRange, normalizedBeat);
            _rotateRoot.localEulerAngles = new Vector3(0, 0, newAngleZ);
        }

        public bool IsAvailableAtBeat(double beat)
        {
            return beat >= _expirationBeat;
        }
    }
}


