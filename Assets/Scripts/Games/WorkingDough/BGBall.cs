using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class BGBall : MonoBehaviour
    {
        public double startBeat;
        public float firstBeatsToTravel = 3f;
        public float secondBeatsToTravel = 1f;
        public float thirdBeatsToTravel = 3f;
        public enum CurveStage
        {
            Conveyer = 0,
            StartFall = 1,
            Fall = 2
        }
        public CurveStage currentCurveStage;
        [NonSerialized] public BezierCurve3D firstCurve;
        [NonSerialized] public BezierCurve3D secondCurve;
        [NonSerialized] public BezierCurve3D thirdCurve;

        private void Update()
        {
            var cond = Conductor.instance;

            float flyPos = 0f;

            switch (currentCurveStage)
            {
                case CurveStage.Conveyer:
                    flyPos = cond.GetPositionFromBeat(startBeat, firstBeatsToTravel);
                    transform.position = firstCurve.GetPoint(flyPos);
                    if (flyPos > 1f)
                    {
                        currentCurveStage = CurveStage.StartFall;
                    }
                    break;
                case CurveStage.StartFall:
                    flyPos = cond.GetPositionFromBeat(startBeat + firstBeatsToTravel, secondBeatsToTravel);
                    transform.position = secondCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentCurveStage = CurveStage.Fall;
                    break;
                case CurveStage.Fall:
                    flyPos = cond.GetPositionFromBeat(startBeat + secondBeatsToTravel + firstBeatsToTravel, thirdBeatsToTravel);

                    transform.position = thirdCurve.GetPoint(flyPos);
                    if (flyPos > 1f) GameObject.Destroy(gameObject);
                    break;
            }
        }
    }
}


