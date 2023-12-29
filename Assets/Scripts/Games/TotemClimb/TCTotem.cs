using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCTotem : MonoBehaviour
    {
        [NonSerialized] public double beat;
        [SerializeField] private Animator _anim;
        [SerializeField] private Transform _jumperPoint;
        public Transform JumperPoint => _jumperPoint;

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("Bop", 0.5f);
        }
    }
}

