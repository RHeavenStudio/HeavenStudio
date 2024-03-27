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
        public double startBeat, targetBeat, lengthBeat = 1;
        public int endTime;
        public Vector3 CorrectionPos;
        private Animator squareAnim;

        private BuiltToScaleRvl game;

        public void Init()
        {
            game = BuiltToScaleRvl.instance;
            squareAnim = GetComponent<Animator>();
            var endTime = (int)Math.Ceiling((targetBeat - startBeat)/lengthBeat);
            transform.position = transform.position - endTime * CorrectionPos;
            double beat = targetBeat - lengthBeat * endTime;
            squareAnim.Play(anim, 0, (beat==0 ? 0 : 1));
            Recursion(beat, lengthBeat);
        }

        private void Recursion(double beat, double length)
        {
            if (beat > targetBeat + 10 * lengthBeat) End();
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