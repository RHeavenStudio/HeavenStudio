using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;


namespace HeavenStudio.Games.Scripts_DogNinja
{
    public class SpawnHalves : MonoBehaviour
    {
        public double startBeat;
        public Vector3 objPos;
        private Vector3 posModifier;
        public bool lefty;
        float bpmModifier;
        double songPos;
        
        [SerializeField] float rotSpeed;

        [Header("References")]
        [SerializeField] BezierCurve3D fallLeftCurve;
        [SerializeField] BezierCurve3D fallRightCurve;
        BezierCurve3D curve;
        [SerializeField] Transform halvesParent;

        
        private void Awake()
        {
            bpmModifier = Conductor.instance.songBpm / 100;
            songPos = Conductor.instance.songPositionInBeatsAsDouble;
        }

        private void Start() 
        {
            curve = lefty ? fallRightCurve : fallLeftCurve;
        }

        private void Update()
        {
            // holy shit
            float flyPosHalves = (Conductor.instance.GetPositionFromBeat(songPos, 3f)*(Conductor.instance.GetPositionFromBeat(songPos, 2f)))+Conductor.instance.GetPositionFromBeat(songPos, 1f);
            flyPosHalves = (flyPosHalves * 0.2f) + 0.35f;
            transform.position = curve.GetPoint(flyPosHalves) + objPos;

            float rot = rotSpeed;
            rot *= lefty ? bpmModifier : -1 * bpmModifier;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));

            // clean-up logic
            if (flyPosHalves > 1f) {
                Destroy(gameObject);
            };
            
            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) 
                || GameManager.instance.currentGame != "dogNinja") {
                Destroy(gameObject);
            };
        }
    }
}
