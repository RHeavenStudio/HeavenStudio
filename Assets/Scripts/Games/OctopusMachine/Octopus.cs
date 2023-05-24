using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] sr;
        [SerializeField] bool player;
        [SerializeField] Material mat;
        public Animator anim;

        public bool noBop;
        public bool cantBop;
        public bool isSqueezed;
        public bool isPreparing;
        public bool queuePrepare;
        public float lastReportedBeat = 0f;

        private OctopusMachine game;

        void Awake()
        {
            game = OctopusMachine.instance;
        }

        void Update()
        {
            if (queuePrepare && Conductor.instance.NotStopped()) {
                if (!(isPreparing || isSqueezed)) {
                    anim.DoScaledAnimationAsync("Prepare", 0.5f);
                    isPreparing = true;
                    queuePrepare = false;
                }
            }
            
            if (gameObject.activeInHierarchy && player)
            {
                if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN)) 
                    OctoAction("Squeeze");

                if (PlayerInput.PressedUp() && !game.IsExpectingInputNow(InputType.STANDARD_UP)) {
                    if (PlayerInput.Pressing(true)) OctoAction("Pop");
                    else OctoAction("Release");
                }
            }
        }

        void LateUpdate()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)
                && !anim.IsPlayingAnimationName("Release")
                && !anim.IsPlayingAnimationName("Squeeze")
                && !anim.IsPlayingAnimationName("Pop")
                && !isPreparing
                && !isSqueezed
                && !noBop
                && !cantBop)
            {
                Bop();
                Debug.Log("dAOWNDOJANDWAN");
            }
        }

        public void Bop(bool singleBop = false)
        {
            if (game.hasHit) {
                PlayAnimation(1);
                game.bopIterate++;
                if (game.bopIterate == 3) game.hasHit = false;
            } else if (game.hasMissed) {
                PlayAnimation(player ? 3 : 2);
                game.bopIterate++;
                if (game.bopIterate == 3) game.hasMissed = false;
            } else {
                PlayAnimation(0);
                game.bopIterate = 0;
            }
        }

        public void PlayAnimation(int whichBop)
        {
            anim.DoScaledAnimationAsync(whichBop switch {
                0 => "Bop",
                1 => "Happy",
                2 => "Angry",
                3 => "Oops",
            }, 0.5f);
        }

        public void ForceSqueeze()
        {
            anim.DoScaledAnimationAsync("ForceSqueeze", 0.5f);
            isSqueezed = true;
        }

        public void OctopusModifiers(float x, float y, bool isActive)
        {
            gameObject.transform.position = new Vector3(x, y, 0);
            gameObject.SetActive(isActive);
        }

        public void OctoAction(string action) 
        {
            anim.DoScaledAnimationAsync(action, 0.5f);
            Jukebox.PlayOneShotGame($"octopusMachine/{action.ToLower()}");
            isSqueezed = (action == "Squeeze");
            OctopusMachine.canPrepare = (action != "Squeeze");
            isPreparing = false;
        }

        public void AnimationColor(int poppingColor) 
        {
            foreach (var sprite in sr) sprite.material.SetColor("_ColorAlpha", (poppingColor == 0 ? OctopusMachine.octopodesColor : OctopusMachine.octopodesSqueezedColor));
        }
    }
}