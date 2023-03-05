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
        float modifier;
        float songPos;
        
        [SerializeField] float rotSpeed = 140f;

        [Header("References")]
        [SerializeField] BezierCurve3D fallLeftCurve;
        [SerializeField] BezierCurve3D fallRightCurve;
        BezierCurve3D curve;
        [SerializeField] Transform halvesParent;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
            modifier = Conductor.instance.songBpm / 100;
            songPos = Conductor.instance.songPositionInBeats;
        }

        private void Start() 
        {
            curve = lefty ? fallRightCurve : fallLeftCurve;
            halvesParent.position = objPos;
        }

        private void Update()
        {
            float flyPosHalves = Conductor.instance.GetPositionFromBeat(songPos, 1f)+1.4f;
            flyPosHalves *= 0.2f;
            transform.position = curve.GetPoint(flyPosHalves);

            float rot = rotSpeed;
            if (lefty) rot *= -1 * modifier;
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
