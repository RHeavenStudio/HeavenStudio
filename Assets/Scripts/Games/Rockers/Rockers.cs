using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class NtrRockersLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("rockers", "Rockers", "EB4C94", false, false, new List<GameAction>()
            {

            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Rockers : Minigame
    {
        public static Rockers instance;

        public static CallAndResponseHandler crHandlerInstance;

        private void Awake()
        {
            instance = this;
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(8);
            }
        }

        private void OnDestroy()
        {
            if (crHandlerInstance != null && (!Conductor.instance.isPlaying || Conductor.instance.isPaused))
            {
                crHandlerInstance = null;
            }
        }
    }
}

