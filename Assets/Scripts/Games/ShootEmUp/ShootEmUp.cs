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
                        new Param("x", new EntityTypes.Float(-5, 5, 0), "X"),
                        new Param("y", new EntityTypes.Float(-5, 5, 0), "Y"),
                    },
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; ShootEmUp.PrePassTurn(e.beat); },
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

        public float scaleSpeed;

        private List<Enemy> spawnedEnemies = new List<Enemy>();

        public enum PlacementType
        {
            PatternA = 0,
            PatternB,
            PatternC,
            Manual,
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
            if (crHandlerInstance != null && crHandlerInstance.queuedEvents.Count > 0)
            {
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    SpawnEnemy(crEvent.beat, crEvent.DynamicData["pos"], false, crHandlerInstance.intervalLength);
                }
            }
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
        }

        public override void OnPlay(double beat)
        {
            crHandlerInstance = null;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            queuedIntervals.Clear();
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

        public void SpawnEnemy(double beat, Vector2 pos, bool active = true, float interval = 4f)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;

            crHandlerInstance.AddEvent(beat, crParams: new(){
                new CallAndResponseHandler.CallAndResponseEventParam("pos", pos),
            });

            var newEnemy = Instantiate(baseEnemy, enemyHolder).GetComponent<Enemy>();
            spawnedEnemies.Add(newEnemy);
            newEnemy.createBeat = beat;
            newEnemy.scaleSpeed = scaleSpeed/interval;
            newEnemy.pos = pos;
            newEnemy.Init();

            if (active)
            {
                SoundByte.PlayOneShotGame("shootEmUp/spawn", beat);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        newEnemy.gameObject.SetActive(true);
                        newEnemy.SpawnAnim();
                    })
                });
            }
            else
            {
                newEnemy.gameObject.SetActive(true);
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
                    SpawnEnemy(evt.beat, pos, evt.beat >= gameSwitchBeat, interval);
                }
            }
            else
            {
                foreach (var evt in relevantInputs)
                {
                    var pos = new Vector2(evt["x"], evt["y"]);
                    SpawnEnemy(evt.beat, pos, evt.beat >= gameSwitchBeat, interval);
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
            if (GameManager.instance.currentGame == "tambourine")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }
    }
}