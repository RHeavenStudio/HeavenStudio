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

        [SerializeField] private Transform _jumperPointLeft;
        [SerializeField] private Transform _jumperPointMiddle;
        [SerializeField] private Transform _jumperPointRight;

        public Transform JumperPointLeft => _jumperPointLeft;
        public Transform JumperPointMiddle => _jumperPointMiddle;
        public Transform JumperPointRight => _jumperPointRight;

        public void FallPiece(int part)
        {
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
    }
}

