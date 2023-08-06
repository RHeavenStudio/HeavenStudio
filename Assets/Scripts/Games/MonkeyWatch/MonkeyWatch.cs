using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlMonkeyWatchLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("monkeyWatch", "Monkey Watch", "f0338d", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Clapping")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("min", new EntityTypes.Integer(0, 60, 0), "Start Minutes", "1 minute is equivalent to 1 monkey.")
                    }
                },
                new GameAction("off", "Pink Monkeys")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("muteC", false, "Mute Ooki"),
                        new Param("muteE", false, "Mute Eeks")
                    }
                },
                new GameAction("offStretch", "Pink Monkeys (Stretchable)")
                {
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
    //using Scripts_MonkeyWatch;
    public class MonkeyWatch : Minigame
    {
        private const float degreePerMonkey = 6f;
        public enum WatchPoint
        {
            PersistGameSwitch,
            SetTime
        }
        public static MonkeyWatch instance;

        [Header("Components")]
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraMoveable;

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
            CameraUpdate();
        }

        private void Update()
        {
            CameraUpdate();
        }

        private void CameraUpdate()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused && cameraMovements.Count > 0)
            {
                if (cameraIndex + 1 < cameraMovements.Count && cond.songPositionInBeats >= cameraMovements[cameraIndex].beat + cameraMovements[cameraIndex].length)
                {
                    lastAngle = cameraMovements[cameraIndex].degreeTo;
                    cameraIndex++;
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

        #region Persist

        public override void OnGameSwitch(double beat)
        {
            GetCameraMovements(beat, false);
        }

        public override void OnPlay(double beat)
        {
            GetCameraMovements(beat, true);
        }

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

            double startClappingBeat = 0;
            float startAngle = 0;
            bool overrideStartBeat = true;

            var clappingEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "clap" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (clappingEvents.Count > 0)
            {
                clappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                startClappingBeat = clappingEvents[0].beat;
                startAngle = clappingEvents[0]["min"];
                overrideStartBeat = false;
            }
            lastAngle = startAngle;

            var pinkClappingEvents = EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "off", "offStretch", "offInterval" }).FindAll(x => x.beat >= lastGameSwitchBeat && x.beat < nextGameSwitchBeat);
            if (pinkClappingEvents.Count > 0)
            {
                pinkClappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));
                if (overrideStartBeat) startClappingBeat = pinkClappingEvents[0].beat;

                var relevantPinkClappingEvents = pinkClappingEvents.FindAll(x => (x.beat - startClappingBeat) % 2 == 0);
                relevantPinkClappingEvents.Sort((x, y) => x.beat.CompareTo(y.beat));

                double lastClappingBeat = startClappingBeat;
                float lastAngleToCheck = startAngle;

                for (int i = 0; i < relevantPinkClappingEvents.Count; i++)
                {
                    var e = relevantPinkClappingEvents[i];
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
                        angleToAdd = Mathf.Round(e.length) * degreePerMonkey;
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

            if (clappingEvents.Count > 0)
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

        #endregion

        private List<RiqEntity> FindCustomOffbeatMonkeysBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("monkeyWatch", new string[] { "offCustom" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }
    }
}
