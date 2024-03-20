using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Square : MonoBehaviour
    {
        public string anim;
        public double startBeat, endBeat, lengthBeat = 1;
        public Vector3 CorrectionPos;
        private Animator squareAnim;

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            squareAnim = GetComponent<Animator>();
            var n = (int)Math.Floor((endBeat - startBeat)/lengthBeat);
            transform.position = transform.position - n*CorrectionPos;
            double beat = endBeat - lengthBeat * n;
            Debug.Log(beat);
            squareAnim.Play(anim, 0, 0);
            Recursion(beat, lengthBeat);
        }

        private void Recursion(double beat, double length)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate
                {
                    transform.position = transform.position + CorrectionPos;
                    squareAnim.Play(anim, 0, 0);
                    Recursion(beat + length, length);
                }),
            });
        }

        void PositionCorrection()
        {
            var pos = transform.position;
            Debug.Log(transform.position);
            transform.position = pos + CorrectionPos;
            Debug.Log(transform.position);
        }

        void ChangeSortingOrder(int order)
        {
            GetComponent<SortingGroup>().sortingOrder = order;
        }

        void End()
        {
            Destroy(gameObject);
        }
    }
}