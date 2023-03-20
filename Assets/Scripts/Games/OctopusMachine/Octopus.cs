using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] SpriteRenderer sr;
        [SerializeField] bool player;
        
        public bool singing;
        public bool disappeared = false;
        public bool shouldMegaClose;

        private OctopusMachine game;
        public static Octopus instance;

        void Awake()
        {
            game = OctopusMachine.instance;
        }

        void OnDestroy()
        {
            
        }

        public void TogglePresence(bool disappear)
        {
            /*
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
                if (player && !PlayerInput.Pressing() && !GameManager.instance.autoplay) 
                {
                    StartSinging();
                    game.leftChorusKid.MissPose();
                    game.middleChorusKid.MissPose();
                } 
            }
            */
        }

        public void MissPose()
        {
            
        }

        public void StartCrouch()
        {
            
        }

        public void StartYell()
        {
            /*
            if (singing || disappeared) return;
            singing = true;
            anim.SetBool("Mega", true);
            anim.Play("OpenMouth", 0, 0);
            shouldMegaClose = true;
            if (currentSound != null) Jukebox.KillLoop(currentSound, 0f);
            Jukebox.PlayOneShotGame("gleeClub/LoudWailStart");
            currentSound = Jukebox.PlayOneShotGame("gleeClub/LoudWailLoop", -1, currentPitch, 1f, true);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 1f, delegate { UnYell(); })
            });
            */
        }

        void UnYell()
        {
            //if (singing && !anim.GetCurrentAnimatorStateInfo(0).IsName("YellIdle")) anim.Play("YellIdle", 0, 0);
        }

        public void StartSinging(bool forced = false)
        {
            /*
            if ((singing && !forced) || disappeared) return;
            singing = true;
            anim.SetBool("Mega", false);
            shouldMegaClose = false;
            anim.Play("OpenMouth", 0, 0);
            if (currentSound != null) Jukebox.KillLoop(currentSound, 0f);
            currentSound = Jukebox.PlayOneShotGame("gleeClub/WailLoop", -1, currentPitch, 1f, true);
            */
        }

        public void StopSinging(bool mega = false, bool playSound = true)
        {
            /*
            if (!singing || disappeared) return;
            singing = false;
            anim.Play(mega ? "MegaCloseMouth" : "CloseMouth", 0, 0);
            if (currentSound != null) Jukebox.KillLoop(currentSound, 0f);
            if (playSound) Jukebox.PlayOneShotGame("gleeClub/StopWail");
            */
        }

        public void Bop(float beat)
        {
            if (!game.isPreparing && game.bopOn)
            {
                if (anim.IsAnimationNotPlaying() || anim.IsPlayingAnimationName("Idle"))
                if (game.isHappy) {
                    anim.DoScaledAnimation("Happy", 0.5f);
                } else if (game.isAngry) {
                    anim.DoScaledAnimation("Angry", 0.5f);
                } else if (game.isShocked) {
                    anim.DoScaledAnimation("Oops", 0.5f);
                } else {
                    anim.DoScaledAnimation("Bop", 0.5f);
                }
            }
        }

        public void PlayAnimation(float beat, bool keepBopping, int whichBop)
        {
            switch (whichBop)
            {
                case 0:
                anim.DoScaledAnimation("Bop", 0.5f);
                break;
                case 1:
                anim.DoScaledAnimation("Happy", 0.5f);
                break;
                case 2:
                anim.DoScaledAnimation("Angry", 0.5f);
                break;
                case 3:
                anim.DoScaledAnimation("Oops", 0.5f);
                break;
            }
            if (keepBopping) {
                game.isHappy =   whichBop == 1 ? keepBopping : !keepBopping;
                game.isAngry =   whichBop == 2 ? keepBopping : !keepBopping;
                game.isShocked = whichBop == 3 ? keepBopping : !keepBopping;
            }
        }
    }
}