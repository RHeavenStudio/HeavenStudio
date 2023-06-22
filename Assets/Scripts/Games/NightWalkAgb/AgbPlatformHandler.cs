using HeavenStudio.Util;
using System;
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
        public float heightAmount = 2;
        [NonSerialized] public int defaultHeightUnits = 0;
        public float platformDistance = 3.80f;
        public float playerXPos = -6.78f;
        [Range(1, 100)]
        public int platformCount = 20;
        private float lastHeight = 0;
        private float heightToRaiseTo = 0;
        private double raiseBeat = -1;

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
                    platform.handler = this;
                    platform.StartInput(game.countInBeat + i + 8 - (platformCount * 0.5), game.countInBeat + i + 8);
                    platform.gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < platformCount; i++)
                {
                    AgbPlatform platform = Instantiate(platformRef, transform);
                    platform.handler = this;
                    platform.StartInput(beat, Math.Ceiling(beat) + i - platformCount);
                    platform.gameObject.SetActive(true);
                }
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (raiseBeat != -1)
                {
                    float normalizedBeat = Mathf.Clamp(cond.GetPositionFromBeat(raiseBeat, 1), 0, 1);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuint);

                    float newPosY = func(lastHeight, heightToRaiseTo, normalizedBeat);

                    transform.localPosition = new Vector3(0, -newPosY, 0);
                }
            }
        }

        public void RaiseHeight(double beat, int lastUnits, int currentUnits)
        {
            raiseBeat = beat;
            lastHeight = lastUnits * heightAmount * transform.localScale.y;
            heightToRaiseTo = currentUnits * heightAmount * transform.localScale.y;
            Update();
        }
    }
}

