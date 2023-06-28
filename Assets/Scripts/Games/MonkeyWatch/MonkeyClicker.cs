using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class MonkeyClicker : MonoBehaviour
    {
        public float rotation;

        [Header("Animators")]
        public Animator anim;

        private MonkeyWatch game;
        
        private void Awake()
        {
            game = MonkeyWatch.instance;
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        // animation event
        public void UpdateRotation()
        {
            //var rot = gameObject.transform;
        }

        private void Miss(PlayerActionEvent caller)
        {
            
        }

        private void Out(PlayerActionEvent caller) 
        {
            
        }
    }
}
