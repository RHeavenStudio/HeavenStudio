using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Rockers
{
    public class RockersRocker : MonoBehaviour
    {
        private Sound[] stringSounds = new Sound[6];
        private Sound[] bendedStringSounds = new Sound[6];
        private Sound chordSound;
        private Animator anim;

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
            foreach (var sound in bendedStringSounds)
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
            Jukebox.PlayOneShotGame("rockers/noise");
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
        }

        public void BendUp(bool G5, int[] pitches)
        {
            if (bending || !strumming) return;
            bending = true;
            int soundCounter = 0;
            for (int i = 0; i < stringSounds.Length; i++)
            {
                if (pitches[i] == -1) continue;
                if (stringSounds[i] != null)
                {
                    bendedStringSounds[i] = Jukebox.PlayOneShotGame("rockers/" + (G5 ? "BendG5" : "BendC6"), -1, Jukebox.GetPitchFromSemiTones(pitches[i], true), stringSounds[i].volume, true);
                    stringSounds[i].Pause();
                    soundCounter++;
                }
            }
            if (soundCounter > 0) Jukebox.PlayOneShotGame("rockers/bendUp");
        }

        public void BendDown()
        {
            if (!bending) return;
            bending = false;
            foreach (var sound in bendedStringSounds)
            {
                if (sound != null)
                {
                    sound.KillLoop(0);
                }
            }
            foreach (var sound in stringSounds)
            {
                if (sound != null)
                {
                    sound.UnPause();
                }
            }
            Jukebox.PlayOneShotGame("rockers/bendDown");
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

