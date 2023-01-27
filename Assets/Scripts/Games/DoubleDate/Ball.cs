using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DoubleDate
{
    public class Ball : PlayerActionObject
    {
        public float startBeat;
        
        private DoubleDate game;

        void Awake()
        {
            game = DoubleDate.instance;
        }

        private void Start()
        {
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN, Hit, Out, Miss);
        }

        void Update()
        {
            
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("doubleDate/kick");
        }

        private void Out(PlayerActionEvent caller)
        {

        }

        private void Miss(PlayerActionEvent caller)
        {
            
        }
    }
}