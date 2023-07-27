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
                new GameAction("catch", "Catch Birdie")
                {

                },
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
                new GameAction("4beat", "4 Beat Count-In")
                {
                    defaultLength = 4f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn4(e.currentEntity.beat, e.currentEntity.length);
                    },
                },
                new GameAction("8beat", "8 Beat Count-In")
                {
                    defaultLength = 8f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn8(e.currentEntity.beat, e.currentEntity.length);
                    },
                },
                new GameAction("forthington voice lines", "Count")
                {
                    preFunction = delegate { AirRally.ForthVoice(e.currentEntity.beat, e.currentEntity["type"]); }, 
                    parameters = new List<Param>()
                    { 
                        new Param("type", AirRally.CountSound.one, "Type", "The number Forthington will say"),
                    },
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
                        AirRally.instance.SetCloudRates(e.currentEntity["main"], e.currentEntity["side"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("main", new EntityTypes.Integer(0, 300, 30), "Main Clouds", "How many clouds per second?"),
                        new Param("side", new EntityTypes.Integer(0, 100, 10), "Side Clouds", "How many clouds per second?"),
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
        public static AirRally instance { get; set; }

        [Header("Component")]
        [SerializeField] Animator Baxter;
        [SerializeField] Animator Forthington;
        private Transform forthTrans;
        private Transform baxterTrans;
        [SerializeField] GameObject Shuttlecock;
        public GameObject ActiveShuttle;
        [SerializeField] GameObject objHolder;
        [SerializeField] private CloudsManager cloudManagerMain, cloudManagerLeft, cloudManagerRight;

        [Header("Day/Night Cycle")]
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
                SoundByte.PlayOneShotGame("airRally/whooshForth_Close", -1f);
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
        }

        public void SetCloudRates(int main, int side)
        {
            cloudManagerMain.SetCloudsPerSecond(main);
            cloudManagerLeft.SetCloudsPerSecond(side);
            cloudManagerRight.SetCloudsPerSecond(side);
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

            if (normalizedBeat < 0)
            {
                bgMat.SetColor("_Color", bgColorFrom);
                cloudMat.SetColor("_Color", cloudColorFrom);
                objectMat.SetColor("_Color", objectsColorFrom);
            }
            else if (normalizedBeat >= 0 && normalizedBeat <= 1f)
            {
                bgMat.SetColor("_Color", GetEasedColor(bgColorFrom, bgColorTo));
                cloudMat.SetColor("_Color", GetEasedColor(cloudColorFrom, cloudColorTo));
                objectMat.SetColor("_Color", GetEasedColor(objectsColorFrom, objectsColorTo));
            }
            else if (normalizedBeat > 1)
            {
                bgMat.SetColor("_Color", bgColorTo);
                cloudMat.SetColor("_Color", cloudColorTo);
                objectMat.SetColor("_Color", objectsColorTo);
            }

            Color GetEasedColor(Color start, Color end)
            {
                var func = Util.EasingFunction.GetEasingFunction(timeEase);
                float r = func(start.r, end.r, normalizedBeat);
                float g = func(start.g, end.g, normalizedBeat);
                float b = func(start.b, end.b, normalizedBeat);

                return new Color(r, g, b, 1);
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
            close,
            far,
            farther,
            farthest
        }

        public enum CountSound
        {
            one,
            two,
            three,
            four
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
                        ActiveShuttle = GameObject.Instantiate(Shuttlecock, objHolder.transform);
                        ActiveShuttle.SetActive(true);
                        var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
                        shuttleScript.flyPos = 0f;
                        shuttleScript.startBeat = beat - 0.5;
                        shuttleScript.flyBeats = 0.5;
                        shuttleScript.isTossed = true;

                        shuttleActive = true;

                        Forthington.DoScaledAnimationAsync("Ready", 0.5f);
                    }
                }),
                new BeatAction.Action(beat, delegate
                {
                    var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
                    shuttleScript.flyPos = 0f;
                    shuttleScript.isReturning = false;
                    shuttleScript.startBeat = beat;
                    shuttleScript.flyBeats = targetBeat - beat;
                    shuttleScript.flyType = type;
                    shuttleScript.isTossed = false;

                    Forthington.DoScaledAnimationAsync("Hit", 0.5f);
                })
            });     
        }

        public void ReturnObject(double beat, double targetBeat, bool type)
        {
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = true;
            shuttleScript.startBeat = beat;
            shuttleScript.flyBeats = targetBeat - beat;
            shuttleScript.flyType = type;
            shuttleScript.isTossed = false;
        }

        #region count-ins
        public static void ForthCountIn4(double beat, float length)
        {
            float realLength = length / 4;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat, true), beat),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (1 * realLength), true), beat + (1 * realLength)),
                new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + (2 * realLength), true), beat + (2 * realLength), 1, 1, false, 0.107f),
                new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + (3 * realLength), true), beat + (3 * realLength), 1, 1, false, 0.051f),
            }, forcePlay: true);

            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, instance.ForthCountIn4Action(beat, length));
            }
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
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat, true), beat),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (2 * realLength), true), beat + (2 * realLength)),
                new MultiSound.Sound("airRally/countIn1" + GetDistanceStringAtBeat(beat + (4 * realLength), true), beat + (4 * realLength)),
                new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + (5 * realLength), true), beat + (5 * realLength)),
                new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + (6 * realLength), true), beat + (6 * realLength), 1, 1, false, 0.107f),
                new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + (7 * realLength), true), beat + (7 * realLength), 1, 1, false, 0.051f),
            }, forcePlay: true);

            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, instance.ForthCountIn8Action(beat, length));
            }
        }

        private BeatAction.Action ForthVoiceAction(double beat)
        {
            return new BeatAction.Action(beat, delegate
            {
                Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
            });
        }

        public static void ForthVoice(double beat, int type)
        {
            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    instance.ForthVoiceAction(beat)
                });
            }
            float offset = 0f;
            if (type == 2)
            {
                offset = 0.107f;
            }
            else if (type == 3)
            {
                offset = 0.051f;
            }

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
            PersistDayNight(beat);
            PersistEnter(beat);
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
            PersistDayNight(beat);
            PersistEnter(beat);
            InitClouds(beat);
        }

        private void InitClouds(double beat)
        {
            var cloudEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "cloud" }).Find(x => x.beat == beat);
            if (cloudEvent != null)
            {
                SetCloudRates(cloudEvent["main"], cloudEvent["side"]);
            }
            cloudManagerMain.Init();
            cloudManagerLeft.Init();
            cloudManagerRight.Init();
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
            bool isBaBumBeat = IsBaBumBeat(beat);
            bool countBaBum = CountBaBum(beat);
            bool silent = IsSilentAtBeat(beat);
            bool isCatch = IsCatchBeat(beat + 2);
            bool altBum = AltBaBum(beat);

            SoundByte.PlayOneShotGame("airRally/whooshForth_" + GetDistanceStringAtBeat(beat + 0.25), beat + 0.25);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.5, delegate
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
                    if (!(silent || isBaBumBeat) || (isCatch && !silent)) SoundByte.PlayOneShotGame("airRally/nya_" + distanceString);
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
            bool isCatch = IsCatchBeat(beat + 6);
            bool isBaBumBeat = IsBaBumBeat(beat + 4);
            bool countBaBum = CountBaBum(beat + 4);
            bool altBum = AltBaBum(beat + 4);

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            sounds.AddRange(new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat + - 0.5) + "1", beat - 0.5),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat) + "2", beat),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat + 1f) + "3", beat + 1),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAlt(beat + 2f) + "4", beat + 2),
                new MultiSound.Sound("airRally/hitForth_" + GetDistanceStringAtBeat(beat + 2f), beat + 2),
                new MultiSound.Sound("airRally/whooshForth_" + GetDistanceStringAtBeat(beat + 2.5), beat + 2.5),
            });

            string GetDistanceStringAlt(double beatAlt)
            {
                string distanceString = GetDistanceStringAtBeat(beatAlt);
                string altString = alt ? "Alt" : "";
                if (distanceString != "Far") altString = "";
                return distanceString + altString;
            }

            if (GetDistanceStringAtBeat(beat + 3f) == "Far") sounds.Add(new MultiSound.Sound("airRally/whooshForth_Far2", beat + 3f));

            if (count && !isBaBumBeat && !isCatch)
            {
                sounds.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("airRally/countIn2" + GetDistanceStringAtBeat(beat + 3f, true), beat + 3),
                    new MultiSound.Sound("airRally/countIn3" + GetDistanceStringAtBeat(beat + 4f, true), beat + 4, 1, 1, false, 0.107f),
                    new MultiSound.Sound("airRally/countIn4" + GetDistanceStringAtBeat(beat + 5f, true), beat + 5, 1, 1, false, 0.051f),
                });
            }

            MultiSound.Play(sounds.ToArray());
            //ready after 2 beat, close, far, far, farthest

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
            if (ActiveShuttle != null) Destroy(ActiveShuttle);
        }

        public void RallyOnHit(PlayerActionEvent caller, float state)
        {
            Baxter.DoScaledAnimationAsync("Hit", 0.5f);

            if (state >= 1 || state <= -1)
            { 
                ActiveShuttle.GetComponent<Shuttlecock>().DoNearMiss();
                hasMissed = true;
                shuttleActive = false;
                ActiveShuttle = null;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeatsAsDouble, caller.startBeat + caller.timer + 1f, false);
                hasMissed = false;
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(DistanceAtBeat(caller.startBeat + caller.timer));
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
                ActiveShuttle.GetComponent<Shuttlecock>().DoThrough();
                hasMissed = true;
                shuttleActive = false;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeatsAsDouble, caller.startBeat + caller.timer + 2f, true);
                hasMissed = false;
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(DistanceAtBeat(caller.startBeat + caller.timer));

                string distanceString = DistanceAtBeat(caller.startBeat + caller.timer) switch
                {
                    DistanceSound.close => "Close",
                    DistanceSound.far => "Far",
                    DistanceSound.farther => "Farther",
                    DistanceSound.farthest => "Farthest",
                    _ => throw new System.NotImplementedException()
                };

                SoundByte.PlayOneShotGame("airRally/hitBaxter_" + distanceString);

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
            ActiveShuttle.GetComponent<Shuttlecock>().DoThrough();
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

