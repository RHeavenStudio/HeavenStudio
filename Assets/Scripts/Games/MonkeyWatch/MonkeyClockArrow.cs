using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class MonkeyClockArrow : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator anim;
        [SerializeField] private Transform anchorRotateTransform;

        public void Move()
        {
            anchorRotateTransform.localEulerAngles = new Vector3(0, 0, anchorRotateTransform.localEulerAngles.z - 6);
            anim.DoScaledAnimationAsync("Click", 0.5f);
        }

        public void MoveToAngle(float angle)
        {
            anchorRotateTransform.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }
}


