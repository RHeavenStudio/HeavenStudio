using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class RvlBirds : MonoBehaviour
    {
        [Header("Birds")]
        [SerializeField] private SpriteRenderer[] srs;
        [SerializeField] private Animator[] birdAnims;
        [Header("Properties")]
        [SerializeField] private float birdSpeedX = 0.2f;
        [SerializeField] private float birdSpeedZ = 0.5f;
        [NonSerialized] public float speedMultX = 1f;
        [NonSerialized] public float speedMultZ = 1f;
        [SerializeField] private bool isRainbow = false;
        private double fadeInBeat = double.MinValue;
        private float defaultOpacity;

        private void Awake()
        {
            if (srs.Length != 0) defaultOpacity = srs[0].color.a;
            foreach (var anim in birdAnims)
            {
                anim.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
            }
            RainbowUpdate();
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying) return;

            float moveX = birdSpeedX * speedMultX * Time.deltaTime;
            float moveZ = birdSpeedZ * speedMultZ * Time.deltaTime;

            transform.position = new Vector3(transform.position.x - moveX, transform.position.y, transform.position.z - moveZ);

            if (isRainbow) RainbowUpdate();
        }

        private void RainbowUpdate()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(fadeInBeat, 1);

            float newA = Mathf.Lerp(0, defaultOpacity, normalizedBeat);

            foreach (var sr in srs)
            {
                sr.color = new Color(1, 1, 1, newA);
            }
        }

        public void FadeIn(double beat)
        {
            fadeInBeat = beat;
        }
    }
}

