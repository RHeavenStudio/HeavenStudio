using Starpelly;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class IslandsManager : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private float endZMult = 0.25f;
        [NonSerialized] public float endZ;
        public float speedMult = 1f;
        [NonSerialized] public float additionalSpeedMult = 1;

        [SerializeField] private RvlIsland[] islands;

        private float fullLengthZ;

        private void Start()
        {
            float[] allZ = new float[islands.Length];

            for (int i = 0; i < islands.Length; i++)
            {
                islands[i].manager = this;
                allZ[i] = islands[i].startPos.z;
            }

            if (islands.Length > 0)
            {
                float minValueZ = Mathf.Min(allZ);
                float maxValueZ = Mathf.Max(allZ);
                fullLengthZ = maxValueZ - minValueZ;
                endZ = -fullLengthZ * endZMult;
                foreach (var island in islands)
                {
                    island.normalizedOffset = 1 - Mathp.Normalize(island.startPos.z, minValueZ, maxValueZ);
                }
            }
        }
    }
}


