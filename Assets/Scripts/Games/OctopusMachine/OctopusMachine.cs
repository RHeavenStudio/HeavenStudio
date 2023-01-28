using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("OctopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "CEU789", false, false, new List<GameAction>()
            {

            new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; OctopusMachine.instance.Bop(e.beat, e["toggle"]); },
                        parameters = new List<Param>()
                        {
                        new Param("toggle", false, "Reset Pose", "Resets to idle pose.")
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

         
       
        private float lastReportedBeat = 0f;

         

        public static OctopusMachine instance { get; set; }

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }
        
        public void Bop(float beat, bool reset)

        {
            
            
            {
                Octopus.DoScaledAnimationAsync("Bop", 0.5f);
                OtherOctopus.DoScaledAnimationAsync("Bop", 0.5f);
                OctopusPlayer.DoScaledAnimationAsync("Bop", 0.5f);
            }

        }
    }
}
