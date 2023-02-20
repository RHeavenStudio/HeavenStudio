using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MeatGrinder
{
    public class MeatToss : PlayerActionObject
    {
        public float startBeat;
        
        const string sfxName = "meatGrinder/";
        
        private MeatGrinder game;

        private void Awake()
        {
            game = MeatGrinder.instance;
        }

        private void Start() 
        {
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Miss, Nothing);
        }

        private void Update()
        {
            
        }
        private void Hit(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame(sfxName+"meatHit");
            GameObject.Destroy(gameObject);
        }
            

        private void Miss(PlayerActionEvent caller)
        {

        }

        private void Nothing(PlayerActionEvent caller) 
        {
            
        }
    }
}
