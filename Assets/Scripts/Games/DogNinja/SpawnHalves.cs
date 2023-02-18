using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DogNinja
{
    public class SpawnHalves : PlayerActionObject
    {
        public float startBeat;
        public Vector3 objPos;
        const float rotSpeed = 90f;

        [Header("References")]
        public SpriteRenderer WhichHalf;
        public Transform Half;
        
        [Header("Curves")]
        public BezierCurve3D HalfCurve;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
        }

        private void Start()
        {

        }

        private void Update()
        {
            float flyPosHalves = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+0.5f;
            flyPosHalves *= 0.2f;
            transform.position = HalfCurve.GetPoint(flyPosHalves);

            float rot = -rotSpeed;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));

            // clean-up logic
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            };
        }
    }
}
