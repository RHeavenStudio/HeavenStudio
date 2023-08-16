using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class SpaceballBall : MonoBehaviour
    {
        #region Public

        public double startBeat;

        public bool high;
        public bool isTacobell;

        public Transform Holder;
        public SpriteRenderer Sprite;

        #endregion

        #region Private

        private Minigame.Eligible e = new Minigame.Eligible();

        [SerializeField] private BezierCurve3D pitchLowCurve;
        [SerializeField] private BezierCurve3D pitchHighCurve;

        private bool hit;
        private double hitBeat;
        private Vector3 hitPos;
        private float hitRot;
        private float randomEndPosX;
        private float startRot;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            e.gameObject = this.gameObject;

            startRot = Random.Range(0, 360);
        }

        private void Start()
        {
            Spaceball.instance.ScheduleInput(startBeat, high ? 2f : 1f, InputType.STANDARD_DOWN, Just, Miss, Out);
        }

        private void Update()
        {
            if (hit)
            {
                float nba = Conductor.instance.GetPositionFromBeat(hitBeat, 10);
                Holder.localPosition = Vector3.Lerp(hitPos, new Vector3(randomEndPosX, 0f, -600f), nba);
                Holder.eulerAngles = Vector3.Lerp(new Vector3(0, 0, hitRot), new Vector3(0, 0, -2260), nba);
            }
            else
            {
                var beatLength = (high) ? 2f : 1f;

                var normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(
                    startBeat,
                    beatLength + 0.15f
                    );

                var animCurve = (high) ? pitchHighCurve : pitchLowCurve;

                Holder.position = animCurve.GetPoint(normalizedBeatAnim);
                Sprite.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(startRot, startRot - 210, normalizedBeatAnim));
            }
        }

        #endregion

        #region PlayerActionObject

        private void Hit()
        {
            hit = true;
            hitBeat = Conductor.instance.songPositionInBeatsAsDouble;
            hitPos = Holder.localPosition;
            hitRot = Holder.eulerAngles.z;

            if (isTacobell)
            {
                SoundByte.PlayOneShotGame("spaceball/tacobell");
            }
            SoundByte.PlayOneShotGame("spaceball/hit");

            // jank fix for a bug with autoplay - freeform
            if (GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput)
            {
                SoundByte.PlayOneShotGame("spaceball/swing");
            }

            randomEndPosX = Random.Range(4f, 16f);

            SpaceballPlayer.instance.Swing(this);
        }

        private void NearMiss()
        {
            Holder.GetChild(0).gameObject.AddComponent<Rotate>().rotateSpeed = -325;

            enabled = false;

            // Rigidbody physics, in MY rhythm game??!!!
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(transform.up * 1100);
            rb.AddForce(transform.right * 400);
            rb.gravityScale = 9;

            SoundByte.PlayOneShot("miss");

            Destroy(gameObject, 5f);

            Spaceball.instance.ScoreMiss();
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                NearMiss();
                return;
            }
            Hit();
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("spaceball/fall");
            Instantiate(Spaceball.instance.Dust, Spaceball.instance.Dust.transform.parent).SetActive(true);
            Destroy(this.gameObject);

            Spaceball.instance.ScoreMiss();
        }

        private void Out(PlayerActionEvent caller) { }

        #endregion
    }
}
