using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCFrog : MonoBehaviour
    {
        [NonSerialized] public double beat;

        [SerializeField] private Animator _animLeft;
        [SerializeField] private Animator _animMiddle;
        [SerializeField] private Animator _animRight;
        [SerializeField] private Animator _anim;

        [SerializeField] private Transform _jumperPointLeft;
        [SerializeField] private Transform _jumperPointMiddle;
        [SerializeField] private Transform _jumperPointRight;

        public Transform JumperPointLeft => _jumperPointLeft;
        public Transform JumperPointMiddle => _jumperPointMiddle;
        public Transform JumperPointRight => _jumperPointRight;

        private bool _hasWings = false;
        private bool _flapSet = false; // I hate unity

        private void Update()
        {
            if (!_anim.GetCurrentAnimatorStateInfo(0).IsName("Wings") && _hasWings && !_flapSet)
            {
                _anim.Play("Wings", 0, 0);
                _flapSet = true;
            }
        }

        public void FallPiece(int part)
        {
            if (_hasWings) _anim.Play("WingsNoFlap", 0, 0);
            switch (part)
            {
                case -1:
                    _animLeft.DoScaledAnimationAsync("Fall", 0.5f);
                    break;
                case 0:
                    _animMiddle.DoScaledAnimationAsync("Fall", 0.5f);
                    break;
                default:
                    _animRight.DoScaledAnimationAsync("Fall", 0.5f);
                    break;
            }
        }

        public void SetHasWings()
        {
            _hasWings = true;
        }
    }
}

