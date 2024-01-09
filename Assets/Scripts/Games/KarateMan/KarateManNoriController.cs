using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Jukebox;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class KarateManNoriController : MonoBehaviour
    {
        public GameObject NoriController;
        public GameObject NoriHeart;
        public Transform NoriHolderTengoku;
        public Transform NoriHolderMania00;
        public Transform NoriHolderMania01;

        public GameObject NoriManiaInk00;
        public GameObject NoriManiaInk01;
        public Material NoriMaterial;

        public Color[] NoriColorsTengoku;
        public Color[] NoriColorsMania;

        public float Nori;
        public int MaxNori;
        Animator[] NoriHeartAnimators;
        Material[] NoriHeartMaterials;
        Transform NoriHolder;

        static Vector2 HeartScale = new Vector2(60, 52);
        static float ScaleFactorTengoku = 1 / 46f;
        static float ScaleFactorMania = 1 / 72f;

        static float CameraOffset = 10;
        static float PeriodLow = 24 / 60f;
        static float PeriodHigh = 15 / 60f;

        int noriMode = (int)KarateMan.NoriMode.None;
        bool playedJust = false;

        int inputsToSwitch = 0;
        //takes 12% of inputs to fill the nori bar
        float hitNoriAdd { get { return MaxNori / (inputsToSwitch * 0.12f); } }


        void Start()
        {

        }

        public void SetNoriMode(double fromBeat, int mode, int startingNori = 0)
        {
            float scaleFactor = 0f;
            //clear all children of the holder
            if (NoriHolder != null) {
                foreach (Transform child in NoriHolder) {
                    Destroy(child.gameObject);
                }
            }

            switch (mode)
            {
                case (int) KarateMan.NoriMode.Tengoku:
                    MaxNori = 5;
                    Nori = Mathf.Clamp(startingNori, 0, MaxNori);
                    scaleFactor = ScaleFactorTengoku;
                    NoriHolder = NoriHolderTengoku;
                    NoriManiaInk00.SetActive(false);
                    NoriManiaInk01.SetActive(false);
                    playedJust = false;
                    break;
                case (int) KarateMan.NoriMode.Mania:
                    MaxNori = 10;
                    Nori = Mathf.Clamp(startingNori, 0, MaxNori);
                    scaleFactor = ScaleFactorMania;
                    NoriHolder = NoriHolderMania00;
                    NoriManiaInk00.SetActive(true);
                    NoriManiaInk01.SetActive(false);
                    playedJust = false;

                    inputsToSwitch = CountHitsToEnd(fromBeat);
                    break;
                case (int) KarateMan.NoriMode.ManiaHorizontal:
                    MaxNori = 10;
                    Nori = Mathf.Clamp(startingNori, 0, MaxNori);
                    scaleFactor = ScaleFactorMania;
                    NoriHolder = NoriHolderMania01;
                    NoriManiaInk00.SetActive(true);
                    NoriManiaInk01.SetActive(true);
                    playedJust = false;

                    inputsToSwitch = CountHitsToEnd(fromBeat);
                    break;
                default: //KarateMan.NoriMode.None
                    MaxNori = 0;
                    Nori = 0;
                    NoriManiaInk00.SetActive(false);
                    NoriManiaInk01.SetActive(false);
                    playedJust = false;
                    return;
            }

            NoriHeartAnimators = new Animator[MaxNori];
            NoriHeartMaterials = new Material[MaxNori];
            noriMode = mode;

            if (mode == (int) KarateMan.NoriMode.None) return;

            //add the label?

            for (int i = 0; i < MaxNori; i++)
            {
                GameObject h = GameObject.Instantiate(NoriHeart, NoriHolder);
                h.SetActive(true);

                Material m_Material = Instantiate(NoriMaterial);
                NoriHeartMaterials[i] = m_Material;
                h.GetComponent<Image>().material = m_Material;

                RectTransform hrect = h.GetComponent<RectTransform>();
                hrect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HeartScale.x * scaleFactor);
                hrect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HeartScale.y * scaleFactor);

                NoriHeartAnimators[i] = h.GetComponent<Animator>();
                NoriHeartAnimators[i].Play(i <= (int) (startingNori - 1) ? "NoriFull" : "NoriNone", -1, (Time.time * PeriodLow) % 1f);
            }
        }

        public void DoHit()
        {
            if (noriMode == (int) KarateMan.NoriMode.None) return;
            if (MaxNori <= 0) return;
            float oldNori = Nori;
            if (noriMode == (int) KarateMan.NoriMode.Tengoku)
            {
                Nori += 1;
                if (Nori > MaxNori) Nori = MaxNori;
                if (Nori - 1 >= 0)
                    NoriHeartAnimators[(int) Nori - 1].Play("NoriFull", -1, (Time.time * PeriodHigh) % 1f);
            }
            else
            {
                Nori += hitNoriAdd;
                if (Nori > MaxNori) Nori = MaxNori;
                for (int i = 0; i < MaxNori; i++)
                {
                    if (i <= (int) Nori && i >= (int) oldNori)
                        NoriHeartAnimators[i].Play("NoriFull", -1, (Time.time * PeriodHigh) % 1f);
                }
            }
            if (KarateMan.instance.NoriPerformance >= 0.6f && oldNori / MaxNori < 0.6f && !playedJust)
            {
                playedJust = true;
                SoundByte.PlayOneShotGame("karateman/nori_just");
            }
            UpdateHeartColours();
        }

        public void DoNG()
        {
            if (noriMode == (int) KarateMan.NoriMode.None) return;
            if (MaxNori <= 0) return;
            float oldNori = Nori;
            if (noriMode == (int) KarateMan.NoriMode.Tengoku)
            {
                Nori -= 1;
                if (Nori < 0) Nori = 0;
                NoriHeartAnimators[(int)Nori].Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
            }
            else
            {
                Nori -= hitNoriAdd;
                if (Nori < 0) Nori = 0;
                if (Nori == 0)
                {
                    foreach (Animator anim in NoriHeartAnimators)
                    {
                        anim.Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
                    }
                }
                else
                {
                    for (int i = 0; i < MaxNori; i++)
                    {
                        if (i > (int) Nori)
                            NoriHeartAnimators[i].Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
                    }
                }
            }
            if (KarateMan.instance.NoriPerformance < 0.6f && oldNori / MaxNori >= 0.6f)
            {
                playedJust = false;
                SoundByte.PlayOneShotGame("karateman/nori_ng");
            }
            UpdateHeartColours();
        }

        public void DoThrough()
        {
            if (noriMode == (int) KarateMan.NoriMode.None) return;
            if (MaxNori <= 0) return;
            if (noriMode == (int) KarateMan.NoriMode.Tengoku)
            {
                if (Nori >= MaxNori)
                    SoundByte.PlayOneShotGame("karateman/nori_through");
                playedJust = false;
                Nori = 0;
                foreach (Animator anim in NoriHeartAnimators)
                {
                    anim.Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
                }
            }
            else
            {
                Nori -= hitNoriAdd * 2;
                if (Nori < 0) Nori = 0;
                if (Nori == 0)
                {
                    foreach (Animator anim in NoriHeartAnimators)
                    {
                        anim.Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
                    }
                }
                else
                {
                    for (int i = 0; i < MaxNori; i++)
                    {
                        if (i > (int) Nori)
                            NoriHeartAnimators[i].Play("NoriNone", -1, (Time.time * PeriodLow) % 1f);
                    }
                }
            }
            if (KarateMan.instance.NoriPerformance < 0.6f)
                playedJust = false;
            UpdateHeartColours();
        }

        void UpdateHeartColours()
        {
            var cond = Conductor.instance;
            if (noriMode == (int) KarateMan.NoriMode.None) return;
            if (MaxNori <= 0) return;
            float flashPeriod;
            for (int i = 0; i < NoriHeartMaterials.Length; i++)
            {
                Material mat = NoriHeartMaterials[i];
                if (noriMode == (int) KarateMan.NoriMode.Tengoku)
                {
                    if (Nori == MaxNori) {
                        mat.SetColor("_ColorAlpha", NoriColorsTengoku[3]);
                    } else if (KarateMan.instance.NoriPerformance < 0.6) {
                        mat.SetColor("_ColorAlpha", NoriColorsTengoku[0]);
                    } else if (i < 2) {
                        mat.SetColor("_ColorAlpha", NoriColorsTengoku[1]);
                    } else {
                        mat.SetColor("_ColorAlpha", NoriColorsTengoku[2]);
                    }
                }
                else
                {
                    Color c = NoriColorsMania[0];
                    Color s = Color.black;
                    if (Nori == MaxNori)
                    {
                        flashPeriod = Mathf.Sin((cond.songPositionInBeats - i / (float) MaxNori) * Mathf.PI);
                        c = NoriColorsMania[2] + (NoriColorsMania[3] * ((1 - flashPeriod * 0.5f) + 0.5f));
                        s = Color.HSVToRGB(((cond.songPositionInBeats + 0.5f) * 4) % 1, 1, flashPeriod * 0.6f + 0.4f);
                    }
                    else
                    {
                        flashPeriod = Mathf.Sin(cond.songPositionInBeats * Mathf.PI);
                        if (KarateMan.instance.NoriPerformance < 0.6)
                            c = NoriColorsMania[0];
                        else if (i < MaxNori - 2) {
                            c = NoriColorsMania[1];
                        } else {
                            c = NoriColorsMania[2];
                        }
                        c *= (flashPeriod * 0.5f) + 1f;
                    }
                    mat.SetColor("_ColorAlpha", c);
                    mat.SetColor("_AddColor", s);
                }
            }
        }

        void Update()
        {
            Transform target = GameCamera.instance.transform;

            Vector3 displacement = target.forward * CameraOffset;
            transform.position = target.position + displacement;
            transform.rotation = target.rotation;

            UpdateHeartColours();

            float inkRot = (Conductor.instance.songPositionInBeats / 8f) % 1f;
            NoriManiaInk00.transform.localRotation = Quaternion.Euler(0, 0, inkRot * 360);
            NoriManiaInk01.transform.localRotation = Quaternion.Euler(0, 0, inkRot * 360);
        }
        public int CountHitsToEnd(double fromBeat)
        {
            List<RiqEntity> allHits = EventCaller.GetAllInGameManagerList("karateman", new string[] { "hit", "bulb", "kick", "combo" });
            List<RiqEntity> allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" });

            allHits.Sort((x, y) => x.beat.CompareTo(y.beat));
            allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
            double endBeat = double.MaxValue;

            //get the beat of the closest end event
            foreach (var end in allEnds) {
                if (end.beat > fromBeat) {
                    endBeat = end.beat;
                    break;
                }
            }

            //count each hit event beginning from our current beat to the beat of the closest game switch or end
            int count = 0;
            string type;
            for (int i = 0; i < allHits.Count; i++)
            {
                RiqEntity h = allHits[i];
                if (h.beat >= fromBeat)
                {
                    if (h.beat < endBeat) {
                        //kicks and combos count for 2 hits
                        type = h.datamodel.Split('/')[1];
                        count += (type is "kick" or "combo") ? 2 : 1;
                    } else {
                        break;
                    }
                }
            }
            return count;
        }
    }
}