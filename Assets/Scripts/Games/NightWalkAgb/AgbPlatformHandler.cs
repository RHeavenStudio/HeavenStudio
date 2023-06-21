using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatformHandler : MonoBehaviour
    {
        private AgbNightWalk game;
        [Header("Properties")]
        [SerializeField] private AgbPlatform platformRef;
        public float defaultYPos = -11.76f;
        public float platformDistance = 3.80f;
        public float playerXPos = -6.78f;
        [Range(1, 100)]
        public int platformCount = 20;

        private void Awake()
        {
            game = AgbNightWalk.instance;
        }

        public void SpawnPlatforms(double beat)
        {
            if (game.countInBeat != -1)
            {
                for (int i = 0; i < platformCount; i++)
                {
                    AgbPlatform platform = Instantiate(platformRef, transform);
                    platform.Init(game.countInBeat + i - (platformCount * 0.5), game.countInBeat + i + 8, this);
                    platform.gameObject.SetActive(true);
                }
            }
            else
            {

            }
        }
    }
}

