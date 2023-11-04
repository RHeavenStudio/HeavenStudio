using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSoccerLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceSoccer", "Space Soccer", "ff7d27", false, false, new List<GameAction>()
            {
                new GameAction("ball dispense", "Ball Dispense")
                {
                    function = delegate { SpaceSoccer.instance.Dispense(eventCaller.currentEntity.beat, !eventCaller.currentEntity["toggle"], false, eventCaller.currentEntity["down"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Disable Sound", "Disables the dispense sound"),
                        new Param("down", false, "Down Sound", "Will the Down sound be played?")
                    },
                    inactiveFunction = delegate
                    {
                        if (!eventCaller.currentEntity["toggle"]) { SpaceSoccer.DispenseSound(eventCaller.currentEntity.beat, eventCaller.currentEntity["down"]);}
                    }
                },
                new GameAction("high kick-toe!", "High Kick-Toe!")
                {
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("swing", new EntityTypes.Float(0, 1, 0.5f), "Swing", "The amount of swing")
                    }
                },
                new GameAction("npc kickers enter or exit", "NPC Kickers Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.NPCKickersEnterOrExit(e.beat, e.length, e["choice"], e["ease"], e["amount"], e["x"], e["y"], e["z"], e["override"], e["preset"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("preset", SpaceSoccer.EnterExitPresets.FiveKickers, "Preset", "Which preset should be used?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)SpaceSoccer.EnterExitPresets.Custom, new string[] { "amount", "x", "y", "z" })
                        }),
                        
                        
                        new Param("amount", new EntityTypes.Integer(2, 30, 5), "Amount", "Amount of Space Kickers."),
                        new Param("x", new EntityTypes.Float(-30, 30, 2f), "X Distance", "How much distance should there be between the space kickers on the x axis?"),
                        new Param("y", new EntityTypes.Float(-30, 30, -0.5f), "Y Distance", "How much distance should there be between the space kickers on the y axis?"),
                        new Param("z", new EntityTypes.Float(-30, 30, 1.25f), "Z Distance", "How much distance should there be between the space kickers on the z axis?"),

                        new Param("choice", SpaceSoccer.AnimationToPlay.Enter, "Enter Or Exit", "Whether the kickers should exit or enter."),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The Ease of the entering or exiting."),
                        new Param("override", true, "Override Easing", "Should this block override the easing of the space kickers' positions?")
                    },
                    resizable = true
                },
                new GameAction("easePos", "Ease NPC Space Kicker Distances")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceSoccer.instance.EaseSpaceKickersPositions(e.beat, e.length, e["ease"], e["x"], e["y"], e["z"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("x", new EntityTypes.Float(-30, 30, 2f), "X Distance", "How much distance should there be between the space kickers on the x axis?"),
                        new Param("y", new EntityTypes.Float(-30, 30, -0.5f), "Y Distance", "How much distance should there be between the space kickers on the y axis?"),
                        new Param("z", new EntityTypes.Float(-30, 30, 1.25f), "Z Distance", "How much distance should there be between the space kickers on the z axis?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The Ease of the space kickers moving."),
                    }
                },
                new GameAction("pMove", "Move Player")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceSoccer.instance.MovePlayerKicker(e.beat, e.length, e["ease"], e["x"], e["y"], e["z"], e["sound"], e["preset"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("preset", SpaceSoccer.PlayerPresets.LaunchStart, "Preset", "Which preset should be used?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)SpaceSoccer.PlayerPresets.Custom, new string[] { "x", "y", "z", "ease", "sound" })
                        }),
                        new Param("x", new EntityTypes.Float(-30, 30, 0f), "X Pos", "Which position should the player move to on the x axis?"),
                        new Param("y", new EntityTypes.Float(-30, 30, 0f), "Y Pos", "Which position should the player move to on the y axis?"),
                        new Param("z", new EntityTypes.Float(-30, 30, 0f), "Z Pos", "Which position should the player move to on the z axis?"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The Ease of the space kickers moving."),
                        new Param("sound", SpaceSoccer.LaunchSoundToPlay.None, "Sound", "Which launch sound should be played at the start of this block?")
                    }
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceSoccer.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["startDots"], e["endDots"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", SpaceSoccer.defaultBGColor, "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", SpaceSoccer.defaultBGColor, "End Color", "The end color for the fade."),
                        new Param("startDots", Color.white, "Start Color (Dots)", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("endDots", Color.white, "End Color (Dots)", "The end color for the fade."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("scroll", "Scrolling Background")
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.UpdateScrollSpeed(e["x"], e["y"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>() {
                        new Param("x", new EntityTypes.Float(-10f, 10, 0.1f), "Horizontal", "How fast does the background move horizontally?"),
                        new Param("y", new EntityTypes.Float(-10, 10f, 0.3f), "Vertical", "How fast does the background move vertically?"),
                    }
                },
                new GameAction("stopBall", "Stop Ball")
                {
                    function = delegate { SpaceSoccer.instance.StopBall(eventCaller.currentEntity["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Stop ball?", "Should the ball be stopped? If ticked off the kickers will regain their ability to use the ball.")
                    }
                },
                // This is still here for "backwards-compatibility" but is hidden in the editor (it does absolutely nothing however)
                new GameAction("keep-up", "")
                {
                    defaultLength = 4f,
                    resizable = true,
                    hidden = true
                },
                new GameAction("npc kickers instant enter or exit", "NPC Kickers Instant Enter or Exit")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        int choice;
                        if (e["toggle"])
                        {
                            choice = (int)SpaceSoccer.AnimationToPlay.Exit;
                        }
                        else
                        {
                            choice = (int)SpaceSoccer.AnimationToPlay.Enter;
                        }
                        SpaceSoccer.instance.NPCKickersEnterOrExit(e.beat, e.length, choice, (int)EasingFunction.Ease.Instant, 5, 1.75f, 0.25f, 0.75f, true, (int)SpaceSoccer.EnterExitPresets.Custom);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should Exit?", "Whether the kickers should be exited or entered.")
                    },
                    hidden = true
                },
            },
            new List<string>() { "ntr", "keep" },
            "ntrsoccer", "en",
            new List<string>() { "en" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SpaceSoccer;
    using HeavenStudio.Common;
    using UnityEngine.Rendering;
    using UnityEngine.UI;

    public class SpaceSoccer : Minigame
    {
        public enum EnterExitPresets
        {
            FiveKickers,
            DuoKickers,
            Custom
        }
        public enum PlayerPresets
        {
            LaunchStart = 0,
            LaunchEnd = 1,
            Custom = 2
        }
        public enum LaunchSoundToPlay
        {
            None = 0,
            LaunchStart = 1,
            LaunchEnd = 2
        }
        public enum AnimationToPlay
        {
            Enter = 0,
            Exit = 1
        }
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FF7D27", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        [Header("Components")]
        [SerializeField] private GameObject kickerPrefab;
        [SerializeField] private GameObject ballRef;
        [SerializeField] private List<Kicker> kickers;
        [SerializeField] private CanvasScroll backgroundSprite;
        [SerializeField] private RawImage bgImage;
        [SerializeField] private SpriteRenderer bg;

        [Header("Properties")]
        [SerializeField] SuperCurveObject.Path[] ballPaths;
        public bool ballDispensed;
        double lastDispensedBeat;
        float xScrollMultiplier = 0.1f;
        float yScrollMultiplier = 0.3f;
        [SerializeField] private float xBaseSpeed = 1;
        [SerializeField] private float yBaseSpeed = 1;
        #region Space Kicker Position Easing
        float easeBeat;
        float easeLength;
        EasingFunction.Ease lastEase;
        Vector3 lastPos = new Vector3();
        Vector3 currentPos = new Vector3();
        float easeBeatP;
        float easeLengthP;
        EasingFunction.Ease lastEaseP;
        Vector3 lastPosP = new Vector3();
        Vector3 currentPosP = new Vector3();
        #endregion

        public static SpaceSoccer instance { get; private set; }

        private void Awake()
        {
            instance = this;
            colorStart = defaultBGColor;
            colorEnd = defaultBGColor;
        }

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballPaths)
            {
                if (path.preview)
                {
                    ballRef.GetComponent<Ball>().DrawEditorGizmo(path);
                }
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            BackgroundColorUpdate();
            backgroundSprite.NormalizedX -= xBaseSpeed * xScrollMultiplier * Time.deltaTime;
            backgroundSprite.NormalizedY += yBaseSpeed * yScrollMultiplier * Time.deltaTime;

            float normalizedEaseBeat = cond.GetPositionFromBeat(easeBeat, easeLength);
            if (normalizedEaseBeat <= 1 && normalizedEaseBeat > 0)
            {
                EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                float newPosX = func(lastPos.x, currentPos.x, normalizedEaseBeat);
                float newPosY = func(lastPos.y, currentPos.y, normalizedEaseBeat);
                float newPosZ = func(lastPos.z, currentPos.z, normalizedEaseBeat);
                UpdateKickersPositions(newPosX, newPosY, newPosZ);
            }

            float normalizedPBeat = cond.GetPositionFromBeat(easeBeatP, easeLengthP);
            if (normalizedPBeat <= 1 && normalizedPBeat > 0)
            {
                EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEaseP);
                float newPosX = func(lastPosP.x, currentPosP.x, normalizedPBeat);
                float newPosY = func(lastPosP.y, currentPosP.y, normalizedPBeat);
                float newPosZ = func(lastPosP.z, currentPosP.z, normalizedPBeat);
                kickers[0].transform.parent.position = new Vector3(3.384f - newPosX, newPosY, newPosZ);
            }
        }

        public void StopBall(bool stop)
        {
            foreach (var kicker in kickers)
            {
                kicker.StopBall(stop);
            }
        }

        public void NPCKickersEnterOrExit(double beat, float length, int animToPut, int easeToPut, int amount, float xDistance, float yDistance, float zDistance, bool overrideEasing, int preset)
        {
            switch (preset)
            {
                case (int)EnterExitPresets.Custom:
                    UpdateSpaceKickers(amount, xDistance, yDistance, zDistance, overrideEasing);
                    break;
                case (int)EnterExitPresets.DuoKickers:
                    UpdateSpaceKickers(2, 7, -6, 10, overrideEasing);
                    break;
                case (int)EnterExitPresets.FiveKickers:
                    UpdateSpaceKickers(5, 2, -0.5f, 1.25f, overrideEasing);
                    break;
            }

            string animName = "Enter";
            switch (animToPut)
            {
                case (int)AnimationToPlay.Enter:
                    animName = "Enter";
                    break;
                case (int)AnimationToPlay.Exit:
                    animName = "Exit";
                    break;
            }
            foreach (var kicker in kickers)
            {
                if (kicker.player) continue;
                kicker.SetAnimParams(beat, length, animName, easeToPut);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            foreach(var entity in GameManager.instance.Beatmap.Entities)
            {
                if(entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if(entity.datamodel != "spaceSoccer/ball dispense" || entity.beat + entity.length <= beat) //check for dispenses that happen right before the switch
                {
                    continue;
                }
                bool isOnGameSwitchBeat = entity.beat == beat;
                Debug.Log(isOnGameSwitchBeat);
                Dispense(entity.beat, isOnGameSwitchBeat && !entity["toggle"], false, isOnGameSwitchBeat && entity["down"]);
                break;
            }

            PersistColor(beat);
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in ballPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        public void UpdateScrollSpeed(float scrollSpeedX, float scrollSpeedY) 
        {
            xScrollMultiplier = scrollSpeedX;
            yScrollMultiplier = scrollSpeedY;
        }

        public void EaseSpaceKickersPositions(double beat, float length, int ease, float xDistance, float yDistance, float zDistance)
        {
            easeBeat = (float)beat;
            easeLength = length;
            lastEase = (EasingFunction.Ease)ease;
            lastPos = currentPos;
            currentPos = new Vector3(xDistance, yDistance, zDistance);
        }

        public void UpdateKickersPositions(float xDistance, float yDistance, float zDistance)
        {
            for (int i = 1; i < kickers.Count; i++)
            {
                kickers[i].transform.parent.position = new Vector3(3.384f - xDistance * i, -yDistance * i, zDistance * i);
                CircularMotion circularMotion = kickers[i].GetComponent<CircularMotion>();
                circularMotion.width = 0.85f - Mathf.Pow(zDistance * 10f, -1f);
                circularMotion.height = 0.5f - Mathf.Pow(zDistance * 10f, -1f);
            }
        }

        public void MovePlayerKicker(double beat, float length, int ease, float xPos, float yPos, float zPos, int soundToPlay, int preset)
        {
            switch (preset)
            {
                case (int)PlayerPresets.Custom:
                    break;
                case (int)PlayerPresets.LaunchStart:
                    lastEaseP = EasingFunction.Ease.EaseInOutCubic;
                    xPos = -6;
                    yPos = 15;
                    zPos = 0;
                    soundToPlay = (int)LaunchSoundToPlay.LaunchStart;
                    break;
                case (int)PlayerPresets.LaunchEnd:
                    lastEaseP = EasingFunction.Ease.EaseInOutQuint;
                    xPos = -4;
                    yPos = 15;
                    zPos = 0;
                    soundToPlay = (int)LaunchSoundToPlay.LaunchEnd;
                    break;
            }
            easeBeatP = (float)beat;
            easeLengthP = length;
            lastEaseP = (EasingFunction.Ease)ease;
            lastPosP = currentPosP;
            currentPosP = new Vector3(-xPos, yPos, -zPos);
            switch (soundToPlay)
            {
                case (int)LaunchSoundToPlay.None:
                    break;
                case (int)LaunchSoundToPlay.LaunchStart:
                    SoundByte.PlayOneShotGame("spaceSoccer/jet1");
                    break;
                case (int)LaunchSoundToPlay.LaunchEnd:
                    SoundByte.PlayOneShotGame("spaceSoccer/jet2");
                    break;
            }
        }

        public void UpdateSpaceKickers(int amount, float xDistance = 1.75f, float yDistance = 0.25f, float zDistance = 0.75f, bool overrideEasing = true)
        {
            for (int i = kickers.Count - 1; i > 0; i--)
            {
                if (i >= amount)
                {
                    Kicker kickerToDestroy = kickers[i];
                    kickers.Remove(kickerToDestroy);
                    Destroy(kickerToDestroy.transform.parent.gameObject);
                }
            }
            if (overrideEasing) 
            {
                UpdateKickersPositions(xDistance, yDistance, zDistance);
                currentPos = new Vector3(xDistance, yDistance, zDistance);
            }

            for (int i = kickers.Count; i < amount; i++)
            {
                Transform kickerHolder = Instantiate(kickerPrefab, transform).transform;
                kickerHolder.transform.position = new Vector3(kickerHolder.transform.position.x - xDistance * i, kickerHolder.transform.position.y - yDistance * i, kickerHolder.transform.position.z + zDistance * i);
                Kicker spawnedKicker = kickerHolder.GetChild(0).GetComponent<Kicker>();
                CircularMotion circularMotion = spawnedKicker.GetComponent<CircularMotion>();
                circularMotion.width = 0.85f - Mathf.Pow(zDistance * 10f, -1f);
                circularMotion.height = 0.5f - Mathf.Pow(zDistance * 10f, -1f);
                circularMotion.timeOffset = kickers[0].GetComponent<CircularMotion>().timeCounter;
                if (0 > zDistance)
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = i;
                }
                else
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = -i;
                }

                kickers.Add(spawnedKicker);
                kickerHolder.gameObject.SetActive(true);
            }
            if (ballDispensed) Dispense(lastDispensedBeat, false, true);
        }

        public void Dispense(double beat, bool playSound = true, bool ignorePlayer = false, bool playDown = false)
        {
            if (!ballDispensed) lastDispensedBeat = beat;
            ballDispensed = true;
            for (int i = 0; i < kickers.Count; i++)
            {
                Kicker kicker = kickers[i];
                kicker.player = i == 0;
                if (kicker.ball != null || (ignorePlayer && kicker.player)) continue;

                GameObject ball = Instantiate(ballRef, kicker.transform.GetChild(0));
                ball.SetActive(true);
                Ball ball_ = ball.GetComponent<Ball>();
                ball_.Init(kicker, (float)beat);
                if (kicker.player && playSound)
                {
                    DispenseSound((float)beat, playDown);
                }
                kicker.DispenseBall((float)beat);

                kicker.canKick = true;
            }
        }

        public static void DispenseSound(double beat, bool playDown)
        {
            if (playDown) SoundByte.PlayOneShot("games/spaceSoccer/down", beat);
            MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("spaceSoccer/dispenseNoise",   beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble1", beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2", beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2B",beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble3", beat + 0.75f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble4", beat + 1f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble5", beat + 1.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6", beat + 1.5f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6B",beat + 1.75f),
                }, forcePlay:true);
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart; //obviously put to the default color of the game
        private Color colorEnd;
        private Color colorStartDot = Color.white; //obviously put to the default color of the game
        private Color colorEndDot = Color.white;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);

            float newRDot = func(colorStartDot.r, colorEndDot.r, normalizedBeat);
            float newGDot = func(colorStartDot.g, colorEndDot.g, normalizedBeat);
            float newBDot = func(colorStartDot.b, colorEndDot.b, normalizedBeat);

            bgImage.color = new Color(newRDot, newGDot, newBDot);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, Color colorStartDotSet, Color colorEndDotSet, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorStartDot = colorStartDotSet;
            colorEndDot = colorEndDotSet;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("spaceSoccer", new string[] { "changeBG" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["startDots"], lastEvent["endDots"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }
    }
}