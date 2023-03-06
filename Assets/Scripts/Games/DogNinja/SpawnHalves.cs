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
        private Vector3 posModifier;
        public bool lefty;
        float bpmModifier;
        float songPos;
        
        [SerializeField] float rotSpeed;

        [Header("References")]
        [SerializeField] BezierCurve3D fallLeftCurve;
        [SerializeField] BezierCurve3D fallRightCurve;
        BezierCurve3D curve;
        [SerializeField] Transform halvesParent;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
            bpmModifier = Conductor.instance.songBpm / 100;
            songPos = Conductor.instance.songPositionInBeats;
            posModifier = (objPos/3);
        }

        private void Start() 
        {
            curve = lefty ? fallRightCurve : fallLeftCurve;
        }

        private void Update()
        {
            float flyPosHalves = Conductor.instance.GetPositionFromBeat(songPos, 1f)+1.35f;
            flyPosHalves *= 0.2f;
            transform.position = curve.GetPoint(flyPosHalves)+posModifier;

            float rot = rotSpeed;
            rot *= lefty ? bpmModifier : -1 * bpmModifier;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));

            // clean-up logic
            if (flyPosHalves > 1f) {
                GameObject.Destroy(gameObject);
            };
            
            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) 
                || GameManager.instance.currentGame != "dogNinja") {
                GameObject.Destroy(gameObject);
            };
        }
    }
}
