using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatformHandler : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private GameObject platformRef;
        [SerializeField] private float defaultYPos = -11.76f;
        [SerializeField] private float platformDistance = 3.80f;
        [SerializeField] private float playerXPos = -6.78f;
        [Range(1, 100)]
        [SerializeField] private int platformAmount = 15;
    }
}

