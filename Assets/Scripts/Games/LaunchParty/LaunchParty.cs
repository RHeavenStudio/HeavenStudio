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
                new GameAction("rocket", "Launch Family Rocket")
                {
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchRocket(e.beat); },
                    defaultLength = 5f,
                },
                new GameAction("partyCracker", "Launch Party Cracker")
                {
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchPartyCracker(e.beat); },
                    defaultLength = 4f,
                },
                new GameAction("bell", "Launch Bell")
                {
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchBell(e.beat); },
                    defaultLength = 4f,
                },
                new GameAction("bowlingPin", "Launch Bowling Pin")
                {
                    function = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchBowlingPin(e.beat); },
                    defaultLength = 4f,
                },
                new GameAction("toggleStars", "Toggle Falling Stars")
                {
                    function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.CreateParticles(e.beat, e["toggle"], e["valA"], e["valB"], e["valC"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Stars Enabled", "Starfall Or No?"),
                        new Param("valA", new EntityTypes.Float(0.1f, 10f, 1f), "Star Density", "How many stars are on the screen"),
                        new Param("valB", new EntityTypes.Float(0.01f, 5f, 0.1f), "Front Star Fall Speed", "How fast the front stars fall to the edge of the screen"),
                        new Param("valC", new EntityTypes.Float(0.01f, 5f, 0.1f), "Back Star Fall Speed", "How fast the stars fall to the edge of the screen")
                        
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class LaunchParty : Minigame
    {
        [Header("Rockets")]
        [SerializeField] GameObject rocket;
        [SerializeField] GameObject partyCracker;
        [SerializeField] GameObject bell;
        [SerializeField] GameObject bowlingPin;

        [Header("Components")]
        [SerializeField] ParticleSystem fallingStars;
        [SerializeField] ParticleSystem fallingStarsBack;
        [SerializeField] GameObject starGO;
        [SerializeField] Transform launchPad;

        public static LaunchParty instance;

        //Normal Rocket = (beat, beat + 1, beat + 2)
        //Party Cracker = (beat, beat + 2/3, beat + 1, beat + 4/3, beat + 5/3)
/*      // Bell                  
 *              new BeatAction.Action(beat, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0); }),
                new BeatAction.Action(beat + 1f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                new BeatAction.Action(beat + 7/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                new BeatAction.Action(beat + 8/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                new BeatAction.Action(beat + 9/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                new BeatAction.Action(beat + 10/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);}),
                new BeatAction.Action(beat + 11/6f, delegate { LaunchParty.instance.Bells.Play(awakeAnim, 0, 0);})
*/
//Pin
/*
 *                         new MultiSound.Sound("launchParty/VT_CL",   beat, pitch1),
                new MultiSound.Sound("launchParty/rocket_pin_prepare",   beat, 1f, 0.75f),
                new MultiSound.Sound("launchParty/VT_CL",   beat, pitch2, 0.02f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 1/6f, pitch3, 0.02f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 2/6f, pitch2, 0.06f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 3/6f, pitch3, 0.1f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 4/6f, pitch2, 0.16f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 5/6f, pitch3, 0.22f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 1f, pitch2, 0.3f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 7/6f, pitch3, 0.4f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 8/6f, pitch2, 0.6f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 9/6f, pitch3, 0.75f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 10/6f, pitch2, 0.89f),
                new MultiSound.Sound("launchParty/VT_CL",   beat + 11/6f, pitch3),
*/

        void Awake()
        {
            instance = this;
        }

        public void LaunchRocket(float beat)
        {   

        }

        public void LaunchPartyCracker(float beat)
        {

        }

        public void LaunchBell(float beat)
        {

        }

        public void LaunchBowlingPin(float beat)
        {

        }

        public void CreateParticles(float beat, bool toggle, float starDensity, float starSpeed, float starSpeedBack)
        {
            ParticleSystem.EmissionModule emm;
            ParticleSystem.EmissionModule emm2;
            switch (toggle)
            {
                case true:
                    var emmrate = fallingStars.velocityOverLifetime;
                    var emmrate2 = fallingStarsBack.velocityOverLifetime;
                    starGO.SetActive(true);
                    emmrate.speedModifier = starSpeed;
                    emmrate2.speedModifier = starSpeedBack;
                    emm = fallingStars.emission;
                    emm2 = fallingStarsBack.emission;
                    emm.rateOverTime = starDensity * 6f;
                    emm2.rateOverTime = starDensity * 6f;
                    fallingStars.Play();
                    fallingStarsBack.Play();
                    break;
                default:
                    fallingStars.Stop();
                        fallingStarsBack.Stop();
                    break;
            }
        }
    }

}





