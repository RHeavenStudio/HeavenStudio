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
        public static Minigame AddGame(EventCaller eventCaller)
        {
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
                new GameAction("chair", "Chair")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreTossObject(eventCaller.currentEntity.beat, (int)TrickClass.TrickObjType.Chair);
                    },
                    defaultLength = 2,
                },
                new GameAction("phone", "Phone")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreTossObject(eventCaller.currentEntity.beat, (int)TrickClass.TrickObjType.Phone, eventCaller.currentEntity["nx"]);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("nx", false, "Switch", "Replace the phone with a Switch"),
                    }
                },
                new GameAction("shock", "Lightning Bolt")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreTossObject(eventCaller.currentEntity.beat, (int)TrickClass.TrickObjType.Shock);
                    },
                    defaultLength = 2,
                },
                new GameAction("blast", "Optic Blast")
                {
                    preFunction = delegate
                    {
                        TrickClass.PreBlast(eventCaller.currentEntity.beat);
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
            new List<string>() { "mob", "normal" },
            "mobtrick", "en",
            new List<string>() { }
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
        public enum TrickObjType
        {
            Ball,
            Plane,
            Chair,
            Shock,
            Phone
        }
        public struct QueuedObject
        {
            public double beat;
            public int type;
            public bool variant;
        }
        public static List<QueuedObject> queuedInputs = new List<QueuedObject>();

        [Header("Objects")]
        public Animator playerAnim;
        public Animator girlAnim;
        public Animator warnAnim;

        [Header("References")]
        public GameObject[] objPrefab;
        public GameObject[] objPrefabVariant;
        public string[] objWarnAnim;
        public string[] objWarnAnimVariant;
        public string[] objThrowAnim;
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
                                warnAnim.Play(input.variant ? objWarnAnimVariant[input.type] : objWarnAnim[input.type], 0, 0);
                            }),
                            new BeatAction.Action(input.beat, delegate
                            {
                                warnAnim.Play("NoPose", 0, 0);
                                TossObject(input.beat, input.type, input.variant);
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

        public static void PreTossObject(double beat, int type, bool variant = false)
        {
            if (GameManager.instance.currentGame == "trickClass")
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - 1, delegate
                {
                    if (instance.showBubble == true)
                    {
                        instance.warnAnim.Play(variant ? instance.objWarnAnimVariant[type] : instance.objWarnAnim[type], 0, 0);
                    }
                }),
                    new BeatAction.Action(beat, delegate
                    {
                        instance.warnAnim.Play("NoPose", 0, 0);
                        instance.TossObject(beat, type, variant);
                    })
                });
            }
            else
            {
                queuedInputs.Add(new QueuedObject
                {
                    beat = beat,
                    type = type,
                    variant = variant
                });
            }
            switch (type)
            {
                case (int)TrickObjType.Plane:
                    PlaySoundSequence("trickClass", "planeThrow", beat);
                    break;
                case (int)TrickObjType.Chair:
                    PlaySoundSequence("trickClass", "chairThrow", beat);
                    break;
                case (int)TrickObjType.Shock:
                    PlaySoundSequence("trickClass", "shockThrow", beat);
                    break;
                case (int)TrickObjType.Phone:
                    PlaySoundSequence("trickClass", "phoneThrow", beat);
                    break;
                default:
                    PlaySoundSequence("trickClass", "ballThrow", beat);
                    break;
            }
        }

        public void TossObject(double beat, int type, bool variant = false)
        {
            SpawnObject(beat, type, variant);

            girlAnim.DoScaledAnimationAsync(objThrowAnim[type]);
            girlBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
        }

        public void SpawnObject(double beat, int type, bool variant = false)
        {
            GameObject objectToSpawn;
            BezierCurve3D curve;
            switch (type)
            {
                case (int)TrickObjType.Plane:
                    curve = planeTossCurve;
                    break;
                case (int)TrickObjType.Shock:
                    curve = shockTossCurve;
                    break;
                default:
                    curve = ballTossCurve;
                    break;
            }
            objectToSpawn = variant ? objPrefabVariant[type] : objPrefab[type];
            var mobj = Instantiate(objectToSpawn, objHolder);
            var thinker = mobj.GetComponent<MobTrickObj>();

            thinker.startBeat = beat;
            thinker.curve = curve;
            // thinker.type = type;

            mobj.SetActive(true);
        }

        public static void PreBlast(double beat)
        {
            PlaySoundSequence("trickClass", "girlCharge", beat);
        }

        public void PlayerDodge(bool slow = false, bool type = false)
        {
            if (playerCanDodge > Conductor.instance.songPositionInBeatsAsDouble) return;

            //anim
            SoundByte.PlayOneShotGame("trickClass/player_dodge");
            playerAnim.DoScaledAnimationAsync(type ? "DodgeAlt" : "Dodge", slow ? 0.6f : 1f);
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerReady = false;
        }

        public void PlayerDodgeNg(bool shock = false)
        {
            playerAnim.DoScaledAnimationAsync(shock ? "DodgeNgShock" : "DodgeNg");
            playerReady = false;
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeatsAsDouble + 0.15f;
        }

        public void PlayerThrough(bool shock = false)
        {
            playerAnim.DoScaledAnimationAsync(shock ? "ThroughShock" : "Through");
            playerReady = false;
            playerBopStart = Conductor.instance.songPositionInBeatsAsDouble + 0.75f;
            playerCanDodge = Conductor.instance.songPositionInBeatsAsDouble + 0.15f;
        }
    }
}