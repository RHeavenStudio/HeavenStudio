using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_GleeClub
{
    public class ChorusKid : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] SpriteRenderer sr;
        [SerializeField] bool player;
        Sound currentSound;

        public float currentPitch = 1f;

        public float gameSwitchFadeOutTime = 0f;

        public bool singing;

        public bool disappeared = false;

        public bool shouldMegaClose;

        private GleeClub game;

        void Awake()
        {
            game = GleeClub.instance;
        }

        void OnDestroy()
        {
            if (currentSound != null) SoundByte.KillLoop(currentSound, gameSwitchFadeOutTime);
        }

        public void TogglePresence(bool disappear)
        {
            if (disappear)
            {
                sr.color = new Color(1, 1, 1, 0);
                StopSinging(false, false);
                anim.Play("Idle", 0, 0);
                disappeared = disappear;
            }
            else
            {
                disappeared = disappear;
                sr.color = new Color(1, 1, 1, 1);
                if (player && !PlayerInput.GetIsAction(GleeClub.InputAction_BasicPressing) && !GameManager.instance.autoplay)
                {
                    StartSinging();
                    game.leftChorusKid.MissPose();
                    game.middleChorusKid.MissPose();
                }
            }
        }

        public void MissPose()
        {
            if (!singing && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsPlayingAnimationName("MissIdle")) anim.Play("MissIdle", 0, 0);
        }

        public void StartCrouch()
        {
            if (singing || disappeared) return;
            anim.Play("CrouchStart", 0, 0);
        }

        public void StartYell()
        {
            if (singing || disappeared) return;
            singing = true;
            anim.SetBool("Mega", true);
            anim.Play("OpenMouth", 0, 0);
            shouldMegaClose = true;
            if (currentSound != null) currentSound.Stop();
            SoundByte.PlayOneShotGame("gleeClub/LoudWailStart");
            currentSound = SoundByte.PlayOneShotGame("gleeClub/LoudWailLoop", -1, currentPitch, 1f, true);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 1f, delegate { UnYell(); })
            });
        }

        void UnYell()
        {
            if (singing && !anim.GetCurrentAnimatorStateInfo(0).IsName("YellIdle")) anim.Play("YellIdle", 0, 0);
        }

        public void StartSinging(bool forced = false)
        {
            if ((singing && !forced) || disappeared) return;
            singing = true;
            anim.SetBool("Mega", false);
            shouldMegaClose = false;
            anim.Play("OpenMouth", 0, 0);
            if (currentSound != null) currentSound.Stop();
            currentSound = SoundByte.PlayOneShotGame("gleeClub/WailLoop", -1, currentPitch, 1f, true);
        }

        public void StopSinging(bool mega = false, bool playSound = true)
        {
            if (!singing || disappeared) return;
            singing = false;
            anim.Play(mega ? "MegaCloseMouth" : "CloseMouth", 0, 0);
            if (currentSound != null) currentSound.Stop();
            if (playSound) SoundByte.PlayOneShotGame("gleeClub/StopWail");
        }
    }
}


