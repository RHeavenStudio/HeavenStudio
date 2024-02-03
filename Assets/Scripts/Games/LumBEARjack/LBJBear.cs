using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBear : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _anim;

        public void SwingWhiff()
        {
            SoundByte.PlayOneShotGame("lumbearjack/swing", -1, SoundByte.GetPitchFromCents(Random.Range(-200, 201), false));
        }
    }

}
