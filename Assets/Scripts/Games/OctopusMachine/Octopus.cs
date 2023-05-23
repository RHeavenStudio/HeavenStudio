using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] bool player;
        [SerializeField] Material mat;
        public Animator anim;

        public bool noBop;
        public bool cantBop;
        public bool isSqueezed;
        public float lastReportedBeat = 0f;

        private OctopusMachine game;

        void Awake()
        {
            game = OctopusMachine.instance;
        }

        void Update()
        {
            if (gameObject.activeInHierarchy && player)
            {
                if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    Squeeze();
                }
                if (PlayerInput.PressedUp() && !game.IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    if (PlayerInput.Pressing(true)) {
                        Pop();
                    } else {
                        Release();
                    }
                }
            }
        }

        void LateUpdate()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat)
                && !anim.IsPlayingAnimationName("Prepare")
                && !anim.IsPlayingAnimationName("PrepareIdle")
                && !anim.IsPlayingAnimationName("Squeeze")
                && !anim.IsPlayingAnimationName("ForceSqueeze")
                && !anim.IsPlayingAnimationName("Release")
                && !anim.IsPlayingAnimationName("Pop")
                && !isSqueezed
                && !noBop
                && !cantBop)
            {
                Bop();
            }
        }

        void OnDestroy()
        {
            
        }

        public void Bop(bool singleBop = false)
        {
            if (game.hasHit) {
                PlayAnimation(1);
            } else if (game.hasMissed) {
                PlayAnimation(player ? 3 : 2);
            } else {
                PlayAnimation(0);
            }
        }

        public void PlayAnimation(int whichBop, bool keepBopping = false)
        {
            string tempAnim = whichBop switch
            {
                0 => "Bop",
                1 => "Happy",
                2 => "Angry",
                3 => "Oops",
                4 => "Prepare",
            };
            anim.DoScaledAnimationAsync(tempAnim, 0.5f);
        }

        public void ForceSqueeze()
        {
            anim.DoScaledAnimationAsync("ForceSqueeze", 0.5f);
            isSqueezed = true;
        }

        public void SetColor(Color octoColor)
        {
            mat.SetColor("_ColorAlpha", octoColor);
        }

        public void OctopusModifiers(float x, float y, bool isActive)
        {
            gameObject.transform.position = new Vector3(x, y, 0);
            gameObject.SetActive(isActive);
        }

        public void Squeeze() 
        {
            anim.DoScaledAnimationAsync("Squeeze", 0.5f);
            Jukebox.PlayOneShotGame("octopusMachine/squeeze");
            isSqueezed = true;
        }

        public void Release() 
        {
            anim.DoScaledAnimationAsync("Release", 0.5f);
            Jukebox.PlayOneShotGame("octopusMachine/release");
            isSqueezed = false;
        }

        public void Pop() 
        {
            anim.DoScaledAnimationAsync("Pop", 0.5f);
            Jukebox.PlayOneShotGame("octopusMachine/pop");
            isSqueezed = false;
        }
    }
}