using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TramAndPauline
{
    public class AgbAnimalKid : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform rootBody;
        [SerializeField] private Animator trampolineAnim;
        [SerializeField] private Animator bodyAnim;

        [Header("Properties")]
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float jumpHeightIdle = 0.8f;
    }
}

