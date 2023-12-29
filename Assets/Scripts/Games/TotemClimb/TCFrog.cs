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

    }
}

