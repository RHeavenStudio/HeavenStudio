using HeavenStudio.Util;
using System;
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
        private double _stopDanceBeat = double.MaxValue;

        private void Awake()
        {
            _moveScript = GetComponent<LBJCatMove>();
            _anim = transform.GetChild(0).GetComponent<Animator>();
        }

        private void Update()
        {
            if (Conductor.instance.songPositionInBeatsAsDouble >= _stopDanceBeat)
            {
                _anim.DoNormalizedAnimation("CatDance", 0);
                return;
            }
            if (_danceBeat > Conductor.instance.songPositionInBeatsAsDouble)
            {
                _anim.DoNormalizedAnimation("CatIdle", 0);
                return;
            }
            float normalized = Conductor.instance.GetPositionFromBeat(_danceBeat, 2, false) % 1;
            _anim.DoNormalizedAnimation("CatDance", normalized);
        }

        public void Activate(double beat, double length, bool inToScene, bool moveInstant, bool dance, bool instantStart, bool instantEnd)
        {
            _moveScript.Move(beat, moveInstant ? 0 : length, inToScene);

            double overflowBeat = beat % 2;
            double toBeat = 2.0 - overflowBeat;

            _danceBeat = beat + (instantStart ? -overflowBeat : toBeat) - 0.5;
            if (!inToScene) _danceBeat = double.MaxValue;

            _stopDanceBeat = beat + (instantEnd ? -overflowBeat : toBeat) - 0.5;
            if (dance) _stopDanceBeat = double.MaxValue;
        }
    }
}

