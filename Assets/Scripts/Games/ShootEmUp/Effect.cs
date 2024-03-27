using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ShootEmUp
{
    public class Effect : MonoBehaviour
    {
        void End()
        {
            Destroy(gameObject);
        }
    }
}