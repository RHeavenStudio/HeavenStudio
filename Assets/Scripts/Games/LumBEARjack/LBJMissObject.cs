using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJMissObject : SuperCurveObject
    {
        [Header("Properties")]
        [SerializeField] private float _rot;
        [SerializeField] private float _height;
        [SerializeField] private float _beatDuration;
        [SerializeField] private float _jumpDistanceX;
        [SerializeField] private float _jumpDistanceY;

        private Path _path;
        private SpriteRenderer _objectSr;
        private double _startBeat;
        private bool _right;

        private void Awake()
        {
            _path = new Path()
            {
                positions = new PathPos[2],
            };
            _path.positions[0].height = _height;
            _path.positions[0].duration = _beatDuration;
        }

        public void Activate(Transform objectToMove, SpriteRenderer objectSr)
        {
            _startBeat = Conductor.instance.songPositionInBeatsAsDouble;
            _objectSr = objectSr;
            transform.position = objectToMove.position;
            objectToMove.SetParent(transform, true);
            _right = objectToMove.localEulerAngles.z <= 180;
            _path.positions[0].pos = transform.localPosition;
            _path.positions[1].pos = transform.localPosition + new Vector3(_right ? _jumpDistanceX : -_jumpDistanceX, _jumpDistanceY);
            Update();
        }

        private void Update()
        {
            float normalized = Mathf.Max(Conductor.instance.GetPositionFromBeat(_startBeat, _beatDuration, false), 0);

            _objectSr.color = new Color(1, 1, 1, 1 - Mathf.Clamp01(normalized));
            transform.localPosition = GetPathPositionFromBeat(_path, Conductor.instance.songPositionInBeatsAsDouble, _startBeat);

            float newRot = Mathf.Lerp(0, _right ? _rot : -_rot, normalized);
            transform.localEulerAngles = new Vector3(0, 0, newRot);

            if (normalized > 1) Destroy(gameObject);
        }
    }
}

