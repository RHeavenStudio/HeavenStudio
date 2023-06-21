using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlayYan : MonoBehaviour
    {
        private AgbNightWalk game;
        [SerializeField] private List<Animator> balloons = new List<Animator>();

        private void Awake()
        {
            game = AgbNightWalk.instance;
            foreach (var balloon in balloons)
            {
                balloon.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
            }
        }

        public void PopBalloon(int index, bool instant)
        {
            if (instant)
            {
                balloons[index].DoNormalizedAnimation("Pop", 1);
                return;
            }
            balloons[index].DoScaledAnimationAsync("Pop", 0.5f);
        }

        public void PopAll()
        {
            foreach (var balloon in balloons)
            {
                balloon.DoNormalizedAnimation("Pop", 1);
            }
        }
    }
}


