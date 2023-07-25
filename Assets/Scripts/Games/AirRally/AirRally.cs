using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

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
                    preFunction = delegate { AirRally.PreStartRally(e.currentEntity.beat, e.currentEntity["toggle"]); }, 
                    defaultLength = 2f, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", false, "Silent", "Make Forthington Silent"),
                    }
                },
                new GameAction("ba bum bum bum", "Ba Bum Bum Bum")
                {
                    defaultLength = 6f, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", true, "Count", "Make Forthington Count"),
                    }
                },
                new GameAction("set distance", "Set Distance")
                {
                    defaultLength = .5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?"),
                        new Param("ease", EasingFunction.Ease.EaseInOutQuad, "Ease")
                    }
                },
                new GameAction("4beat", "4 Beat Count-In")
                {
                    defaultLength = 4f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn4(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["distance"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("distance", AirRally.DistanceSound.close, "Distance", "How far is Forthington?")
                    }
                },
                new GameAction("8beat", "8 Beat Count-In")
                {
                    defaultLength = 8f,
                    resizable = true,
                    preFunction = delegate
                    {
                        AirRally.ForthCountIn8(e.currentEntity.beat, e.currentEntity.length, e.currentEntity["distance"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("distance", AirRally.DistanceSound.close, "Distance", "How far is Forthington?")
                    }
                },
                new GameAction("forthington voice lines", "Count")
                {
                    preFunction = delegate { AirRally.ForthVoice(e.currentEntity.beat, e.currentEntity["type"]); }, 
                    parameters = new List<Param>()
                    { 
                        new Param("type", AirRally.CountSound.one, "Type", "The number Forthington will say"),
                    },
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
    using Scripts_AirRally;

    public class AirRally : Minigame
    {
        public static AirRally instance { get; set; }

        [Header("Component")]
        [SerializeField] Animator Baxter;
        [SerializeField] Animator Forthington;
        [SerializeField] GameObject Shuttlecock;
        public GameObject ActiveShuttle;
        [SerializeField] GameObject objHolder;
        public DistanceSound e_BaBumState;

        [Header("Tween")]
        Tween tweenForBaxter;
        Tween tweenForForth;

        [Header("Variables")]
        bool shuttleActive;
        public bool hasMissed;

        [Header("Waypoint")]
        public float wayPointZForForth;

        void Start()
        {
            Baxter.Play("Idle");
            Forthington.Play("Idle");
        }

        private void Awake()
        {
            instance = this;
        }      

        // Update is called once per frame
        void Update()
        {
            if(PlayerInput.Pressed() && !IsExpectingInputNow())
            {
                Baxter.DoScaledAnimationAsync("Hit", 0.5f);
                SoundByte.PlayOneShotGame("airRally/whooshForth_Close", -1f);
            }
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
            if (!shuttleActive)
            {
                ActiveShuttle = GameObject.Instantiate(Shuttlecock, objHolder.transform);
                ActiveShuttle.SetActive(true);
            }
            
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = false;
            shuttleScript.startBeat = beat;
            shuttleScript.flyBeats = targetBeat - beat;
            shuttleScript.flyType = type;
            
            shuttleActive = true;

            Forthington.DoScaledAnimationAsync("Hit", 0.5f);
        }

        public void ReturnObject(double beat, double targetBeat, bool type)
        {
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = true;
            shuttleScript.startBeat = beat;
            shuttleScript.flyBeats = targetBeat - beat;
            shuttleScript.flyType = type;
        }

        #region count-ins
        public static void ForthCountIn4(double beat, float length, int distance)
        {
            string distanceString = (DistanceSound)distance switch
            {
                DistanceSound.close => "",
                DistanceSound.far => "Far",
                DistanceSound.farther => "Farther",
                DistanceSound.farthest => "Farthest",
                _ => throw new System.NotImplementedException()
            };
            float realLength = length / 4;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airRally/countIn1" + distanceString, beat),
                new MultiSound.Sound("airRally/countIn2" + distanceString, beat + (1 * realLength)),
                new MultiSound.Sound("airRally/countIn3" + distanceString, beat + (2 * realLength), 1, 1, false, 0.107f),
                new MultiSound.Sound("airRally/countIn4" + distanceString, beat + (3 * realLength), 1, 1, false, 0.051f),
            }, forcePlay: true);

            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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
                });
            }
        }

        public static void ForthCountIn8(double beat, float length, int distance)
        {
            string distanceString = (DistanceSound)distance switch
            {
                DistanceSound.close => "",
                DistanceSound.far => "Far",
                DistanceSound.farther => "Farther",
                DistanceSound.farthest => "Farthest",
                _ => throw new System.NotImplementedException()
            };
            float realLength = length / 8;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("airRally/countIn1" + distanceString, beat),
                new MultiSound.Sound("airRally/countIn2" + distanceString, beat + (2 * realLength)),
                new MultiSound.Sound("airRally/countIn1" + distanceString, beat + (4 * realLength)),
                new MultiSound.Sound("airRally/countIn2" + distanceString, beat + (5 * realLength)),
                new MultiSound.Sound("airRally/countIn3" + distanceString, beat + (6 * realLength), 1, 1, false, 0.107f),
                new MultiSound.Sound("airRally/countIn4" + distanceString, beat + (7 * realLength), 1, 1, false, 0.051f),
            }, forcePlay: true);

            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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
                });
            }
        }

        public static void ForthVoice(double beat, int type)
        {
            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate 
                    { 
                        instance.Forthington.DoScaledAnimationAsync("TalkShort", 0.5f);
                    })
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

            int distance = 0;
            
            switch (distance)
            {
                case (int)DistanceSound.close:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case (int)DistanceSound.far:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Far", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case (int)DistanceSound.farther:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Farther", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
                case (int)DistanceSound.farthest:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound($"airRally/countIn{type + 1}Farthest", beat, 1, 1, false, offset),
                    }, forcePlay: true);
                    break;
            }
        }
        #endregion

        public void SetDistance(int type, bool instant = false)
        {
            switch (type)
            {
                case 0:
                    e_BaBumState = DistanceSound.close;
                    wayPointZForForth = 3.55f;
                    break;
                case 1:
                    e_BaBumState = DistanceSound.far;
                    wayPointZForForth = 35.16f;
                    break;
                case 2:
                    e_BaBumState = DistanceSound.farther;
                    wayPointZForForth = 105.16f;
                    break;
                case 3:
                    e_BaBumState = DistanceSound.farthest;
                    wayPointZForForth = 255.16f;
                    break;     
            }
            if (instant)
            {
                tweenForForth.Kill();
                Forthington.gameObject.transform.position = new Vector3(Forthington.gameObject.transform.position.x, Forthington.gameObject.transform.position.y, wayPointZForForth);
            }
            else
            {
                tweenForForth = Forthington.gameObject.transform.DOMoveZ(wayPointZForForth, .7f).SetEase(Ease.OutQuad);
            }
        }

        public static void PreStartRally(double beat, bool silent)
        {
            if (GameManager.instance.currentGame == "airRally")
            {
                instance.StartRally(beat, silent);
            }
        }

        private bool recursingRally;
        private void StartRally(double beat, bool silent)
        {
            if (recursingRally) return;
            recursingRally = true;

            RallyRecursion(beat, silent);
        }

        private void RallyRecursion(double beat, bool silent)
        {
            bool isBaBumBeat = IsBaBumBeat(beat);
            bool countBaBum = CountBaBum(beat);
            string distanceString = e_BaBumState switch
            {
                DistanceSound.close => "Close",
                DistanceSound.far => "Far",
                DistanceSound.farther => "Farther",
                DistanceSound.farthest => "Farthest",
                _ => throw new System.NotImplementedException()
            };

            SoundByte.PlayOneShotGame("airRally/whooshForth_" + distanceString, beat + 0.25);

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.5, delegate
                {
                    if (isBaBumBeat) BaBumBumBum(beat, countBaBum, silent);
                    else RallyRecursion(beat + 2, silent);
                }),
                new BeatAction.Action(beat, delegate
                {
                    Baxter.DoScaledAnimationAsync((distanceString == "Close") ? "CloseReady" : "FarReady", 0.5f);
                    ServeObject(beat, beat + 1, false);
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

        private bool CountBaBum(double beat)
        {
            var baBumEvent = EventCaller.GetAllInGameManagerList("airRally", new string[] { "ba bum bum bum" }).Find(x => x.beat == beat);
            if (baBumEvent == null) return false;

            return baBumEvent["toggle"];
        }

        private void BaBumBumBum(double beat, bool count, bool silent)
        {
            bool isBaBumBeat = IsBaBumBeat(beat + 4);
            bool countBaBum = CountBaBum(beat + 4);
            string distanceString = e_BaBumState switch
            {
                DistanceSound.close => "Close",
                DistanceSound.far => "Far",
                DistanceSound.farther => "Farther",
                DistanceSound.farthest => "Farthest",
                _ => throw new System.NotImplementedException()
            };

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            sounds.AddRange(new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("airRally/baBumBumBum_" + distanceString + "1", beat - 0.5),
                new MultiSound.Sound("airRally/baBumBumBum_" + distanceString + "2", beat),
                new MultiSound.Sound("airRally/baBumBumBum_" + distanceString + "3", beat + 1),
                new MultiSound.Sound("airRally/baBumBumBum_" + distanceString + "4", beat + 2),
                new MultiSound.Sound("airRally/hitForth_" + distanceString, beat + 2),
                new MultiSound.Sound("airRally/whooshForth_" + distanceString, beat + 2.5),
            });

            if (e_BaBumState == DistanceSound.far) sounds.Add(new MultiSound.Sound("airRally/whooshForth_Far2", beat + 3.5f));

            if (count && !isBaBumBeat)
            {
                bool isClose = e_BaBumState == DistanceSound.close;
                sounds.AddRange(new List<MultiSound.Sound>()
                {
                    new MultiSound.Sound("airRally/countIn2" + (isClose ? "" : distanceString), beat + 3),
                    new MultiSound.Sound("airRally/countIn3" + (isClose ? "" : distanceString), beat + 4, 1, 1, false, 0.107f),
                    new MultiSound.Sound("airRally/countIn4" + (isClose ? "" : distanceString), beat + 5, 1, 1, false, 0.051f),
                });
            }

            MultiSound.Play(sounds.ToArray());
            //ready after 2 beat, close, far, far, farthest

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate 
                {
                    if (isBaBumBeat) BaBumBumBum(beat + 4, countBaBum, silent);
                    else RallyRecursion(beat + 6, silent); 
                }),
                new BeatAction.Action(beat + 1f, delegate { Forthington.DoScaledAnimationAsync("Ready", 0.5f); }),
                new BeatAction.Action(beat + 2f, delegate { ServeObject(beat + 2f, beat + 4f, true); } ),
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
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(e_BaBumState);

                if (e_BaBumState == DistanceSound.close)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Close");
                }
                if (e_BaBumState == DistanceSound.far)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Far");
                }
                if (e_BaBumState == DistanceSound.farther)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Farther");
                }
                if (e_BaBumState == DistanceSound.farthest)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Farthest");
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
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(e_BaBumState);

                if (e_BaBumState == DistanceSound.close)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Close");
                }
                if (e_BaBumState == DistanceSound.far)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Far");
                }
                if (e_BaBumState == DistanceSound.farther)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Farther");
                }
                if (e_BaBumState == DistanceSound.farthest)
                {
                    SoundByte.PlayOneShotGame("airRally/hitBaxter_Farthest");
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

