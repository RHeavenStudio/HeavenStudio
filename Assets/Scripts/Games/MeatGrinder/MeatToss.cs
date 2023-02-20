using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class MeatToss : PlayerActionObject
    {
        public float startBeat;
        
        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
        }

        private void Start() 
        {
            
        }

        private void Update()
        {
            
        }
    }
}
