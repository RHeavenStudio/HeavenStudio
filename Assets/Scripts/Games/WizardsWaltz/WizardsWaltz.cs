using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

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

        private int timer = 0;
        public float beatInterval = 4f;
        float intervalStartBeat;
        bool intervalStarted;
        public float wizardBeatOffset = 0f;

        [NonSerialized] public int plantsLeft = 0;

        public static WizardsWaltz instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            List<float> starts = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "wizardsWaltz/start interval").Select(c => c.beat).ToList();

            if (starts.Count > 0)
            {
                var nextInterval = starts.IndexOf(Mathp.GetClosestInList(starts, Conductor.instance.songPositionInBeats));
                wizardBeatOffset = starts[nextInterval];
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
                var songPos = Conductor.instance.songPositionInBeats - wizardBeatOffset;
                var am = beatInterval / 2f;
                var x = Mathf.Sin(Mathf.PI * songPos / am) * 6 + UnityEngine.Random.Range(-0.5f, 0.5f);
                var y = Mathf.Cos(Mathf.PI * songPos / am) * 0.5f + UnityEngine.Random.Range(-0.5f, 0.5f);
                var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f + UnityEngine.Random.Range(-0.2f, 0.2f); ;

                MagicFX magic = Instantiate(fxBase, fxHolder.transform).GetComponent<MagicFX>();

                magic.transform.position = new Vector3(x, 2f + y, 0);
                magic.transform.localScale = wizard.gameObject.transform.localScale;
                magic.gameObject.SetActive(true);
            }

            timer++;
        }

        public void SetIntervalStart(float beat, float interval = 4f)
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

        public void SpawnFlower(float beat)
        {
            // If interval hasn't started, assume this is the first hair of the interval.
            if (!intervalStarted)
                SetIntervalStart(beat, beatInterval);

            Jukebox.PlayOneShotGame("wizardsWaltz/plant", beat);
            Plant plant = Instantiate(plantBase, plantHolder.transform).GetComponent<Plant>();

            var songPos = Conductor.instance.songPositionInBeats - wizardBeatOffset;
            var am = (beatInterval / 2f);
            var x = Mathf.Sin(Mathf.PI * songPos / am) * 6;
            var y = -3f + Mathf.Cos(Mathf.PI * songPos / am) * 1.5f;
            var scale = 1 - Mathf.Cos(Mathf.PI * songPos / am) * 0.35f;
            var xscale = scale;
            if (y > -3.5f) xscale *= -1;

            plant.transform.localPosition = new Vector3(x, y, 0);
            plant.transform.localScale = new Vector3(xscale, scale, 1);

            plant.order = (int)Math.Round((scale - 1) * 1000);
            plant.gameObject.SetActive(true);

            plant.createBeat = beat;
            plantsLeft++;
        }

    }
}