using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class NPCDoughBall : PlayerActionObject
    {
        public float startBeat;
        //public bool isBig;
        public enum FlyingStage
        {
            EnteringUp,
            EnteringDown,
            ExitingUp,
            ExitingDown
        }

        FlyingStage currentFlyingStage = FlyingStage.EnteringUp;


        [NonSerialized] public BezierCurve3D enterUpCurve;
        [NonSerialized] public BezierCurve3D enterDownCurve;
        [NonSerialized] public BezierCurve3D exitUpCurve;
        [NonSerialized] public BezierCurve3D exitDownCurve;

        private WorkingDough game;

        private void Awake()
        {
            game = WorkingDough.instance;
        }

        private void Update()
        {
            var cond = Conductor.instance;

            float flyPos = 0f;

            switch (currentFlyingStage) {
                case FlyingStage.EnteringUp:
                    flyPos = cond.GetPositionFromBeat(startBeat, 0.5f);
                    transform.position = enterUpCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentFlyingStage = FlyingStage.EnteringDown;
                    break;
                case FlyingStage.EnteringDown:
                    flyPos = cond.GetPositionFromBeat(startBeat + 0.5f, 0.5f);

                    transform.position = enterDownCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentFlyingStage = FlyingStage.ExitingUp;
                    break;
                case FlyingStage.ExitingUp:
                    flyPos = cond.GetPositionFromBeat(startBeat + 1f, 0.5f);

                    transform.position = exitUpCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentFlyingStage = FlyingStage.ExitingDown;
                    break;
                case FlyingStage.ExitingDown:
                    flyPos = cond.GetPositionFromBeat(startBeat + 1.5f, 0.5f);

                    transform.position = exitDownCurve.GetPoint(flyPos);
                    if (flyPos > 1f) GameObject.Destroy(gameObject);
                    break;
            }
        }
    }
}
