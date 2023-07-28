using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class RvlIsland : MonoBehaviour
    {
        [NonSerialized] public IslandsManager manager;
        [NonSerialized] public Vector3 startPos;
        private float normalized = 0f;
        [NonSerialized] public float normalizedOffset = 0f;
        [SerializeField] private SpriteRenderer[] srs;

        private void Awake()
        {
            startPos = transform.position;
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
            }
        }
    }
}

