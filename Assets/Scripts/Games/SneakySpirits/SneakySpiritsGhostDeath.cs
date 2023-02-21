using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SneakySpirits
{
    public class SneakySpiritsGhostDeath : MonoBehaviour
    {
        public string animToPlay;
        public float startBeat;
        public float length;
        [SerializeField] Animator anim;

        void Awake()
        {
            anim = GetComponent<Animator>();
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(startBeat, length);
                anim.DoNormalizedAnimation(animToPlay, normalizedBeat);
                if (normalizedBeat > 1)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}


