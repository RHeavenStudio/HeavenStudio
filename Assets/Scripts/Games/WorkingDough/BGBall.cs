using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class BGBall : SuperCurveObject
    {
        private double startBeat = double.MinValue;
        private Path path;
        [SerializeField] private GameObject gandw;

        public void Init(double beat, bool hasGandw)
        {
            startBeat = beat;
            path = WorkingDough.instance.GetPath("BGBall");
            if (gandw != null) gandw.SetActive(hasGandw);
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                double beat = cond.songPositionInBeats;
                if (startBeat !=  double.MinValue)
                {
                    Vector3 pos = GetPathPositionFromBeat(path, Math.Max(startBeat, beat), startBeat);
                    transform.position = pos;
                    transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z - (-90 * Time.deltaTime * (1f / Conductor.instance.pitchedSecPerBeat)));
                    if (beat >= startBeat + 9) Destroy(gameObject);
                }
            }
        }
    }
}


