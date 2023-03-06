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
            return new Minigame("trickClass", "Trick on the Class", "C0171D", false, false, new List<GameAction>()
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
                new GameAction("toggleBubble", "Toggle Speech Bubble")
                {
                    function = delegate { TrickClass.instance.BubbleToggle(); },
                    defaultLength = 1,
                },
                new GameAction("bop", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; TrickClass.instance.Bop(e.beat, e.length); },
                    resizable = true, 
                    hidden = true
                }
            });
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
            public float beat;
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

        public float playerCanDodge = Single.MinValue;
        float playerBopStart = Single.MinValue;
        float girlBopStart = Single.MinValue;
        bool showBubble = true;


        void OnDestroy()
        {
            if (queuedInputs.Count > 0) queuedInputs.Clear();
        }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (cond.songPositionInBeats > playerBopStart)
                    playerAnim.DoScaledAnimationAsync("Bop");

                if (cond.songPositionInBeats > girlBopStart)
                    girlAnim.DoScaledAnimationAsync("Bop");
            }

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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

            if (PlayerInput.Pressed() && !IsExpectingInputNow() && (playerCanDodge <= Conductor.instance.songPositionInBeats))
            {
                PlayerDodge(true);
                playerCanDodge = Conductor.instance.songPositionInBeats + 0.6f;
            }
        }

        public void Bop(float beat, float length)
        {
            bop.startBeat = beat;
            bop.length = length;
        }

        public void BubbleToggle()
        {
            instance.showBubble = !instance.showBubble;
        }
        
                public static void PreTossObject(float beat, int type)
        {
            if (GameManager.instance.currentGame == "trickClass")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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

        public void TossObject(float beat, int type)
        {
            switch (type)
            {
                case (int) TrickObjType.Plane:
                    Jukebox.PlayOneShotGame("trickClass/girl_toss_plane");
                    break;
                default:
                    Jukebox.PlayOneShotGame("trickClass/girl_toss_ball");
                    break;
            }
            SpawnObject(beat, type);

            girlAnim.DoScaledAnimationAsync("Throw");
            girlBopStart = Conductor.instance.songPositionInBeats + 0.75f;
        }

        public void SpawnObject(float beat, int type)
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
            if (playerCanDodge > Conductor.instance.songPositionInBeats) return;

            //anim
            Jukebox.PlayOneShotGame("trickClass/player_dodge");
            playerAnim.DoScaledAnimationAsync("Dodge", slow ? 0.6f : 1f);
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            
        }

        public void PlayerDodgeNg()
        {
            playerAnim.DoScaledAnimationAsync("DodgeNg");
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeats + 0.15f;
        }

        public void PlayerThrough()
        {
            playerAnim.DoScaledAnimationAsync("Through");
            playerBopStart = Conductor.instance.songPositionInBeats + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeats + 0.15f;
        }
    }
}