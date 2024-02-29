using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;
using System.Linq;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbStarHandler : MonoBehaviour
    {
        [SerializeField] private AgbStar starRef;

        public float boundaryX = 10;
        public float boundaryY = 10;

        [SerializeField] private int starCount = 45;
        [SerializeField] private double blinkFrequency = 0.125;
        [SerializeField] private int blinkAmount = 5;

        [NonSerialized] public float normalizedX;
        [NonSerialized] public float normalizedY;

        private AgbStar[] currentStars;
        private int collectiveEvoStage = 1;
        private GameEvent blinkEvent = new GameEvent();
        private float halfScreenBoundaryX = 17.77695f / 2;

        private void Awake()
        {
            currentStars = new AgbStar[starCount];
            for (int i = 0; i < starCount; i++)
            {
                AgbStar spawnedStar = Instantiate(starRef, transform);
                float xPos = UnityEngine.Random.Range(-boundaryX, boundaryX);
                float yPos = UnityEngine.Random.Range(-boundaryY, boundaryY);
                spawnedStar.Init(xPos, yPos, this);
                currentStars[i] = spawnedStar;
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            
            if (cond.isPlaying && !cond.isPaused)
            {
                if (ReportBlinkBeat(ref blinkEvent.lastReportedBeat))
                {
                    Blink();
                }
            }
        }

        private void Blink()
        {
            List<AgbStar> alreadyBlinked = new List<AgbStar>();
            for (int i = 0; i < blinkAmount; i++)
            {
                AgbStar blinkedStar = currentStars[UnityEngine.Random.Range(0, currentStars.Length)];
                if (alreadyBlinked.Count > 0 && alreadyBlinked.Contains(blinkedStar)) continue;
                blinkedStar.Blink();
                alreadyBlinked.Add(blinkedStar);
            }
        }

        public void Evolve(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                List<AgbStar> nonEvolvedStars = currentStars.ToList().FindAll(x => x.evoStage == collectiveEvoStage);
                List<AgbStar> onScreenStars = nonEvolvedStars.FindAll(x => x.transform.localPosition.x <= halfScreenBoundaryX && x.transform.localPosition.x >= -halfScreenBoundaryX && x.transform.localPosition.y <= 5 && x.transform.localPosition.y >= -5);
                //Debug.Log("OnScreen: " + onScreenStars.Count + " nonEvolved: " + nonEvolvedStars.Count);
                if (onScreenStars.Count > 0)
                {
                    AgbStar randomStar = onScreenStars[UnityEngine.Random.Range(0, onScreenStars.Count)];
                    randomStar.Evolve();
                }
                else if (nonEvolvedStars.Count > 0)
                {
                    AgbStar randomStar = nonEvolvedStars[UnityEngine.Random.Range(0, nonEvolvedStars.Count)];
                    randomStar.Evolve();
                }
                else
                {
                    collectiveEvoStage++;
                    if (collectiveEvoStage > 5) collectiveEvoStage = 5;
                    currentStars[UnityEngine.Random.Range(0, currentStars.Length)].Evolve();
                }
            }
        }

        public void Devolve()
        {
            foreach (var star in currentStars)
            {
                star.Devolve();
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
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundaryX * 2, boundaryY * 2, 0));
        }

        private bool ReportBlinkBeat(ref double lastReportedBeat)
        {
            var cond = Conductor.instance;
            bool result = cond.songPositionInBeats >= (lastReportedBeat) + blinkFrequency;
            if (result)
            {
                lastReportedBeat += blinkFrequency;
            }
            return result;
        }
    }
}

