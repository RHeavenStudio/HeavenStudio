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
        public Animator anim;
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
            switch (type)
            {
                case (int) SamuraiSliceNtr.ObjectType.Demon:
                    anim.Play("ObjDemon");
                    break;
                default:
                    anim.Play("ObjMelon");
                    break;
            }

            launchProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, LaunchSuccess, LaunchMiss, LaunchThrough);
            SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, DoLaunchAutoplay, LaunchThrough, LaunchThrough);
            currentCurve = SamuraiSliceNtr.instance.InCurve;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * startBeat));

            var cond = Conductor.instance;
            float flyPos = cond.GetPositionFromBeat(launchProg.startBeat, 3f);
            transform.position = currentCurve.GetPoint(flyPos);
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime));
        }

        void Update()
        {
            var cond = Conductor.instance;
            float flyPos;
            if (flying)
            {
                switch (flyProg)
                {
                    case 2:
                        // TODO: fishe bounce
                        break;
                    case 1:
                        float flyDur = 3f;
                        switch (type)
                        {
                            case (int) SamuraiSliceNtr.ObjectType.Demon:
                                flyDur = 5f;
                                break;
                            default:
                                flyDur = 3f;
                                break;
                        }
                        flyPos = cond.GetPositionFromBeat(hitProg.startBeat, flyDur);
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
            switch (type)
            {
                case (int) SamuraiSliceNtr.ObjectType.Fish:
                    if (flyProg == 2)
                    {
                        //did second bounce, start returning to samurai
                    }
                    else 
                    {
                        //start first bounce
                    }
                    break;
                case (int) SamuraiSliceNtr.ObjectType.Demon:
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 4f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 4f, InputType.STANDARD_ALT_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough);
                    currentCurve = SamuraiSliceNtr.instance.LaunchHighCurve;
                    break;
                default:
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 2f, InputType.STANDARD_ALT_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough);
                    currentCurve = SamuraiSliceNtr.instance.LaunchCurve;
                    break;
            }
        }

        void DoLaunchAutoplay(PlayerActionEvent caller, float state)
        {
            SamuraiSliceNtr.instance.DoStep();
        }

        void DoSliceAutoplay(PlayerActionEvent caller, float state)
        {
            SamuraiSliceNtr.instance.DoSlice();
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
            flyProg = -1;
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