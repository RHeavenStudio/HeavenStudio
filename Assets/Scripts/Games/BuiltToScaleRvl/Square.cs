using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Square : MonoBehaviour
    {
        public string anim;
        public double beat;
        public Vector3 CorrectionPos;
        private Animator squareAnim;

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            squareAnim = GetComponent<Animator>();
            transform.position = transform.position - (10+1)*CorrectionPos;
            Recursion(beat, 1);
        }

        private void Recursion(double beat, double length)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    transform.position = transform.position + CorrectionPos;
                    squareAnim.Play(anim, 0, 0);
                    Recursion(beat + length, length);
                }),
            });
        }

        void PositionCorrection() {
            var pos = transform.position;
            Debug.Log(transform.position);
            transform.position = pos + CorrectionPos;
            Debug.Log(transform.position);
        }
    }
}