using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_MonkeyWatch
{
    public class MonkeyClockArrow : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator anim;
        [SerializeField] private Transform anchorRotateTransform;
        [SerializeField] private Animator playerMonkeyAnim;
        [SerializeField] private ParticleSystem yellowClap;
        [SerializeField] private ParticleSystem pinkClap;

        private MonkeyWatch game;

        private void Awake()
        {
            game = MonkeyWatch.instance;
        }

        private void Update()
        {
            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                PlayerClap(false, false, true);
            }
        }

        public void Move()
        {
            anchorRotateTransform.localEulerAngles = new Vector3(0, 0, anchorRotateTransform.localEulerAngles.z - 6);
            anim.DoScaledAnimationAsync("Click", 0.4f);
        }

        public void MoveToAngle(float angle)
        {
            anchorRotateTransform.localEulerAngles = new Vector3(0, 0, -angle);
        }

        public bool PlayerIsClapAnim()
        {
            return !playerMonkeyAnim.IsAnimationNotPlaying();
        }

        public void PlayerClap(bool big, bool barely, bool whiff)
        {
            if (!playerMonkeyAnim.IsAnimationNotPlaying() && whiff) return;

            if (barely || whiff)  
            {
                playerMonkeyAnim.DoScaledAnimationAsync("PlayerClapBarely", 0.4f);
            }
            else
            {
                playerMonkeyAnim.DoScaledAnimationAsync(big ? "PlayerClapBig" : "PlayerClap", 0.4f);
                if (big)
                {
                    pinkClap.transform.eulerAngles = Vector3.zero;
                    pinkClap.transform.GetChild(0).GetComponent<ParticleSystem>().SetAsyncScaling(0.4f);
                    pinkClap.PlayScaledAsync(0.4f);
                }
                else
                {
                    yellowClap.transform.eulerAngles = Vector3.zero;
                    yellowClap.transform.GetChild(0).GetComponent<ParticleSystem>().SetAsyncScaling(0.4f);
                    yellowClap.PlayScaledAsync(0.4f);
                }
            }
        }
    }
}


