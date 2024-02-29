using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMonkeyWatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("monkeyWatch", "Monkey Watch", "f0338d", false, false, new List<GameAction>()
            {
                new GameAction("appear", "Monkeys Appear")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.instance.MonkeysAppear(e.beat, e.length, e["value"], e.beat);
                    },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("value", new EntityTypes.Integer(1, 30, 4), "Repeat Amount")
                    }
                },
                new GameAction("clap", "Clapping")
                {
                    preFunction = delegate
                    {
                        MonkeyWatch.PreStartClapping(eventCaller.currentEntity.beat);
                    },
                    preFunctionLength = 4f,
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("min", new EntityTypes.Integer(0, 59, 0), "Set Starting Second", "A second is equivalent to one monkey.")
                    }
                },
                new GameAction("off", "Pink Monkeys")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.PinkMonkeySound(e.beat, e.length, e["muteC"], e["muteE"]);
                    },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("muteC", false, "Mute Ooki"),
                        new Param("muteE", false, "Mute Eeks")
                    }
                },
                new GameAction("offStretch", "Pink Monkeys (Stretchable)")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.PinkMonkeySound(e.beat, e.length, e["muteC"], e["muteE"]);
                    },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("muteC", false, "Mute Ooki"),
                        new Param("muteE", false, "Mute Eeks")
                    }
                },
                new GameAction("offInterval", "Custom Pink Monkey Interval")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.PinkMonkeySoundCustom(e.beat, e.length, e["muteC"], e["muteE"]);
                    },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("muteC", false, "Mute Ooki"),
                        new Param("muteE", false, "Mute Eeks")
                    }
                },
                new GameAction("offCustom", "Custom Pink Monkey")
                {
                    defaultLength = 0.5f
                },
                new GameAction("zoomOut", "Zoom Out")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.instance.ZoomOut(e.beat, e["timeMode"], e["hour"], e["minute"], e["instant"]);
                    },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant"),
                        new Param("timeMode", MonkeyWatch.TimeMode.RealTime, "Time Mode", "Set the clock to system time or a certain time"),
                        new Param("hour", new EntityTypes.Integer(0, 12, 3), "Hour"),
                        new Param("minute", new EntityTypes.Integer(0, 59, 0), "Minute")
                    }
                },
                new GameAction("zoomIn", "Zoom In")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        MonkeyWatch.instance.ZoomIn(e.beat, e["timeMode"], e["hour"], e["minute"], e["instant"]);
                    },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant"),
                        new Param("timeMode", MonkeyWatch.TimeMode.RealTime, "Time Mode", "Set the clock to system time or a certain time"),
                        new Param("hour", new EntityTypes.Integer(0, 12, 3), "Hour"),
                        new Param("minute", new EntityTypes.Integer(0, 59, 0), "Minute")
                    }
                },
                new GameAction("balloon", "Balloon Movement")
                {
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("angleStart", new EntityTypes.Float(-360, 360, 0), "Start Angle"),
                        new Param("angleEnd", new EntityTypes.Float(-360, 360, 0), "End Angle"),
                        new Param("yStart", new EntityTypes.Float(-200, 200, 0), "Y Start"),
                        new Param("yEnd", new EntityTypes.Float(-200, 200, 0), "Y End"),
                        new Param("xStart", new EntityTypes.Float(-200, 200, 0), "X Start"),
                        new Param("xEnd", new EntityTypes.Float(-200, 200, 0), "X End"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MonkeyWatch;

    public class MonkeyWatch : Minigame
    {
        public enum TimeMode
        {
            RealTime,
            SetTime
        }

        private const float degreePerMonkey = 6f;

        public static MonkeyWatch instance;

        [Header("Components")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraMoveable;
        public MonkeyClockArrow monkeyClockArrow;
        [SerializeField] private WatchMonkeyHandler monkeyHandler;
        [SerializeField] private WatchBackgroundHandler backgroundHandler;
        [SerializeField] private BalloonHandler balloonHandler;
        public Animator middleMonkey;

        [Header("Properties")]
        [SerializeField] private float fullZoomOut = 40f;
        [SerializeField] private Util.EasingFunction.Ease zoomOutEase;
        [SerializeField] private float zoomOutBeatLength = 2f;
        [SerializeField] private float zoomInBeatLength = 2f;
        [SerializeField] private Util.EasingFunction.Ease zoomInEase;
        private float lastAngle = 0f;
        private int cameraIndex = 0;
        private float cameraWantAngle, cameraAngleDelay;
        private float delayRate = 0.5f, targetDelayRate;

        private void Awake()
        {
            instance = this;
            funcOut = Util.EasingFunction.GetEasingFunction(zoomOutEase);
            funcIn = Util.EasingFunction.GetEasingFunction(zoomInEase);
            pinkMonkeys = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "off", "offStretch" });
            pinkMonkeysCustom = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offInterval" });
        }

        private void Start()
        {
            CameraUpdate();
        }

        private double lastReportedBeat = 0;

        private void Update()
        {
            CameraUpdate();
        }

        public override void OnBeatPulse(double beat)
        {
            middleMonkey.DoScaledAnimationAsync("MiddleMonkeyBop", 0.4f);
        }

        public void PlayerMonkeyClap(bool big, bool barely)
        {
            monkeyClockArrow.PlayerClap(big, barely, false);
        }

        public void ZoomOut(double beat, int timeMode, int hours, int minutes, bool instant)
        {
            zoomOutStartBeat = beat - (instant ? zoomOutBeatLength : 0);
            zoomIn = false;
            backgroundHandler.SetFade(beat, instant ? 0 : 0.25f, true, (TimeMode)timeMode, hours, minutes);
            CameraUpdate();
        }
        public void ZoomIn(double beat, int timeMode, int hours, int minutes, bool instant)
        {
            zoomOutStartBeat = beat - (instant ? zoomInBeatLength : 0);
            zoomIn = true;
            backgroundHandler.SetFade(beat, instant ? 0 : 0.25f, false, (TimeMode)timeMode, hours, minutes);
            CameraUpdate();
        }

        public void PersistZoomOut(double beat)
        {
            var allZooms = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "zoomOut" }).FindAll(x => x.beat < beat && x.beat + x.length > beat);
            foreach (var zoom in allZooms)
            {
                ZoomOut(zoom.beat, zoom["timeMode"], zoom["hour"], zoom["minute"], zoom["instant"]);
            }
            var allZoomsIn = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "zoomIn" }).FindAll(x => x.beat < beat && x.beat + x.length > beat);
            foreach (var zoom in allZoomsIn)
            {
                ZoomIn(zoom.beat, zoom["timeMode"], zoom["hour"], zoom["minute"], zoom["instant"]);
            }
        }

        private double persistBeat = 0;
        private double theNextGameSwitchBeat = double.MaxValue;
        private double clappingBeat = 0;

        public override void OnGameSwitch(double beat)
        {
            balloonHandler.Init(beat);
            persistBeat = beat;
            GetCameraMovements(beat, false);
            monkeyClockArrow.MoveToAngle(lastAngle);
            monkeyHandler.Init((int)(lastAngle / degreePerMonkey));
            if (wantClap >= beat && IsClapBeat(wantClap))
            {
                StartClapping(wantClap);
            }
            PersistAppear(beat);
            PersistZoomOut(beat);

            bool IsClapBeat(double clapBeat)
            {
                return EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "clap" }).Find(x => x.beat == clapBeat) != null;
            }
        }

        public override void OnPlay(double beat)
        {
            balloonHandler.Init(beat);
            persistBeat = beat;
            GetCameraMovements(beat, true);
            monkeyClockArrow.MoveToAngle(lastAngle);
            monkeyHandler.Init((int)(lastAngle / degreePerMonkey));
            PersistAppear(beat);
            PersistZoomOut(beat);
        }

        private void PersistAppear(double beat)
        {
            var allEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "appear" }).FindAll(x => x.beat + x["value"] > beat && x.beat < beat);

            foreach (var e in allEvents)
            {
                MonkeysAppear(e.beat, e.length, e["value"], beat);
            }
        }

        public void MonkeysAppear(double beat, float length, int repeatAmount, double gameSwitchBeat)
        {
            List<BeatAction.Action> actions = new();

            double lastBeat = clappingBeat;

            int index = 0;
            while (index < repeatAmount)
            {

                if (IsPinkMonkeyAtBeat(lastBeat, out float pinkLength))
                {
                    for (int i = 0; i < pinkLength; i++)
                    {
                        if (index >= repeatAmount) break;
                        bool beforeGameSwitch = beat + (index * length) < gameSwitchBeat;
                        double realLastBeat = lastBeat;
                        actions.Add(new BeatAction.Action(beat + (index * length), delegate
                        {
                            monkeyHandler.SpawnMonkey(realLastBeat, true, beforeGameSwitch);
                        }));
                        index++;
                        lastBeat += 1;
                    }
                }
                else if (IsCustomPinkMonkeyAtBeat(lastBeat, out float pinkCustomLength))
                {
                    var relevantPinks = FindCustomOffbeatMonkeysBetweenBeat(lastBeat, lastBeat + pinkCustomLength);
                    relevantPinks.Sort((x, y) => x.beat.CompareTo(y.beat));

                    for (int i = 0; i < relevantPinks.Count; i++)
                    {
                        if (index >= repeatAmount) break;
                        int jindex = i;
                        bool beforeGameSwitch = beat + (index * length) < gameSwitchBeat;
                        actions.Add(new BeatAction.Action(beat + (index * length), delegate
                        {
                            monkeyHandler.SpawnMonkey(relevantPinks[jindex].beat, true, beforeGameSwitch);
                        }));
                        index++;
                    }
                    lastBeat += pinkCustomLength;
                }
                else
                {
                    double realLastBeat = lastBeat;
                    bool beforeGameSwitch = beat + (index * length) < gameSwitchBeat;
                    actions.Add(new BeatAction.Action(beat + (index * length), delegate
                    {
                        monkeyHandler.SpawnMonkey(realLastBeat, false, beforeGameSwitch);
                    }));
                    index++;
                    lastBeat += 2;
                }
            }

            actions.Sort((x, y) => x.beat.CompareTo(y.beat));
            BeatAction.New(instance, actions);
        }

        #region clapping

        private bool clapRecursing = false;
        private List<RiqEntity> pinkMonkeys = new();
        private List<RiqEntity> pinkMonkeysCustom = new();

        private bool IsPinkMonkeyAtBeat(double beat, out float length)
        {
            length = 2;
            var e = pinkMonkeys.Find(x => x.beat == beat);
            bool isNotNull = e != null;
            if (isNotNull) length = e.length;
            return isNotNull;
        }

        private bool IsCustomPinkMonkeyAtBeat(double beat, out float length)
        {
            length = 2;
            var e = pinkMonkeysCustom.Find(x => x.beat == beat);
            bool isNotNull = e != null;
            if (isNotNull) length = e.length;
            return isNotNull;
        }

        private static double wantClap = double.MinValue;

        public static void PreStartClapping(double beat)
        {
            if (GameManager.instance.currentGame == "monkeyWatch")
            {
                instance.StartClapping(beat);
            }
            wantClap = beat;
        }

        private void StartClapping(double beat)
        {
            if (clapRecursing) return;
            clapRecursing = true;

            ClapRecursing(beat);
        }

        private void ClapRecursing(double beat)
        {
            if (beat >= theNextGameSwitchBeat) return;
            if (IsPinkMonkeyAtBeat(beat, out float length1))
            {
                PinkClap(length1);
            }
            else if (IsCustomPinkMonkeyAtBeat(beat, out float length2))
            {
                PinkClapCustom(length2);
            }
            else
            {
                NormalClap();
            }

            void NormalClap()
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 4, delegate
                    {
                        monkeyHandler.SpawnMonkey(beat, false, beat - 4 < persistBeat);
                        ClapRecursing(beat + 2);
                        cameraWantAngle += degreePerMonkey;
                    }),
                    new BeatAction.Action(beat - 1, delegate
                    {
                        monkeyHandler.GetMonkeyAtBeat(beat).Prepare(beat, beat + 1);
                    }),
                });
            }

            void PinkClap(float length)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 4, delegate
                    {
                        ClapRecursing(beat + length);
                    })
                };

                for (int i = 0; i < length; i++)
                {
                    int index = i;
                    actions.AddRange(new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i - 4, delegate
                        {
                            monkeyHandler.SpawnMonkey(beat + index, true, beat + index - 4 < persistBeat);
                            cameraWantAngle += degreePerMonkey;
                        }),
                        new BeatAction.Action(beat + i - 1, delegate
                        {
                            monkeyHandler.GetMonkeyAtBeat(beat + index).Prepare(beat + index, beat + index + 0.5);
                        }),
                    });
                }
                actions.Sort((x, y) => x.beat.CompareTo(y.beat));
                BeatAction.New(instance, actions);
            }

            void PinkClapCustom(float length)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 4, delegate
                    {
                        ClapRecursing(beat + length);
                    })
                };

                var relevantEvents = FindCustomOffbeatMonkeysBetweenBeat(beat, beat + length);
                relevantEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

                for (int i = 0; i < relevantEvents.Count; i++)
                {
                    var e = relevantEvents[i];
                    actions.AddRange(new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(e.beat - 4, delegate
                        {
                            monkeyHandler.SpawnMonkey(e.beat, true, e.beat - 4 < persistBeat);
                            cameraWantAngle += degreePerMonkey;
                        }),
                        new BeatAction.Action(e.beat - 1.5, delegate
                        {
                            monkeyHandler.GetMonkeyAtBeat(e.beat).Prepare(e.beat - 0.5, e.beat);
                        }),
                    });
                }
                actions.Sort((x, y) => x.beat.CompareTo(y.beat));
                BeatAction.New(instance, actions);
            }
        }
        #endregion

        #region Camera
        private void GetCameraMovements(double beat, bool onPlay)
        {
            double lastGameSwitchBeat = beat;
            if (onPlay)
            {
                var allEndsBeforeBeat = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat <= beat);
                if (allEndsBeforeBeat.Count > 0)
                {
                    allEndsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                    lastGameSwitchBeat = allEndsBeforeBeat[^1].beat;
                }
                else
                {
                    lastGameSwitchBeat = 0f;
                }
            }

            double nextGameSwitchBeat = double.MaxValue;

            var allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" }).FindAll(x => x.beat > lastGameSwitchBeat);
            if (allEnds.Count > 0)
            {
                allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
                nextGameSwitchBeat = allEnds[0].beat;
            }

            theNextGameSwitchBeat = nextGameSwitchBeat;

            double startClappingBeat = 0;
            float startAngle = 0;
            bool overrideStartBeat = true;

            var clappingEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "clap" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (clappingEvents.Count > 0)
            {
                clappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                startClappingBeat = clappingEvents[0].beat;
                startAngle = clappingEvents[0]["min"] * degreePerMonkey;
                cameraWantAngle = startAngle;
                cameraAngleDelay = startAngle;
                overrideStartBeat = false;
            }
            lastAngle = startAngle;
            clappingBeat = startClappingBeat;

            var pinkClappingEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "off", "offStretch", "offInterval" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (pinkClappingEvents.Count > 0)
            {
                pinkClappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                if (overrideStartBeat) startClappingBeat = pinkClappingEvents[0].beat;
                clappingBeat = startClappingBeat;

                var relevantPinkClappingEvents = pinkClappingEvents.FindAll(x => (x.beat - startClappingBeat) % 2 == 0);
                relevantPinkClappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

                double lastClappingBeat = startClappingBeat;
                float lastAngleToCheck = startAngle;

                for (int i = 0; i < relevantPinkClappingEvents.Count; i++)
                {
                    var e = relevantPinkClappingEvents[i];
                    if (e.beat < lastClappingBeat) continue;

                    float angleToAdd;
                    if (e.datamodel == "monkeyWatch/offInterval")
                    {
                        angleToAdd = FindCustomOffbeatMonkeysBetweenBeat(e.beat, e.beat + e.length).Count * degreePerMonkey;
                    }
                    else
                    {
                        angleToAdd = Mathf.Ceil(e.length) * degreePerMonkey;
                    }

                    if (e.beat - lastClappingBeat > 0)
                    {
                        lastAngleToCheck += (float)((e.beat - lastClappingBeat) / 2) * degreePerMonkey;
                    }

                    lastAngleToCheck += angleToAdd;
                    lastClappingBeat = e.beat + e.length;
                }
                startClappingBeat = lastClappingBeat;
                startAngle = lastAngleToCheck;
            }
        }

        private void UpdateCamera()
        {
            // lastAngle = cameraMovements[cameraIndex].degreeTo;
            // cameraIndex++;
            // if (cameraIndex + 1 < cameraMovements.Count && conductor.songPositionInBeats >= cameraMovements[cameraIndex].beat + cameraMovements[cameraIndex].length)
            // {
            //     UpdateCamera();
            // }
        }

        private double zoomOutStartBeat = -2;
        private bool zoomIn = true;
        private Util.EasingFunction.Function funcOut;
        private Util.EasingFunction.Function funcIn;

        private void CameraUpdate()
        {
            if (conductor.isPlaying && !conductor.isPaused)
            {
                float degreesBehind = cameraWantAngle - 2 * degreePerMonkey;
                float degreesBehindFast = cameraWantAngle - 3f * degreePerMonkey;
                targetDelayRate = Mathf.Max((cameraAngleDelay - degreesBehindFast) / (degreesBehindFast - cameraWantAngle), 0) + 0.5f;
                delayRate = Mathf.Lerp(delayRate, targetDelayRate, Time.deltaTime * (1f / conductor.pitchedSecPerBeat) * 0.5f);
                if (cameraAngleDelay < degreesBehind)
                {
                    cameraAngleDelay += degreePerMonkey * Time.deltaTime * (1f / conductor.pitchedSecPerBeat) * delayRate;
                    cameraAngleDelay = Mathf.Min(cameraAngleDelay, cameraWantAngle);
                }
                cameraAnchor.localEulerAngles = new Vector3(0, 0, cameraAngleDelay);
            }

            float normalizedZoomBeat = conductor.GetPositionFromBeat(zoomOutStartBeat, zoomIn ? zoomInBeatLength : zoomOutBeatLength);
            float newX = 0f;
            float newY = 0f;
            float newZ = 0f;
            if (zoomIn)
            {
                newX = funcIn(0, cameraTransform.position.x, Mathf.Clamp01(normalizedZoomBeat));
                newY = funcIn(0, cameraTransform.position.y, Mathf.Clamp01(normalizedZoomBeat));
                newZ = funcIn(fullZoomOut, 0, Mathf.Clamp01(normalizedZoomBeat));
            }
            else
            {
                newX = funcOut(cameraTransform.position.x, 0, Mathf.Clamp01(normalizedZoomBeat));
                newY = funcOut(cameraTransform.position.y, 0, Mathf.Clamp01(normalizedZoomBeat));
                newZ = funcOut(0, fullZoomOut, Mathf.Clamp01(normalizedZoomBeat));
            }

            if (zoomIn && normalizedZoomBeat > 1f)
            {
                cameraMoveable.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y * -1);
            }
            else
            {
                cameraMoveable.position = new Vector3(newX, -newY, newZ);
            }
        }
        #endregion

        public static List<RiqEntity> FindCustomOffbeatMonkeysBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offCustom" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        #region pink monkey sounds

        public static void PinkMonkeySound(double beat, float length, bool muteOoki, bool muteEek)
        {
            List<MultiSound.Sound> soundsToPlay = new();
            if (!muteOoki)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("monkeyWatch/voiceUki1", beat - 2),
                    new MultiSound.Sound("monkeyWatch/voiceUki1Echo1", beat - 1.75),
                    new MultiSound.Sound("monkeyWatch/voiceUki2", beat - 1),
                    new MultiSound.Sound("monkeyWatch/voiceUki2Echo1", beat - 0.75),
                    new MultiSound.Sound("monkeyWatch/voiceUki3", beat),
                    new MultiSound.Sound("monkeyWatch/voiceUki3Echo1", beat + 0.25),
                });
            }

            if (!muteEek)
            {
                for (int i = 0; i < length; i++)
                {
                    int randomKi = UnityEngine.Random.Range(1, 3);
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{randomKi}", beat + i + 0.5),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{randomKi}Echo{UnityEngine.Random.Range(1, 3)}", beat + i + 0.75),
                    });
                }
            }

            if (soundsToPlay.Count > 0) MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
        }

        public static void PinkMonkeySoundCustom(double beat, float length, bool muteOoki, bool muteEek)
        {
            List<MultiSound.Sound> soundsToPlay = new();
            var allCustoms = FindCustomOffbeatMonkeysBetweenBeat(beat, beat + length);
            if (!muteOoki)
            {
                soundsToPlay.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("monkeyWatch/voiceUki1", beat - 2),
                    new MultiSound.Sound("monkeyWatch/voiceUki1Echo1", beat - 1.75),
                    new MultiSound.Sound("monkeyWatch/voiceUki2", beat - 1),
                    new MultiSound.Sound("monkeyWatch/voiceUki2Echo1", beat - 0.75),
                });
                if (allCustoms.Find(x => x.beat == beat) == null)
                {
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound("monkeyWatch/voiceUki3", beat),
                        new MultiSound.Sound("monkeyWatch/voiceUki3Echo1", beat + 0.25),
                    });
                }
            }

            if (!muteEek)
            {
                foreach (var custom in allCustoms)
                {
                    int randomKi = UnityEngine.Random.Range(1, 3);
                    soundsToPlay.AddRange(new List<MultiSound.Sound>()
                    {
                        new MultiSound.Sound($"monkeyWatch/voiceKi{randomKi}", custom.beat),
                        new MultiSound.Sound($"monkeyWatch/voiceKi{randomKi}Echo{UnityEngine.Random.Range(1, 3)}", custom.beat + 0.25),
                    });
                }
            }

            if (soundsToPlay.Count > 0) MultiSound.Play(soundsToPlay.ToArray(), forcePlay: true);
        }

        #endregion
    }
}