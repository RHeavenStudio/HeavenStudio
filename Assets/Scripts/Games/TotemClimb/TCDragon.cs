using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCDragon : MonoBehaviour
    {
        [NonSerialized] public double beat;
        [SerializeField] private Animator _anim;
        [SerializeField] private Transform _jumperPoint;
        public Transform JumperPoint => _jumperPoint;

        public void Hold()
        {
            _anim.DoScaledAnimationAsync("Hold", 0.25f);
        }

        public void Release()
        {
            _anim.DoScaledAnimationAsync("Release", 0.5f);
        }
    }
}

