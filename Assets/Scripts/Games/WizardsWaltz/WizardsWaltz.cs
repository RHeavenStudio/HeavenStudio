using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

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
                    function = delegate { WizardsWaltz.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    defaultLength = 6f, 
                    resizable = true,
                    priority = 1
                },
                new GameAction("plant", "Plant")
                {
                    function = delegate { WizardsWaltz.instance.SpawnFlower(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                },
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
    using Scripts_WizardsWaltz;

    public class WizardsWaltz : Minigame
    {
        [Header("References")]
        public Wizard wizard;
        public Girl girl;
        public GameObject plantHolder;
        public GameObject plantBase;
        public GameObject fxHolder;
        public GameObject fxBase;

        [Header("Properties")]
        private int timer = 0;
        [NonSerialized] public float beatInterval = 6f;
        [NonSerialized] public double intervalStartBeat;
        bool intervalStarted;
        public double wizardBeatOffset = 0f;
        public float xRange = 5;
        public float zRange = 5;
        public float yRange = 0.5f;
        [SerializeField] private float plantYOffset = -2f;

        [NonSerialized] public int plantsLeft = 0; //this variable is unused

        public static WizardsWaltz instance;

        private static CallAndResponseHandler crInstance;

        private void Awake()
        {
            instance = this;
            wizard.Init();

            var nextStart = GameManager.instance.Beatmap.Entities.Find(c => c.datamodel == "wizardsWaltz/start interval" && c.beat + c.length >= Conductor.instance.songPositionInBeatsAsDouble);

            if (nextStart != null)
            {
                EventCaller.instance.CallEvent(nextStart, true);
            }
        }

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused && intervalStarted)
            {
                intervalStarted = false;
            }
        }

        private void FixedUpdate()
        {
            if (timer % 8 == 0 || UnityEngine.Random.Range(0,8) == 0)
            {
                var songPos = (float)(Conductor.instance.songPositionInBeatsAsDouble - wizardBeatOffset);
                var am = beatInterval / 2f;
                var x = Mathf.Sin(Mathf.PI * songPos / am) * xRange + UnityEngine.Random.Range(-0.5f, 0.5f);
                var y = Mathf.Cos(Mathf.PI * songPos / am) * (yRange * 0.5f) + UnityEngine.Random.Range(-0.5f, 0.5f);
                var z = Mathf.Cos(Mathf.PI * songPos / am) * zRange + UnityEngine.Random.Range(-0.5f, 0.5f);
                //var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f + UnityEngine.Random.Range(-0.2f, 0.2f);

                MagicFX magic = Instantiate(fxBase, fxHolder.transform).GetComponent<MagicFX>();

                magic.transform.position = new Vector3(x, 2f + y, z);;
                magic.gameObject.SetActive(true);
            }

            timer++;
        }

        public void SetIntervalStart(double beat, float interval = 4f)
        {
            // Don't do these things if the interval was already started.
            if (!intervalStarted)
            {
                plantsLeft = 0;
                intervalStarted = true;
            }

            wizardBeatOffset = beat;
            intervalStartBeat = beat;
            beatInterval = interval;
        }

        public void SpawnFlower(double beat)
        {
            // If interval hasn't started, assume this is the first hair of the interval.
            if (!intervalStarted)
                SetIntervalStart(beat, beatInterval);

            SoundByte.PlayOneShotGame("wizardsWaltz/plant", beat);
            Plant plant = Instantiate(plantBase, plantHolder.transform).GetComponent<Plant>();

            var songPos = (float)(Conductor.instance.songPositionInBeatsAsDouble - wizardBeatOffset);
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
            plant.gameObject.SetActive(true);

            plant.createBeat = beat;
            plantsLeft++;
        }

    }
}