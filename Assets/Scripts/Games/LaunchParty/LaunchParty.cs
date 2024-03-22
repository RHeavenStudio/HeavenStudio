using HeavenStudio.Util;
using HeavenStudio.Common;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRocketLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) 
        {
            return new Minigame("launchParty", "Launch Party", "000000", false, false, new List<GameAction>()
            {
                new GameAction("rocket", "Family Model")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.LaunchRocket(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 2, -1), "Spawn Offset", "Set when the rocket should rise up."),
                        new Param("note1", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 2, 0,4, "launchParty/rocket_note"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 4, 0,4, "launchParty/rocket_note"), "2nd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 5, 0,4, "launchParty/rocket_note"), "3rd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 7, 0,4, "launchParty/rocket_note"), "4th Note", "Set the number of semitones up or down this note should be pitched.")
                    }
                },
                new GameAction("partyCracker", "Party-Popper")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.LaunchPartyCracker(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "Set when the rocket should rise up."),
                        new Param("note1", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 4, 0, 4, "launchParty/popper_note"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 5, 0, 4, "launchParty/popper_note"), "2nd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 7, 0, 4, "launchParty/popper_note"), "3rd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 9, 0, 4, "launchParty/popper_note"), "4th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note5", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 11, 0, 4, "launchParty/popper_note"), "5th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note6", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 12, 0, 4, "launchParty/popper_note"), "6th Note", "Set the number of semitones up or down this note should be pitched.")
                    }
                },
                new GameAction("bell", "Bell")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.LaunchBell(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"], e["note7"], e["note8"],
                        e["note9"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "Set when the rocket should rise up."),
                        new Param("note1", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/bell_note"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 2, 0, 4, "launchParty/bell_short"), "2nd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 4, 0, 4, "launchParty/bell_short"), "3rd Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 5, 0, 4, "launchParty/bell_short"), "4th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note5", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 7, 0, 4, "launchParty/bell_short"), "5th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note6", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 9, 0, 4, "launchParty/bell_short"), "6th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note7", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 11, 0, 4, "launchParty/bell_short"), "7th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note8", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 12, 0, 4, "launchParty/bell_short"), "8th Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note9", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/bell_blast"), "9th Note (Launch)", "Set the number of semitones up or down this note should be pitched."),
                    }
                },
                new GameAction("bowlingPin", "Bowling Pin")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; LaunchParty.LaunchBowlingPin(e.beat, e["offset"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"], e["note7"], 
                        e["note8"], e["note9"], e["note10"], e["note11"], e["note12"], e["note13"], e["note14"], e["note15"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("offset", new EntityTypes.Float(-1, 1, -1), "Spawn Offset", "Set when the rocket should rise up."),
                        new Param("note1", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 5, 0, 4, "launchParty/pin"), "1st Note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "2nd Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "3rd Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "4th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note5", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "5th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note6", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "6th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note7", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "7th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note8", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "8th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note9", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "9th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note10", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "10th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note11", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "11th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note12", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, -1, 0, 4, "launchParty/flute"), "12th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note13", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0, 0, 4, "launchParty/flute"), "13th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note14", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 7, 0, 4, "launchParty/flute"), "14th Note (Flute)", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note15", new EntityTypes.Note(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 7, 0, 4, "launchParty/pin"), "15th Note", "The number of semitones up or down this note should be pitched")
                    }
                },
                new GameAction("posMove", "Change Launch Pad Position")
                {
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("xPos", new EntityTypes.Float(-40f, 40f, 0f), "X Position", "Set the position on the X axis that the Launch Pad should travel to."),
                        new Param("yPos", new EntityTypes.Float(-30f, 30f, 0f), "Y Position", "Set the position on the Y axis that the Launch Pad should travel to."),
                        new Param("zPos", new EntityTypes.Float(-90f, 90f, 0f), "Z Position", "Set the position on the Z axis that the Launch Pad should travel to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("rotMove", "Change Launch Pad Rotation")
                {
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("rot", new EntityTypes.Float(-360, 360, 0), "Angle", "Which angle of rotation should the Launch Pad rotate towards?"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                new GameAction("toggleStars", "Falling Stars")
                {
                    // function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.CreateParticles(e.beat, e["toggle"], e["valA"], e["valB"], e["valC"]);},
                    hidden = true,
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stars", "Toggle if stars should fall from the top of the screen.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "valA", "valB", "valC"})
                        }),
                        new Param("valA", new EntityTypes.Float(0.1f, 10f, 1f), "Star Density", "Set how many stars are spawned."),
                        new Param("valB", new EntityTypes.Float(0.01f, 5f, 0.1f), "Front Star Fall Speed", "Set how fast the front stars fall to the bottom edge of the screen."),
                        new Param("valC", new EntityTypes.Float(0.01f, 5f, 0.1f), "Back Star Fall Speed", "Set how fast the stars fall to the bottom edge of the screen.")
                    }
                },
                new GameAction("scrollSpeed", "Scroll Speed")
                {
                    // function = delegate {var e = eventCaller.currentEntity; LaunchParty.instance.UpdateScrollSpeed(e["speed"]); },
                    hidden = true,
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("speed", new EntityTypes.Float(0, 100, 0.5f), "Scroll Speed", "Set how fast the background will scroll down."),
                    }
                }
            },
            new List<string>() {"rvl", "normal"},
            "rvlrocket", "en",
            new List<string>() {}
            );
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
        [SerializeField] Transform launchPad;
        [SerializeField] Transform launchPadRotatable;
        [SerializeField] Transform spawnPad;
        public Animator launchPadSpriteAnim;
        [SerializeField] private SpriteRenderer _bgWhiteOverlay;

        [Header("Variables")]
        private float currentRotBeat;
        private float currentPosBeat;
        private float currentRotLength;
        private float currentPosLength;
        private Vector3 lastPadPos = new Vector3(0, -2.4f, 0);
        private Vector3 currentPadPos = new Vector3(0, -2.4f, 0);
        private float lastPadRotation;
        private float currentPadRotation;
        private Util.EasingFunction.Ease lastPosEase;
        private Util.EasingFunction.Ease lastRotEase;
        public enum RocketType
        {
            Family = 0,
            Cracker = 1,
            Bell = 2,
            BowlingPin = 3
        }
        public struct QueuedRocket
        {
            public RocketType type;
            public double beat;
            public float offSet;
            public List<int> notes; 
        }
        private static List<QueuedRocket> queuedRockets = new List<QueuedRocket>();

        private int currentPosIndex;

        private int currentRotIndex;

        private List<RiqEntity> allPosEvents = new();

        private List<RiqEntity> allRotEvents = new();

        private List<RiqEntity> _allOverlayEvents = new();

        public static LaunchParty instance;

        void OnDestroy()
        {
            if (queuedRockets.Count > 0) queuedRockets.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        } 

        void Awake()
        {
            instance = this;
        }

        public override void OnGameSwitch(double beat)
        {
            HandleLaunchPadMoveEvents(beat);
        }

        public override void OnPlay(double beat)
        {
            HandleLaunchPadMoveEvents(beat);
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedRockets.Count > 0)
                {
                    foreach (var rocket in queuedRockets)
                    {
                        SpawnRocket(rocket.beat, rocket.offSet, rocket.type, rocket.notes);
                    }
                    queuedRockets.Clear();
                }
            }
            LaunchPadPositionAndRotationUpdate(cond);
        }

        private void UpdateOverlay(Conductor cond)
        {

        }

        #region Launch Pad Position and Rotation

        private void HandleLaunchPadMoveEvents(double beat)
        {
            var posEvents = EventCaller.GetAllInGameManagerList("launchParty", new string[] { "posMove" });
            allPosEvents = posEvents;

            var rotEvents = EventCaller.GetAllInGameManagerList("launchParty", new string[] { "rotMove" });
            allRotEvents = rotEvents;

            UpdateLaunchPadPos();
            UpdateLaunchPadRot();
            LaunchPadPositionAndRotationUpdate(Conductor.instance);
        }

        private void LaunchPadPositionAndRotationUpdate(Conductor cond)
        {
            if (allPosEvents.Count > 0)
            {
                if (currentPosIndex < allPosEvents.Count && currentPosIndex >= 0)
                {
                    if (cond.songPositionInBeatsAsDouble >= allPosEvents[currentPosIndex].beat)
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
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastPosEase);

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
                    if (cond.songPositionInBeatsAsDouble >= allRotEvents[currentRotIndex].beat)
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
                        launchPadRotatable.rotation = Quaternion.Euler(0, 0, currentPadRotation);
                    }
                    else
                    {
                        if (currentRotLength < 0)
                        {
                            launchPadRotatable.rotation = Quaternion.Euler(0, 0, currentPadRotation);
                        }
                        else
                        {
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastRotEase);

                            float newRotZ = func(lastPadRotation, currentPadRotation, normalizedBeat);
                            launchPadRotatable.rotation = Quaternion.Euler(0, 0, newRotZ);
                        }
                    }
                }
            }
        }

        private void UpdateLaunchPadPos()
        {
            if (currentPosIndex < allPosEvents.Count && currentPosIndex >= 0)
            {
                lastPadPos = launchPad.position;
                currentPosBeat = (float)allPosEvents[currentPosIndex].beat;
                currentPosLength = allPosEvents[currentPosIndex].length;
                currentPadPos = new Vector3(allPosEvents[currentPosIndex]["xPos"], allPosEvents[currentPosIndex]["yPos"], allPosEvents[currentPosIndex]["zPos"]);
                lastPosEase = (Util.EasingFunction.Ease)allPosEvents[currentPosIndex]["ease"];
            }
        }

        private void UpdateLaunchPadRot()
        {
            if (currentRotIndex < allRotEvents.Count && currentRotIndex >= 0)
            {
                lastPadRotation = launchPadRotatable.rotation.eulerAngles.z;
                currentRotBeat = (float)allRotEvents[currentRotIndex].beat;
                currentRotLength = allRotEvents[currentRotIndex].length;
                currentPadRotation = allRotEvents[currentRotIndex]["rot"];
                lastRotEase = (Util.EasingFunction.Ease)allRotEvents[currentRotIndex]["ease"];
            }
        }

        #endregion

        #region Rockets

        public void SpawnRocket(double beat, float beatOffset, RocketType type, List<int> notes)
        {
            GameObject rocketToSpawn = rocket;
            switch (type)
            {
                case RocketType.Family:
                    rocketToSpawn = rocket;
                    break;
                case RocketType.Cracker:
                    rocketToSpawn = partyCracker;
                    break;
                case RocketType.Bell:
                    rocketToSpawn = bell;
                    break;
                case RocketType.BowlingPin:
                    rocketToSpawn = bowlingPin;
                    break;
            }
            GameObject spawnedRocket = Instantiate(rocketToSpawn, spawnPad, false);
            var rocketScript = spawnedRocket.GetComponent<LaunchPartyRocket>();
            List<float> pitchedNotes = new List<float>();
            foreach (var note in notes)
            {
                pitchedNotes.Add(SoundByte.GetPitchFromSemiTones(note, true));
            }
            rocketScript.pitches.AddRange(pitchedNotes);
            switch (type)
            {
                case RocketType.Family:
                    rocketScript.InitFamilyRocket(beat);
                    break;
                case RocketType.Cracker:
                    rocketScript.InitPartyCracker(beat);
                    break;
                case RocketType.Bell:
                    rocketScript.InitBell(beat);
                    break;
                case RocketType.BowlingPin:
                    rocketScript.InitBowlingPin(beat);
                    break;
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + beatOffset, delegate { rocketScript.Rise(); })
            });
        }

        public static void LaunchRocket(double beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour)
        {
            List<int> pitches = new List<int>()
            {
                noteOne,
                noteTwo,
                noteThree,
                noteFour
            };
            if (GameManager.instance.currentGame == "launchParty")
            {
                LaunchParty.instance.SpawnRocket(beat, beatOffset, RocketType.Family, pitches);
            }
            else
            {
                queuedRockets.Add(new QueuedRocket { beat = beat, offSet = beatOffset, notes = pitches, type = RocketType.Family});
            }
        }

        public static void LaunchPartyCracker(double beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix)
        {
            List<int> pitches = new List<int>()
            {
                noteOne,
                noteTwo,
                noteThree,
                noteFour,
                noteFive,
                noteSix,
            };
            if (GameManager.instance.currentGame == "launchParty")
            {
                LaunchParty.instance.SpawnRocket(beat, beatOffset, RocketType.Cracker, pitches);
            }
            else
            {
                queuedRockets.Add(new QueuedRocket { beat = beat, offSet = beatOffset, notes = pitches, type = RocketType.Cracker });
            }
        }

        public static void LaunchBell(double beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix, int noteSeven, int noteEight, int noteNine)
        {
            List<int> pitches = new List<int>()
            {
                noteOne,
                noteTwo,
                noteThree,
                noteFour,
                noteFive,
                noteSix,
                noteSeven,
                noteEight,
                noteNine
            };
            if (GameManager.instance.currentGame == "launchParty")
            {
                LaunchParty.instance.SpawnRocket(beat, beatOffset, RocketType.Bell, pitches);
            }
            else
            {
                queuedRockets.Add(new QueuedRocket { beat = beat, offSet = beatOffset, notes = pitches, type = RocketType.Bell });
            }
        }

        public static void LaunchBowlingPin(double beat, float beatOffset, int noteOne, int noteTwo, int noteThree, int noteFour, int noteFive, int noteSix, int noteSeven, 
            int noteEight, int noteNine, int noteTen, int noteEleven, int noteTwelve, int noteThirteen, int noteFourteen, int noteFifteen)
        {
            List<int> pitches = new List<int>()
            {
                noteOne,
                noteTwo,
                noteThree,
                noteFour,
                noteFive,
                noteSix,
                noteSeven,
                noteEight,
                noteNine,
                noteTen,
                noteEleven,
                noteTwelve,
                noteThirteen,
                noteFourteen,
                noteFifteen
            };
            if (GameManager.instance.currentGame == "launchParty")
            {
                LaunchParty.instance.SpawnRocket(beat, beatOffset, RocketType.BowlingPin, pitches);
            }
            else
            {
                queuedRockets.Add(new QueuedRocket { beat = beat, offSet = beatOffset, notes = pitches, type = RocketType.BowlingPin });
            }
        }

        #endregion
    }
}





