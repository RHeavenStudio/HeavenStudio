using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBomb : SuperCurveObject
    {
        [SerializeField] private float _rot = 720f;
        [SerializeField] private Path _path;

        [NonSerialized] public double startBeat;

        private void Awake()
        {
            _path.positions[1].pos = new Vector3(UnityEngine.Random.Range(-_path.positions[1].pos.x, _path.positions[1].pos.x), _path.positions[1].pos.y, _path.positions[1].pos.z);
        }

        private void OnDrawGizmos()
        {
            DrawEditorGizmo(_path);
        }

        private void Update()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(startBeat, _path.positions[0].duration, false);
            transform.localPosition = GetPathPositionFromBeat(_path, Conductor.instance.songPositionInBeatsAsDouble, startBeat);
            transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, _rot, normalizedBeat));

            if (normalizedBeat > 1)
            {
                Destroy(gameObject);
            }
        }
    }
}

