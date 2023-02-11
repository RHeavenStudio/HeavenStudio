using HeavenStudio.Util;
using HeavenStudio.Common;
using JetBrains.Annotations;
using Starpelly.Transformer;
using System;
using System.Linq;
using System.Collections;
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
                new GameAction("rocket", "Family Model")
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
                new GameAction("partyCracker", "Party-Popper")
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
                new GameAction("bell", "Bell")
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
                new GameAction("bowlingPin", "Bowling Pin")
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
                new GameAction("posMove", "Change Launch Pad Position")
                {
                    function = delegate { LaunchParty.instance.Nothing(); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("xPos", new EntityTypes.Float(-40f, 40f, 0f), "X Position", "Which position on the X axis should the Launch Pad travel to?"),
                        new Param("yPos", new EntityTypes.Float(-30f, 30f, 0f), "Y Position", "Which position on the Y axis should the Launch Pad travel to?"),
                        new Param("zPos", new EntityTypes.Float(-90f, 90f, 0f), "Z Position", "Which position on the Z axis should the Launch Pad travel to?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the Launch Pad use?")
                    }
                },
                new GameAction("rotMove", "Change Launch Pad Rotation")
                {
                    function = delegate { LaunchParty.instance.Nothing(); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("rot", new EntityTypes.Float(-360, 360, 0), "Angle", "Which angle of rotation should the Launch Pad rotate towards?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the Launch Pad use?")
                    }
                },
                new GameAction("toggleStars", "Toggle Falling Stars")
                {
                    function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.CreateParticles(e.beat, e["toggle"], e["valA"], e["valB"], e["valC"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars Enabled", "Starfall Or No?"),
                        new Param("valA", new EntityTypes.Float(0.1f, 10f, 1f), "Star Density", "How many stars are on the screen"),
                        new Param("valB", new EntityTypes.Float(0.01f, 5f, 0.1f), "Front Star Fall Speed", "How fast the front stars fall to the edge of the screen"),
                        new Param("valC", new EntityTypes.Float(0.01f, 5f, 0.1f), "Back Star Fall Speed", "How fast the stars fall to the edge of the screen")
                    }
                },
                new GameAction("scrollSpeed", "Change Scroll Speed")
                {
                    function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.UpdateScrollSpeed(e["speed"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("speed", new EntityTypes.Float(0, 100, 0.5f), "Scroll Speed", "How fast will the background scroll down?"),
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
        [SerializeField] Transform launchPad;
        [SerializeField] Transform spawnPad;
        [SerializeField] Scroll scrollScript;
        public Animator launchPadSpriteAnim;

        [Header("Variables")]
        private float currentRotBeat;
        private float currentPosBeat;
        private float currentRotLength;
        private float currentPosLength;
        private Vector3 lastPadPos = new Vector3(0, -2.4f, 0);
        private Vector3 currentPadPos = new Vector3(0, -2.4f, 0);
        private float lastPadRotation;
        private float currentPadRotation;
        private EasingFunction.Ease lastPosEase;
        private EasingFunction.Ease lastRotEase;

        private int currentPosIndex;

        private int currentRotIndex;

        private List<DynamicBeatmap.DynamicEntity> allPosEvents = new List<DynamicBeatmap.DynamicEntity>();

        private List<DynamicBeatmap.DynamicEntity> allRotEvents = new List<DynamicBeatmap.DynamicEntity>();

        public static LaunchParty instance;

        void Awake()
        {
            instance = this;
            var posEvents = EventCaller.GetAllInGameManagerList("launchParty", new string[] { "posMove" });
            List<DynamicBeatmap.DynamicEntity> tempPosEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < posEvents.Count; i++)
            {
                if (posEvents[i].beat + posEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempPosEvents.Add(posEvents[i]);
                }
            }

            allPosEvents = tempPosEvents;

            var rotEvents = EventCaller.GetAllInGameManagerList("launchParty", new string[] { "rotMove" });
            List<DynamicBeatmap.DynamicEntity> tempRotEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < rotEvents.Count; i++)
            {
                if (rotEvents[i].beat + rotEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempRotEvents.Add(rotEvents[i]);
                }
            }

            allRotEvents = tempRotEvents;

            UpdateLaunchPadPos();
            UpdateLaunchPadRot();
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (allPosEvents.Count > 0)
            {
                if (currentPosIndex < allPosEvents.Count && currentPosIndex >= 0)
                {
                    if (cond.songPositionInBeats >= allPosEvents[currentPosIndex].beat)
                    {
                        UpdateLaunchPadPos();
                        currentPosIndex++;
                    }
                }

                float normalizedBeat = cond.GetPositionFromBeat(currentPosBeat, currentPosLength);

                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1)
                    {
                        launchPad.position = currentPadPos;
                    }
                    else
                    {
                        if (currentPosLength < 0)
                        {
                            launchPad.position = currentPadPos;
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(lastPosEase);

                            float newPosX = func(lastPadPos.x, currentPadPos.x, normalizedBeat);
                            float newPosY = func(lastPadPos.y, currentPadPos.y, normalizedBeat);
                            float newPosZ = func(lastPadPos.z, currentPadPos.z, normalizedBeat);
                            launchPad.position = new Vector3(newPosX, newPosY, newPosZ);
                        }
                    }
                }
            }
            if (allRotEvents.Count > 0)
            {
                if (currentRotIndex < allRotEvents.Count && currentRotIndex >= 0)
                {
                    if (cond.songPositionInBeats >= allRotEvents[currentRotIndex].beat)
                    {
                        UpdateLaunchPadRot();
                        currentRotIndex++;
                    }
                }

                float normalizedBeat = cond.GetPositionFromBeat(currentRotBeat, currentRotLength);

                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1)
                    {
                        launchPad.rotation = Quaternion.Euler(0, 0, currentPadRotation);
                    }
                    else
                    {
                        if (currentRotLength < 0)
                        {
                            launchPad.rotation = Quaternion.Euler(0, 0, currentPadRotation);
                        }
                        else
                        {
                            EasingFunction.Function func = EasingFunction.GetEasingFunction(lastRotEase);

                            float newRotZ = func(lastPadRotation, currentPadRotation, normalizedBeat);
                            launchPad.rotation = Quaternion.Euler(0, 0, newRotZ);
                        }
                    }
                }
            }
        }

        public void UpdateScrollSpeed(float speed)
        {
            scrollScript.scrollSpeedY = speed * -1;
        } 

        private void UpdateLaunchPadPos()
        {
            if (currentPosIndex < allPosEvents.Count && currentPosIndex >= 0)
            {
                lastPadPos = launchPad.position;
                currentPosBeat = allPosEvents[currentPosIndex].beat;
                currentPosLength = allPosEvents[currentPosIndex].length;
                currentPadPos = new Vector3(allPosEvents[currentPosIndex]["xPos"], allPosEvents[currentPosIndex]["yPos"], allPosEvents[currentPosIndex]["zPos"]);
                lastPosEase = (EasingFunction.Ease)allPosEvents[currentPosIndex]["ease"];
            }
        }

        private void UpdateLaunchPadRot()
        {
            if (currentRotIndex < allRotEvents.Count && currentRotIndex >= 0)
            {
                lastPadRotation = launchPad.rotation.eulerAngles.z;
                currentRotBeat = allRotEvents[currentRotIndex].beat;
                currentRotLength = allRotEvents[currentRotIndex].length;
                currentPadRotation = allRotEvents[currentRotIndex]["rot"];
                lastRotEase = (EasingFunction.Ease)allRotEvents[currentRotIndex]["ease"];
            }
        }

        public void LaunchRocket(float beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour)
        {

            GameObject spawnedRocket = Instantiate(rocket, spawnPad, false);
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
            GameObject spawnedRocket = Instantiate(partyCracker, spawnPad, false);
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
            GameObject spawnedRocket = Instantiate(bell, spawnPad, false);
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
            GameObject spawnedRocket = Instantiate(bowlingPin, spawnPad, false);
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

        public void Nothing()
        {

        }
    }
}





