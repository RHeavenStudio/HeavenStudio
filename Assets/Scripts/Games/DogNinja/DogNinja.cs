using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("dogNinja", "Dog Ninja \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("item", "Throw Object")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.item(e.beat, e["type"]); }, 
                    defaultLength = 2,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DogNinja;

    public class DogNinja : Minigame
    {
        public static DogNinja instance;
        
        private void Awake()
        {
            instance = this;
        }

        private void CutSound(bool applause)
        {
            Jukebox.PlayOneShotGame("dogNinja/cut");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        public void item(float beat, int type)
        {

        }

        public void sprite(float beat, int type)
        {

        }
    }

}