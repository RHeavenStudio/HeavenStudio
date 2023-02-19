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
        public bool lefty;
        const float rotSpeed = 140f;

        [Header("References")]
        public SpriteRenderer HalfSprite;
        public Transform Half;
        
        [Header("Curves")]
        public BezierCurve3D HalfCurve;
        public Transform HalfCurveTrans;


        public Sprite[] WhichHalf;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
        }

        private void Start()
        {
            if (!lefty) {
                HalfCurveTrans.position = new Vector3(-13.5f, 1.36f, 0);
                HalfCurveTrans.localScale = new Vector3(-1, 1 , 0);
            }
        }

        private void Update()
        {
            float flyPosHalves = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+0.65f;
            flyPosHalves *= 0.2f;
            transform.position = HalfCurve.GetPoint(flyPosHalves);

            float rot = rotSpeed;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));

            // clean-up logic
            if (flyPosHalves > 1f) {
                GameObject.Destroy(gameObject);
            };
            
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            };
        }
    }
}
