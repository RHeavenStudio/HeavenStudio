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
        [Header("Properties")]
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;

        private float additionalXOffset;
        private float additionalYOffset;

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

        public void Init(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "balloon" });
            allEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
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
            if (movementIndex + 1 < movements.Count && Conductor.instance.songPositionAsDouble > movements[movementIndex].beat + movements[movementIndex].length)
            {
                UpdateIndex();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            balloonTrans.gameObject.SetActive(movements.Count > 0);
            if (movements.Count > 0)
            {
                if (movementIndex + 1 < movements.Count && cond.songPositionAsDouble > movements[movementIndex].beat + movements[movementIndex].length)
                {
                    UpdateIndex();
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

