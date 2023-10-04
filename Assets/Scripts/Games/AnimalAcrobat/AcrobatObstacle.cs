using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using UnityEngine.Events;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class AcrobatObstacle : MonoBehaviour
    {
        [SerializeField] private UnityEvent<double> _onInit;

        [Header("Values")]
        [SerializeField] private float _fullRotRange;
        [SerializeField] private float _spawnOffset;
        [SerializeField] private double _holdLength;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;

        private double _startBeat;
        private double _expirationBeat = -1;
        private float _halfRotRange;
        private EasingFunction.Function _func;

        public float SpawnOffset => _spawnOffset;

        [Header("Components")]
        [SerializeField] private Transform _rotateRoot;
        [SerializeField] private Transform _gripPoint;

        private void Awake()
        {
            _halfRotRange = _fullRotRange * 0.5f;
            _func = EasingFunction.GetEasingFunction(_ease);
        }

        public void Init(double beat, double expirationBeat)
        {
            _startBeat = beat;
            _expirationBeat = expirationBeat;
            _onInit.Invoke(_startBeat);
            Update();
        }

        public float GetRotationDistance()
        {
            float result = Mathf.Cos((_fullRotRange + 180) * Mathf.Deg2Rad) * _gripPoint.localPosition.y;
            return Mathf.Abs(result * 2);
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;

            float normalNoMod = Mathf.Abs(cond.GetPositionFromBeat(_startBeat, _holdLength));

            float normalizedBeat = normalNoMod % 1;
            
            if (Mathf.Floor(normalNoMod) % 2 != 0)
            {
                normalizedBeat = 1 - normalizedBeat;
            }

            float newAngleZ = _func(-_halfRotRange, _halfRotRange, normalizedBeat);
            _rotateRoot.localEulerAngles = new Vector3(0, 0, newAngleZ);

            if (cond.songPositionInBeatsAsDouble >= _expirationBeat)
            {
                gameObject.SetActive(false);
            }
        }

        public bool IsAvailableAtBeat(double beat)
        {
            return beat >= _expirationBeat;
        }
    }
}


