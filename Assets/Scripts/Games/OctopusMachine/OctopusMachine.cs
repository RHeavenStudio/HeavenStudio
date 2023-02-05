using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
// using GhostlyGuy's Balls;

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
                     function = delegate { var e = eventCaller.currentEntity; OctopusMachine.instance.Bop(e.beat, e["toggle"], e["type"], e["type2"]); },
                     parameters = new List<Param>()                     
                     {
                      new Param("toggle", false, "Joyful", "Plays the animations as if you hit an input"),
                      new Param("type", false, "Upset", "Plays the animations as if you missed."),
                      new Param("type2", false, "Shocked", "What happened?")
                     },
                     defaultLength = 1f,
                 },   
                 new GameAction("prepare", "Prepare")
                 {
                     function = delegate { var e = eventCaller.currentEntity; OctopusMachine.instance.Prepare(e.beat, (e.length == 1f)); },
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
        public Animator OctopusPlayer;
        public Animator Octopus;
        public Animator OtherOctopus;

        public GameObject Player;

        [Header("Properties")]
        public GameEvent bop = new GameEvent();
        
        public GameEvent prepare = new GameEvent();

        public static OctopusMachine instance { get; set; }

        void Awake()
        {
            instance = this;
        }
        public void Prepare(float beat, bool prepare)
        {
           if (prepare)
           {
              Octopus.DoScaledAnimationAsync("Prepare", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Prepare", 0.5f);
              OctopusPlayer.DoScaledAnimationAsync("Prepare", 0.5f);
           } 
           else 
           {
              Octopus.DoScaledAnimationAsync("Idle", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Idle", 0.5f);
              OctopusPlayer.DoScaledAnimationAsync("Idle", 0.5f);
           }
        }
 
        public void Bop(float beat, bool joyful, bool upset, bool shocked)
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
           else if (shocked)
           {
              Octopus.DoScaledAnimationAsync("Oops", 0.5f);
              OtherOctopus.DoScaledAnimationAsync("Oops", 0.5f);
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
            