using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;


namespace HeavenStudio.Games.Scripts_AirRally
{
    public class Shuttlecock : MonoBehaviour
    {
        [SerializeField] Transform PlayerTarget;
        [SerializeField] Transform OtherTarget;
        [SerializeField] float TargetHeight;
        [SerializeField] float TargetHeightLong;
        [SerializeField] ParticleSystem hitEffect;

        public float startBeat;
        public float flyBeats;

        public bool flyType;
        bool miss = false;
        public float flyPos;
        public bool isReturning;
        AirRally game;

        private void Awake()
        {
            game = AirRally.instance;
        }

        void Start()
        {
            transform.position = OtherTarget.position;
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            Vector3 startPos = isReturning ? PlayerTarget.position : OtherTarget.position;
            Vector3 endPos = isReturning ? OtherTarget.position : PlayerTarget.position;
            Vector3 lastPos = transform.position;
            if (!GetComponent<Rigidbody2D>().simulated)
            {
                flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);

                transform.position = Vector3.LerpUnclamped(startPos, endPos, flyPos);

                float yMul = flyPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                transform.position += Vector3.up * yWeight * (flyType ? TargetHeightLong : TargetHeight);
            }

            // calculates next position
            {
                float rotation;
                if (flyPos > 0.5)
                {
                    Vector3 midPos = Vector3.LerpUnclamped(startPos, endPos, 0.5f);
                    midPos += Vector3.up * (flyType ? TargetHeightLong : TargetHeight);
                    Vector3 direction = (transform.position - midPos).normalized;
                    rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                }
                else
                {
                    Vector3 direction = (transform.position - lastPos).normalized;
                    rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                }

                this.transform.eulerAngles = new Vector3(0, 0, rotation - 90f);
            }

            if (miss && flyPos > 4f)
            {
                if (cond.GetPositionFromBeat(startBeat, flyBeats + 1f) >= 1f)
                {
                    GameObject.Destroy(gameObject);
                    return;
                }
            }
        }

        public void DoHit(AirRally.DistanceSound distance)
        {
            ParticleSystem.MainModule main = hitEffect.main;
            switch (distance)
            {
                case AirRally.DistanceSound.close:
                    main.startSize = 2f;
                    break;
                case AirRally.DistanceSound.far:
                    main.startSize = 3f;
                    break;
                case AirRally.DistanceSound.farther:
                    main.startSize = 4f;
                    break;
                case AirRally.DistanceSound.farthest:
                    main.startSize = 6f;
                    break;
            }
            hitEffect.Play();
        }

        public void DoNearMiss()
        {
            miss = true;
            Jukebox.PlayOneShot("miss");
            transform.position = PlayerTarget.position;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.simulated = true;
            rb.WakeUp();
            rb.velocity = Vector3.zero;
            rb.gravityScale = 10f;
            rb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * -10, ForceMode2D.Impulse);
        }

        public void DoThrough()
        {
            miss = true;
        }
    }
}

