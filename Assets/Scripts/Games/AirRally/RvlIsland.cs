using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class RvlIsland : MonoBehaviour
    {
        [NonSerialized] public IslandsManager manager;
        [NonSerialized] public Vector3 startPos;
        private float normalized = 0f;
        [NonSerialized] public float normalizedOffset = 0f;
        [SerializeField] private SpriteRenderer[] srs;

        private Tween[] fadeInTweens;

        private void Awake()
        {
            startPos = transform.position;
            fadeInTweens = new Tween[srs.Length];
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying) return;
            float moveZ = Mathf.LerpUnclamped(startPos.z, startPos.z + manager.endZ, normalized);
            transform.position = new Vector3(transform.position.x, transform.position.y, moveZ);
            normalized += manager.speedMult * manager.additionalSpeedMult * Time.deltaTime;
            if (transform.position.z < manager.endZ)
            {
                normalized = -normalizedOffset;
                for(int i = 0; i < srs.Length; i++)
                {
                    srs[i].color = new Color(1, 1, 1, 0);
                    if (fadeInTweens[i] != null) fadeInTweens[i].Kill(true);
                    fadeInTweens[i] = srs[i].DOColor(Color.white, 0.4f);
                }
            }
        }
    }
}

