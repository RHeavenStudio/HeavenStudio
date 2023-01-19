using NaughtyBezierCurves;
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
                new GameAction("ThrowObject", "Throw Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat); }, 
                    defaultLength = 2,
                },
                new GameAction("CutEverything", "Cut Everything!")
                {
                    function = delegate { DogNinja.instance.CutEverything(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    function = delegate { DogNinja.instance.HWG(eventCaller.currentEntity.beat); }, 
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

        public enum Object
        {
            Apple,
            Bone,
            Broc,
            Carrot,
            Cucumber,
            Pan,
            Pepper,
            Potato,
            Tire,
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void CutSound(bool applause)
        {
            Jukebox.PlayOneShotGame("dogNinja/cut");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        public void ThrowObject(float beat)
        {
            
        }

        public void CutEverything(float beat)
        {

        }

        public void HWG(float beat)
        {

        }
    }

}