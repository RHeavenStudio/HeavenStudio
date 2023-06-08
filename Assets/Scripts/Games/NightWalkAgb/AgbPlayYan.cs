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
    }
}


