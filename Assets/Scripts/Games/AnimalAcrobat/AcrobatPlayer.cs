using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class AcrobatPlayer : SuperCurveObject
    {
        [Header("Values")]
        [SerializeField] private float _jumpDistanceStart = 6f;
        [SerializeField] private float _jumpDistance = 8f;
        [SerializeField] private float _jumpHeight = 4f;
        [SerializeField] private float _jumpDistanceGiraffe = 32f;
        [SerializeField] private float _jumpHeightGiraffe = 8f;
        private Animator _anim;
        private Path _path;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _path = new Path()
            {
                positions = new PathPos[2]
            };
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("PlayerBop", 0.5f);
        }

        public void InitialJump(double beat)
        {
            StartCoroutine(InitialJumpCo(beat));
        }

        private IEnumerator InitialJumpCo(double beat)
        {
            var cond = Conductor.instance;
            yield return new WaitUntil(() => cond.songPositionInBeatsAsDouble >= beat);
            _anim.DoScaledAnimationAsync("PlayerJump", 0.5f);
            Jump(beat, 1, transform.localPosition.x, _jumpDistanceStart, _jumpHeight, transform.localPosition.y);
        }

        private void Jump(double beat, float length, float startPoint, float distance, float height, float restY)
        {
            StartCoroutine(JumpCo(beat, length, startPoint, distance, height, restY));
        }

        private IEnumerator JumpCo(double beat, float length, float startPoint, float distance, float height, float restY)
        {
            var cond = Conductor.instance;
            _path.positions[0] = new PathPos()
            {
                pos = new Vector3(startPoint, restY),
                height = height,
                duration = length
            };
            _path.positions[1] = new PathPos()
            {
                pos = new Vector3(startPoint + distance, restY),
            };
            while (true)
            {
                transform.localPosition = GetPathPositionFromBeat(_path, cond.songPositionInBeatsAsDouble, beat);
                yield return null;
            }
        }
    }
}

