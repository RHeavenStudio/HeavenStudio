using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    using Scripts_DrummingPractice;

    public class DrummingPractice : Minigame
    {
        public enum MiiType
        {
            Random = -1,
            GuestA,
            GuestB,
            GuestC,
            GuestD,
            GuestE,
            GuestF,
            Matt,
            Tsunku,
            Marshal
        }

        [Header("References")]
        public SpriteRenderer backgroundGradient;
        public Drummer player;
        public Drummer leftDrummer;
        public Drummer rightDrummer;
        public GameObject hitPrefab;

        public GameEvent bop = new GameEvent();
        public int count = 0;

        public static DrummingPractice instance;

        private void Awake()
        {
            instance = this;
        }

        // TODO: Move this to OnGameSwitch() when functional?
        private void Start()
        {
            SetMiis();
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (Conductor.instance.songPositionInBeats >= bop.startBeat && Conductor.instance.songPositionInBeats < bop.startBeat + bop.length)
                {
                    Bop();
                }
            }
        }

        public void SetBop(float beat, float length)
        {
            bop.startBeat = beat;
            bop.length = length;
        }

        public void Bop()
        {
            player.Bop();
            leftDrummer.Bop();
            rightDrummer.Bop();
        }

        public void Prepare(float beat, bool applause)
        {
            int type = count % 2;
            player.Prepare(type);
            leftDrummer.Prepare(type);
            rightDrummer.Prepare(type);
            count++;

            SetFaces(0);
            Jukebox.PlayOneShotGame("drummingPractice/prepare");

            GameObject hit = Instantiate(hitPrefab);
            hit.transform.parent = hitPrefab.transform.parent;
            hit.SetActive(true);
            DrummerHit h = hit.GetComponent<DrummerHit>();
            h.startBeat = beat;
            h.applause = applause;
        }

        public void SetFaces(int type)
        {
            player.SetFace(type);
            leftDrummer.SetFace(type);
            rightDrummer.SetFace(type);
        }

        public void SetMiis(int playerFace = (int) MiiType.Random, int leftFace = (int) MiiType.Random, int rightFace = (int) MiiType.Random, bool all = false)
        {
            if (playerFace == (int) MiiType.Random)
            {
                do
                {
                    player.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                }
                while (player.mii == leftFace || player.mii == rightFace);
            }
            else
                player.mii = playerFace;

            if (all && playerFace != -1)
            {
                leftDrummer.mii = playerFace;
                rightDrummer.mii = playerFace;
            }
            else
            {
                if (leftFace == (int) MiiType.Random)
                {
                    do
                    {
                        leftDrummer.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                    }
                    while (leftDrummer.mii == player.mii);
                }
                else
                    leftDrummer.mii = leftFace;

                if (rightFace == (int) MiiType.Random)
                {
                    do
                    {
                        rightDrummer.mii = UnityEngine.Random.Range(0, player.miiFaces.Count);
                    }
                    while (rightDrummer.mii == leftDrummer.mii || rightDrummer.mii == player.mii);
                }
                else
                    rightDrummer.mii = rightFace;
            }

            SetFaces(0);
        }

    }
}