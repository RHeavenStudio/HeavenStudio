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

        [SerializeField] private bool JJ;

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
            StopSounds();
            Jukebox.PlayOneShotGame("rockers/noise");
            List<int> eligiblePitches = new List<int>();
            foreach (var pitch in pitches)
            {
                if (pitch != -1) eligiblePitches.Add(pitch);
            }
            for (int i = 0; i < eligiblePitches.Count; i++)
            {
                float pitch = Jukebox.GetPitchFromSemiTones(eligiblePitches[i], true);
                float volume = GetVolumeBasedOnAmountOfStrings(eligiblePitches.Count);
                string soundName = "rockers/strings/" + (gleeClub ? "gleeClub/" : "normal/" + (i + 1));
                Debug.Log("Pitch: " + pitch + " Volume: " + volume + " Name: " + soundName);
                stringSounds[i] = Jukebox.PlayOneShotGame(soundName, -1, pitch, volume, true);
            }
            DoScaledAnimationAsync("Idle", 0.5f);
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
            StopSounds();
            Jukebox.PlayOneShotGame("rockers/mute");
            DoScaledAnimationAsync("Crouch", 0.5f);
        }

        public void UnHold()
        {
            DoScaledAnimationAsync("Idle", 0.5f);
        }

        private void DoScaledAnimationAsync(string name, float time)
        {
            anim.DoScaledAnimationAsync((JJ ? "JJ" : "") + name, time);
        }
    }
}

