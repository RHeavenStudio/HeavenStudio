using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
// using GhostlyGuy's balls; 

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("OctopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {

            new GameAction("bop", "Bop")
            {
                function = delegate { var e = eventCaller.currentEntity; OctopusMachine.instance.Bop(e.beat, e["toggle"], e["type"]); },
                parameters = new List<Param>()                     
                {
                 new Param("toggle", false, "Joyful", "Plays the animations as if you hit an input"),
                 new Param("type", false, "Upset", "Plays the animations as if you missed."),
                },
                   defaultLength = 1f,
                },
            });     
        }
    }
}


namespace HeavenStudio.Games
{
    public partial class OctopusMachine : Minigame
    {
        //   private Animator octopus;

        public Animator OctopusPlayer;
        public Animator Octopus;
        public Animator OtherOctopus;

        public GameObject Player;

        [Header("Properties")]
        public GameEvent bop = new GameEvent();

        public static OctopusMachine instance { get; set; }

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }
        
        public void Bop(float beat, bool joyful, bool upset)
        {
           if (joyful)
           {
              Octopus.DoScaledAnimationAsync("Happy", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Happy", 0.5f);
              OctopusPlayer.DoScaledAnimationAsync("Happy", 0.5f);
           } 
           else if (upset)
           {
              Octopus.DoScaledAnimationAsync("Angry", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Angry", 0.5f);
              OctopusPlayer.DoScaledAnimationAsync("Oops", 0.5f);
           }
           else
           {
              Octopus.DoScaledAnimationAsync("Bop", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Bop", 0.5f);
              OctopusPlayer.DoScaledAnimationAsync("Bop", 0.5f);                   
           }
        }
    }
}               
            