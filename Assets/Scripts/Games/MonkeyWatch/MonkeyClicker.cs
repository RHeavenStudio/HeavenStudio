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
        public Animator MonkeyAnim;
        public Animator ClickerAnim;
        public ParticleSystem YellowStars;
        public ParticleSystem PinkStars;

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

        private void Miss(PlayerActionEvent caller)
        {
            
        }

        private void Out(PlayerActionEvent caller) 
        {
            
        }

        // animation events
        public void UpdateRotation(float rot)
        {
            gameObject.transform.localEulerAngles -= new Vector3(0, 0, rot);
        }

        public void PlayAnimation(string anim)
        {
            ClickerAnim.DoScaledAnimationAsync(anim, 0.5f);
        }
    }
}
