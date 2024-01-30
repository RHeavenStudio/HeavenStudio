using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class BalloonHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform anchor;
        [SerializeField] private Transform target;
        [SerializeField] private Transform balloonTrans;
        [SerializeField] private SpriteRenderer[] srs;
        [SerializeField] private SpriteRenderer shadow;
        [Header("Properties")]
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;

        private float shadowOpacity;
        private float additionalXOffset;
        private float additionalYOffset;
        private double movementFirstBeat = -2;
        private double movementLastBeat = -1;

        private List<Movement> movements = new();
        private int movementIndex = 0;

        private struct Movement
        {
            public double beat;
            public float length;

            public float xStart;
            public float xEnd;
            public float yStart;
            public float yEnd;
            public float angleStart;
            public float angleEnd;
            public Util.EasingFunction.Ease ease;
        }

        private void Awake()
        {
            shadowOpacity = shadow.color.a;
        }

        public void Init(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "balloon" });
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
            if (allEvents.Count > 0)
            {
                movementFirstBeat = allEvents[0].beat;
                movementLastBeat = allEvents[^1].beat + allEvents[^1].length - 0.25;
            }
            foreach (var e in allEvents)
            {
                movements.Add(new Movement
                {
                    beat = e.beat,
                    length = e.length,
                    xStart = e["xStart"],
                    xEnd = e["xEnd"],
                    yStart = e["yStart"],
                    yEnd = e["yEnd"],
                    angleStart = e["angleStart"],
                    angleEnd = e["angleEnd"],
                    ease = (Util.EasingFunction.Ease)e["ease"]
                });
            }
            Update();
        }

        private void UpdateIndex()
        {
            movementIndex++;
            if (movementIndex >= movements.Count) return;
            if (Conductor.instance.songPositionInBeatsAsDouble > movements[movementIndex].beat + movements[movementIndex].length)
            {
                UpdateIndex();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            float normalizedFirst = Mathf.Clamp01(cond.GetPositionFromBeat(movementFirstBeat, 0.25f));
            float normalizedLast = Mathf.Clamp01(cond.GetPositionFromBeat(movementLastBeat, 0.25f));
            if (normalizedFirst >= 1)
            {
                foreach (var sr in srs)
                {
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1 - normalizedLast);
                }
                shadow.color = new Color(shadow.color.r, shadow.color.g, shadow.color.b, Mathf.Lerp(shadowOpacity, 0, normalizedLast));
            }
            else
            {
                foreach (var sr in srs)
                {
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, normalizedFirst);
                }
                shadow.color = new Color(shadow.color.r, shadow.color.g, shadow.color.b, Mathf.Lerp(0, shadowOpacity, normalizedFirst));
            }
            if (movements.Count > 0 && movementIndex < movements.Count)
            {
                if (cond.songPositionInBeatsAsDouble > movements[movementIndex].beat + movements[movementIndex].length)
                {
                    UpdateIndex();
                    if (movementIndex >= movements.Count) return;
                }

                var e = movements[movementIndex];

                float normalizedBeat = Mathf.Clamp01(cond.GetPositionFromBeat(e.beat, e.length));

                var func = Util.EasingFunction.GetEasingFunction(e.ease);

                float newX = func(e.xStart, e.xEnd, normalizedBeat);
                float newY = func(e.yStart, e.yEnd, normalizedBeat);
                float newAngle = func(e.angleStart, e.angleEnd, normalizedBeat);

                additionalXOffset = newX;
                additionalYOffset = newY;

                anchor.localEulerAngles = new Vector3(0, 0, newAngle);
            }

            balloonTrans.position = new Vector3(target.position.x + xOffset + additionalXOffset, target.position.y + yOffset + additionalYOffset, target.position.z);
        }
    }
}

