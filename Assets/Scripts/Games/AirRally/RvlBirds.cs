using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class RvlBirds : MonoBehaviour
    {
        [Header("Birds")]
        [SerializeField] private Animator[] birdAnims;
        [Header("Properties")]
        [SerializeField] private float birdSpeedX = 0.2f;
        [SerializeField] private float birdSpeedZ = 0.5f;
        [NonSerialized] public float speedMultX = 1f;
        [NonSerialized] public float speedMultZ = 1f;

        private void Awake()
        {
            foreach (var anim in birdAnims)
            {
                anim.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
            }
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying) return;

            float moveX = birdSpeedX * speedMultX * Time.deltaTime;
            float moveZ = birdSpeedZ * speedMultZ * Time.deltaTime;

            transform.position = new Vector3(transform.position.x - moveX, transform.position.y, transform.position.z - moveZ);
        }
    }
}

