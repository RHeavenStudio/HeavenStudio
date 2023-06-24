using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbStarHandler : MonoBehaviour
    {
        [SerializeField] private AgbStar starRef;

        public float boundaryX = 10;
        public float boundaryY = 10;

        [SerializeField] private int starCount = 45;

        [NonSerialized] public float normalizedX;
        [NonSerialized] public float normalizedY;

        private void Awake()
        {
            for (int i = 0; i < starCount; i++)
            {
                AgbStar spawnedStar = Instantiate(starRef, transform);
                float xPos = UnityEngine.Random.Range(-boundaryX, boundaryX);
                float yPos = UnityEngine.Random.Range(-boundaryY, boundaryY);
                spawnedStar.Init(xPos, yPos, this);
            }
        }

        public Vector3 GetRelativePosition(ref float ogX, ref float ogY)
        {
            float x = Mathf.LerpUnclamped(0, boundaryX, normalizedX) + ogX;
            if (x > boundaryX)
            {
                ogX -= boundaryX * 2;
                x = Mathf.LerpUnclamped(0, boundaryX, normalizedX) + ogX;
            }
            else if (x < -boundaryX)
            {
                ogX += boundaryX * 2;
                x = Mathf.LerpUnclamped(0, boundaryX, normalizedX) + ogX;
            }
            float y = Mathf.LerpUnclamped(0, boundaryY, normalizedY) + ogY;
            if (y > boundaryY)
            {
                ogY -= boundaryY * 2;
                y = Mathf.LerpUnclamped(0, boundaryY, normalizedY) + ogY;
            }
            else if (y < -boundaryY)
            {
                ogY += boundaryY * 2;
                y = Mathf.LerpUnclamped(0, boundaryY, normalizedY) + ogY;
            }
            return new Vector3(x, y);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundaryX, boundaryY, 0));
        }
    }
}

