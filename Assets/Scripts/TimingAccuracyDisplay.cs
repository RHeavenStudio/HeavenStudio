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

        public static TimingAccuracyDisplay instance;

        [SerializeField] GameObject NG;
        [SerializeField] GameObject OK;
        [SerializeField] GameObject Just;
        [SerializeField] Transform barTransform;
        [SerializeField] Transform barJustTransform;
        [SerializeField] Transform barOKTransform;
        [SerializeField] Transform barNGTransform;


        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void MakeAccuracyVfx(double time, bool late = false)
        {
            GameObject it;
            Rating type = Rating.NG;

            // centre of the transfor would be "perfect ace"
            // move the object up or down the bar depending on hit time
            // use bar's scale Y for now, we're waiting for proper assets

            // this probably doesn't work
            float frac = 0f;
            float y = barTransform.position.y;
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
                    // goes "up"
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
                    // goes "down"
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
                y *= 0.5f;
            }

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
            it.transform.position = barTransform.position + new Vector3(0, barTransform.localScale.y * y, 0);
            it.GetComponent<ParticleSystem>().Play();
        }
    }
}