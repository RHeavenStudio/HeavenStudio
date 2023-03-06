using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Games;

namespace HeavenStudio.Common
{
    public class TimingAccuracyDisplay : MonoBehaviour
    {
        public enum Rating
        {
            NG,
            OK,
            Just
        }

        public static TimingAccuracyDisplay instance { get; private set; }

        [SerializeField] GameObject NG;
        [SerializeField] GameObject OK;
        [SerializeField] GameObject Just;
        [SerializeField] GameObject MinimalJust;
        [SerializeField] GameObject MinimalOK;
        [SerializeField] GameObject MinimalNG;

        [SerializeField] Animator MetreAnim;

        [SerializeField] Transform arrowTransform;
        [SerializeField] Transform barTransform;
        [SerializeField] Transform barJustTransform;
        [SerializeField] Transform barOKTransform;
        [SerializeField] Transform barNGTransform;

        float targetArrowPos = 0f;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            arrowTransform.localPosition = Vector3.Lerp(arrowTransform.localPosition, new Vector3(0, targetArrowPos, 0), 4f * Time.deltaTime);
        }

        public void ResetArrow()
        {
            targetArrowPos = 0f;
            arrowTransform.localPosition = Vector3.zero;
            StopStarFlash();

            NG.GetComponent<ParticleSystem>().Stop();
            OK.GetComponent<ParticleSystem>().Stop();
            Just.GetComponent<ParticleSystem>().Stop();
            MinimalNG.GetComponent<ParticleSystem>().Stop();
            MinimalOK.GetComponent<ParticleSystem>().Stop();
            MinimalJust.GetComponent<ParticleSystem>().Stop();
        }

        public void StartStarFlash()
        {
            MetreAnim.Play("StarWarn", -1, 0f);
        }

        public void StopStarFlash()
        {
            MetreAnim.Play("NoPose", -1, 0f);
        }

        public void MakeAccuracyVfx(double time, bool late = false)
        {
            if (!OverlaysManager.OverlaysEnabled) return;
            GameObject it;
            Rating type = Rating.NG;

            // centre of the transfor would be "perfect ace"
            // move the object up or down the bar depending on hit time
            // use bar's scale Y for now, we're waiting for proper assets

            // this probably doesn't work
            float frac = 0f;
            float y = barTransform.position.y;

            // SetArrowPos(time);

            // no Clamp() because double
            time = System.Math.Max(Minigame.EarlyTime(), System.Math.Min(Minigame.EndTime(), time));

            if (time >= Minigame.AceStartTime() && time <= Minigame.AceEndTime())
            {
                type = Rating.Just;
                frac = (float)((time - Minigame.AceStartTime()) / (Minigame.AceEndTime() - Minigame.AceStartTime()));
                y = barJustTransform.localScale.y * frac - (barJustTransform.localScale.y * 0.5f);
            }
            else
            {
                if (time > 1.0)
                {
                    // goes "down"
                    if (time <= Minigame.LateTime())
                    {
                        type = Rating.OK;
                        frac = (float)((time - Minigame.AceEndTime()) / (Minigame.LateTime() - Minigame.AceEndTime()));
                        y = ((barOKTransform.localScale.y - barJustTransform.localScale.y) * frac) + barJustTransform.localScale.y;
                    }
                    else
                    {
                        type = Rating.NG;
                        frac = (float)((time - Minigame.LateTime()) / (Minigame.EndTime() - Minigame.LateTime()));
                        y = ((barNGTransform.localScale.y - barOKTransform.localScale.y) * frac) + barOKTransform.localScale.y;
                    }
                }
                else
                {
                    // goes "up"
                    if (time >= Minigame.PerfectTime())
                    {
                        type = Rating.OK;
                        frac = (float)((time - Minigame.PerfectTime()) / (Minigame.AceStartTime() - Minigame.PerfectTime()));
                        y = ((barOKTransform.localScale.y - barJustTransform.localScale.y) * -frac) - barJustTransform.localScale.y;
                    }
                    else
                    {
                        type = Rating.NG;
                        frac = (float)((time - Minigame.EarlyTime()) / (Minigame.PerfectTime() - Minigame.EarlyTime()));
                        y = ((barNGTransform.localScale.y - barOKTransform.localScale.y) * -frac) - barOKTransform.localScale.y;
                    }
                }
                y *= -0.5f;
            }

            targetArrowPos = (targetArrowPos + y) * 0.5f;

            if (PersistentDataManager.gameSettings.timingDisplayMinMode)
            {
                switch (type)
                {
                    case Rating.OK:
                        it = MinimalOK;
                        break;
                    case Rating.Just:
                        it = MinimalJust;
                        break;
                    default:
                        it = MinimalNG;
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case Rating.OK:
                        it = OK;
                        break;
                    case Rating.Just:
                        it = Just;
                        break;
                    default:
                        it = NG;
                        break;
                }
            }

            it.transform.position = barTransform.position + new Vector3(0, barTransform.localScale.y * y, 0);
            it.GetComponent<ParticleSystem>().Play();
        }
    }
}