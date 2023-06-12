using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class NPCDoughBall : SuperCurveObject
    {
        private double startBeat = double.MinValue;
        private Path path;

        public void Init(double beat)
        {
            startBeat = beat;
            path = WorkingDough.instance.GetPath("NPCBall");
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                double beat = cond.songPositionInBeats;
                if (startBeat > double.MinValue)
                {
                    Vector3 pos = GetPathPositionFromBeat(path, Math.Max(beat, startBeat), startBeat);
                    transform.position = pos;
                    if (beat >= startBeat + 2) Destroy(gameObject);
                }
            }
        }
    }
}
