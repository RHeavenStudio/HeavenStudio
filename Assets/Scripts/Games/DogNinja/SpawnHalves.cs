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
        public bool lefty;

        [Header("References")]
        public Animator anim;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
        }

        private void Start() 
        {
            if (lefty) {
                anim.DoScaledAnimationAsync("FallLeft", 0.5f);
            } else {
                anim.DoScaledAnimationAsync("FallRight", 0.5f);
            }
        }

        private void Update()
        {
            /*
            float flyPosHalves = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+0.65f;
            flyPosHalves *= 0.2f;
            transform.position = HalfCurve.GetPoint(flyPosHalves);

            float rot = rotSpeed;
            if (!lefty) rot *= -1;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));

            // clean-up logic
            if (flyPosHalves > 1f) {
                GameObject.Destroy(gameObject);
            };
            */
            
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused) {
                GameObject.Destroy(gameObject);
            };
        }
    }
}
