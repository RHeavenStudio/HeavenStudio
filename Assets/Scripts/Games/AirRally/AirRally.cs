using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    
    public static class RvlBadmintonLoader
    {
        public static Minigame AddGame(EventCaller e)
        {
            return new Minigame("airRally", "Air Rally", "b5ffff", false, false, new List<GameAction>()
            {
                new GameAction("rally", "Rally")
                {
                    preFunction = delegate { AirRally.PreStartRally(e.currentEntity.beat); }, 
                    defaultLength = 2f, 
                    preFunctionLength = 1
                },
                new GameAction("ba bum bum bum", "Ba Bum Bum Bum")
                {
                    preFunction = delegate { AirRally.PreStartBaBumBumBum(e.currentEntity.beat, e.currentEntity["toggle"], e.currentEntity["toggle2"]); },
                    defaultLength = 6f, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", true, "Count", "Make Forthington Count"),
                        new Param("toggle2", false, "Alternate Voiceline")
                    },
                    preFunctionLength = 1
                },
                new GameAction("set distance", "Set Distance")
                {
                    function = delegate { AirRally.instance.SetDistance(e.currentEntity.beat, e.currentEntity["type"], e.currentEntity["ease"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?"),
                        new Param("ease", EasingFunction.Ease.EaseOutQuad, "Ease")
                    }
                },
                new GameAction("catch", "Catch Birdie"),
                new GameAction("enter", "Enter")
                {
                    function = delegate
                    {
                        AirRally.instance.SetEnter(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["ease"], true);
                    },
                    resizable = true,
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("ease", EasingFunction.Ease.EaseOutQuad, "Ease")
                    }
                },
                new GameAction("forward", "Animals Look Forward")
                {
                    function = delegate
                    {
                        AirRally.instance.Forward(e.currentEntity["reset"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("reset", false, "Reset", "Reset to Idle pose?")
                    }
                },
                new GameAction("4beat", "4 Beat Count-In")
                {
                    defaultLength = 4f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn4(e.currentEntity.beat, e.currentEntity.length);
                    },
                    function = delegate
                    {
                        AirRally.instance.ForthCountIn4Do(e.currentEntity.beat, e.currentEntity.length);
                    }
                },
                new GameAction("8beat", "8 Beat Count-In")
                {
                    defaultLength = 8f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn8(e.currentEntity.beat, e.currentEntity.length);
                    },
                    function = delegate
                    {
                        AirRally.instance.ForthCountIn8Do(e.currentEntity.beat, e.currentEntity.length);
                    }
                },
                new GameAction("forthington voice lines", "Count")
                {
                    function = delegate { AirRally.instance.ForthVoiceDo(e.currentEntity.beat); },
                    preFunction = delegate { AirRally.ForthVoice(e.currentEntity.beat, e.currentEntity["type"]); }, 
                    parameters = new List<Param>()
                    { 
                        new Param("type", AirRally.CountSound.one, "Type", "The number Forthington will say"),
                    },
                },
                new GameAction("spawnBird", "Spawn Birds")
                {
                    function = delegate
                    {
                        AirRally.instance.SpawnBirds(e.currentEntity["type"], e.currentEntity["xSpeed"], e.currentEntity["zSpeed"],
                            e.currentEntity["startZ"], e.currentEntity["invert"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("type", AirRally.BirdType.Pterosaurs, "Type"),
                        new Param("xSpeed", new EntityTypes.Float(-10, 10, 1), "X Speed Multiplier"),
                        new Param("zSpeed", new EntityTypes.Float(-10, 10, 1), "Z Speed Multiplier"),
                        new Param("startZ", new EntityTypes.Float(0, 1000, 200), "Z Start Position"),
                        new Param("invert", false, "Invert X Direction")
                    }
                },
                new GameAction("rainbow", "Rainbow")
                {
                    function = delegate
                    {
                        AirRally.instance.SpawnRainbow(e.currentEntity.beat, e.currentEntity["speed"], e.currentEntity["start"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("start", new EntityTypes.Float(0, 500, 100), "Start Position"),
                        new Param("speed", new EntityTypes.Float(-10, 10, 1), "Speed Multiplier")
                    }
                },
                new GameAction("day", "Day/Night Cycle")
                {
                    function = delegate 
                    {
                        AirRally.instance.SetDayNightCycle(e.currentEntity.beat, e.currentEntity.length,
                            (AirRally.DayNightCycle)e.currentEntity["start"], (AirRally.DayNightCycle)e.currentEntity["end"],
                            (EasingFunction.Ease)e.currentEntity["ease"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", AirRally.DayNightCycle.Day, "Start Time"),
                        new Param("end", AirRally.DayNightCycle.Noon, "End Time"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("cloud", "Cloud Density")
                {
                    function = delegate
                    {
                        AirRally.instance.SetCloudRates(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["main"], e.currentEntity["side"], e.currentEntity["top"], 
                            e.currentEntity["speed"], e.currentEntity["endSpeed"], e.currentEntity["ease"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("main", new EntityTypes.Integer(0, 300, 30), "Main Clouds", "How many clouds per second?"),
                        new Param("side", new EntityTypes.Integer(0, 100, 10), "Side Clouds", "How many clouds per second?"),
                        new Param("top", new EntityTypes.Integer(0, 100, 0), "Top Clouds", "How many clouds per second?"),
                        new Param("speed", new EntityTypes.Float(-10, 10, 1), "Speed Multiplier"),
                        new Param("endSpeed", new EntityTypes.Float(-10, 10, 1), "End Speed Multiplier"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("snowflake", "Snowflake Density")
                {
                    function = delegate
                    {
                        AirRally.instance.SetSnowflakeRates(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["cps"], 
                            e.currentEntity["speed"], e.currentEntity["endSpeed"], e.currentEntity["ease"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("cps", new EntityTypes.Integer(0, 200, 0), "Snowflakes per Second"),
                        new Param("speed", new EntityTypes.Float(-10, 10, 1), "Speed Multiplier"),
                        new Param("endSpeed", new EntityTypes.Float(-10, 10, 1), "End Speed Multiplier"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("tree", "Trees")
                {
                    function = delegate
                    {
                        AirRally.instance.SetTreeRates(e.currentEntity["enable"], e.currentEntity.beat, e.currentEntity.length, e.currentEntity["main"], e.currentEntity["side"],
                            e.currentEntity["speed"], e.currentEntity["endSpeed"], e.currentEntity["ease"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("enable", true, "Enable", "", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (bool)x, new string[] { "main", "side", "speed", "endSpeed", "ease" })
                        }),
                        new Param("main", new EntityTypes.Integer(0, 300, 50), "Main Trees", "How many trees per second?"),
                        new Param("side", new EntityTypes.Integer(0, 100, 30), "Side Trees", "How many trees per second?"),
                        new Param("speed", new EntityTypes.Float(-10, 10, 1), "Speed Multiplier"),
                        new Param("endSpeed", new EntityTypes.Float(-10, 10, 1), "End Speed Multiplier"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("islandSpeed", "Islands Speed")
                {
                    function = delegate
                    {
                        AirRally.instance.SetIslandSpeed(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["speed"],
                            e.currentEntity["endSpeed"], e.currentEntity["ease"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("speed", new EntityTypes.Float(-10, 10, 1), "Speed"),
                        new Param("endSpeed", new EntityTypes.Float(-10, 10, 1), "End Speed"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    }
                },
                new GameAction("silence", "Silence")
                {
                    defaultLength = 2f,
                    resizable = true,
                }
            },
            new List<string>() {"rvl", "normal"},
            "rvlbadminton", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_AirRally;

    public class AirRally : Minigame
    {
        private static readonly float[] countInOffsets = new float[4]
        {
            0.142f, //one
            0.140f, //two
            0.150f, //three
            0.160f, //four
        };

        private static readonly float[] nyaOffsets = new float[4]
        {
            -0.01f, //close
            -0.01f, //far
            0.003f, //farther
            -0.01f //farthest
        };

        //both of the whoosh offsets are scheduled for the beat that the player hits the birdie,
        //offset them to end just before the player hits the birdie

        //for normal rally
        private static readonly float[] whooshOffsetsRally = new float[4]
        {
            0f, //leave be
            0.210f, //far
            0.210f, //farther
            0.170f //farthest
        };

        // for ba bum bum bum
        private static readonly float[] whooshOffsetsBaBum = new float[4]
        {
            0f, //leave be
            0.380f, //far
            0.380f, //farther
            0.380f //farthest
        };

        private static readonly float[,] baBumBumBumOffsets = new float[4, 4]
        {
            { // close
                0.009f, // ba
                0.017f, // bum1
                0.014f, // bum2
                0.010f, // bum3
            },
            { // far
                0.003f, // ba
                0.020f, // bum1
                0.004f, // bum2
                0.010f, // bum3
            },
            { // farther
                0.008f, // ba
                0.080f, // bum1
                0.075f, // bum2
                0.028f, // bum3
            },
            { // farthest
                0.012f, // ba
                0.040f, // bum1
                0.026f, // bum2
                0.040f, // bum3
            },
        };

        private static readonly float[] baBumBumBumFarAltOffsets = new float[4]
        {
                0.001f, // ba
                0.012f, // bum1
                0.012f, // bum2
                0.012f, // bum3
        };

        public enum BirdType
        {
            Pterosaurs,
            Geese,
            Bluebirds
        }

        public static AirRally instance { get; set; }

        [Header("Component")]
        [SerializeField] Animator Baxter;
        [SerializeField] Animator Forthington;
        private Transform forthTrans;
        private Transform baxterTrans;
        [SerializeField] Shuttlecock Shuttlecock;
        private Shuttlecock ActiveShuttle;
        [SerializeField] GameObject objHolder;
        [SerializeField] private CloudsManager cloudManagerMain, cloudManagerLeft, cloudManagerRight, cloudManagerTop, snowflakeManager;
        [SerializeField] private CloudsManager treeManagerMain, treeManagerLeft, treeManagerRight, treeManagerLeftInner, treeManagerRightInner;
        [SerializeField] private GameObject treeHolder;
        [SerializeField] private IslandsManager islandManager;
        [SerializeField] private RvlBirds pterosaurs, geese, bluebirds, rainbow;

        [Header("Day/Night Cycle")]
        [SerializeField] private SpriteRenderer island2Lights;
        [SerializeField] private Material bgMat;
        [SerializeField] private Material objectMat;
        [SerializeField] private Material cloudMat;
        [SerializeField] private Color noonColor;
        [SerializeField] private Color nightColor;
        [SerializeField] private Color noonColorCloud;
        [SerializeField] private Color nightColorCloud;
        private DayNightCycle lastTime = DayNightCycle.Day;
        private DayNightCycle currentTime = DayNightCycle.Day;
        private Util.EasingFunction.Ease timeEase = Util.EasingFunction.Ease.Instant;
        private double startTimeBeat = 0;
        private float timeLength = 0f;

        [Header("Variables")]
        bool shuttleActive;
        public bool hasMissed;

        [Header("Waypoint")]
        [SerializeField] private float wayPointBeatLength = 0.25f;
        [SerializeField] private float wayPointEnter = -3.16f;
        private double enterStartBeat = -1;
        private float enterLength = 0f;
        private Util.EasingFunction.Ease enterEase = Util.EasingFunction.Ease.EaseOutQuad;
        private double wayPointStartBeat = 0;
        private float lastWayPointZForForth = 3.16f;
        private float wayPointZForForth = 3.16f;
        private HeavenStudio.Util.EasingFunction.Ease currentEase = HeavenStudio.Util.EasingFunction.Ease.EaseOutQuad;
        private double nextGameSwitchBeatGlobal = double.MaxValue;

        void Start()
        {
            Baxter.Play("Idle");
            Forthington.Play("Idle");
        }

        private void Awake()
        {
            instance = this;
            forthTrans = Forthington.transform;
            baxterTrans = Baxter.transform;
            if (!Conductor.instance.isPlaying)
            {
                InitClouds(0);
            }
        }      

        // Update is called once per frame
        void Update()
        {
            if(PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                Baxter.DoScaledAnimationAsync("Hit", 0.5f);
                SoundByte.PlayOneShotGame("airRally/swing");
            }

            float normalizedEnterBeat = Conductor.instance.GetPositionFromBeat(enterStartBeat, enterLength);

            if (normalizedEnterBeat < 0)
            {
                forthTrans.position = new Vector3(forthTrans.position.x, forthTrans.position.y, wayPointEnter);
                baxterTrans.position = new Vector3(baxterTrans.position.x, baxterTrans.position.y, wayPointEnter);
            }
            else if (normalizedEnterBeat >= 0 && normalizedEnterBeat <= 1f)
            {
                var func = Util.EasingFunction.GetEasingFunction(enterEase);

                float newZ = func(wayPointEnter, 3.16f, normalizedEnterBeat);
                forthTrans.position = new Vector3(forthTrans.position.x, forthTrans.position.y, newZ);
                baxterTrans.position = new Vector3(baxterTrans.position.x, baxterTrans.position.y, newZ);
            }
            else
            {
                DistanceUpdate();
            }
            DayNightCycleUpdate();
            WeatherUpdate();
            IslandSpeedUpdate();
            TreeUpdate();
        }

        #region visual doodads

        public void Forward(bool reset)
        {
            Baxter.Play(reset ? "Idle" : "Forward", 0, 0);
            Forthington.Play(reset ? "Idle" : "Forward", 0, 0);
        }

        //lol funny name
        private void WeatherUpdate()
        {
            float normalizedBeatC = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(startBeatCloudSpeed, cloudSpeedLength));
            float normalizedBeatS = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(startBeatSnowflakeSpeed, snowflakeSpeedLength));

            var funcC = Util.EasingFunction.GetEasingFunction(cloudSpeedEase);

            float newSpeedC = funcC(cloudSpeed, cloudEndSpeed, normalizedBeatC);

            cloudManagerMain.speedMult = newSpeedC;
            cloudManagerLeft.speedMult = newSpeedC;
            cloudManagerRight.speedMult = newSpeedC;
            cloudManagerTop.speedMult = newSpeedC;

            var funcS = Util.EasingFunction.GetEasingFunction(snowflakeSpeedEase);

            float newSpeedS = funcC(snowflakeSpeed, snowflakeEndSpeed, normalizedBeatC);

            snowflakeManager.speedMult = newSpeedS;
        }

        public void SpawnBirds(int type, float xSpeed, float zSpeed, float startZ, bool invert)
        {
            RvlBirds birdsToSpawn = null;
            switch ((BirdType)type)
            {
                case BirdType.Pterosaurs:
                    birdsToSpawn = pterosaurs;
                    break;
                case BirdType.Geese:
                    birdsToSpawn = geese;
                    break;
                case BirdType.Bluebirds:
                    birdsToSpawn = bluebirds;
                    break;
                default:
                    break;
            }

            RvlBirds spawnedBird = Instantiate(birdsToSpawn, transform);

            spawnedBird.speedMultX = invert ? -xSpeed : xSpeed;
            spawnedBird.speedMultZ = zSpeed;
            spawnedBird.transform.position =
                new Vector3(invert ? -spawnedBird.transform.position.x : spawnedBird.transform.position.x, 
                spawnedBird.transform.position.y, startZ);
            spawnedBird.transform.localScale = new Vector3(invert ? -1 : 1, 1, 1);
        }

        public void SpawnRainbow(double beat, float speed, float start)
        {
            RvlBirds spawnedRainbow = Instantiate(rainbow, transform);
            spawnedRainbow.speedMultZ = speed;
            spawnedRainbow.transform.position = new Vector3(spawnedRainbow.transform.position.x, spawnedRainbow.transform.position.y, start);
            spawnedRainbow.FadeIn(beat);
        }

        public void SetIslandSpeed(double beat, float length, float speed, float endSpeed, int ease)
        {
            islandStartBeat = beat;
            islandLength = length;
            islandSpeed = speed;
            islandEndSpeed = endSpeed;
            islandEase = (Util.EasingFunction.Ease)ease;
            IslandSpeedUpdate();
        }

        private double islandStartBeat = -1f;
        private float islandLength = 0;
        private float islandSpeed = 1f;
        private float islandEndSpeed = 1f;
        private Util.EasingFunction.Ease islandEase = Util.EasingFunction.Ease.Linear;

        private void IslandSpeedUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(islandStartBeat, islandLength));

            var func = Util.EasingFunction.GetEasingFunction(islandEase);

            float newSpeed = func(islandSpeed, islandEndSpeed, normalizedBeat);

            islandManager.additionalSpeedMult = newSpeed;
        }

        private double startBeatCloudSpeed = -1f;
        private float cloudSpeedLength = 0f;
        private float cloudSpeed = 1f;
        private float cloudEndSpeed = 1f;
        private Util.EasingFunction.Ease cloudSpeedEase = Util.EasingFunction.Ease.Linear;

        public void SetCloudRates(double beat, float length, int main, int side, int top, float speed, float endSpeed, int ease)
        {
            cloudManagerMain.SetCloudsPerSecond(main);
            cloudManagerLeft.SetCloudsPerSecond(side);
            cloudManagerRight.SetCloudsPerSecond(side);
            cloudManagerTop.SetCloudsPerSecond(top);

            startBeatCloudSpeed = beat;
            cloudSpeedLength = length;
            cloudEndSpeed = endSpeed;
            cloudSpeed = speed;
            cloudSpeedEase = (Util.EasingFunction.Ease)ease;
            WeatherUpdate();
        }

        private double startBeatSnowflakeSpeed = -1f;
        private float snowflakeSpeedLength = 0f;
        private float snowflakeSpeed = 1f;
        private float snowflakeEndSpeed = 1f;
        private Util.EasingFunction.Ease snowflakeSpeedEase = Util.EasingFunction.Ease.Linear;

        public void SetSnowflakeRates(double beat, float length, int cps, float speed, float endSpeed, int ease)
        {
            snowflakeManager.SetCloudsPerSecond(cps);
            startBeatSnowflakeSpeed = beat;
            snowflakeSpeedLength = length;
            snowflakeEndSpeed = endSpeed;
            snowflakeSpeed = speed;
            snowflakeSpeedEase = (Util.EasingFunction.Ease)ease;
            WeatherUpdate();
        }

        private double startBeatTreeSpeed = -1f;
        private float treeSpeedLength = 0f;
        private float treeSpeed = 1f;
        private float treeEndSpeed = 1f;
        private Util.EasingFunction.Ease treeSpeedEase = Util.EasingFunction.Ease.Linear;

        public void SetTreeRates(bool enable, double beat, float length, int cpsMain, int cpsSides, float speed, float endSpeed, int ease)
        {
            treeHolder.SetActive(enable);
            islandManager.gameObject.SetActive(!enable);
            treeManagerMain.SetCloudsPerSecond(cpsMain);
            treeManagerRightInner.SetCloudsPerSecond(cpsMain);
            treeManagerLeftInner.SetCloudsPerSecond(cpsMain);
            treeManagerRight.SetCloudsPerSecond(cpsSides);
            treeManagerLeft.SetCloudsPerSecond(cpsSides);

            startBeatTreeSpeed = beat;
            treeSpeedLength = length;
            treeSpeed = speed;
            treeEndSpeed = endSpeed;
            treeSpeedEase = (Util.EasingFunction.Ease)ease;

            TreeUpdate();
        }

        private void TreeUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(startBeatTreeSpeed, treeSpeedLength));

            var func = Util.EasingFunction.GetEasingFunction(treeSpeedEase);

            float newSpeed = func(treeSpeed, treeEndSpeed, normalizedBeat);

            treeManagerMain.speedMult = newSpeed;
            treeManagerRightInner.speedMult = newSpeed;
            treeManagerLeftInner.speedMult = newSpeed;
            treeManagerRight.speedMult = newSpeed;
            treeManagerLeft.speedMult = newSpeed;
        }

        private Color objectsColorFrom = Color.white;
        private Color objectsColorTo = Color.white;
        private Color bgColorFrom = Color.white;
        private Color bgColorTo = Color.white;
        private Color cloudColorFrom = Color.white;
        private Color cloudColorTo = Color.white;

        public void SetEnter(double beat, float length, int ease, bool playSound = true)
        {
            if (playSound) SoundByte.PlayOneShotGame("airRally/planesSpeedUp");
            enterStartBeat = beat;
            enterLength = length;
            enterEase = (Util.EasingFunction.Ease)ease;
        }

        private void DistanceUpdate()
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(wayPointStartBeat, wayPointBeatLength);

            if (normalizedBeat >= 0f && normalizedBeat <= 1f)
            {
                HeavenStudio.Util.EasingFunction.Function func = HeavenStudio.Util.EasingFunction.GetEasingFunction(currentEase);

                float newZ = func(lastWayPointZForForth, wayPointZForForth, normalizedBeat);

                forthTrans.position = new Vector3(forthTrans.position.x, forthTrans.position.y, newZ);
                if (shuttleActive) ActiveShuttle.SetStartAndEndPos();
            }
            else if (normalizedBeat > 1f)
            {
                forthTrans.position = new Vector3(forthTrans.position.x, forthTrans.position.y, wayPointZForForth);
            }
        }

        private void DayNightCycleUpdate()
        {
            var cond = Conductor.instance;

            float normalizedBeat = cond.GetPositionFromBeat(startTimeBeat, timeLength);

            Color lightsColor = new Color(1, 1, 1, 0);

            if (normalizedBeat < 0)
            {
                bgMat.SetColor("_Color", bgColorFrom);
                cloudMat.SetColor("_Color", cloudColorFrom);
                objectMat.SetColor("_Color", objectsColorFrom);
                lightsColor = (lastTime == DayNightCycle.Night) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
            }
            else if (normalizedBeat >= 0 && normalizedBeat <= 1f)
            {
                bgMat.SetColor("_Color", GetEasedColor(bgColorFrom, bgColorTo));
                cloudMat.SetColor("_Color", GetEasedColor(cloudColorFrom, cloudColorTo));
                objectMat.SetColor("_Color", GetEasedColor(objectsColorFrom, objectsColorTo));
                lightsColor = GetEasedColor((lastTime == DayNightCycle.Night) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0),
                    (currentTime == DayNightCycle.Night) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0));
            }
            else if (normalizedBeat > 1)
            {
                bgMat.SetColor("_Color", bgColorTo);
                cloudMat.SetColor("_Color", cloudColorTo);
                objectMat.SetColor("_Color", objectsColorTo);
                lightsColor = (currentTime == DayNightCycle.Night) ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
            }

            island2Lights.color = lightsColor;

            Color GetEasedColor(Color start, Color end)
            {
                var func = Util.EasingFunction.GetEasingFunction(timeEase);
                float r = func(start.r, end.r, normalizedBeat);
                float g = func(start.g, end.g, normalizedBeat);
                float b = func(start.b, end.b, normalizedBeat);
                float a = func(start.a, end.a, normalizedBeat);

                return new Color(r, g, b, a);
            }
        }

        public void SetDayNightCycle(double beat, float length, DayNightCycle start, DayNightCycle end, Util.EasingFunction.Ease ease)
        {
            startTimeBeat = beat;
            timeLength = length;
            lastTime = start;
            currentTime = end;
            timeEase = ease;
            objectsColorFrom = lastTime switch
            {
                DayNightCycle.Noon => Color.black,
                _ => Color.white,
            };

            objectsColorTo = currentTime switch
            {
                DayNightCycle.Noon => Color.black,
                _ => Color.white,
            };

            bgColorFrom = lastTime switch
            {
                DayNightCycle.Day => Color.white,
                DayNightCycle.Noon => noonColor,
                DayNightCycle.Night => nightColor,
                _ => throw new System.NotImplementedException()
            };

            bgColorTo = currentTime switch
            {
                DayNightCycle.Day => Color.white,
                DayNightCycle.Noon => noonColor,
                DayNightCycle.Night => nightColor,
                _ => throw new System.NotImplementedException()
            };

            cloudColorFrom = lastTime switch
            {
                DayNightCycle.Day => Color.white,
                DayNightCycle.Noon => noonColorCloud,
                DayNightCycle.Night => nightColorCloud,
                _ => throw new System.NotImplementedException()
            };

            cloudColorTo = currentTime switch
            {
                DayNightCycle.Day => Color.white,
                DayNightCycle.Noon => noonColorCloud,
                DayNightCycle.Night => nightColorCloud,
                _ => throw new System.NotImplementedException()
            };
            DayNightCycleUpdate();
        }

        #endregion

        private static bool IsCatchBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("airRally", new string[] { "catch" }).Find(x => beat == x.beat) != null;
        }

        private bool IsSilentAtBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("airRally", new string[] { "silence" }).Find(x => beat >= x.beat && beat < x.beat + x.length) != null;
        }

        public enum DistanceSound
        {
            close = 0,
            far = 1,
            farther = 2,
            farthest = 3
        }

        public enum CountSound
        {
            one = 0,
            two = 1,
            three = 2,
            four = 3
        }

        public enum DayNightCycle
        {
            Day = 0,
            Noon = 1,
            Night = 2
        }

        public void ServeObject(double beat, double targetBeat, bool type)
        {
            BeatAction.New(gameObject, new List<BeatAction.Action>
            {
                new BeatAction.Action(beat - 0.5, delegate
                {
                    if (!shuttleActive)
                    {
                        ActiveShuttle = Instantiate(Shuttlecock, objHolder.transform);
                        ActiveShuttle.gameObject.SetActive(true);
                        ActiveShuttle.flyPos = 0f;
                        ActiveShuttle.startBeat = beat - 0.5;
                        ActiveShuttle.flyBeats = 0.5;
                        ActiveShuttle.isTossed = true;
                        ActiveShuttle.SetStartAndEndPos();

                        shuttleActive = true;

                        Forthington.DoScaledAnimationAsync("Ready", 0.5f);
                    }
                }),
                new BeatAction.Action(beat, delegate
                {
                    ActiveShuttle.flyPos = 0f;
                    ActiveShuttle.isReturning = false;
                    ActiveShuttle.startBeat = beat;
                    ActiveShuttle.flyBeats = targetBeat - beat;
                    ActiveShuttle.flyType = type;
                    ActiveShuttle.isTossed = false;
                    ActiveShuttle.currentDist = DistanceAtBeat(beat);
                    ActiveShuttle.SetStartAndEndPos();

                    Forthington.DoScaledAnimationAsync("Hit", 0.5f);
                })
            });     
        }

        public void ReturnObject(double beat, double targetBeat, bool type)
        {
            ActiveShuttle.flyPos = 0f;
            ActiveShuttle.isReturning = true;
            ActiveShuttle.startBeat = beat;
            ActiveShuttle.flyBeats = targetBeat - beat;
            ActiveShuttle.flyType = type;
            ActiveShuttle.isTossed = false;
            ActiveShuttle.currentDist = DistanceAtBeat(beat);
            ActiveShuttle.SetStartAndEndPos();
        }

        #region count-ins
        public static void ForthCountIn4(double beat, float length)
        {
            float realLength = length / 4;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat, true), beat, 1, 1, false, countInOffsets[0]),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (1 * realLength), true), beat + (1 * realLength), 1, 1, false, countInOffsets[1]),
                new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + (2 * realLength), true), beat + (2 * realLength), 1, 1, false, countInOffsets[2]),
                new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + (3 * realLength), true), beat + (3 * realLength), 1, 1, false, countInOffsets[3]),
            }, forcePlay: true);
        }

        public void ForthCountIn4Do(double beat, float length)
        {
            BeatAction.New(instance.gameObject, instance.ForthCountIn4Action(beat, length));
        }

        public void ForthCountIn8Do(double beat, float length)
        {
            BeatAction.New(instance.gameObject, instance.ForthCountIn8Action(beat, length));
        }

        private List<BeatAction.Action> ForthCountIn4Action(double beat, float length)
        {
            float realLength = length / 4;

            return new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (1 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (2 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (3 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
            };
        }

        private List<BeatAction.Action> ForthCountIn8Action(double beat, float length)
        {
            float realLength = length / 8;

            return new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (2 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (4 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (5 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (6 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
                new BeatAction.Action(beat + (7 * realLength), delegate
                {
                    instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                }),
            };
        }

        public static void ForthCountIn8(double beat, float length)
        {
            float realLength = length / 8;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat, true), beat, 1, 1, false, countInOffsets[0]),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (2 * realLength), true), beat + (2 * realLength), 1, 1, false, countInOffsets[1]),
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat + (4 * realLength), true), beat + (4 * realLength), 1, 1, false, countInOffsets[0]),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (5 * realLength), true), beat + (5 * realLength), 1, 1, false, countInOffsets[1]),
                new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + (6 * realLength), true), beat + (6 * realLength), 1, 1, false, countInOffsets[2]),
                new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + (7 * realLength), true), beat + (7 * realLength), 1, 1, false, countInOffsets[3]),
            }, forcePlay: true);
        }

        private BeatAction.Action ForthVoiceAction(double beat)
        {
            return new BeatAction.Action(beat, delegate
            {
                Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
            });
        }

        public void ForthVoiceDo(double beat)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                instance.ForthVoiceAction(beat)
            });
        }
        
        public static void ForthVoice(double beat, int type)
        {
            float offset = countInOffsets[type];

            DistanceSound distance = DistanceAtBeat(beat);
            
            switch (distance)
            {
                case DistanceSound.close:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case DistanceSound.far:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Far", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case DistanceSound.farther:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Farther", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case DistanceSound.farthest:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Farthest", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
            }
        }
        #endregion

        public void SetDistance(double beat, int type, int ease)
        {
            wayPointStartBeat = beat;
            currentEase = (HeavenStudio.Util.EasingFunction.Ease)ease;
            lastWayPointZForForth = wayPointZForForth;
            wayPointZForForth = (DistanceSound)type switch
            {
                DistanceSound.close => 3.55f,
                DistanceSound.far => 35.16f,
                DistanceSound.farther => 105.16f,
                DistanceSound.farthest => 255.16f,
                _ => throw new System.NotImplementedException()
            };
            DistanceUpdate();
        }

        private static DistanceSound DistanceAtBeat(double beat)
        {
            var allDistances = EventCaller.GetAllInGameManagerList("airRally", new string[] { "set distance" }).FindAll(x => x.beat <= beat);
            if (allDistances.Count == 0) return DistanceSound.close;
            return (DistanceSound)allDistances[^1]["type"];
        }

        private static string GetDistanceStringAtBeat(double beat, bool emptyClose = false, bool farFarther = false)
        {
            if (farFarther)
            {
                return DistanceAtBeat(beat) switch
                {
                    DistanceSound.close => "Close",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Far",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };
            }
            else if (emptyClose)
            {
                return DistanceAtBeat(beat) switch
                {
                    DistanceSound.close => "",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Farther",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };
            }
            else
            {
                return DistanceAtBeat(beat) switch
                {
                    DistanceSound.close => "Close",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Farther",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };
            }
        }

        private static bool TryGetLastDistanceEvent(double beat, out RiqEntity distanceEvent)
        {
            var allDistances = EventCaller.GetAllInGameManagerList("airRally", new string[] { "set distance" }).FindAll(x => x.beat <= beat);
            if (allDistances.Count == 0) 
            {
                distanceEvent = null;
                return false;
            } 
            distanceEvent = allDistances[^1];
            return true;
        }

        private static double wantStartRally = double.MinValue;
        private static double wantStartBaBum = double.MinValue;
        private static bool wantCount = true;
        private static bool wantAlt = false;

        public override void OnGameSwitch(double beat)
        {
            var nextGameSwitch = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat > beat);
            if (nextGameSwitch != null) nextGameSwitchBeatGlobal = nextGameSwitch.beat;
            PersistDayNight(beat);
            PersistEnter(beat);
            PersistIslandSpeed(beat);
            InitClouds(beat);
            if (TryGetLastDistanceEvent(beat, out RiqEntity distanceEvent))
            {
                SetDistance(distanceEvent.beat, distanceEvent["type"], distanceEvent["ease"]);
            }

            if (wantStartRally >= beat && IsRallyBeat(wantStartRally))
            {
                StartRally(wantStartRally);
            }
            else if (wantStartBaBum >= beat && IsBaBumBeat(wantStartBaBum))
            {
                StartBaBumBumBum(wantStartBaBum, wantCount, wantAlt);
            }

            var allCounts = EventCaller.GetAllInGameManagerList("airRally", new string[] { "forthington voice lines", "4beat", "8beat" }).FindAll(x => x.beat < beat && x.beat + x.length > beat);

            List<BeatAction.Action> counts = new();

            foreach (var count in allCounts)
            {
                if (count.datamodel == "airRally/forthington voice lines")
                {
                    counts.Add(ForthVoiceAction(count.beat));
                }
                else
                {
                    counts.AddRange((count.datamodel == "airRally/4beat") ? ForthCountIn4Action(count.beat, count.length) : ForthCountIn8Action(count.beat, count.length));
                }
            }

            var tempCounts = counts.FindAll(x => x.beat >= beat);

            if (tempCounts.Count == 0) return;

            tempCounts.Sort((x, y) => x.beat.CompareTo(y.beat));

            BeatAction.New(instance.gameObject, tempCounts);
        }

        public override void OnPlay(double beat)
        {
            var nextGameSwitch = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat > beat);
            if (nextGameSwitch != null) nextGameSwitchBeatGlobal = nextGameSwitch.beat;
            PersistDayNight(beat);
            PersistEnter(beat);
            PersistIslandSpeed(beat);
            InitClouds(beat);
        }

        #region persist

        private void InitClouds(double beat)
        {
            var cloudEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "cloud" }).Find(x => x.beat == beat);
            if (cloudEvent != null)
            {
                SetCloudRates(cloudEvent.beat, cloudEvent.length, cloudEvent["main"], cloudEvent["side"], cloudEvent["top"], 
                    cloudEvent["speed"], cloudEvent["endSpeed"], cloudEvent["ease"]);
            }
            cloudManagerMain.Init();
            cloudManagerLeft.Init();
            cloudManagerRight.Init();
            cloudManagerTop.Init();

            var snowflakeEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "snowflake" }).Find(x => x.beat == beat);
            if (snowflakeEvent != null)
            {
                SetSnowflakeRates(snowflakeEvent.beat, snowflakeEvent.length, snowflakeEvent["cps"], snowflakeEvent["speed"], 
                    snowflakeEvent["endSpeed"], snowflakeEvent["ease"]);
            }

            snowflakeManager.Init();

            var treeEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "tree" }).Find(x => x.beat == beat);
            if (treeEvent != null)
            {
                SetTreeRates(treeEvent["enable"], treeEvent.beat, treeEvent.length, treeEvent["main"], treeEvent["side"], treeEvent["speed"],
                    treeEvent["endSpeed"], treeEvent["ease"]);
            }

            treeManagerMain.Init();
            treeManagerLeft.Init();
            treeManagerRight.Init();
            treeManagerLeftInner.Init();
            treeManagerRightInner.Init();
        }

        private void PersistEnter(double beat)
        {
            double nextGameSwitchBeat = double.MaxValue;

            var nextGameSwitch = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat > beat);
            if (nextGameSwitch != null) nextGameSwitchBeat = nextGameSwitch.beat;
            var allEnters = EventCaller.GetAllInGameManagerList("airRally", new string[] { "enter" });
            if (allEnters.Count == 0) return;
            var nextEnter = allEnters.Find(x => x.beat >= beat && x.beat < nextGameSwitchBeat);
            if (nextEnter != null)
            {
                SetEnter(nextEnter.beat, nextEnter.length, nextEnter["ease"], false);
            }
            else
            {
                var overlappingEnters = allEnters.FindAll(x => x.beat < beat && x.beat + x.length > beat);
                if (overlappingEnters.Count == 0) return;
                foreach (var overlappingEnter in overlappingEnters)
                {
                    SetEnter(overlappingEnter.beat, overlappingEnter.length, overlappingEnter["ease"], false);
                }
            }
        }

        private void PersistDayNight(double beat)
        {
            var allDayNights = EventCaller.GetAllInGameManagerList("airRally", new string[] { "day" }).FindAll(x => x.beat < beat);
            if (allDayNights.Count == 0) return;

            var e = allDayNights[^1];

            SetDayNightCycle(e.beat, e.length, (DayNightCycle)e["start"], (DayNightCycle)e["end"], (Util.EasingFunction.Ease)e["ease"]);
        }

        private void PersistIslandSpeed(double beat)
        {
            var allSpeeds = EventCaller.GetAllInGameManagerList("airRally", new string[] { "islandSpeed" }).FindAll(x => x.beat < beat);
            if (allSpeeds.Count == 0) return;

            var e = allSpeeds[^1];

            SetIslandSpeed(e.beat, e.length, e["speed"], e["endSpeed"], e["ease"]);
        }
        #endregion

        public static void PreStartRally(double beat)
        {
            if (IsCatchBeat(beat)) return;
            if (GameManager.instance.currentGame == "airRally")
            {
                instance.StartRally(beat);
            }
            else wantStartRally = beat;
        }

        private bool recursingRally;
        private void StartRally(double beat)
        {
            if (recursingRally) return;
            recursingRally = true;

            RallyRecursion(beat);
        }

        public static void PreStartBaBumBumBum(double beat, bool count, bool alt)
        {
            if (IsCatchBeat(beat)) return;
            if (GameManager.instance.currentGame == "airRally")
            {
                instance.StartBaBumBumBum(beat, count, alt);
            }
            else
            {
                wantStartBaBum = beat;
                wantCount = count;
                wantAlt = alt;
            }
        }

        private void StartBaBumBumBum(double beat, bool count, bool alt)
        {
            if (recursingRally || IsRallyBeat(beat)) return;
            recursingRally = true;

            BaBumBumBum(beat, count, alt);
        }

        private void RallyRecursion(double beat)
        {
            if (beat >= nextGameSwitchBeatGlobal) return;
            bool isBaBumBeat = IsBaBumBeat(beat);
            bool countBaBum = CountBaBum(beat);
            bool silent = IsSilentAtBeat(beat);
            bool isCatch = IsCatchBeat(beat + 2);
            bool altBum = AltBaBum(beat);

            string distanceString = GetDistanceStringAtBeat(beat);
            if (distanceString != "Close") SoundByte.PlayOneShotGame("airRally/whooshForth_" + distanceString, beat + 1, 1, 1, false, false, whooshOffsetsRally[(int)DistanceAtBeat(beat)]);
            if (!(silent || isBaBumBeat) || (isCatch && !silent)) 
                SoundByte.PlayOneShotGame("airRally/nya_" + distanceString, beat, 1, 1, false, false, nyaOffsets[(int)DistanceAtBeat(beat)]);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 1, delegate
                {
                    ServeObject(beat, beat + 1, false);

                    if (isCatch) return;

                    if (isBaBumBeat) BaBumBumBum(beat, countBaBum, altBum);
                    else RallyRecursion(beat + 2);
                }),
                new BeatAction.Action(beat, delegate
                {
                    string distanceString = GetDistanceStringAtBeat(beat);
                    Baxter.DoScaledAnimationAsync((distanceString == "Close") ? "CloseReady" : "FarReady", 0.5f);
                    SoundByte.PlayOneShotGame("airRally/hitForth_" + distanceString);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    if (!isBaBumBeat) Forthington.DoScaledAnimationAsync("Ready", 0.5f);
                })
            });

            ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, RallyOnHit, RallyOnMiss, RallyEmpty);
        }    
        
        private bool IsBaBumBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("airRally", new string[] { "ba bum bum bum" }).Find(x => x.beat == beat) != null;
        }

        private bool IsRallyBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("airRally", new string[] { "rally" }).Find(x => x.beat == beat) != null;
        }

        private bool CountBaBum(double beat)
        {
            var baBumEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "ba bum bum bum" }).Find(x => x.beat == beat);
            if (baBumEvent == null) return false;

            return baBumEvent["toggle"];
        }

        private bool AltBaBum(double beat)
        {
            var baBumEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "ba bum bum bum" }).Find(x => x.beat == beat);
            if (baBumEvent == null) return false;

            return baBumEvent["toggle2"];
        }

        private void BaBumBumBum(double beat, bool count, bool alt)
        {
            if (beat >= nextGameSwitchBeatGlobal) return;
            bool isCatch = IsCatchBeat(beat + 6);
            bool isBaBumBeat = IsBaBumBeat(beat + 4);
            bool countBaBum = CountBaBum(beat + 4);
            bool altBum = AltBaBum(beat + 4);

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            string distanceStringTwoBeat = GetDistanceStringAtBeat(beat + 2f);

            sounds.AddRange(new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat - 0.5) + "1", beat - 0.5, offset: GetBaBumOffset(beat - 0.5, 0)),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat) + "2", beat, offset: GetBaBumOffset(beat, 0)),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat + 1f) + "3", beat + 1, offset: GetBaBumOffset(beat + 1, 0)),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat + 2f) + "4", beat + 2, offset: GetBaBumOffset(beat + 2, 0)),
                
            });

            if (distanceStringTwoBeat != "Close") 
            {
                sounds.Add(new MultiSound.Sound("airRally/whooshForth_" + distanceStringTwoBeat + "2", beat + 4, 1, 1, false, whooshOffsetsBaBum[(int)DistanceAtBeat(beat + 2)]));
                sounds.Add(new MultiSound.Sound("airRally/hitForth_" + distanceStringTwoBeat + "2", beat + 2));
            } 
            else
            {
                sounds.Add(new MultiSound.Sound("airRally/hitForth_Close", beat + 2));
            }

            string GetDistanceStringAlt(double beatAlt)
            {
                string distanceString = GetDistanceStringAtBeat(beatAlt);
                string altString = alt ? "Alt" : "";
                if (distanceString != "Far") altString = "";
                return distanceString + altString;
            }

            float GetBaBumOffset(double beatOffset, int index)
            {
                int theDistance = (int)DistanceAtBeat(beatOffset);
                if (theDistance == 1 && alt)
                {
                    return baBumBumBumFarAltOffsets[index];
                }
                else
                {
                    return baBumBumBumOffsets[theDistance, index];
                }
            }

            if (count && !isBaBumBeat && !isCatch)
            {
                sounds.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + 3f, true), beat + 3, 1, 1, false, countInOffsets[1]),
                    new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + 4f, true), beat + 4, 1, 1, false, countInOffsets[2]),
                    new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + 5f, true), beat + 5, 1, 1, false, countInOffsets[3]),
                });
            }

            MultiSound.Play(sounds.ToArray());

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate 
                {
                    if (isCatch) return;
                    if (isBaBumBeat) BaBumBumBum(beat + 4, countBaBum, altBum);
                    else RallyRecursion(beat + 6); 
                }),
                new BeatAction.Action(beat + 1f, delegate 
                { 
                    Forthington.DoScaledAnimationAsync("Ready", 0.5f);
                    ServeObject(beat + 2f, beat + 4f, true);
                }),
                new BeatAction.Action(beat + 2f, delegate 
                { 
                    Baxter.DoScaledAnimationAsync(GetDistanceStringAtBeat(beat + 2f, false, true) + "Ready", 0.5f);
                } ),
                new BeatAction.Action(beat + 3f, delegate { Forthington.DoScaledAnimationAsync("TalkShort", 0.5f); }),
                new BeatAction.Action(beat + 3.5f, delegate { if(!count || isBaBumBeat) Forthington.DoScaledAnimationAsync("TalkShort", 0.5f); }),
                new BeatAction.Action(beat + 4f, delegate { Forthington.DoScaledAnimationAsync("Ready", 0.5f); }),
            });

            ScheduleInput(beat, 4f, InputType.STANDARD_DOWN, LongShotOnHit, RallyOnMiss, RallyEmpty);
        }


        private void CatchBirdie() 
        {
            Forthington.DoScaledAnimationAsync("Catch", 0.5f);
            SoundByte.PlayOneShotGame("airRally/birdieCatch");
            shuttleActive = false;
            recursingRally = false;
            if (ActiveShuttle != null) Destroy(ActiveShuttle.gameObject);
        }

        public void RallyOnHit(PlayerActionEvent caller, float state)
        {
            Baxter.DoScaledAnimationAsync("Hit", 0.5f);

            if (state >= 1 || state <= -1)
            { 
                ActiveShuttle.DoNearMiss();
                hasMissed = true;
                shuttleActive = false;
                ActiveShuttle = null;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeatsAsDouble, caller.startBeat + caller.timer + 1f, false);
                hasMissed = false;
                ActiveShuttle.DoHit(DistanceAtBeat(caller.startBeat + caller.timer));
                string distanceString = DistanceAtBeat(caller.startBeat + caller.timer) switch
                {
                    DistanceSound.close => "Close",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Farther",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };

                SoundByte.PlayOneShotGame("airRally/hitBaxter_" + distanceString);

                if (IsCatchBeat(caller.startBeat + caller.timer + 1))
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 1, delegate
                        {
                            CatchBirdie();
                        })
                    });
                }
            }
        }

        public void LongShotOnHit(PlayerActionEvent caller, float state)
        {
            Baxter.DoScaledAnimationAsync("Hit", 0.5f);

            if (state >= 1 || state <= -1)
            { 
                ActiveShuttle.DoThrough();
                hasMissed = true;
                shuttleActive = false;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeatsAsDouble, caller.startBeat + caller.timer + 2f, true);
                hasMissed = false;
                ActiveShuttle.DoHit(DistanceAtBeat(caller.startBeat + caller.timer));

                string distanceString = DistanceAtBeat(caller.startBeat + caller.timer) switch
                {
                    DistanceSound.close => "Close",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Farther",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };

                if (distanceString == "Close")
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Close");
                }
                else SoundByte.PlayOneShotGame("airRally/hitBaxter_" + distanceString + "2");

                if (IsCatchBeat(caller.startBeat + caller.timer + 2))
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate
                        {
                            CatchBirdie();
                        })
                    });
                }
            }
        }

        public void RallyOnMiss(PlayerActionEvent caller)
        {
            ActiveShuttle.DoThrough();
            hasMissed = true;
            shuttleActive = false;
            ActiveShuttle = null;
        }

        public void RallyEmpty(PlayerActionEvent caller)
        {
            //empty
        }
    }
}

