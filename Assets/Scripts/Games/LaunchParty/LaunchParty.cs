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
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchRocket(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 2, -1), "Spawn Offset", "When should the rocket rise up?"),
                        new Param("note1", new EntityTypes.Integer(-24, 24, 2), "1st Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note2", new EntityTypes.Integer(-24, 24, 4), "2nd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note3", new EntityTypes.Integer(-24, 24, 5), "3rd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note4", new EntityTypes.Integer(-24, 24, 7), "4th Note", "The number of semitones up or down this note should be pitched")
                    }
                },
                new GameAction("partyCracker", "Launch Party Cracker")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchPartyCracker(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "When should the rocket rise up?"),
                        new Param("note1", new EntityTypes.Integer(-24, 24, 4), "1st Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note2", new EntityTypes.Integer(-24, 24, 5), "2nd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note3", new EntityTypes.Integer(-24, 24, 7), "3rd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note4", new EntityTypes.Integer(-24, 24, 9), "4th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note5", new EntityTypes.Integer(-24, 24, 11), "5th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note6", new EntityTypes.Integer(-24, 24, 12), "6th Note", "The number of semitones up or down this note should be pitched")
                    }
                },
                new GameAction("bell", "Launch Bell")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchBell(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"], e["note7"], e["note8"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "When should the rocket rise up?"),
                        new Param("note1", new EntityTypes.Integer(-24, 24, 0), "1st Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note2", new EntityTypes.Integer(-24, 24, 2), "2nd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note3", new EntityTypes.Integer(-24, 24, 4), "3rd Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note4", new EntityTypes.Integer(-24, 24, 5), "4th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note5", new EntityTypes.Integer(-24, 24, 7), "5th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note6", new EntityTypes.Integer(-24, 24, 9), "6th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note7", new EntityTypes.Integer(-24, 24, 11), "7th Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note8", new EntityTypes.Integer(-24, 24, 12), "8th Note", "The number of semitones up or down this note should be pitched")
                    }
                },
                new GameAction("bowlingPin", "Launch Bowling Pin")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.instance.LaunchBowlingPin(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"], e["note7"], 
                        e["note8"], e["note9"], e["note10"], e["note11"], e["note12"], e["note13"], e["note14"], e["note15"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "When should the rocket rise up?"),
                        new Param("note1", new EntityTypes.Integer(-24, 24, 5), "1st Note", "The number of semitones up or down this note should be pitched"),
                        new Param("note2", new EntityTypes.Integer(-24, 24, -1), "2nd Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note3", new EntityTypes.Integer(-24, 24, 0), "3rd Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note4", new EntityTypes.Integer(-24, 24, -1), "4th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note5", new EntityTypes.Integer(-24, 24, 0), "5th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note6", new EntityTypes.Integer(-24, 24, -1), "6th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note7", new EntityTypes.Integer(-24, 24, 0), "7th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note8", new EntityTypes.Integer(-24, 24, -1), "8th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note9", new EntityTypes.Integer(-24, 24, 0), "9th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note10", new EntityTypes.Integer(-24, 24, -1), "10th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note11", new EntityTypes.Integer(-24, 24, 0), "11th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note12", new EntityTypes.Integer(-24, 24, -1), "12th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note13", new EntityTypes.Integer(-24, 24, 0), "13th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note14", new EntityTypes.Integer(-24, 24, 7), "14th Note (Flute)", "The number of semitones up or down this note should be pitched"),
                        new Param("note15", new EntityTypes.Integer(-24, 24, 7), "15th Note", "The number of semitones up or down this note should be pitched")
                    }
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
    using Scripts_LaunchParty;
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

        void Awake()
        {
            instance = this;
        }

        public void LaunchRocket(float beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour)
        {

            GameObject spawnedRocket = Instantiate(rocket, launchPad, false);
            var rocketScript = spawnedRocket.GetComponent<LaunchPartyRocket>();
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteOne) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTwo) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteThree) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFour) * Conductor.instance.musicSource.pitch);
            rocketScript.InitFamilyRocket(beat);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatOffset, delegate { rocketScript.Rise(); })
            });
        }

        public void LaunchPartyCracker(float beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix)
        {
            GameObject spawnedRocket = Instantiate(partyCracker, launchPad, false);
            var rocketScript = spawnedRocket.GetComponent<LaunchPartyRocket>();
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteOne) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTwo) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteThree) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFour) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFive) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteSix) * Conductor.instance.musicSource.pitch);
            rocketScript.InitPartyCracker(beat);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatOffset, delegate { rocketScript.Rise(); })
            });
        }

        public void LaunchBell(float beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix, int noteSeven, int noteEight)
        {
            GameObject spawnedRocket = Instantiate(bell, launchPad, false);
            var rocketScript = spawnedRocket.GetComponent<LaunchPartyRocket>();
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteOne) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTwo) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteThree) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFour) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFive) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteSix) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteSeven) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteEight) * Conductor.instance.musicSource.pitch);
            rocketScript.InitBell(beat);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatOffset, delegate { rocketScript.Rise(); })
            });
        }

        public void LaunchBowlingPin(float beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix, int noteSeven, 
            int noteEight, int noteNine, int noteTen, int noteEleven, int noteTwelve, int noteThirteen, int noteFourteen, int noteFifteen)
        {
            GameObject spawnedRocket = Instantiate(bowlingPin, launchPad, false);
            var rocketScript = spawnedRocket.GetComponent<LaunchPartyRocket>();
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteOne) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTwo) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteThree) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFour) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFive) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteSix) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteSeven) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteEight) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteNine) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTen) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteEleven) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteTwelve) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteThirteen) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFourteen) * Conductor.instance.musicSource.pitch);
            rocketScript.pitches.Add(Mathf.Pow(2f, (1f / 12f) * noteFifteen) * Conductor.instance.musicSource.pitch);
            rocketScript.InitBowlingPin(beat);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatOffset, delegate { rocketScript.Rise(); })
            });
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





