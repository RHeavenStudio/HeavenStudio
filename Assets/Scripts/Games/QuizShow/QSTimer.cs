using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_QuizShow
{
    public class QSTimer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform timerTrans;

        private double startBeat = double.MaxValue;
        private float length = 0;

        public void Init(double beat, float interval)
        {
            startBeat = beat;
            length = interval;
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            float normalizedBeat = cond.GetPositionFromBeat(startBeat, length);
            if (normalizedBeat >= 0 && normalizedBeat <= 1) timerTrans.rotation = Quaternion.Euler(0, 0, normalizedBeat * -360);
        }
    }
}
