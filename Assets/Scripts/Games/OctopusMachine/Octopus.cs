using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] sr;
        [SerializeField] SpriteRenderer[] srAll;
        [SerializeField] bool player;
        public Animator anim;

        public bool noBop;
        public bool cantBop;
        public bool isSqueezed;
        public bool isPreparing;
        public bool queuePrepare;
        public float lastReportedBeat = 0f;
        public float lastSqueezeBeat;

        private OctopusMachine game;

        void Awake()
        {
            game = OctopusMachine.instance;
        }

        void Update()
        {
            if (queuePrepare && Conductor.instance.NotStopped()) {
                if (!(isPreparing || isSqueezed || anim.IsPlayingAnimationName("Release") || anim.IsPlayingAnimationName("Pop"))) 
                {
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

        public void OctopusModifiers(float x, float y, bool enable)
        {
            gameObject.transform.position = new Vector3(x, y, 0);
            foreach (var sprite in srAll) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, enable ? 1 : 0);
        }

        public void OctoAction(string action) 
        {
            if (action != "Release" || (Conductor.instance.songPositionInBeats - lastSqueezeBeat) > 0.3f) 
                Jukebox.PlayOneShotGame($"octopusMachine/{action.ToLower()}");

            if (action == "Squeeze") {
                lastSqueezeBeat = Conductor.instance.songPositionInBeats;
                isSqueezed = true;
            }

            anim.DoScaledAnimationAsync(action, 0.5f);
            isPreparing =
            queuePrepare = false;
        }

        public void AnimationColor(int poppingColor) 
        {
            foreach (var sprite in sr) sprite.material.SetColor("_ColorAlpha", (poppingColor == 0 ? OctopusMachine.octopodesColor : OctopusMachine.octopodesSqueezedColor));
        }
    }
}