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
        public GameObject HalvesLeftBase;
        public GameObject HalvesRightBase;

        [Header("Curves")]
        public BezierCurve3D HalvesCurve;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
        }

        private void Start()
        {
            var HalvesGO = GameObject.Instantiate(game.HalvesLeftBase);
            HalvesGO.transform.position = transform.position;
        }

        private void Update()
        {
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 0.5f);
            flyPos *= 0.5f;
            transform.position = HalvesCurve.GetPoint(flyPos);
            
        }
    }
}
