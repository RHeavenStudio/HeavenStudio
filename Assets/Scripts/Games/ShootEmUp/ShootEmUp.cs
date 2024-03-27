using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrShootEmUpLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("shootEmUp", "Shoot-'Em-Up", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; ShootEmUp.instance.ToggleBop(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("toggle2", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; ShootEmUp.PreInterval(e.beat, e.length, e["placement"], e["auto"]); }, 
                    defaultLength = 4f, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("placement", ShootEmUp.PlacementType.PatternA, "Placement Pattern"),
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                    },
                },
                new GameAction("spawn enemy", "Spawn Enemy")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        
                        new Param("fine", false, "Fine placement", "Change placement by decimals.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "x_int", "y_int" }),
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "x_float", "y_float" }),
                        }),
                        new Param("x_int", new EntityTypes.Integer(-3, 3, 0), "X (Integer)"),
                        new Param("y_int", new EntityTypes.Integer(-3, 3, 0), "Y (Integer)"),
                        new Param("x_float", new EntityTypes.Float(-8, 8, 0), "X (Decimal)"),
                        new Param("y_float", new EntityTypes.Float(-8, 8, 0), "Y (Decimal)"),
                        new Param("type", ShootEmUp.EnemyType.Basic, "Enemy", "Choose the enemy to spawn."),
                    },
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; ShootEmUp.PrePassTurn(e.beat); },
                },
                new GameAction("gate events", "Gate Animations")
                {
                    function = delegate { var e = eventCaller.currentEntity; ShootEmUp.instance.GateAnims(e.beat, e.length, e["mute"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Toggle if the cue should be muted."),
                    }
                },
                new GameAction("monitor events", "Monitor Animations")
                {
                    function = delegate { var e = eventCaller.currentEntity; ShootEmUp.instance.MonitorAnims(e.beat, e.length, e["toggle"], e["mute"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", ShootEmUp.MonitorAnimation.Enter, "Animation", "Set the animation for the monitor to perform."),
                        new Param("mute", false, "Mute", "Toggle if the cue should be muted."),
                    }
                },
            },
            new List<string>() { "ntr", "normal" }, "ntrShootEmUp", "en", new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ShootEmUp;
    public class ShootEmUp : Minigame
    {
        [Header("Camera")]
        [SerializeField] Transform cameraPos;

        [Header("References")]
        public GameObject baseEnemy;
        public Transform enemyHolder;
        public Ship playerShip;
        public ParticleSystem hitEffect;
        public Animator introGate;
        public Animator monitor;
        public Animator captain;

        public float scaleSpeed;

        private List<Enemy> spawnedEnemies = new List<Enemy>();

        public enum PlacementType
        {
            PatternA = 0,
            PatternB,
            PatternC,
            Manual,
        }

        public enum EnemyType
        {
            Basic = 0,
            Practice,
            Endless,
            Lockstep = 100,
        }

        [System.Serializable]
        public struct PatternItem
        {
            public PosPatternItem[] posPattern;

            [System.Serializable]
            public struct PosPatternItem
            {
                public Vector2[] posData;
            }
        }

        [SerializeField] PatternItem[] PlacementPattern;

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        public static PlayerInput.InputAction InputAction_Press =
            new("NtrShootPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);

        public static ShootEmUp instance;
        private static CallAndResponseHandler crHandlerInstance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            SetupBopRegion("shootEmUp", "bop", "toggle");
            if (crHandlerInstance != null && crHandlerInstance.queuedEvents.Count > 0)
            {
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    SpawnEnemy(crEvent.beat, crEvent.DynamicData["pos"], (int)Enum.Parse(typeof(EnemyType), crEvent.tag), false, crHandlerInstance.intervalLength, true);
                }
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop();
        }

        
        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }

            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press))
            {
                if (!playerShip.isDamage)
                {
                    SoundByte.PlayOneShotGame("shootEmUp/16");
                    playerShip.Shoot();
                }
            }
            GameCamera.AdditionalPosition = cameraPos.position;
        }

        public override void OnGameSwitch(double beat)
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedIntervals.Count > 0)
                {
                    foreach (var interval in queuedIntervals)
                    {
                        SetIntervalStart(interval.beat, beat, interval.interval, interval.placement, interval.autoPassTurn);
                    }
                    queuedIntervals.Clear();
                }
            }
            GateClose(beat);
        }

        public override void OnPlay(double beat)
        {
            crHandlerInstance = null;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            queuedIntervals.Clear();
            GateClose(beat);
        }

        private void OnDestroy()
        {
            if (!Conductor.instance.isPlaying)
            {
                crHandlerInstance = null;
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public void SpawnEnemy(double beat, Vector2 pos, int type,
                               bool active = true, float interval = 4f, bool awake = false)
        {
            if (!awake)
            {
                if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
                crHandlerInstance.AddEvent(beat, tag: Enum.GetName(typeof(EnemyType), type), crParams: new(){
                    new CallAndResponseHandler.CallAndResponseEventParam("pos", pos),
                });
            }

            var newEnemy = Instantiate(baseEnemy, enemyHolder).GetComponent<Enemy>();
            spawnedEnemies.Add(newEnemy);
            newEnemy.createBeat = beat;
            newEnemy.type = type;
            newEnemy.pos = pos;
            newEnemy.scaleSpeed = scaleSpeed/interval;

            if (active)
            {
                SoundByte.PlayOneShotGame("shootEmUp/spawn", beat);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        newEnemy.gameObject.SetActive(true);
                        newEnemy.Init();
                        newEnemy.SpawnAnim();
                    })
                });
            }
            else
            {
                newEnemy.gameObject.SetActive(true);
                newEnemy.Init();
            }
        }

        private static List<double> passedTurns = new();
        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public int placement;
            public bool autoPassTurn;
        }
        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();

        private void SetIntervalStart(double beat, double gameSwitchBeat, float interval = 4f, int placement = -1, bool autoPassTurn = true)
        {
            CallAndResponseHandler newHandler = new();
            crHandlerInstance = newHandler;
            crHandlerInstance.StartInterval(beat, interval);
            var relevantInputs = GetAllInputsBetweenBeat(beat, beat + interval);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));
            
            if (placement >= 0 && placement < (int)PlacementType.Manual)
            {
                PatternItem plcPattern = PlacementPattern[Mathf.Min(placement, PlacementPattern.Length - 1)];

                int relevantInputsCount = relevantInputs.Count;
                int posPatternLength = plcPattern.posPattern.Length;
                for (int i = 0; i < relevantInputsCount; i++)
                {
                    var evt = relevantInputs[i];
                    
                    int relevantIndex = Mathf.Min(relevantInputsCount - 1, posPatternLength - 1);
                    var posData = plcPattern.posPattern[relevantIndex].posData;
                    int posDataIndex = Mathf.Min(posData.Length - 1, i);
                    var pos = posData[posDataIndex];
                    SpawnEnemy(evt.beat, pos, evt["type"], evt.beat >= gameSwitchBeat, interval);
                }
            }
            else
            {
                foreach (var evt in relevantInputs)
                {
                    Vector2 pos = new Vector2(evt["x_int"], evt["y_int"]);
                    if (evt["fine"]) pos = new Vector2(evt["x_float"], evt["y_float"]);
                    SpawnEnemy(evt.beat, pos, evt["type"], evt.beat >= gameSwitchBeat, interval);
                }
            }
            if (autoPassTurn)
            {
                PassTurn(beat + interval, interval, newHandler);
            }
        }

        public static void PreInterval(double beat, float interval = 4f, int placement = -1, bool autoPassTurn = true)
        {
            if (GameManager.instance.currentGame == "shootEmUp")
            {
                instance.SetIntervalStart(beat, beat, interval, placement, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    placement = placement,
                    autoPassTurn = autoPassTurn,
                });
            }
        }

        private List<RiqEntity> GetAllInputsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("shootEmUp", new string[] { "spawn enemy" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        private void PassTurnStandalone(double beat)
        {
            if (crHandlerInstance != null) PassTurn(beat, crHandlerInstance.intervalLength, crHandlerInstance);
        }

        private void PassTurn(double beat, double length, CallAndResponseHandler crHandler)
        {
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate
                {
                    if (crHandler.queuedEvents.Count > 0)
                    {
                        foreach (var crEvent in crHandler.queuedEvents)
                        {
                            var enemyToInput = spawnedEnemies.Find(x => x.createBeat == crEvent.beat);
                            enemyToInput.StartInput(beat, crEvent.relativeBeat);
                        }
                        crHandler.queuedEvents.Clear();
                    }

                }),
            });
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "shootEmUp")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        public enum MonitorAnimation
        {
            Enter,
            Exit,
            Talk,
            Idle,
            Bop,
        }
        private bool canBop = false;

        public void MonitorAnims(double beat, double length, int type, bool mute)
        {
            canBop = false;
            switch (type) {
                case (int)MonitorAnimation.Enter:
                    captain.Play("capHidden");
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate {
                            monitor.DoScaledAnimationAsync("monitorIn", 1f);
                        }),
                        new BeatAction.Action(beat + length, delegate {
                            captain.DoScaledAnimationAsync("capShow", 1f);
                        }),
                    });
                    if (!mute) SoundByte.PlayOneShotGame("shootEmUp/commStart", beat + length);
                    break;
                case (int)MonitorAnimation.Exit:
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate {
                            captain.DoScaledAnimationAsync("capHide", 1f);
                        }),
                        new BeatAction.Action(beat + length, delegate {
                            monitor.DoScaledAnimationAsync("monitorOut", 1f);
                        }),
                    });
                    if (!mute) SoundByte.PlayOneShotGame("shootEmUp/commEnd", beat);
                    break;
                case (int)MonitorAnimation.Talk:
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate {
                            captain.SetBool("isTalk", true);
                            captain.DoScaledAnimationAsync("capTalk", 1f);
                        }),
                        new BeatAction.Action(beat + length, delegate {
                            captain.SetBool("isTalk", false);
                        }),
                    });

                    break;
                case (int)MonitorAnimation.Idle:
                    monitor.DoScaledAnimationAsync("monitorIdle", 1f);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat, delegate { captain.Play("capIdle");}),
                    });
                    break;
                case (int)MonitorAnimation.Bop:
                    canBop = true;
                    captain.DoScaledAnimationAsync("capBop", 1f);
                    break;
            }
        }

        public void GateAnims(double beat, double length, bool mute)
        {
            if (!mute)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("shootEmUp/gate1", beat),
                    new MultiSound.Sound("shootEmUp/gate2", beat + length),
                    new MultiSound.Sound("shootEmUp/gate3", beat + 2*length),
                });
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate {
                    introGate.DoScaledAnimationAsync("gateOpen1", 1f);
                }),
                new BeatAction.Action(beat + length, delegate {
                    introGate.DoScaledAnimationAsync("gateOpen2", 1f);
                }),
                new BeatAction.Action(beat + 2*length, delegate {
                    introGate.DoScaledAnimationAsync("gateOpen3", 1f);
                }),
            });
        }

        private void GateClose(double beat)
        {
            double endBeat = double.MaxValue;
            var firstEnd = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).Find(x => x.beat > beat);
            endBeat = firstEnd?.beat ?? endBeat;
            if (EventCaller.GetAllInGameManagerList("shootEmUp", new string[] { "gate events" }).Find(x => x.beat >= beat && x.beat <= endBeat) is not null)
            {
                introGate.Play("gateShow", 0, 0);
            }
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new() {new BeatAction.Action(beat + i, delegate {Bop();}) });
                }
            }
        }

        public void Bop()
        {
            if (canBop) captain.DoScaledAnimationAsync("capBop", 1f);
        }
    }
}