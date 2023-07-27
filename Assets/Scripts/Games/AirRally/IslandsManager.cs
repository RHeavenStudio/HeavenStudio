using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class IslandsManager : MonoBehaviour
    {
        [Header("Properties")]
        private float endY;
        private float endZ;
        public float speedMult = 1f;

        private struct Island
        {
            public Transform transform;
            public Vector3 startPos;
        }

        private Island[] islands;

        private float yMult;

        private float fullLengthY;
        private float fullLengthZ;

        private void Awake()
        {
            islands = new Island[transform.childCount];

            float[] allY = new float[transform.childCount];
            float[] allZ = new float[transform.childCount];

            for (int i = 0; i < islands.Length; i++)
            {
                islands[i].transform = transform.GetChild(i);
                islands[i].startPos = transform.GetChild(i).position;
                allY[i] = islands[i].startPos.y;
                allZ[i] = islands[i].startPos.z;
            }

            if (islands.Length > 0)
            {
                float minValueY = Mathf.Min(allY);
                float maxValueY = Mathf.Max(allY);
                fullLengthY = maxValueY - minValueY;
                endY = -fullLengthY;
                float minValueZ = Mathf.Min(allZ);
                float maxValueZ = Mathf.Max(allZ);
                fullLengthZ = maxValueZ - minValueZ;
                endZ = -fullLengthZ;
            }

            yMult = Mathf.Abs(endY) / Mathf.Abs(endZ);
        }

        private void Update()
        {
            float moveY = yMult * speedMult * Time.deltaTime;
            float moveZ = speedMult * Time.deltaTime;
            foreach (var island in islands)
            {
                island.transform.position = new Vector3(island.transform.position.x,
                    island.transform.position.y - moveY, island.transform.position.z - moveZ);
                if (island.transform.position.z < endZ)
                {
                    island.transform.position = new Vector3(island.startPos.x, island.startPos.y + fullLengthY,
                        island.startPos.z + fullLengthZ);
                }
            }
        }
    }
}


