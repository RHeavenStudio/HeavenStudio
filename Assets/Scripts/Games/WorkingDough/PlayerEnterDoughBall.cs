using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class PlayerEnterDoughBall : PlayerActionObject
    {
        public float startBeat;
        bool goingDown = false;

        [NonSerialized] public BezierCurve3D enterUpCurve;
        [NonSerialized] public BezierCurve3D enterDownCurve;

        private void Update()
        {
            var cond = Conductor.instance;

            float flyPos = 0f;

            if (goingDown)
            {
                flyPos = cond.GetPositionFromBeat(startBeat + 0.5f, 0.5f);
                transform.position = enterDownCurve.GetPoint(flyPos);
                if (flyPos > 1f) GameObject.Destroy(gameObject);
            }
            else
            {
                flyPos = cond.GetPositionFromBeat(startBeat, 0.5f);
                transform.position = enterUpCurve.GetPoint(flyPos);
                if (flyPos > 1f) goingDown = true;
            }
        }
    }
}


