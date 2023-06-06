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

        public int stepTimes = 0;
        public int blipSize = 0;
        public bool shouldGrow;
        public string blipString = "M";

        public void Blip()
        {
            float c = Conductor.instance.songPositionInBeats;
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(MathF.Floor(c) + 0.5f, delegate {
                    if (MrUpbeat.shouldBlip) {
                        Jukebox.PlayOneShotGame("mrUpbeat/blip");
                        blipAnim.Play("Blip"+(blipSize+1), 0, 0);
                        blipText.text = (blipSize == 4 && blipString != "") ? blipString : "";
                        if (shouldGrow && blipSize < 4) blipSize++;
                    }
                }),
                new BeatAction.Action(MathF.Floor(c) + 1f, delegate { 
                    Blip();
                }),
            });
        }

        public void Step()
        {
            stepTimes++;
            
            bool x = (stepTimes % 2 == 1);
            shadows[0].SetActive(!x);
            shadows[1].SetActive(x);
            transform.localScale = new Vector3(x ? -1 : 1, 1);

            anim.DoScaledAnimationAsync("Step", 0.5f);
            letterAnim.DoScaledAnimationAsync(x ? "StepRight" : "StepLeft", 0.5f);
            Jukebox.PlayOneShotGame("mrUpbeat/step");
        }

        public void Fall()
        {
            blipSize = 0;
            blipAnim.Play("Idle", 0, 0);
            blipText.text = "";
            
            anim.DoScaledAnimationAsync("Fall", 0.5f);
            Jukebox.PlayOneShot("miss");
            shadows[0].SetActive(false);
            shadows[1].SetActive(false);
        }
    }
}