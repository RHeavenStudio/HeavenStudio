using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Games;

namespace HeavenStudio.Common
{
    public class SectionMedalsManager : MonoBehaviour
    {
        public static SectionMedalsManager instance { get; private set; }
        Conductor cond;
        
        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            cond = Conductor.instance;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}