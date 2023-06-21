using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlatformHandler : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private AgbPlatform platformRef;
        public float defaultYPos = -11.76f;
        public float platformDistance = 3.80f;
        public float playerXPos = -6.78f;
    }
}

