using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class Shuttlecock : MonoBehaviour
    {
        [SerializeField] Transform PlayerTarget;
        [SerializeField] Transform OtherTarget;
        [SerializeField] float TargetHeight;
        [SerializeField] float TargetHeightLong;
        [SerializeField] float TargetHeightToss = 2.5f;
        [SerializeField] ParticleSystem hitEffect;

        private Rigidbody2D rb;

        [NonSerialized] public double startBeat;
        [NonSerialized] public double flyBeats;

        [NonSerialized] public bool flyType;
        bool miss = false;
        [NonSerialized] public float flyPos;
        [NonSerialized] public bool isReturning;
        [NonSerialized] public bool isTossed = false;
        [NonSerialized] public AirRally.DistanceSound currentDist = AirRally.DistanceSound.close;
        AirRally game;

        private void Awake()
        {
            game = AirRally.instance;
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            transform.position = OtherTarget.position;
        }

        private Vector3 startPos;
        private Vector3 endPos;

        public void SetStartAndEndPos()
        {
            startPos = isReturning ? PlayerTarget.position : OtherTarget.position;
            endPos = isReturning ? OtherTarget.position : PlayerTarget.position;
            if (isTossed)
            {
                startPos = OtherTarget.position;
                endPos = OtherTarget.position;
            }
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            bool isFartherOrMore = currentDist != AirRally.DistanceSound.close && currentDist != AirRally.DistanceSound.far;

            Vector3 lastPos = transform.position;
            if (!rb.simulated)
            {
                flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);

                transform.position = Vector3.LerpUnclamped(startPos, endPos, flyPos);

                float yMul = flyPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                if (isTossed) transform.position += Vector3.up * yWeight * TargetHeightToss;
                else transform.position += Vector3.up * yWeight * (flyType ? TargetHeightLong : TargetHeight);
            }

            // calculates next position
            {
                float rotation;
                if (isTossed)
                {
                    rotation = Mathf.Lerp(90, -90, flyPos);
                }
                else
                {
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
                }


                if (isFartherOrMore)
                {
                    transform.eulerAngles = new Vector3(0, 0, isReturning ? -90f : 90f);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, rotation - 90f);
                }
            }

            if (miss && flyPos > 4f)
            {
                if (cond.GetPositionFromBeat(startBeat, flyBeats + 1) >= 1.0)
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
            SoundByte.PlayOneShot("miss");
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

