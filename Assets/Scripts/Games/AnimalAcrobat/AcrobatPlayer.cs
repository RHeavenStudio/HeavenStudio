using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class AcrobatPlayer : SuperCurveObject
    {
        [Header("Components")]
        [SerializeField] private Transform _scroll;
        [SerializeField] private ParticleSystem _releaseParticle, _trailParticle;
        [Header("Values")]
        [SerializeField] private float _jumpDistanceStart = 6f;
        [SerializeField] private float _jumpDistance = 8f;
        [SerializeField] private float _jumpHeight = 4f;
        [SerializeField] private float _jumpHeightInitial = 2f;
        [SerializeField] private float _jumpDistanceGiraffe = 32f;
        [SerializeField] private float _jumpHeightGiraffe = 8f;
        [SerializeField] private float _jumpStartAngle = 120f;
        private Animator _anim;
        private Path _path;
        private float _restY;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _path = new Path()
            {
                positions = new PathPos[2]
            };
            _restY = transform.localPosition.y;
        }

        private void LateUpdate()
        {
            _trailParticle.transform.eulerAngles = Vector3.zero;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _trailParticle.Stop();
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("PlayerBop", 0.5f);
        }

        public void JumpBetweenAnimals(double beat, float startPoint)
        {
            _anim.Play("PlayerAir", 0, 0);

            Jump(beat, 2, startPoint, _jumpDistance, _jumpHeight, _restY);
            RotateJump(beat, 2);

            SpawnReleaseParticle();
        }
        
        public void JumpBetweenGiraffe(double beat, float startPoint)
        {
            _anim.Play("PlayerAir", 0, 0);

            _trailParticle.Play();
            Jump(beat, 4, startPoint, _jumpDistanceGiraffe, _jumpHeightGiraffe, _restY);
            RotateJump(beat, 4);

            SpawnReleaseParticle();
        }

        private void SpawnReleaseParticle()
        {
            ParticleSystem spawnedParticle = Instantiate(_releaseParticle, _scroll);
            spawnedParticle.transform.position = transform.position;
            spawnedParticle.Play();
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
            Jump(beat, 1, transform.localPosition.x, _jumpDistanceStart, _jumpHeightInitial, _restY);
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

        private void RotateJump(double beat, double length)
        {
            StartCoroutine(RotateJumpCo(beat, length));
        }

        private IEnumerator RotateJumpCo(double beat, double length)
        {
            var cond = Conductor.instance;
            float normalized = 0;
            while (normalized <= 1)
            {
                normalized = cond.GetPositionFromBeat(beat, length - 1);
                float newAngle = Mathf.Lerp(_jumpStartAngle, 360, normalized);

                transform.localEulerAngles = new Vector3(0, 0, newAngle);
                yield return null;
            }

            normalized = 0;
            while (normalized <= 1)
            {
                normalized = cond.GetPositionFromBeat(beat + length - 1, 0.5);
                float newAngle = Mathf.Lerp(0, 720, normalized);

                transform.localEulerAngles = new Vector3(0, 0, newAngle);
                yield return null;
            }
        }
    }
}

