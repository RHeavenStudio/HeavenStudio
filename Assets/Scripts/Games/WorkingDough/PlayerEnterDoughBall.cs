using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class PlayerEnterDoughBall : MonoBehaviour
    {
        public double startBeat;
        public float firstBeatsToTravel = 0.5f;
        public float secondBeatsToTravel = 0.5f;
        public bool goingDown = false;
        public bool deletingAutomatically = true;
        [NonSerialized] public BezierCurve3D firstCurve;
        [NonSerialized] public BezierCurve3D secondCurve;

        private void Update()
        {
            var cond = Conductor.instance;

            float flyPos = 0f;

            if (goingDown)
            {
                flyPos = cond.GetPositionFromBeat(startBeat + firstBeatsToTravel, secondBeatsToTravel);

                transform.position = secondCurve.GetPoint(flyPos);
                if (flyPos > 1f) if (deletingAutomatically) GameObject.Destroy(gameObject);
            }
            else
            {
                flyPos = cond.GetPositionFromBeat(startBeat, firstBeatsToTravel);
                transform.position = firstCurve.GetPoint(flyPos);
                if (flyPos > 1f) goingDown = true;
            }
        }
    }
}


