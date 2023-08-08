using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;
using TMPro;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MrUpbeat
{
    public class UpbeatMan : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Animator anim;
        [SerializeField] Animator blipAnim;
        [SerializeField] Animator letterAnim;
        [SerializeField] GameObject[] shadows;
        [SerializeField] TMP_Text blipText;

        public int blipSize = 0;
        public bool shouldGrow;
        public string blipString = "M";

        static MrUpbeat game; 

        void Awake()
        {
            game = MrUpbeat.instance;
        }

        public void RecursiveBlipping(double beat)
        {
            if (game.stopBlipping) {
                game.stopBlipping = false;
                return;
            } 
            SoundByte.PlayOneShotGame("mrUpbeat/blip");
            blipAnim.Play("Blip"+(blipSize+1), 0, 0);
            blipText.text = (blipSize == 4 && blipString != "") ? blipString : "";
            if (shouldGrow && blipSize < 4) blipSize++;
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 1, delegate { RecursiveBlipping(beat + 1); })
            });
        }

        public void Step(bool isInput = false)
        {
            if (isInput || ((game.stepIterate % 2 == 0) == IsMirrored())) {
                shadows[0].SetActive(IsMirrored());
                shadows[1].SetActive(!IsMirrored());
                transform.localScale = new Vector3((IsMirrored() ? 1 : -1), 1, 1);
            }
            
            anim.DoScaledAnimationAsync("Step", 0.5f);
            letterAnim.DoScaledAnimationAsync(IsMirrored() ? "StepRight" : "StepLeft", 0.5f);
            SoundByte.PlayOneShotGame("mrUpbeat/step");
        }

        public void Fall()
        {
            blipSize = 0;
            blipAnim.Play("Idle", 0, 0);
            blipText.text = "";

            anim.DoScaledAnimationAsync((game.stepIterate % 2 == 0) == IsMirrored() ? "FallR" : "FallL", 1f);
            SoundByte.PlayOneShot("miss");
            shadows[0].SetActive(false);
            shadows[1].SetActive(false);
            transform.localScale = new Vector3((IsMirrored() ? 1 : -1), 1, 1);
        }

        bool IsMirrored()
        {
            return transform.localScale != Vector3.one;
        }
    }
}