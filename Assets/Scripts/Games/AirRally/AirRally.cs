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
            return new Minigame("airRally", "Air Rally", "008c97", false, false, new List<GameAction>()
            {
                new GameAction("set distance", "Set Distance")
                {
                    function = delegate { AirRally.instance.SetDistance(e.currentEntity["type"]); }, 
                    defaultLength = .5f, 
                    parameters = new List<Param>()
                    {
                        new Param("type", AirRally.DistanceSound.close, "Type", "How far is Forthington?")
                    }
                },
                //new GameAction("start rally",                    delegate { AirRally.instance.StartRally(true); }, .5f, false),
                new GameAction("rally", "Rally")
                {
                    function = delegate { AirRally.instance.Rally(e.currentEntity.beat, e.currentEntity["toggle"], e.currentEntity.length); }, 
                    defaultLength = 2f, 
                    resizable = true, 
                    parameters = new List<Param>()
                    { 
                        new Param("toggle", false, "Silent", "Make Forthington Silent"),
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
                    function = delegate { AirRally.instance.ForthVoice(e.currentEntity["type"], e.currentEntity["type2"]); }, 
                    parameters = new List<Param>()
                    { 
                        new Param("type", AirRally.CountSound.one, "Type", "The number Forthington will say"),
                        new Param("type2", AirRally.DistanceSound.close, "Type", "How far is Forthington?")
                    }
                }
            });
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

        [Header("Waypoint")]
        public float wayPointZForForth;

        [Header("Curves")]
        public BezierCurve3D closeRallyCurve;
        public BezierCurve3D farRallyCurve;
        public BezierCurve3D fartherRallyCurve;
        public BezierCurve3D farthestRallyCurve;
        public BezierCurve3D closeRallyReturnCurve;

        [Header("Debug")]
        public float beatShown;
        public float lengthHolder;
        public float lengthShown;
        public int wantDistance;
        public bool wantSilent;
        public float beatHolder;
        public Transform holderPos;

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
                    //convert to 2 decimal places
                    var f = currentBeat;
                    //f = Mathf.Round(f * 10.0f) * 0.1f;
                    Rally(serveBeat + (int)f, wantSilent, lengthHolder);
                    //Debug.Log("Beat Loop: " + serveBeat + f);
                    //Debug.Log("Serve Beat: " + serveBeat);
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

        public void SpawnObject(float beat, int type)
        {
            BezierCurve3D curve = null;

            switch (type)
            {
                case (int)DistanceSound.close:
                    curve = closeRallyCurve;
                    break;
                case (int)DistanceSound.far:
                    curve = farRallyCurve;
                    break;
                case (int)DistanceSound.farther:
                    curve = fartherRallyCurve;
                    break;
                case (int)DistanceSound.farthest:
                    curve = farthestRallyCurve;
                    break;
            }

            //curve.KeyPoints[0].transform.position = new Vector3(holderPos.position.x, holderPos.position.y, wayPointZForForth);

            if (!shuttleActive)
            {
                ActiveShuttle = GameObject.Instantiate(Shuttlecock, objHolder.transform);
                ActiveShuttle.AddComponent<Shuttlecock>();
            }
            
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = false;
            shuttleScript.startBeat = (int)Conductor.instance.songPositionInBeats;
            shuttleScript.curve = curve;
            //float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(beat, 3f);

            //ActiveShuttle.transform.position = curve.GetPoint(normalizedBeatAnim);
            ActiveShuttle.transform.rotation = Quaternion.Euler(0, 0, 0);
            ActiveShuttle.SetActive(true);
            shuttleActive = true;

        }

        public void ReturnObject(float beat, int type)
        {
            
            BezierCurve3D curve = null;
            switch (type)
            {
                case (int)DistanceSound.close:
                    curve = closeRallyCurve;
                    break;
                case (int)DistanceSound.far:
                    curve = farRallyCurve;
                    break;
                case (int)DistanceSound.farther:
                    curve = fartherRallyCurve;
                    break;
                case (int)DistanceSound.farthest:
                    curve = farthestRallyCurve;
                    break;
            }

            ActiveShuttle.transform.localPosition = curve.KeyPoints[0].transform.position;
            var shuttleScript = ActiveShuttle.GetComponent<Shuttlecock>();
            shuttleScript.flyPos = 0f;
            shuttleScript.isReturning = true;
            shuttleScript.startBeat = beat;
            //shuttleScript.curve = closeRallyReturnCurve;
            //float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(beat, 3f);

            //ActiveShuttle.transform.position = curve.GetPoint(normalizedBeatAnim);
            ActiveShuttle.transform.rotation = Quaternion.Euler(0, 0, 0);

            
        }

        //change to something more optimized
        public void ForthVoice(int type, int type2)
        {
            Forthington.GetComponent<Animator>().Play("TalkShort");
            if (type == 0)
            {
                if(type2 == 0)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn1");
                }
                if(type2 == 1)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn1Far");
                }
                if(type2 == 2)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn1Farther");
                }
                if(type2 == 3)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn1Farthest");
                }
            }
            if(type == 1)
            {
                if(type2 == 0)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn2");
                }
                if(type2 == 1)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn2Far");
                }
                if(type2 == 2)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn2Farther");
                }
                if(type2 == 3)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn2Farthest");
                }
            }
            if(type == 2)
            {
                if(type2 == 0)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn3");
                }
                if(type2 == 1)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn3Far");
                }
                if(type2 == 2)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn3Farther");
                }
                if(type2 == 3)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn3Farthest");
                }
            }
            if(type == 3)
            {
                if(type2 == 0)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn4");
                }
                if(type2 == 1)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn4Far");
                }
                if(type2 == 2)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn4Farther");
                }
                if(type2 == 3)
                {
                    Jukebox.PlayOneShotGame("airRally/countIn4Farthest");
                }
            }
        }

        public void SetDistance(int type)
        {
            switch (type)
            {
                case 0:
                    e_BaBumState = DistanceSound.close;
                    break;
                case 1:
                    e_BaBumState = DistanceSound.far;
                    break;
                case 2:
                    e_BaBumState = DistanceSound.farther;
                    break;
                case 3:
                    e_BaBumState = DistanceSound.farthest;
                    break;     
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
                    //new BeatAction.Action(beat, delegate { Forthington.GetComponent<Animator>().Play("Ready");} ),
                    new BeatAction.Action(beat, delegate { Forthington.GetComponent<Animator>().Play("Hit");} ),
                    //new BeatAction.Action(beat, delegate { SpawnObject(beat, (int)e_BaBumState); } ),
                });

                switch (e_BaBumState)
                {
                    case DistanceSound.close:
                        wayPointZForForth = 3.55f;
                        
                        tweenForForth = Forthington.gameObject.transform.DOMoveZ(wayPointZForForth, .15f);
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("CloseReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Close"); }),
                            new BeatAction.Action(beat, delegate { if(!silent) { Jukebox.PlayOneShotGame("airRally/nya_Close"); } }),
                            new BeatAction.Action(beat + .25f, delegate { Jukebox.PlayOneShotGame("airRally/whooshForth_Close");}),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.far:
                        wayPointZForForth = 35.16f;

                        tweenForForth = Forthington.gameObject.transform.DOMoveZ(wayPointZForForth, .15f);
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Far"); }),
                            new BeatAction.Action(beat, delegate { if(!silent) { Jukebox.PlayOneShotGame("airRally/nya_Far"); } }),
                            new BeatAction.Action(beat + .25f, delegate { Jukebox.PlayOneShotGame("airRally/whooshForth_Far");}),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.farther:
                        wayPointZForForth = 105.16f;

                        tweenForForth = Forthington.gameObject.transform.DOMoveZ(wayPointZForForth, .15f);
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Farther"); }),
                            new BeatAction.Action(beat, delegate { if(!silent) { Jukebox.PlayOneShotGame("airRally/nya_Farther"); } }),
                            new BeatAction.Action(beat + .25f, delegate { Jukebox.PlayOneShotGame("airRally/whooshForth_Farther");}),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;

                    case DistanceSound.farthest:
                        wayPointZForForth = 255.16f;

                        tweenForForth = Forthington.gameObject.transform.DOMoveZ(wayPointZForForth, .15f);
                        BeatAction.New(gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat, delegate { Baxter.GetComponent<Animator>().Play("FarReady"); }),
                            new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("airRally/hitForth_Farthest"); }),
                            new BeatAction.Action(beat, delegate { if(!silent) { Jukebox.PlayOneShotGame("airRally/nya_Farthest"); } }),
                            new BeatAction.Action(beat + .25f, delegate { Jukebox.PlayOneShotGame("airRally/whooshForth_Farthest");}),
                            new BeatAction.Action(beat + 1f, delegate { if(!babum) { Forthington.GetComponent<Animator>().Play("Ready"); } }),
                        });
                        break;
                }
                //SpawnObject(beat, (int)e_BaBumState);
                ScheduleInput(beat, 1f, InputType.STANDARD_DOWN, RallyOnHit, RallyOnMiss, RallyEmpty);

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

            //if (!ActiveShuttle)
            //{
            //    SpawnObject(beat, (int)e_BaBumState);
            //}

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
                new BeatAction.Action(beat + 1.5f, delegate { Forthington.GetComponent<Animator>().Play("Ready"); }),
                new BeatAction.Action(beat + 2.5f, delegate { if(babum) { Forthington.GetComponent<Animator>().Play("Hit"); } }),
                new BeatAction.Action(beat + 3.5f, delegate { Forthington.GetComponent<Animator>().Play("TalkShort"); }),
                new BeatAction.Action(beat + 4f, delegate { if(!count) Forthington.GetComponent<Animator>().Play("TalkShort"); }),
                new BeatAction.Action(beat + 4.5f, delegate { Forthington.GetComponent<Animator>().Play("Ready"); }),
                new BeatAction.Action(beat + 7f, delegate { if(babum) { babum = false; } }),
            });

            BeatAction.New(gameObject, new List<BeatAction.Action>() //MultiSound.Sound sounds weird
            {
                new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame(sounds[0]); }),
                new BeatAction.Action(beat + .5f, delegate { Jukebox.PlayOneShotGame(sounds[1]); }),
                new BeatAction.Action(beat + 1.5f, delegate { Jukebox.PlayOneShotGame(sounds[2]); }),
                new BeatAction.Action(beat + 2.5f, delegate { Jukebox.PlayOneShotGame(sounds[3]); }),
                new BeatAction.Action(beat + 2.5f, delegate { Jukebox.PlayOneShotGame(sounds[4]); }),
                new BeatAction.Action(beat + 3f, delegate { Jukebox.PlayOneShotGame(sounds[5]); }),
                new BeatAction.Action(beat + 3.5f, delegate { if(e_BaBumState == DistanceSound.far) { Jukebox.PlayOneShotGame("airRally/whooshForth_Far2"); } }),
            });

            if (count)
            {
                
                var sound2 = new MultiSound.Sound[]
                {
                    new MultiSound.Sound(sounds2[0], beat + 3.5f),
                    new MultiSound.Sound(sounds2[1], beat + 4.3f),
                    new MultiSound.Sound(sounds2[2], beat + 5.4f)
                };

                MultiSound.Play(sound2);
            }

            ScheduleInput(beat, 4.5f, InputType.STANDARD_DOWN, RallyOnHit, RallyOnMiss, RallyEmpty);
        }

        public void RallyOnHit(PlayerActionEvent caller, float beat)
        {
            Baxter.GetComponent<Animator>().Play("Hit");
            hasMissed = false;

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
           
            if (beatHolder % 2 == 0)
            {
                //ReturnObject(beatHolder + 1, 0);
                //Debug.Log("a");
            }
            else
            {
                //ReturnObject(beatHolder + 1, 0);
                //Debug.Log("b");

            }

            //Debug.Log("BeatHolder: " + beatHolder);
            served = false;
        }

        public void RallyOnMiss(PlayerActionEvent caller)
        {
            served = false;
            hasMissed = true;
            shuttleActive = false;
        }

        public void RallyEmpty(PlayerActionEvent caller)
        {
            //empty
        }
    }
}

