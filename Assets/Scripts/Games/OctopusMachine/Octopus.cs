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

        public bool cantBop;
        public bool isSqueezed;
        public bool isPreparing;
        public bool queuePrepare;
        public float lastReportedBeat = 0f;
        float lastSqueezeBeat;
        bool isActive = true;

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
            
            if (isActive && player)
            {
                if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN)) 
                    OctoAction("Squeeze");

                if (PlayerInput.PressedUp() && !game.IsExpectingInputNow(InputType.STANDARD_UP)) {
                    OctoAction(PlayerInput.Pressing(true) ? "Pop" : "Release");
                }
            }
        }

        void LateUpdate()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)
                && !anim.IsPlayingAnimationName("Bop")
                && !anim.IsPlayingAnimationName("Happy")
                && !anim.IsPlayingAnimationName("Angry")
                && !anim.IsPlayingAnimationName("Oops")
                && !anim.IsPlayingAnimationName("Release")
                && !anim.IsPlayingAnimationName("Pop")
                && !isPreparing
                && !isSqueezed
                && !cantBop )
            {
                PlayAnimation(game.bopStatus);
            }
        }

        public void PlayAnimation(int whichBop)
        {
            if (whichBop == 2 && player) whichBop = 3;
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
            isActive = enable;
        }

        public void OctoAction(string action) 
        {
            if (action != "Release" || (Conductor.instance.songPositionInBeats - lastSqueezeBeat) > 0.15f) Jukebox.PlayOneShotGame($"octopusMachine/{action.ToLower()}");
            if (action == "Squeeze") lastSqueezeBeat = Conductor.instance.songPositionInBeats;

            anim.DoScaledAnimationAsync(action, 0.5f);
            isSqueezed = (action == "Squeeze");
            isPreparing =
            queuePrepare = false;
        }

        public void AnimationColor(int poppingColor) 
        {
            foreach (var sprite in sr) sprite.material.SetColor("_ColorAlpha", (poppingColor == 0 ? OctopusMachine.octopodesColor : OctopusMachine.octopodesSqueezedColor));
            if (poppingColor == 1) isSqueezed = true;
        }
    }
}