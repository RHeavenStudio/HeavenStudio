using HeavenStudio.Games.Scripts_SneakySpirits;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbGhostLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("sneakySpirits", "Sneaky Spirits \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("spawn spirit", "Spawn Spirit")
                {
                    defaultLength = 1f,
                    function = delegate { var e = eventCaller.currentEntity; SneakySpirits.instance.SpawnSpirit(e.beat, e["valA"]); },
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(1, 8, 8), "Num of Beats", "The number of beats until you have to shoot the spirit")
                    }
                }
            },
            new List<string>() { "agb", "aim" },
            "agbghost", "en", "ver0",
            new List<string>() {},
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_SneakySpirits;
    public class SneakySpirits : Minigame
    {
        public static SneakySpirits instance;

        // references
        [Header("References")]
        public Transform SpawnRoot;
        public GameObject SneakySpirit;
        public GameObject Arrow;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SpawnSpirit(float beat, int length)
        {
            CreateSpirit(beat, length);
        }

        public GameObject CreateSpirit(float beat, int length)
        {
            GameObject mobj = GameObject.Instantiate(SneakySpirit, SpawnRoot);
            WahSpirits mobjDat = mobj.GetComponent<WahSpirits>();
            mobjDat.startBeat = beat;
            mobjDat.numBeats = length;

            mobj.SetActive(true);

            return mobj;
        }
    }
}
