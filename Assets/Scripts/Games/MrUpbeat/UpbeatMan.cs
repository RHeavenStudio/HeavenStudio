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
        public string blipString = "M";

        public void Blip()
        {
            float c = Conductor.instance.songPositionInBeats;
            // checks if the position is on an offbeat; accurate until you get down to 20 fps or so (i.e unplayable)
            float pos = ((MathF.Floor(c * 10)/10 % 1) == 0.5f) ? MathF.Floor(c) : MathF.Round(c);

            // recursive, should happen on the offbeat (unless downbeatMod is different)
            BeatAction.New(gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(pos + MrUpbeat.downbeatMod, delegate {
                    if (MrUpbeat.shouldBlip) {
                        Jukebox.PlayOneShotGame("mrUpbeat/blip");
                        blipAnim.Play("Blip"+(blipSize+1), 0, 0);
                        blipText.text = (blipSize == 4 && blipString != "") ? blipString : "";
                    }
                }),
                new BeatAction.Action(pos + MrUpbeat.downbeatMod + 0.999f, delegate { 
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