using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBaby : SuperCurveObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _flySprite;
        [SerializeField] private Sprite _standSprite;

        [Header("Properties")]
        [SerializeField] private float _rot;
        [SerializeField] private float _addedHeight;
        [SerializeField] private float _addedY;

        [Header("Path")]
        [SerializeField] private Path _path;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void OnDrawGizmos()
        {
            DrawEditorGizmo(_path);
        }

        public void Activate(double beat, float durationMult, int babyIndex)
        {
            gameObject.SetActive(true);
            _path.positions[0].duration *= durationMult;
            _path.positions[1].pos.y += _addedY * babyIndex;
            _path.positions[1].height += _addedHeight * babyIndex;
            _sr.sprite = _flySprite;
            _sr.sortingOrder += babyIndex;
            StartCoroutine(FlyCo(beat));
        }

        private IEnumerator FlyCo(double beat)
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration, false);
            while (normalizedBeat <= 1)
            {
                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration, false);
                transform.localPosition = GetPathPositionFromBeat(_path, Math.Clamp(Conductor.instance.songPositionInBeatsAsDouble, beat, beat + _path.positions[0].duration), beat);
                transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, _rot, normalizedBeat));
                yield return null;
            }
            transform.localPosition = GetPathPositionFromBeat(_path, Math.Clamp(Conductor.instance.songPositionInBeatsAsDouble, beat, beat + _path.positions[0].duration), beat);
            transform.localEulerAngles = Vector3.zero;
            _sr.sprite = _standSprite;
        }
    }
}

