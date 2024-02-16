using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbWaltzLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("wizardsWaltz", "Wizard's Waltz \n<color=#adadad>(Mahou Tsukai)</color>", "ffef9c", false, false, new List<GameAction>()
            {
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WizardsWaltz.PreInterval(e.beat, e.length, e["auto"]); }, 
                    defaultLength = 6f, 
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    },
                    preFunctionLength = 1
                },
                new GameAction("plant", "Plant")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; WizardsWaltz.PrePassTurn(e.beat); },
                    preFunctionLength = 1
                }
            },
            new List<string>() {"agb", "repeat"},
            "agbwizard", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_WizardsWaltz;

    public class WizardsWaltz : Minigame
    {
        [Header("References")]
        public Wizard wizard;
        public Girl girl;
        public GameObject plantHolder;
        public GameObject plantBase;

        [Header("Properties")]
        [NonSerialized] public float beatInterval = 6f;
        [NonSerialized] public double intervalStartBeat;
        public double wizardBeatOffset = 0f;
        public float xRange = 5;
        public float zRange = 5;
        public float yRange = 0.5f;
        public float plantYOffset = -2f;
        [NonSerialized] public List<Plant> currentPlants = new();

        public static WizardsWaltz instance;

        private static CallAndResponseHandler crHandlerInstance;

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        public static PlayerInput.InputAction InputAction_Press =
            new("AgbWizardPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);

        private void Awake()
        {
            instance = this;
            wizard.Init();
        }

        public override void OnPlay(double beat)
        {
            SetWizardOffset(beat);
            crHandlerInstance = null;
            queuedIntervals.Clear();
            passedTurns.Clear();
        }

        public override void OnGameSwitch(double beat)
        {
            SetWizardOffset(beat);
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in  queuedIntervals)
                {
                    SetIntervalStart(interval.beat, interval.interval, beat, interval.autoPassTurn);
                }
                queuedIntervals.Clear();
            }
            if (crHandlerInstance != null && crHandlerInstance.queuedEvents.Count > 0)
            {
                if (crHandlerInstance.queuedEvents.Find(x => x.beat >= beat) != null)
                {
                    crHandlerInstance = null;
                    return;
                }
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    SpawnFlower(crEvent.beat, true);
                }
            }
        }

        private void SetWizardOffset(double beat)
        {
            var allIntervals = EventCaller.GetAllInGameManagerList("wizardsWaltz", new string[] { "start interval" });
            var tempEvents = allIntervals.FindAll(x => x.beat >= beat);
            if (tempEvents.Count > 0)
            {
                wizardBeatOffset = tempEvents[0].beat;
                beatInterval = tempEvents[0].length;
            }
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (passedTurns.Count > 0)
                {
                    foreach (var pass in passedTurns)
                    {
                        PassTurnStandalone(pass);
                    }
                    passedTurns.Clear();
                }
            }
        }

        private List<RiqEntity> GetAllPlantsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("wizardsWaltz", new string[] { "plant" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        public void SetIntervalStart(double beat, float interval, double gameSwitchBeat, bool autoPassTurn)
        {
            wizardBeatOffset = beat;
            intervalStartBeat = beat;
            beatInterval = interval;

            CallAndResponseHandler newHandler = new();
            crHandlerInstance = newHandler;
            crHandlerInstance.StartInterval(beat, interval);
            var relevantPlantEvents = GetAllPlantsBetweenBeat(beat, beat + interval);
            foreach (var plant in relevantPlantEvents)
            {
                crHandlerInstance.AddEvent(plant.beat);
                SpawnFlower(plant.beat, plant.beat < gameSwitchBeat);
            }

            if (autoPassTurn)
            {
                PassTurn(beat + interval, newHandler);
            }
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn)
        {
            if (GameManager.instance.currentGame == "wizardsWaltz")
            {
                instance.SetIntervalStart(beat, interval, beat, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn
                });
            }
        }

        private void PassTurnStandalone(double beat)
        {
            if (crHandlerInstance != null) PassTurn(beat, crHandlerInstance);
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "wizardsWaltz")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurn(double beat, CallAndResponseHandler crHandler)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.25, delegate
                {
                    beatInterval = crHandler.intervalLength;
                    wizardBeatOffset = beat;
                    intervalStartBeat = beat;
                    foreach (var plant in currentPlants)
                    {
                        var songPos = (float)(plant.createBeat - wizardBeatOffset);
                        var am = (beatInterval / 2f);
                        var x = Mathf.Sin(Mathf.PI * songPos / am) * xRange;
                        var y = plantYOffset + Mathf.Cos(Mathf.PI * songPos / am) * (yRange * 1.5f);
                        var z = Mathf.Cos(Mathf.PI * songPos / am) * zRange;
                        /*var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f;
                        var xscale = scale;
                        if (y > -3.5f) xscale *= -1;*/

                        plant.transform.localPosition = new Vector3(x, y, z);
                        //plant.transform.localScale = new Vector3(xscale, scale, 1);
                    }

                    foreach (var crEvent in crHandler.queuedEvents)
                    {
                        Plant plantToInput = currentPlants.Find(x => x.createBeat == crEvent.beat);
                        plantToInput.StartInput(beat - 0.25, (float)crEvent.relativeBeat + 0.25f);
                    }
                    crHandler.queuedEvents.Clear();
                })
            });
        }

        private static List<QueuedInterval> queuedIntervals = new();

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
        }

        public void SpawnFlower(double beat, bool spawnedInactive)
        {
            if (!spawnedInactive) SoundByte.PlayOneShotGame("wizardsWaltz/plant", beat);
            Plant plant = Instantiate(plantBase, plantHolder.transform).GetComponent<Plant>();
            currentPlants.Add(plant);
            var songPos = (float)(beat - wizardBeatOffset);
            var am = (beatInterval / 2f);
            var x = Mathf.Sin(Mathf.PI * songPos / am) * xRange;
            var y = plantYOffset + Mathf.Cos(Mathf.PI * songPos / am) * (yRange * 1.5f);
            var z = Mathf.Cos(Mathf.PI * songPos / am) * zRange;
            /*var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f;
            var xscale = scale;
            if (y > -3.5f) xscale *= -1;*/

            plant.transform.localPosition = new Vector3(x, y, z);
            //plant.transform.localScale = new Vector3(xscale, scale, 1);

            //plant.order = (int)Math.Round((scale - 1) * 1000);
            plant.order = (int)Math.Round(z * -1);

            plant.createBeat = beat;

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    plant.gameObject.SetActive(true);
                    plant.Init(spawnedInactive);
                })
            });
        }

    }
}