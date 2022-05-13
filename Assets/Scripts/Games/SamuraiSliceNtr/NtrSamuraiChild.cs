using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_NtrSamurai
{
    public class NtrSamuraiChild : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform DebrisPosL;
        public Transform DebrisPosR;
        public Transform WalkPos0;
        public Transform WalkPos1;

        [Header("Objects")]
        public Animator anim;

        public float startBeat = Single.MinValue;
        public bool isMain = true;

        // Update is called once per frame
        void Update()
        {
            if (!isMain)
            {
                var cond = Conductor.instance;
                float prog = Conductor.instance.GetPositionFromBeat(startBeat + 1f, 2f);
                if (prog >= 0)
                {
                    Walk();
                    transform.position = Vector3.Lerp(WalkPos0.position, WalkPos1.position, prog);
                    if (prog >= 1f)
                    {
                        GameObject.Destroy(gameObject);
                    }
                }
            }
        }

        public void Bop()
        {
            anim.Play("ChildBeat", -1, 0);
        }

        public void Walk()
        {
            anim.Play("ChildWalk");
        }
    }
}