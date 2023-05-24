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
        public int[] lastPitches = new int[6];
        public int[] lastBendPitches = new int[6];
        public bool lastG5 = true;

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
        }

        public void BendUp(bool G5, int[] pitches)
        {
            if (bending || !strumming) return;
            bending = true;
            int soundCounter = 0;
            lastBendPitches = pitches;
            lastG5 = G5;
            for (int i = 0; i < stringSounds.Length; i++)
            {
                if (pitches[i] == -1) continue;
                if (stringSounds[i] != null)
                {
                    stringSounds[i].BendUp(0.05f, FindRelativeBendPitch(i, stringSounds[i].pitch, G5, pitches[i]));
                    StartCoroutine(BendUpLoop(i, pitches[i], G5));
                    soundCounter++;
                }
            }
            if (soundCounter > 0) Jukebox.PlayOneShotGame("rockers/bendUp");
        }

        IEnumerator BendUpLoop(int index, int pitch, bool G5)
        {
            while (strumming && !stringSounds[index].hasBended) 
            {
                yield return null;
            }
            if (strumming && bending) bendedStringSounds[index] = Jukebox.PlayOneShotGame("rockers/" + (G5 ? "BendG5" : "BendC6"), -1, Jukebox.GetPitchFromSemiTones(pitch, true), stringSounds[index].volume, true);
        }

        public void BendDown()
        {
            if (!bending) return;
            StopAllCoroutines();
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
                    sound.BendDown(0.05f);
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

        private float FindRelativeBendPitch(int index, float currentPitch, bool G5, int bendSemitones)
        {
            float semiToneValue = Mathf.Pow(2f, 1f / 12f) - 1f;
            int semiTonesPitched = (int)((currentPitch - 1f) / semiToneValue);
            int bendedPitch = 0;
            switch (index)
            {
                case 1:
                    bendedPitch = (G5 ? 39 : 40) + bendSemitones - semiTonesPitched;
                    break;
                case 2:
                    bendedPitch = (G5 ? 46 : 47) + bendSemitones - semiTonesPitched;
                    break;
                case 3:
                    bendedPitch = (G5 ? 29 : 30) + bendSemitones - semiTonesPitched;
                    break;
                case 4:
                    bendedPitch = (G5 ? 24 : 25) + bendSemitones - semiTonesPitched;
                    break;
                case 5:
                    bendedPitch = (G5 ? 16 : 17) + bendSemitones - semiTonesPitched;
                    break;
                case 6:
                    bendedPitch = (G5 ? 15 : 16) + bendSemitones - semiTonesPitched;
                    break;
            }
            return Jukebox.GetPitchFromSemiTones(bendedPitch, true);
        }
    }
}

