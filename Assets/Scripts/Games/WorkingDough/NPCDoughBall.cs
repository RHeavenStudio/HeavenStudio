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
        [SerializeField] private GameObject gandw;

        public void Init(double beat, bool hasGandw)
        {
            startBeat = beat;
            path = WorkingDough.instance.GetPath("NPCBall");
            if (gandw != null) gandw.SetActive(hasGandw);
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
                    if (startBeat <= beat) transform.position = pos;
                    else transform.position = new Vector3(-80, -80);
                    if (beat >= startBeat + 2) Destroy(gameObject);
                }
            }
        }
    }
}
