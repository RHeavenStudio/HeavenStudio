using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockersRocker : MonoBehaviour
    {
        private Sound[] stringSounds = new Sound[6];
        private Sound chordSound;
        private Animator anim;
        public int[] lastPitches = new int[6];
        public int lastBendPitch;

        [SerializeField] private GameObject strumEffect;

        [SerializeField] private bool JJ;

        private bool muted;
        private bool strumming;
        private bool bending;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            StopSounds();
        }

        private void StopSounds()
        {
            foreach (var sound in stringSounds)
            {
                if (sound != null)
                {
                    sound.KillLoop(0);
                }
            }
            if (chordSound != null)
            {
                chordSound.KillLoop(0);
            }
        }

        public void StrumStrings(bool gleeClub, int[] pitches)
        {
            muted = false;
            strumming = true;
            StopSounds();
            //Jukebox.PlayOneShotGame("rockers/noise");
            lastPitches = pitches;
            for (int i = 0; i < pitches.Length; i++)
            {
                if (pitches[i] == -1) continue;
                float pitch = Jukebox.GetPitchFromSemiTones(pitches[i], true);
                float volume = GetVolumeBasedOnAmountOfStrings(pitches.Length);
                string soundName = "rockers/strings/" + (gleeClub ? "gleeClub/" : "normal/" + (i + 1));
                Debug.Log("Pitch: " + pitch + " Volume: " + volume + " Name: " + soundName);
                stringSounds[i] = Jukebox.PlayOneShotGame(soundName, -1, pitch, volume, true);
            }
            DoScaledAnimationAsync("Strum", 0.5f);
            strumEffect.SetActive(true);
        }

        public void BendUp(int pitch)
        {
            if (bending || !strumming) return;
            bending = true;
            lastBendPitch = pitch;
            for (int i = 0; i < stringSounds.Length; i++)
            {
                if (stringSounds[i] != null)
                {
                    stringSounds[i].BendUp(0.05f, Jukebox.GetPitchFromSemiTones(Jukebox.GetSemitonesFromPitch(stringSounds[i].pitch) + pitch, true));
                }
            }
            Jukebox.PlayOneShotGame("rockers/bendUp");
            DoScaledAnimationAsync("Bend", 0.5f);
        }


        public void BendDown()
        {
            if (!bending) return;
            bending = false;
            foreach (var sound in stringSounds)
            {
                if (sound != null)
                {
                    sound.BendDown(0.05f);
                }
            }
            Jukebox.PlayOneShotGame("rockers/bendDown");
            DoScaledAnimationAsync("Unbend", 0.5f);
        }

        private float GetVolumeBasedOnAmountOfStrings(int stringAmount)
        {

            switch (stringAmount)
            {
                default:
                    return 1;
                case 3:
                    return 0.893f;
                case 4:
                    return 0.75f;
                case 5:
                    return 0.686f;
                case 6:
                    return 0.62f;
            }
        }

        public void Mute()
        {
            strumming = false;
            strumEffect.SetActive(false);
            bending = false;
            StopSounds();
            Jukebox.PlayOneShotGame("rockers/mute");
            DoScaledAnimationAsync("Crouch", 0.5f);
            muted = true;
        }

        public void UnHold()
        {
            if (!muted) return;
            DoScaledAnimationAsync("UnCrouch", 0.5f);
            muted = false;
        }

        private void DoScaledAnimationAsync(string name, float time)
        {
            anim.DoScaledAnimationAsync((JJ ? "JJ" : "") + name, time);
        }
    }
}

