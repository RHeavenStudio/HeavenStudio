using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_OctopusMachine
{
    public class Octopus : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] bool player;
        [SerializeField] SpriteRenderer[] sr;
        [SerializeField] Material mat;

        public bool isSqueezed;
        bool isPreparing;
        public float lastReportedBeat = 0f;

        private OctopusMachine game;
        public static Octopus instance;

        void Awake()
        {
            game = OctopusMachine.instance;
        }

        private void Start() 
        {
            mat.SetColor("_ColorAlpha", new Color(1f, 0.34f, 0.62f));
        }

        void Update()
        {
            if (gameObject.activeInHierarchy && player)
            {
                if (PlayerInput.Pressed())
                {
                    Squeeze();
                }
                if (PlayerInput.PressedUp())
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
                && !anim.IsPlayingAnimationName("Bop")
                && !anim.IsPlayingAnimationName("Happy")
                && !anim.IsPlayingAnimationName("Angry")
                && !anim.IsPlayingAnimationName("Oops")
                && !isSqueezed
                && !isPreparing)
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
            if (singleBop) {
                game.hasHit = false;
                game.hasMissed = false;
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
            isPreparing = whichBop == 4 ? true : false;
        }

        public void ForceSqueeze()
        {
            anim.DoScaledAnimationAsync("ForceSqueeze", 0.5f);
            isSqueezed = true;
        }

        public void GameplayModifiers(bool isActive, Color octoColor)
        {
            gameObject.SetActive(isActive);
            mat.SetColor("_ColorAlpha", octoColor);
        }

        public void MoveOctopodes(float x, float y)
        {
            gameObject.transform.position = new Vector3(x, y, 0);
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