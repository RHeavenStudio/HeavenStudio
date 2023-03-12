using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_GleeClub
{
    public class ChorusKid : MonoBehaviour
    {
        [SerializeField] Animator anim;
        Sound currentSound;
        
        public float currentPitch = 1f;

        private GleeClub game;

        void Awake()
        {
            game = GleeClub.instance;
        }

        void OnDestroy()
        {
            Jukebox.KillLoop(currentSound, 0f);
        }

        public void StartSinging()
        {
            anim.Play("OpenMouth", 0, 0);
            currentSound = Jukebox.PlayOneShotGame("gleeClub/WailLoop", -1, currentPitch, 1f, true);
        }

        public void StopSinging()
        {
            anim.Play("CloseMouth", 0, 0);
            Jukebox.KillLoop(currentSound, 0f);
        }
    }
}


