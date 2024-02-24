
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class IslandsManager : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private float loopMult = 1f;
        [NonSerialized] public float endZ;
        public float speedMult = 1f;
        [NonSerialized] public float additionalSpeedMult = 1;

        [SerializeField] private RvlIsland[] islands;

        private float fullLengthZ;

        private void Start()
        {
            speedMult /= loopMult;
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
                endZ = -fullLengthZ * loopMult;
                foreach (var island in islands)
                {
                    island.normalizedOffset = 1 - MathUtils.Normalize(island.startPos.z, minValueZ, maxValueZ);
                    island.normalizedOffset /= loopMult;
                }
            }
        }
    }
}


