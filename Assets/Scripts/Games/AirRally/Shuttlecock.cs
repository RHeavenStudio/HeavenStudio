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

        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;
            
            Vector3 lastPos = transform.position;
            if (!GetComponent<Rigidbody2D>().simulated)
            {
                float flyPos = cond.GetPositionFromBeat(startBeat, flyBeats);

                Vector3 startPos = isReturning ? PlayerTarget.position : OtherTarget.position;
                Vector3 endPos = isReturning ? OtherTarget.position : PlayerTarget.position;

                transform.position = Vector3.LerpUnclamped(startPos, endPos, flyPos);

                float yMul = flyPos * 2f - 1f;
                float yWeight = -(yMul*yMul) + 1f;
                transform.position += Vector3.up * yWeight * (flyType ? TargetHeightLong : TargetHeight);
            }

            Vector3 direction = (transform.position - lastPos).normalized;
            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            this.transform.eulerAngles = new Vector3(0, 0, rotation - 90f);

            if (miss && flyPos > 2f)
            {
                if (cond.GetPositionFromBeat(startBeat, flyBeats + 1f) >= 1f)
                {
                    GameObject.Destroy(gameObject);
                    return;
                }
            }
        }

        public void DoNearMiss()
        {
            miss = true;
            Jukebox.PlayOneShot("miss");
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.simulated = true;
            rb.WakeUp();
            rb.velocity = Vector3.zero;
            rb.gravityScale = 10f;
            rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
            rb.AddForce(Vector2.right * -10, ForceMode2D.Impulse);
        }

        public void DoThrough()
        {
            miss = true;
        }
    }
}

