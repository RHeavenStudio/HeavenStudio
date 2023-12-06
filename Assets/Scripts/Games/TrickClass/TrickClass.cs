using DG.Tweening;
using NaughtyBezierCurves;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class MobTrickLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("trickClass", "Trick on the Class", "ecede4", false, false, new List<GameAction>()
            {
                new GameAction("toss", "Paper Ball")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreTossObject(eventCaller.currentEntity.beat, (int)TrickClass.TrickObjType.Ball);
                    }, 
                    defaultLength = 2,
                },
                new GameAction("plane", "Plane")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreTossObject(eventCaller.currentEntity.beat, (int)TrickClass.TrickObjType.Plane);
                    },
                    defaultLength = 3,
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TrickClass.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the girl and boy bop?"),
                        new Param("autoBop", false, "Bop (Auto)", "Should the girl and boy auto bop?")
                    }
                },
            },
            new List<string>() {"mob", "normal"},
            "mobtrick", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /**
        mob_Trick
    **/

    using Scripts_TrickClass;
    public class TrickClass : Minigame
    {
        public enum TrickObjType {
            Ball,
            Plane,
        }
        public struct QueuedObject
        {
            public double beat;
            public int type;
        }
        public static List<QueuedObject> queuedInputs = new List<QueuedObject>();

        [Header("Objects")]
        public Animator playerAnim;
        public Animator girlAnim;
        public Animator warnAnim;

        [Header("References")]
        public GameObject ballPrefab;
        public GameObject planePrefab;
        public GameObject shockPrefab;
        public Transform objHolder;

        [Header("Curves")]
        public BezierCurve3D ballTossCurve;
        public BezierCurve3D ballMissCurve;
        public BezierCurve3D planeTossCurve;
        public BezierCurve3D planeMissCurve;
        public BezierCurve3D shockTossCurve;

        public static TrickClass instance;
        public GameEvent bop = new GameEvent();
        bool goBop = true;

        public double playerCanDodge = double.MinValue;
        double playerBopStart = double.MinValue;
        double girlBopStart = double.MinValue;
        bool showBubble = true;
        bool playerReady;

        public static PlayerInput.InputAction InputAction_TouchPressing =
            new("PcoTrickTouching", new int[] { IAEmptyCat, IAPressingCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPressing, IA_Empty);

        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Awake()
        {
            instance = this;
            SetupBopRegion("trickClass", "bop", "autoBop");
        }

        public override void OnBeatPulse(double beat)
        {
            var cond = Conductor.instance;
            if (!BeatIsInBopRegion(beat)) return;
            if ((!playerReady) && cond.songPositionInBeatsAsDouble > playerBopStart)
                playerAnim.DoScaledAnimationAsync("Bop");

            if (cond.songPositionInBeatsAsDouble > girlBopStart)
                girlAnim.DoScaledAnimationAsync("Bop");
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs)
                    {
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(input.beat - 1f, delegate
                            {
                                switch (input.type)
                                {
                                    case (int)TrickClass.TrickObjType.Ball:
                                        warnAnim.Play("WarnBall", 0, 0);
                                        break;
                                    case (int)TrickClass.TrickObjType.Plane:
                                        warnAnim.Play("WarnPlane", 0, 0);
                                        break;
                                }
                            }),
                            new BeatAction.Action(input.beat, delegate 
                            {
                                warnAnim.Play("NoPose", 0, 0);
                                TossObject(input.beat, input.type); 
                            })
                        });
                    }
                    queuedInputs.Clear();  
                }
            }

            if (PlayerInput.GetIsAction(InputAction_TouchPressing) && (!playerReady) && (playerCanDodge <= Conductor.instance.songPositionInBeatsAsDouble))
            {
                playerAnim.DoScaledAnimationAsync("Prepare");
                playerReady = true;
            }
            if ((!PlayerInput.GetIsAction(InputAction_TouchPressing)) && playerReady && (playerCanDodge <= Conductor.instance.songPositionInBeatsAsDouble))
            {
                playerAnim.DoScaledAnimationAsync("UnPrepare");
                playerReady = false;
            }

            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress) && (playerCanDodge <= Conductor.instance.songPositionInBeatsAsDouble))
            {
                PlayerDodge(true);
                playerCanDodge = Conductor.instance.songPositionInBeatsAsDouble + 0.6f;
            }
        }

        public void Bop(double beat, float length, bool shouldBop, bool autoBop)
        {
            var cond = Conductor.instance;
            goBop = autoBop;
            if (shouldBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            if ((!playerReady) && cond.songPositionInBeatsAsDouble > playerBopStart)
                                playerAnim.DoScaledAnimationAsync("Bop");

                            if (cond.songPositionInBeatsAsDouble > girlBopStart)
                                girlAnim.DoScaledAnimationAsync("Bop");
                        })
                    });
                }
            }
        }

        public void BubbleToggle()
        {
            instance.showBubble = !instance.showBubble;
        }
        
        public static void PreTossObject(double beat, int type)
        {
            if (GameManager.instance.currentGame == "trickClass")
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1, delegate 
                {
                    if (instance.showBubble == true)
                    {
                        switch (type)
                        {
                            case (int)TrickClass.TrickObjType.Ball:
                                instance.warnAnim.Play("WarnBall", 0, 0);
                                break;
                            case (int)TrickClass.TrickObjType.Plane:
                                instance.warnAnim.Play("WarnPlane", 0, 0);
                                break;
                        }
                    }
                }),
                    new BeatAction.Action(beat, delegate 
                    {
                        instance.warnAnim.Play("NoPose", 0, 0);
                        instance.TossObject(beat, type); 
                    })
                });
            }
            else
            {
                queuedInputs.Add(new QueuedObject
                {
                    beat = beat,
                    type = type,
                });
            }
        }

        public void TossObject(double beat, int type)
        {
            switch (type)
            {
                case (int) TrickObjType.Plane:
                    SoundByte.PlayOneShotGame("trickClass/girl_toss_plane");
                    break;
                default:
                    SoundByte.PlayOneShotGame("trickClass/girl_toss_ball");
                    break;
            }
            SpawnObject(beat, type);

            girlAnim.DoScaledAnimationAsync("Throw");
            girlBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
        }

        public void SpawnObject(double beat, int type)
        {
            GameObject objectToSpawn;
            BezierCurve3D curve;
            bool isPlane = false;
            switch (type)
            {
                case (int) TrickObjType.Plane:
                    objectToSpawn = planePrefab;
                    curve = planeTossCurve;
                    isPlane = true;
                    break;
                default:
                    objectToSpawn = ballPrefab;
                    curve = ballTossCurve;
                    break;
            }
            var mobj = GameObject.Instantiate(objectToSpawn, objHolder);
            var thinker = mobj.GetComponent<MobTrickObj>();

            thinker.startBeat = beat;
            thinker.flyType = isPlane;
            thinker.curve = curve;
            thinker.type = type;

            mobj.SetActive(true);
        }

        public void PlayerDodge(bool slow = false)
        {
            if (playerCanDodge > Conductor.instance.songPositionInBeatsAsDouble) return;

            //anim
            SoundByte.PlayOneShotGame("trickClass/player_dodge");
            playerAnim.DoScaledAnimationAsync("Dodge", slow ? 0.6f : 1f);
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerReady = false;
            
        }

        public void PlayerDodgeNg()
        {
            playerAnim.DoScaledAnimationAsync("DodgeNg");
            playerReady = false;
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeatsAsDouble + 0.15f;
        }

        public void PlayerThrough()
        {
            playerAnim.DoScaledAnimationAsync("Through");
            playerReady = false;
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeatsAsDouble + 0.15f;
        }
    }
}