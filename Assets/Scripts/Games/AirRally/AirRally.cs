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
                    preFunction = delegate { AirRally.PreStartBaBumBumBum(e.currentEntity.beat, e.currentEntity["toggle"]); },
                    defaultLength = 6f, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", true, "Count", "Make Forthington Count"),
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
        [SerializeField] GameObject Shuttlecock;
        public GameObject ActiveShuttle;
        [SerializeField] GameObject objHolder;

        [Header("Variables")]
        bool shuttleActive;
        public bool hasMissed;

        [Header("Waypoint")]
        [SerializeField] private float wayPointBeatLength = 0.25f;
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
        }      

        // Update is called once per frame
        void Update()
        {
            if(PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                Baxter.DoScaledAnimationAsync("Hit", 0.5f);
                SoundByte.PlayOneShotGame("airRally/whooshForth_Close", -1f);
            }

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

        public override void OnGameSwitch(double beat)
        {
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
                StartBaBumBumBum(wantStartBaBum, wantCount);
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

        public static void PreStartRally(double beat)
        {
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

        public static void PreStartBaBumBumBum(double beat, bool count)
        {
            if (GameManager.instance.currentGame == "airRally")
            {
                instance.StartBaBumBumBum(beat, count);
            }
            else
            {
                wantStartBaBum = beat;
                wantCount = count;
            }
        }

        private void StartBaBumBumBum(double beat, bool count)
        {
            if (recursingRally || IsRallyBeat(beat)) return;
            recursingRally = true;

            BaBumBumBum(beat, count);
        }

        private void RallyRecursion(double beat)
        {
            bool isBaBumBeat = IsBaBumBeat(beat);
            bool countBaBum = CountBaBum(beat);
            bool silent = IsSilentAtBeat(beat);

            SoundByte.PlayOneShotGame("airRally/whooshForth_" + GetDistanceStringAtBeat(beat + 0.25), beat + 0.25);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.5, delegate
                {
                    if (isBaBumBeat) BaBumBumBum(beat, countBaBum);
                    else RallyRecursion(beat + 2);

                    ServeObject(beat, beat + 1, false);
                }),
                new BeatAction.Action(beat, delegate
                {
                    string distanceString = GetDistanceStringAtBeat(beat);
                    Baxter.DoScaledAnimationAsync((distanceString == "Close") ? "CloseReady" : "FarReady", 0.5f);
                    SoundByte.PlayOneShotGame("airRally/hitForth_" + distanceString);
                    if (!(silent || isBaBumBeat)) SoundByte.PlayOneShotGame("airRally/nya_" + distanceString);
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

        private void BaBumBumBum(double beat, bool count)
        {
            bool isBaBumBeat = IsBaBumBeat(beat + 4);
            bool countBaBum = CountBaBum(beat + 4);

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            sounds.AddRange(new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAtBeat(beat + - 0.5) + "1", beat - 0.5),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAtBeat(beat) + "2", beat),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAtBeat(beat + 1f) + "3", beat + 1),
                new MultiSound.Sound("airRally/baBumBumBum_" + GetDistanceStringAtBeat(beat + 2f) + "4", beat + 2),
                new MultiSound.Sound("airRally/hitForth_" + GetDistanceStringAtBeat(beat + 2f), beat + 2),
                new MultiSound.Sound("airRally/whooshForth_" + GetDistanceStringAtBeat(beat + 2.5), beat + 2.5),
            });

            if (GetDistanceStringAtBeat(beat + 3f) == "Far") sounds.Add(new MultiSound.Sound("airRally/whooshForth_Far2", beat + 3f));

            if (count && !isBaBumBeat)
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
                    if (isBaBumBeat) BaBumBumBum(beat + 4, countBaBum);
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

