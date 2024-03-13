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
                    preFunction = delegate { var e = eventCaller.currentEntity; ShootEmUp.PreInterval(e.beat, e.length, e["placement"]); }, 
                    defaultLength = 4f, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("placement", ShootEmUp.PlacementType.PatternA, "Placement Pattern")
                    },
                },
                new GameAction("spawn enemy", "Spawn Enemy")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("x", new EntityTypes.Float(-4, 4, 0), "X"),
                        new Param("y", new EntityTypes.Float(-3, 3, 0), "Y"),
                    },
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
        public Animator shipAnim;
        public Animator damageAnim;

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
        }

        
        void Update()
        {
            Debug.Log("?");
            Debug.Log(GameCamera.AdditionalPosition);
            Debug.Log(GameCamera.AdditionalFoV);
            GameCamera.AdditionalPosition = cameraPos.position;
            // Debug.Log(GameCamera.AdditionalPosition);
            // GameCamera.AdditionalPosition = new Vector3(-10,-10,10);
            // Debug.Log(GameCamera.AdditionalPosition);
        }

        public override void OnGameSwitch(double beat)
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedIntervals.Count > 0)
                {
                    foreach (var interval in queuedIntervals)
                    {
                        SetIntervalStart(interval.beat, beat, interval.interval, interval.placement);
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

        public void SpawnEnemy(double beat, float x, float y, bool active = true)
        {
            var newEnemy = Instantiate(baseEnemy, enemyHolder).GetComponent<Enemy>();
            spawnedEnemies.Add(newEnemy);
            newEnemy.createBeat = beat;
            newEnemy.gameObject.transform.localPosition = new Vector3(5.05f/3*x, 2.5f/3*y + 1.25f, 0);
            
            Vector3 angle = new Vector3(0, 0, 0);
            if (x > 0 && y > 0) {
                angle = new Vector3(0, 0, -70);
            } else if (x < 0 && y > 0) {
                angle = new Vector3(0, 0, 70);
            } else if (x > 0 && y < 0) {
                angle = new Vector3(0, 0, -110);
            } else if (x < 0 && y < 0) {
                angle = new Vector3(0, 0, 110);
            }
            newEnemy.SpawnEffect.eulerAngles = angle;

            if (active)
            {
                SoundByte.PlayOneShotGame("shootEmUp/spawn", beat);
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate
                    {
                        newEnemy.gameObject.SetActive(true);
                        newEnemy.GetComponent<Animator>().Play("enemySpawn", 0, 0);
                    })
                });
            }
            else
            {
                newEnemy.gameObject.SetActive(true);
            }
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public int placement;
        }
        private static List<QueuedInterval> queuedIntervals = new List<QueuedInterval>();

        private void SetIntervalStart(double beat, double gameSwitchBeat, float interval = 4f, int placement = -1)
        {
            CallAndResponseHandler newHandler = new();
            crHandlerInstance = newHandler;
            crHandlerInstance.StartInterval(beat, interval);
            var relevantInputs = GetAllInputsBetweenBeat(beat, beat + interval);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));
            
            if (placement >= 0 && placement < (int)PlacementType.Manual)
            {
                PatternItem plcPattern = PlacementPattern[Math.Min(placement, PlacementPattern.Length - 1)];

                int relevantInputsCount = relevantInputs.Count;
                int posPatternLength = plcPattern.posPattern.Length;
                for (int i = 0; i < relevantInputsCount; i++)
                {
                    var evt = relevantInputs[i];
                    crHandlerInstance.AddEvent(evt.beat);
                    
                    int relevantIndex = Math.Min(relevantInputsCount - 1, posPatternLength - 1);
                    var posData = plcPattern.posPattern[relevantIndex].posData;
                    int posDataIndex = Math.Min(posData.Length - 1, i);
                    var pos = posData[posDataIndex];

                    SpawnEnemy(evt.beat, pos.x, pos.y, evt.beat >= gameSwitchBeat);
                }
            }
            else
            {
                foreach (var evt in relevantInputs)
                {
                    crHandlerInstance.AddEvent(evt.beat);
                    SpawnEnemy(evt.beat, evt["x"], evt["y"], evt.beat >= gameSwitchBeat);
                }
            }


            PassTurn(beat + interval, interval, newHandler);
        }

        public static void PreInterval(double beat, float interval = 4f, int placement = -1)
        {
            if (GameManager.instance.currentGame == "shootEmUp")
            {
                instance.SetIntervalStart(beat, beat, interval, placement);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    placement = placement,
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

        public void Damage()
        {
            shipAnim.Play("shipDamage");
            damageAnim.Play("damage");
        }
    }
}