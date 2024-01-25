using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbBonOdoriLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)</color>", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("pan", "Pan")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 1); },
                    defaultLength = 2f,
                },
                new GameAction("pa-n", "Pa-n")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 1.5); },
                    defaultLength = 2.5f,
                },
                    new GameAction("pa", "Pa")
                {
                    function = delegate { BonOdori.instance.Clap(eventCaller.currentEntity.beat, 0.5 ); },
                    defaultLength = 1.5f,
                }



            
            
            });

        }  

    }
        
};


namespace HeavenStudio.Games
{




    public class BonOdori : Minigame
    {
        
        [Header("Animations")]
        public Animator AnimatorPlayer;
        public Animator AnimatorDonpan1;
        public Animator AnimatorDonpan2;
        public Animator AnimatorDonpan3;
        
        
        public static BonOdori instance { get; set; }
        
        
        public void Awake()
        
        {
            instance = this;
            Debug.Log("AWAKEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        }
        public void Clap(double beat, double type)
        {   
            ScheduleInput(beat, beat + 1f * type, InputAction_BasicPress, Success, Miss, Empty);
            Debug.Log("TESTE");


        }

        public void Success(PlayerActionEvent caller, float state)
        {
            AnimatorPlayer.Play("ClapAll", 0, 0);
            AnimatorDonpan1.Play("ClapAll", 0, 0);
            AnimatorDonpan2.Play("ClapAll", 0, 0);
            AnimatorDonpan3.Play("ClapAll", 0, 0);
            Debug.Log("SUCCESS");
        }
        
        public void Miss(PlayerActionEvent caller)
        {
            Debug.Log("MISS");

        }
        
        
        public void Empty(PlayerActionEvent caller)
        {
            Debug.Log("Empty");

        }
        



    }
}

