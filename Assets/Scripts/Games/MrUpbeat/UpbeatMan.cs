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
        [SerializeField] GameObject[] shadows;
        [SerializeField] TMP_Text blipText;

        public int blipSize = 0;
        public bool shouldGrow;
        public bool shouldBlip = true;
        public string blipString = "M";

        static MrUpbeat game; 

        void Awake()
        {
            game = MrUpbeat.instance;
        }

        void Update() 
        {
            blipText.transform.localScale = Vector3.one;
            
            if (PlayerInput.GetIsAction(MrUpbeat.InputAction_BasicPress) && !game.IsExpectingInputNow(MrUpbeat.InputAction_BasicPress)) {
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
                Blipping(beat);
            }
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 1, delegate { RecursiveBlipping(beat + 1); })
            });
        }

        public void Blipping(double beat)
        {
            SoundByte.PlayOneShotGame("mrUpbeat/blip");
            blipAnim.Play("Blip"+(blipSize+1), 0, 0);
            blipText.text = (blipSize == 4 && blipString != "") ? blipString : "";
            if (shouldGrow && blipSize < 4) blipSize++;
        }

        public void Step(bool isInput = false)
        {
            if (isInput || ((game.stepIterate % 2 == 0) == IsMirrored())) {
                shadows[0].SetActive(IsMirrored());
                shadows[1].SetActive(!IsMirrored());
                transform.localScale = new Vector3((IsMirrored() ? 1 : -1), 1, 1);
            }
            
            anim.DoScaledAnimationAsync("Step", 0.5f);
            SoundByte.PlayOneShotGame("mrUpbeat/step");
        }

        public void Fall()
        {
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