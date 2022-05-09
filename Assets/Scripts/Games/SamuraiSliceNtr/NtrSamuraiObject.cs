using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_NtrSamurai
{
    public class NtrSamuraiObject : MonoBehaviour
    {

        public float startBeat;
        public int type;
        public PlayerActionEvent launchProg;
        public PlayerActionEvent hitProg;
        BezierCurve3D currentCurve;

        int flyProg = 0;
        bool flying = true;
        bool missedLaunch = false;
        bool missedHit = false;

        void Awake()
        {
            launchProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, LaunchSuccess, LaunchMiss, LaunchThrough);
            currentCurve = SamuraiSliceNtr.instance.InCurve;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * startBeat));
        }

        void Update()
        {
            var cond = Conductor.instance;
            float flyPos;
            if (flying)
            {
                switch (flyProg)
                {
                    case 1:
                        flyPos = cond.GetPositionFromBeat(hitProg.startBeat, 3f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (4 * 360f * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;

                    default:
                        flyPos = cond.GetPositionFromBeat(launchProg.startBeat, 3f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;
                }
            }
        }

        void DoLaunch()
        {
            // todo: other launches
            hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
            currentCurve = SamuraiSliceNtr.instance.LaunchCurve;
        }

        public void LaunchSuccess(PlayerActionEvent caller, float state)
        {
            DoLaunch();
            flyProg = 1;
            launchProg.Disable();
            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchImpact");

        }

        public void LaunchMiss(PlayerActionEvent caller)
        {
            missedLaunch = true;
        }

        public void LaunchThrough(PlayerActionEvent caller) {}

        public void HitSuccess(PlayerActionEvent caller, float state)
        {
            flyProg = 2;
            hitProg.Disable();
            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchImpact");

            //todo: debris
            GameObject.Destroy(gameObject);
        }

        public void HitMiss(PlayerActionEvent caller)
        {
            missedHit = true;
        }
    }
}