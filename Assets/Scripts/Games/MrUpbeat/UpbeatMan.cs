using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MrUpbeat
{
    public class UpbeatMan : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Animator anim;
        [SerializeField] Animator blipAnim;
        [SerializeField] Transform antennaLight;
        [SerializeField] GameObject[] shadows;
        [SerializeField] TMP_Text blipText;

        public int blipSize = 0;
        public bool shouldGrow;
        public bool shouldBlip = true;
        public string blipString = "M";
        public int blipLength = 4;
        public bool canStep = false; // just disabled when you normally couldn't step, which is anything less than 2 beats before you would start stepping and any time after the Ding!
        public bool canStepFromAnim = true; // disabled when stepping, then reenabled in the animation events. you can step JUST BARELY before the animation ends in fever

        private static MrUpbeat game;

        void Awake()
        {
            game = MrUpbeat.instance;

            canStep = false;
        }

        void Update()
        {
            blipText.transform.localPosition = new Vector3(antennaLight.position.x, antennaLight.position.y + 0.7f);

            if (PlayerInput.GetIsAction(MrUpbeat.InputAction_BasicPress) && !game.IsExpectingInputNow(MrUpbeat.InputAction_BasicPress)
                && canStep && canStepFromAnim) {
                Step(true);
            }
        }

        public void RecursiveBlipping(double beat)
        {
            if (game.stopBlipping) {
                game.stopBlipping = false;
                return;
            }
            if (shouldBlip) {
                Blipping();
            }
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 1, delegate { RecursiveBlipping(beat + 1); })
            });
        }

        public void Blipping()
        {
            int blipLengthReal = blipLength - 4;
            SoundByte.PlayOneShotGame("mrUpbeat/blip");
            blipAnim.Play("Blip" + (blipSize + 1 - blipLengthReal), 0, 0);
            blipText.gameObject.SetActive(blipSize - blipLengthReal >= 4);
            
            blipText.text = blipString != "" ? blipString : "";
            if (shouldGrow && blipSize - blipLengthReal < 4) blipSize++;
        }

        public void Step(bool isInput = false)
        {
            if (isInput || FacingCorrectly()) {
                shadows[0].SetActive(transform.localScale.x < 0);
                shadows[1].SetActive(transform.localScale.x > 0);
                Flip();
            }
            
            anim.DoScaledAnimationAsync("Step", 0.5f);
            SoundByte.PlayOneShotGame("mrUpbeat/step");
        }

        public void Fall()
        {
            anim.DoScaledAnimationAsync(FacingCorrectly() ? "FallR" : "FallL", 1f);
            SoundByte.PlayOneShot("miss");
            shadows[0].SetActive(false);
            shadows[1].SetActive(false);
            Flip();
        }

        void Flip() {
            var scale = transform.localScale;
            transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }


        public bool FacingCorrectly() => (game.stepIterate % 2 == 0) == (transform.localScale.x < 0);

        // animation event
        public void ToggleStepping(int canStep) // why do unity animation events not support booleans??? this is a 1 for true or 0 for false
        {
            canStepFromAnim = canStep == 1;
        }
    }
}