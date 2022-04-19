using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSamuraiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("samuraiSliceNtr", "Samurai Slice (DS) \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class SamuraiSliceNtr : Minigame
    {
        //game scene
        public static SamuraiSliceNtr instance;

        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
