using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBGCat : MonoBehaviour
    {
        private Animator _anim;
        private LBJCatMove _moveScript;

        private double _danceBeat = double.MaxValue;

        private void Awake()
        {
            _moveScript = GetComponent<LBJCatMove>();
            _anim = transform.GetChild(0).GetComponent<Animator>();
        }

        private void Update()
        {
            if (_danceBeat > Conductor.instance.songPositionInBeatsAsDouble)
            {
                _anim.Play("CatIdle");
                return;
            }
            float normalized = Conductor.instance.GetPositionFromBeat(_danceBeat, 2, false) % 1;
            _anim.DoNormalizedAnimation("CatDance", normalized);
        }

        public void Activate(double beat, double length, bool inToScene, bool instant)
        {
            _moveScript.Move(beat, instant ? 0 : length, inToScene);

            double overflowBeat = (beat + 0.5) % 2;
            _danceBeat = beat - overflowBeat + (instant ? 0 : 2);

            if (!inToScene) _danceBeat = double.MaxValue;
        }
    }
}

