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
        [SerializeField] private Transform shadowTrans;
        [SerializeField] private Transform camMoveTrans;
        [Header("Properties")]
        [SerializeField] private float shadowXRange = 2f;
        [SerializeField] private float shadowYRange = 1f;

        private MonkeyWatch game;

        private void Awake()
        {
            game = MonkeyWatch.instance;
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(MonkeyWatch.InputAction_BasicPress) && !game.IsExpectingInputNow(MonkeyWatch.InputAction_BasicPress))
            {
                PlayerClap(false, false, true);
            }
        }

        public void Move()
        {
            anchorRotateTransform.localEulerAngles = new Vector3(0, 0, anchorRotateTransform.localEulerAngles.z - 6);
            anim.DoScaledAnimationAsync("Click", 0.4f);
            MoveShadow();
        }

        private void MoveShadow()
        {
            float realAngle = anchorRotateTransform.localEulerAngles.z % 360 - 360;
            realAngle *= -1;

            float x;
            float y;

            if (realAngle <= 180)
            {
                float normalizedAngle = MathUtils.Normalize(realAngle, 0, 180);
                x = Mathf.Lerp(0, shadowXRange, normalizedAngle);
            }
            else
            {
                float normalizedAngle = MathUtils.Normalize(realAngle, 180, 360);
                x = Mathf.Lerp(shadowXRange, 0, normalizedAngle);
            }

            float realAngleY = anchorRotateTransform.localEulerAngles.z % 180 - 180;
            realAngleY *= -1;

            if (realAngleY <= 90)
            {
                float normalizedAngle = MathUtils.Normalize(realAngleY, 0, 90);
                y = Mathf.Lerp(0, shadowYRange, normalizedAngle);
            }
            else
            {
                float normalizedAngle = MathUtils.Normalize(realAngleY, 90, 180);
                y = Mathf.Lerp(shadowYRange, 0, normalizedAngle);
            }
            shadowTrans.localPosition = new Vector3(x, y);
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
            if (playerMonkeyAnim.IsPlayingAnimationNames("PlayerClapBarely") && whiff) return;
            if (whiff)
            {
                game.middleMonkey.DoScaledAnimationAsync("MiddleMonkeyMiss", 0.4f);
                SoundByte.PlayOneShot("miss");
            }
            if (barely || whiff)
            {
                playerMonkeyAnim.DoScaledAnimationAsync("PlayerClapBarely", 0.4f);
            }
            else
            {
                playerMonkeyAnim.DoScaledAnimationAsync(big ? "PlayerClapBig" : "PlayerClap", 0.4f);
                ParticleSystem clapToSpawn = big ? pinkClap : yellowClap;
                ParticleSystem spawnedClap = Instantiate(clapToSpawn, camMoveTrans, true);
                spawnedClap.transform.eulerAngles = Vector3.zero;
                spawnedClap.transform.GetChild(0).GetComponent<ParticleSystem>().SetAsyncScaling(0.4f);
                spawnedClap.PlayScaledAsync(0.4f);
            }
        }
    }
}


