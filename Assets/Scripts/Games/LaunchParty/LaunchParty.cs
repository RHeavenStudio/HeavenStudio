using HeavenStudio.Util;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using static HeavenStudio.EntityTypes;
namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRocketLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) 
        {
            
            return new Minigame("launchParty", "Launch Party \n<color=#eb5454>[WIP]</color>", "000000", false, false, new List<GameAction>()
            {
                new GameAction("rocket", "Launch Rocket")
                {
                    
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchRocket(e.beat, e["type"], e["type2"], e["type3"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("type", RocketScript.RocketType.Family, "Rocket Model", "The rocket to launch"),
                        new Param("type2", RocketScript.ScaleNote.A, "Scale Note", "What scale note to use"),
                        new Param("type3", RocketScript.ScaleType.Major, "Scale Type", "What type of scale to use"),
                    }
                },
                new GameAction("toggleStars", "Toggle Falling Stars")
                {
                    function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.CreateParticles(e.beat, e["toggle"], e["valA"], e["valB"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars Enabled", "Starfall Or No?"),
                        new Param("valA", new EntityTypes.Float(0f, 64f, 1f), "Star Fall Speed", "How fast the stars fall to the edge of the screen"),
                        new Param("valB", new EntityTypes.Float(1f, 16f, 1f), "Star Density", "How many stars are on the screen at once")
                    }
                    
                }
            });

        }
    }
}

namespace HeavenStudio.Games
{
    // using Scripts_LaunchParty;
    public class LaunchParty : Minigame
    {
        
        [Header("Main")]
        public GameObject PadSprites;

        [Header("Rockets")]
        public GameObject Rocket;
        public GameObject PartyCracker;
        public GameObject Bell;
        public GameObject Bowling;
        public ParticleSystem FallingStars;
        public GameObject StarGO;
        
        [Header("Outcasts")]
        public PlayerActionEvent padLaunch;

        public enum RocketType
        {
            Family,
            PartyCracker,
            Bell,
            Bowling,
        }

        public enum ScaleNote
        {
            A,
            ASharp,
            B,
            C,
            CSharp,
            D,
            DSharp,
            E,
            F,
            FSharp,
            G,
            GSharp
        }

        public enum ScaleType
        {
            Major,
            Minor,
            Dorian,
            Mixolydian,
            Lydian,

        }

        
        [Header("Positions")]
        public Transform SpawnRoot;
        public static LaunchParty instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            Vector3 spawnPos = SpawnRoot.position;
            GameObject mobj = Instantiate(PadSprites, SpawnRoot.parent);
            mobj.SetActive(true);

        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        public GameObject CreateRocketInstance(float beat, string awakeAnim, string awakeAnim2, string awakeAnim3, string awakeAnim4, string awakeAnim5, string awakeAnim6, string awakeAnim7, RocketScript.RocketType type)
        {
            
            GameObject mobk = Instantiate(Rocket, SpawnRoot.parent);
            RocketScript mobkDat = mobk.GetComponent<RocketScript>();
            mobkDat.startBeat = beat;
            mobkDat.awakeAnim = awakeAnim;
            mobkDat.type = type;
            mobk.SetActive(true);
            return mobk;

        }
        
        public void LaunchRocket(float beat, int type, int type2, int type3)
            {   
                switch (type)
                {
                    case 0:
                        CreateRocketInstance(beat, "Rocket3", "Rocket2", "Rocket1", null, null, null, null, RocketScript.RocketType.Family);
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        CreateRocketInstance(beat, "Pin1", null, null, null, null, null, null, RocketScript.RocketType.Bowling);
                        break;
                }                 
            }
        public void CreateParticles(float beat, bool toggle, float starDensity, float starSpeed)
        {
            ParticleSystem.EmissionModule emm;
            switch (toggle)
            {
                case true:
                    StarGO.SetActive(true);
                    FallingStars.Play();
                    emm = FallingStars.emission;
                    emm.rateOverTime = starDensity * 6f;
                    break;
                default:
                    FallingStars.Stop();
                    break;
            }
            emm = FallingStars.emission;
            emm.rateOverTime = starDensity * 6f;
        }
    }

}


        


