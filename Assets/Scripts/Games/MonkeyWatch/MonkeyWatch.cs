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
                        new Param("min", new EntityTypes.Integer(0, 60, 0), "Start Minutes", "1 minute is equivalent to 1 monkey.")
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
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MonkeyWatch;
    using System.Reflection;

    public class MonkeyWatch : Minigame
    {
        private const float degreePerMonkey = 6f;

        public static MonkeyWatch instance;

        [Header("Components")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraMoveable;
        [SerializeField] private MonkeyClockArrow monkeyClockArrow;
        [SerializeField] private WatchMonkeyHandler monkeyHandler;

        private float lastAngle = 0f;
        private int cameraIndex = 0;
        private struct MonkeyCamera
        {
            public float degreeTo;
            public float length;
            public double beat;
        }
        private List<MonkeyCamera> cameraMovements = new();

        private void Awake()
        {
            instance = this;
            pinkMonkeys = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "off", "offStretch" });
            pinkMonkeysCustom = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offInterval" });
            CameraUpdate();
        }

        private void Update()
        {
            CameraUpdate();
        }

        public void PlayerMonkeyClap(bool big, bool barely)
        {
            monkeyClockArrow.PlayerClap(big, barely, false);
        }

        private double persistBeat = 0;
        private double theNextGameSwitchBeat = double.MaxValue;
        private double clappingBeat = 0;

        public override void OnGameSwitch(double beat)
        {
            persistBeat = beat;
            GetCameraMovements(beat, false);
            monkeyClockArrow.MoveToAngle(lastAngle);
            monkeyHandler.Init((int)(lastAngle / degreePerMonkey));
            if (wantClap >= beat && IsClapBeat(wantClap))
            {
                StartClapping(wantClap);
            }
            PersistAppear(beat);

            bool IsClapBeat(double clapBeat)
            {
                return EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "clap" }).Find(x => x.beat == clapBeat) != null;
            }
        }

        public override void OnPlay(double beat)
        {
            persistBeat = beat;
            GetCameraMovements(beat, true);
            monkeyClockArrow.MoveToAngle(lastAngle);
            monkeyHandler.Init((int)(lastAngle / degreePerMonkey));
            PersistAppear(beat);
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
                        int realerIndex = index;
                        double realLastBeat = lastBeat;
                        actions.Add(new BeatAction.Action(beat + (index * length), delegate
                        {
                            monkeyHandler.SpawnMonkey(realLastBeat, true, beat + (realerIndex * length) < gameSwitchBeat);
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
                        int realerIndex = index;
                        actions.Add(new BeatAction.Action(beat + (index * length), delegate
                        {
                            monkeyHandler.SpawnMonkey(relevantPinks[jindex].beat, true, beat + (realerIndex * length) < gameSwitchBeat);
                        }));
                        index++;
                    }
                    lastBeat += pinkCustomLength;
                }
                else
                {
                    int realIndex = index;
                    double realLastBeat = lastBeat;
                    actions.Add(new BeatAction.Action(beat + (index * length), delegate
                    {
                        monkeyHandler.SpawnMonkey(realLastBeat, false, beat + (realIndex * length) < gameSwitchBeat);
                    }));
                    index++;
                    lastBeat += 2;
                }
                Debug.Log(index);
            }

            actions.Sort((x, y) => x.beat.CompareTo(y.beat));
            BeatAction.New(instance.gameObject, actions);
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 4, delegate
                    {
                        monkeyHandler.SpawnMonkey(beat, false, beat - 4 < persistBeat);
                        ClapRecursing(beat + 2);
                    }),
                    new BeatAction.Action(beat - 1, delegate
                    {
                        monkeyHandler.GetMonkeyAtBeat(beat).Prepare(beat, beat + 1);
                    }),
                    new BeatAction.Action(beat + 1, delegate
                    {
                        monkeyClockArrow.Move();
                    })
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
                        }),
                        new BeatAction.Action(beat + i - 1, delegate
                        {
                            monkeyHandler.GetMonkeyAtBeat(beat + index).Prepare(beat + index, beat + index + 0.5);
                        }),
                        new BeatAction.Action(beat + i + 0.5, delegate
                        {
                            monkeyClockArrow.Move();
                        })
                    });
                }
                actions.Sort((x, y) => x.beat.CompareTo(y.beat));
                BeatAction.New(instance.gameObject, actions);
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
                        }),
                        new BeatAction.Action(e.beat - 1.5, delegate
                        {
                            monkeyHandler.GetMonkeyAtBeat(e.beat).Prepare(e.beat - 0.5, e.beat);
                        }),
                        new BeatAction.Action(e.beat, delegate
                        {
                            monkeyClockArrow.Move();
                        })
                    });
                }
                actions.Sort((x, y) => x.beat.CompareTo(y.beat));
                BeatAction.New(instance.gameObject, actions);
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
                    if (e.beat - lastClappingBeat > 0)
                    {
                        cameraMovements.Add(new MonkeyCamera()
                        {
                            beat = lastClappingBeat,
                            length = (float)(e.beat - lastClappingBeat),
                            degreeTo = lastAngleToCheck + (float)((e.beat - lastClappingBeat) / 2) * degreePerMonkey
                        });
                        lastAngleToCheck += (float)((e.beat - lastClappingBeat) / 2) * degreePerMonkey;
                    }
                    float angleToAdd;
                    if (e.datamodel == "monkeyWatch/offInterval")
                    {
                        angleToAdd = FindCustomOffbeatMonkeysBetweenBeat(e.beat, e.beat + e.length).Count * degreePerMonkey;
                    }
                    else
                    {
                        angleToAdd = Mathf.Ceil(e.length) * degreePerMonkey;
                    }
                    cameraMovements.Add(new MonkeyCamera()
                    {
                        beat = e.beat,
                        length = e.length,
                        degreeTo = lastAngleToCheck + angleToAdd
                    });

                    lastAngleToCheck += angleToAdd;
                    lastClappingBeat = e.beat + e.length;
                }
                startClappingBeat = lastClappingBeat;
                startAngle = lastAngleToCheck;
            }

            if (clappingEvents.Count > 0 || pinkClappingEvents.Count > 0)
            {
                cameraMovements.Add(new MonkeyCamera
                {
                    beat = startClappingBeat,
                    degreeTo = startAngle + degreePerMonkey,
                    length = 2
                });
            }


            Debug.Log("Camera Movements Count: " + cameraMovements.Count);
            foreach (var cameraMovement in cameraMovements)
            {
                Debug.Log("Beat: " + cameraMovement.beat + ", Length: " + cameraMovement.length + " , DegreeTo: " + cameraMovement.degreeTo);
            }
        }

        private void UpdateCamera()
        {
            lastAngle = cameraMovements[cameraIndex].degreeTo;
            cameraIndex++;
            if (cameraIndex + 1 < cameraMovements.Count && Conductor.instance.songPositionInBeats >= cameraMovements[cameraIndex].beat + cameraMovements[cameraIndex].length)
            {
                UpdateCamera();
            }
        }

        private void CameraUpdate()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused && cameraMovements.Count > 0)
            {
                if (cameraIndex + 1 < cameraMovements.Count && cond.songPositionInBeats >= cameraMovements[cameraIndex].beat + cameraMovements[cameraIndex].length)
                {
                    UpdateCamera();
                }

                float normalizedBeat = cond.GetPositionFromBeat(cameraMovements[cameraIndex].beat, cameraMovements[cameraIndex].length);

                float newAngle;

                if (normalizedBeat < 0)
                {
                    newAngle = lastAngle;
                }
                else
                {
                    newAngle = Mathf.LerpUnclamped(lastAngle, cameraMovements[cameraIndex].degreeTo, normalizedBeat);
                }

                cameraAnchor.localEulerAngles = new Vector3(0, 0, newAngle);
            }
            cameraMoveable.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y * -1);
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