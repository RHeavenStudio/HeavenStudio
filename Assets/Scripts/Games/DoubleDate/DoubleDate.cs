using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlDoubleDateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("doubleDate", "Double Date \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("soccerBall", "Soccer Ball")
                {
                    function = delegate { var e = eventCaller.currentEntity; DoubleDate.instance.ball(e.beat, e["type"]); }, 
                    defaultLength = 2,
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DoubleDate;

    public class DoubleDate : Minigame
    {
        public static DoubleDate instance;

        public enum soccerBall {
            soccerBall,
        }

        [Header("Objects")]
        public Animator soccerBallAnim;
        
        private void Awake()
        {
            instance = this;
        }

        public void ball(float beat, int type)
        {
            Jukebox.PlayOneShotGame("doubleDate/soccerBall");
        }
    }

}