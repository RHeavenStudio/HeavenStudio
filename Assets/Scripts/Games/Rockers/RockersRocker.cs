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

