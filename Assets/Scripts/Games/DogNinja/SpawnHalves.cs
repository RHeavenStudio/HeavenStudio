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

        [Header("References")]
        public GameObject LeftHalvesBase;
        public GameObject RightHalvesBase;

        [Header("Curves")]
        public BezierCurve3D LeftHalfCurve;
        public BezierCurve3D RightHalfCurve;

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
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1);
            flyPos *= 0.5f;
            //transform.position = curve.GetPoint(flyPos);
            
        }

        void MakeHalves()
        {
            var HalvesGO = GameObject.Instantiate(game.HalvesLeftBase);
            HalvesGO.transform.position = transform.position;
            
            
        }
    }
}
