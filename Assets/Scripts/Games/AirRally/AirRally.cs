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
                new GameAction("set distance", "Set Distance")
                {
                    function = delegate { AirRally.instance.SetDistance(e.currentEntity["type"], e.currentEntity["inst"]); }, 
                    defaultLength = .5f, 
                    parameters = new List<Param>()
                    {
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?"),
                        new Param("inst", false, "Instant", "Instantly move Forthington")
                    }
                },
                //new GameAction("start rally",                    delegate { AirRally.instance.StartRally(true); }, .5f, false),
                new GameAction("rally", "Rally")
                {
                    function = delegate { AirRally.instance.SetDistance(e.currentEntity["type"]); AirRally.instance.Rally(e.currentEntity.beat, e.currentEntity["toggle"], e.currentEntity.length); }, 
                    defaultLength = 2f, 
                    resizable = true, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", false, "Silent", "Make Forthington Silent"),
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?")
                    }
                },
                new GameAction("ba bum bum bum", "Ba Bum Bum Bum")
                {
                    function = delegate { AirRally.instance.BaBumBumBum(e.currentEntity.beat, e.currentEntity["toggle"], e.currentEntity["type"]); }, 
                    defaultLength = 7f, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", false, "Count", "Make Forthington Count"),
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?")
                    }
                },
                new GameAction("forthington voice lines", "Forthington Voice Lines")
                {
                    preFunction = delegate { AirRally.ForthVoice(e.currentEntity.beat, e.currentEntity["type"], e.currentEntity["type2"]); }, 
                    parameters = new List<Param>()
                    { 
                        new Param("type", AirRally.CountSound.one, "Type", "The number Forthington will say"),
                        new Param("type2", AirRally.DistanceSound.close, "Type", "How far is Forthington?")
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
        [SerializeField] GameObject Baxter;
        [SerializeField] GameObject Forthington;
        [SerializeField] GameObject Shuttlecock;
        public GameObject ActiveShuttle;
        [SerializeField] GameObject objHolder;
        public DistanceSound e_BaBumState;

        [Header("Tween")]
        Tween tweenForBaxter;
        Tween tweenForForth;

        [Header("Variables")]
        public float serveBeat;
        public bool started;
        public bool served;
        bool babum;
        bool shuttleActive;
        public bool hasMissed;
        public static List<float> queuedVoiceLines = new List<float>();

        [Header("Waypoint")]
        public float wayPointZForForth;

        [Header("Debug")]
        public float beatShown;
        public float lengthHolder;
        public float lengthShown;
        public int wantDistance;
        public bool wantSilent;
        public float beatHolder;
        public Transform holderPos;

        void OnDestroy()
        {
            if (queuedVoiceLines.Count > 0) queuedVoiceLines.Clear(); 
        }

        void Start()
        {
            Baxter.GetComponent<Animator>().Play("Idle");
            Forthington.GetComponent<Animator>().Play("Idle");
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
                Baxter.GetComponent<Animator>().Play("Hit");
                Jukebox.PlayOneShotGame("airRally/whooshForth_Close", -1f);
            }

            var cond = Conductor.instance;
            var currentBeat = cond.songPositionInBeats;
            var hitBeat = serveBeat;

            if (started)
            {
                if (!served)
                {
                    hitBeat = serveBeat;
                }

                if(lengthHolder != lengthShown)
                {
                    started = true;
                    var f = currentBeat;
                    Rally(serveBeat + (int)f, wantSilent, lengthHolder);
                }
            }

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedVoiceLines.Count > 0)
                {
                    for (int i = 0; i < queuedVoiceLines.Count; i++)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(queuedVoiceLines[i], delegate
                            {
                                Forthington.GetComponent<Animator>().Play("TalkShort", 0, 0);
                            })
                        });
                    }
                    queuedVoiceLines.Clear();
                }
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

        public void ServeObject(float beat, float targetBeat, bool type)
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

            Forthington.GetComponent<Animator>().Play("Hit");
        }

        public void ReturnObject(float beat, float targetBeat, bool type)
        {
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = true;
            shuttleScript.startBeat = beat;
            shuttleScript.flyBeats = targetBeat - beat;
            shuttleScript.flyType = type;
        }

        public static void ForthVoice(float beat, int type, int type2)
        {
            if (GameManager.instance.currentGame == "airRally")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate 
                    { 
                        instance.Forthington.GetComponent<Animator>().Play("TalkShort");
                    })
                });
            }
            else
            {
                queuedVoiceLines.Add(beat);
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
            switch (type2)
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

        public void StartRally(bool start)
        {
            started = start;
        }

        public void Rally(float beat, bool silent, float length)
        {
            started = true;
            beatShown = beat;
            if (started)
            {
                wantSilent = silent;
                serveBeat += 2f;
                lengthHolder = length;
                lengthShown += 2f;
                beatHolder = beat;

                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { ServeObject(beat, beat + 1, false); } ),
                });

                string wooshSnd = "airRally/whooshForth_Close";
                switch (e_BaBumState)
                {
                    case DistanceSound.close:
                        wooshSnd = "airRally/whooshForth_Close";
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("CloseReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Close"); }),
                            new BeatAction.Action(beat, delegate { if(!(silent || babum)) { Jukebox.PlayOneShotGame("airRally/nya_Close"); } }),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.far:
                        wooshSnd = "airRally/whooshForth_Far";
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Far"); }),
                            new BeatAction.Action(beat, delegate { if(!(silent || babum)) { Jukebox.PlayOneShotGame("airRally/nya_Far"); } }),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.farther:
                        wooshSnd = "airRally/whooshForth_Farther";
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Farther"); }),
                            new BeatAction.Action(beat, delegate { if(!(silent || babum)) { Jukebox.PlayOneShotGame("airRally/nya_Farther"); } }),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.farthest:
                        wooshSnd = "airRally/whooshForth_Farthest";
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Farthest"); }),
                            new BeatAction.Action(beat, delegate { if(!(silent || babum)) { Jukebox.PlayOneShotGame("airRally/nya_Farthest"); } }),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;
                }
                //SpawnObject(beat, (int)e_BaBumState);
                ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, RallyOnHit, RallyOnMiss, RallyEmpty);

                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(wooshSnd, beat+.25f),
                });

                //restart rally
                if (lengthShown == lengthHolder || lengthShown >= lengthHolder)
                {
                    serveBeat = 0f;
                    lengthShown = 0f;
                    lengthHolder = 0f;
                }
            }
        }
        

        public void BaBumBumBum(float beat, bool count, int type)
        {
            //This feels wrong, will keep until I figure out what's wrong
            babum = true;
            serveBeat = 0f;
            lengthShown = 0f;
            lengthHolder = 0f;

            string[] sounds = { "", "", "", "", "", ""};
            string[] sounds2 = { "", "", "" };
            if (e_BaBumState == DistanceSound.close || type == 0)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.5f, delegate { Baxter.GetComponent<Animator>().Play("CloseReady");})
                });
                sounds[0] = "airRally/baBumBumBum_Close1";
                sounds[1] = "airRally/baBumBumBum_Close2";
                sounds[2] = "airRally/baBumBumBum_Close3";
                sounds[3] = "airRally/baBumBumBum_Close4";
                sounds[4] = "airRally/hitForth_Close";
                sounds[5] = "airRally/whooshForth_Close";

                sounds2 = new string[] { "airRally/countIn2", "airRally/countIn3", "airRally/countIn4" };
            }

            if (e_BaBumState == DistanceSound.far || type == 1)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.5f, delegate { Baxter.GetComponent<Animator>().Play("FarReady");})
                });
                sounds[0] = "airRally/baBumBumBum_Far1";
                sounds[1] = "airRally/baBumBumBum_Far2";
                sounds[2] = "airRally/baBumBumBum_Far3";
                sounds[3] = "airRally/baBumBumBum_Far4";
                sounds[4] = "airRally/hitForth_Far";
                sounds[5] = "airRally/whooshForth_Far";

                sounds2 = new string[] { "airRally/countIn2Far", "airRally/countIn3Far", "airRally/countIn4Far" };
            }
            
            if (e_BaBumState == DistanceSound.farther || type == 2)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.5f, delegate { Baxter.GetComponent<Animator>().Play("FarReady");})
                });
                sounds[0] = "airRally/baBumBumBum_Farther1";
                sounds[1] = "airRally/baBumBumBum_Farther2";
                sounds[2] = "airRally/baBumBumBum_Farther3";
                sounds[3] = "airRally/baBumBumBum_Farther4";
                sounds[4] = "airRally/hitForth_Farther";
                sounds[5] = "airRally/whooshForth_Farther";

                sounds2 = new string[] { "airRally/countIn2Farther", "airRally/countIn3Farther", "airRally/countIn4Farther" };
            }

            if (e_BaBumState == DistanceSound.farthest || type == 3)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2.5f, delegate { Baxter.GetComponent<Animator>().Play("FarthestReady");})
                });
                sounds[0] = "airRally/baBumBumBum_Farthest1";
                sounds[1] = "airRally/baBumBumBum_Farthest2";
                sounds[2] = "airRally/baBumBumBum_Farthest3";
                sounds[3] = "airRally/baBumBumBum_Farthest4";
                sounds[4] = "airRally/hitForth_Farthest";
                sounds[5] = "airRally/whooshForth_Farthest";

                sounds2 = new string[] { "airRally/countIn2Farthest", "airRally/countIn3Farthest", "airRally/countIn4Farthest" };
            }

            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { SetDistance(type, false); }),
                new BeatAction.Action(beat + 1.5f, delegate { Forthington.GetComponent<Animator>().Play("Ready"); }),
                new BeatAction.Action(beat + 2.5f, delegate { ServeObject(beat + 2.5f, beat + 4.5f, true); } ),
                new BeatAction.Action(beat + 3.5f, delegate { Forthington.GetComponent<Animator>().Play("TalkShort"); }),
                new BeatAction.Action(beat + 4f, delegate { if(!count) Forthington.GetComponent<Animator>().Play("TalkShort"); }),
                new BeatAction.Action(beat + 4.5f, delegate { Forthington.GetComponent<Animator>().Play("Ready"); }),
                new BeatAction.Action(beat + 5.5f, delegate { if(babum) { babum = false; } }),
            });

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound(sounds[0], beat),
                new MultiSound.Sound(sounds[1], beat + .5f),
                new MultiSound.Sound(sounds[2], beat + 1.5f),
                new MultiSound.Sound(sounds[3], beat + 2.5f),
                new MultiSound.Sound(sounds[4], beat + 2.5f),
                new MultiSound.Sound(sounds[5], beat + 3f),
            });
            if(e_BaBumState == DistanceSound.far)
                MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("airRally/whooshForth_Far2", beat + 3.5f) });

            if (count)
            {
                
                var sound2 = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(sounds2[0], beat + 3.5f),
                    new MultiSound.Sound(sounds2[1], beat + 4.5f, 1, 1, false, 0.107f),
                    new MultiSound.Sound(sounds2[2], beat + 5.5f, 1, 1, false, 0.051f)
                };

                MultiSound.Play(sound2);
            }

            ScheduleInput(beat, 4.5f, InputType.STANDARD_DOWN, LongShotOnHit, RallyOnMiss, RallyEmpty);
        }

        public void RallyOnHit(PlayerActionEvent caller, float state)
        {
            Baxter.GetComponent<Animator>().Play("Hit");

            if (state >= 1 || state <= -1)
            { 
                ActiveShuttle.GetComponent<Shuttlecock>().DoNearMiss();
                hasMissed = true;
                shuttleActive = false;
                ActiveShuttle = null;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeats, caller.startBeat + caller.timer + 1f, false);
                hasMissed = false;
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(e_BaBumState);

                if (e_BaBumState == DistanceSound.close)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Close");
                }
                if (e_BaBumState == DistanceSound.far)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Far");
                }
                if (e_BaBumState == DistanceSound.farther)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Farther");
                }
                if (e_BaBumState == DistanceSound.farthest)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Farthest");
                }
            }
            served = false;
        }

        public void LongShotOnHit(PlayerActionEvent caller, float state)
        {
            Baxter.GetComponent<Animator>().Play("Hit");

            if (state >= 1 || state <= -1)
            { 
                ActiveShuttle.GetComponent<Shuttlecock>().DoThrough();
                hasMissed = true;
                shuttleActive = false;
            }
            else
            {
                ReturnObject(Conductor.instance.songPositionInBeats, caller.startBeat + caller.timer + 2f, true);
                hasMissed = false;
                ActiveShuttle.GetComponent<Shuttlecock>().DoHit(e_BaBumState);

                if (e_BaBumState == DistanceSound.close)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Close");
                }
                if (e_BaBumState == DistanceSound.far)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Far");
                }
                if (e_BaBumState == DistanceSound.farther)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Farther");
                }
                if (e_BaBumState == DistanceSound.farthest)
                {
                    Jukebox.PlayOneShotGame("airRally/hitBaxter_Farthest");
                }
            }
            served = false;
        }

        public void RallyOnMiss(PlayerActionEvent caller)
        {
            ActiveShuttle.GetComponent<Shuttlecock>().DoThrough();
            served = false;
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

