using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCEndTotem : MonoBehaviour
    {
        [SerializeField] private TCTotemManager _totemManager;

        public void ActivateFeatherEffect()
        {
            _totemManager.ActivateEndParticles();
        }
    }
}

