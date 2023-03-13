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

        public bool singing;

        private GleeClub game;

        void Awake()
        {
            game = GleeClub.instance;
        }

        void OnDestroy()
        {
            Jukebox.KillLoop(currentSound, 0f);
        }

        public void MissPose()
        {
            if (!singing && anim.IsPlayingAnimationName("Idle") && !anim.IsPlayingAnimationName("MissIdle")) anim.Play("MissIdle", 0, 0);
        }

        public void StartCrouch()
        {
            if (singing) return;
            anim.Play("CrouchStart", 0, 0);
        }

        public void StartYell()
        {
            if (singing) return;
            singing = true;
            anim.SetBool("Mega", true);
            anim.Play("OpenMouth", 0, 0);
            Jukebox.KillLoop(currentSound, 0f);
            Jukebox.PlayOneShotGame("gleeClub/LoudWailStart");
            currentSound = Jukebox.PlayOneShotGame("gleeClub/LoudWailLoop", -1, currentPitch, 1f, true);
        }

        public void StartSinging()
        {
            if (singing) return;
            singing = true;
            anim.SetBool("Mega", false);
            anim.Play("OpenMouth", 0, 0);
            Jukebox.KillLoop(currentSound, 0f);
            currentSound = Jukebox.PlayOneShotGame("gleeClub/WailLoop", -1, currentPitch, 1f, true);
        }

        public void StopSinging(bool mega = false)
        {
            if (!singing) return;
            singing = false;
            anim.Play(mega ? "MegaCloseMouth" : "CloseMouth", 0, 0);
            Jukebox.KillLoop(currentSound, 0f);
            Jukebox.PlayOneShotGame("gleeClub/StopWail");
        }
    }
}


