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
        public ParticleSystem moneyBurst;
        public Animator anim;
        public float startBeat;
        public int type;
        public bool isDebris = false;
        public PlayerActionEvent launchProg;
        public PlayerActionEvent hitProg;
        BezierCurve3D currentCurve;
        public Transform doubleLaunchPos;

        int flyProg = 0;
        public int holdingCash = 1;
        bool flying = true;
        bool missedLaunch = false;
        bool missedHit = false;

        void Awake()
        {
            if (isDebris)
            {
                switch (type)
                {
                    case (int) SamuraiSliceNtr.ObjectType.Fish:
                        anim.Play("ObjFishDebris");
                        break;
                    case (int) SamuraiSliceNtr.ObjectType.Demon:
                        anim.Play("ObjDemonDebris02");
                        break;
                    default:
                        anim.Play("ObjMelonDebris");
                        break;
                }
                currentCurve = SamuraiSliceNtr.instance.DebrisLeftCurve;

                var cond = Conductor.instance;
                float flyPos = cond.GetPositionFromBeat(startBeat, 1f);
                transform.position = currentCurve.GetPoint(flyPos);
            }
            else
            {
                switch (type)
                {
                    case (int) SamuraiSliceNtr.ObjectType.Fish:
                        anim.Play("ObjFish");
                        break;
                    case (int) SamuraiSliceNtr.ObjectType.Demon:
                        anim.Play("ObjDemon");

                        MultiSound.Play(new MultiSound.Sound[] { 
                            new MultiSound.Sound("samuraiSliceNtr/ntrSamurai_in01", startBeat + 1f, 1.5f), 
                            new MultiSound.Sound("samuraiSliceNtr/ntrSamurai_in01", startBeat + 1.5f, 1.25f),
                            new MultiSound.Sound("samuraiSliceNtr/ntrSamurai_in01", startBeat + 2f),
                        });
                        break;
                    default:
                        anim.Play("ObjMelon");
                        break;
                }

                launchProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, LaunchSuccess, LaunchMiss, LaunchThrough);
                //autoplay: launch anim
                SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, DoLaunchAutoplay, LaunchThrough, LaunchThrough);
                //autoplay: unstep
                SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat, 1.75f, InputType.STANDARD_ALT_UP, DoUnStepAutoplay, LaunchThrough, LaunchThrough);

                currentCurve = SamuraiSliceNtr.instance.InCurve;
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * startBeat));

                var cond = Conductor.instance;
                float flyPos = cond.GetPositionFromBeat(launchProg.startBeat, 3f);
                transform.position = currentCurve.GetPoint(flyPos);
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime) + UnityEngine.Random.Range(0f, 180f));
            }
        }

        void Update()
        {
            var cond = Conductor.instance;
            float flyPos;
            if (flying)
            {
                switch (flyProg)
                {
                    case -1:
                        flyPos = cond.GetPositionFromBeat(startBeat, 1f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + ((isDebris? 360f : -360f) * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_catch");
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;
                    case 2:
                        float jumpPos = cond.GetPositionFromBeat(launchProg.startBeat, 2f);
                        float yMul = jumpPos * 2f - 1f;
                        float yWeight = -(yMul*yMul) + 1f;
                        transform.position = doubleLaunchPos.position + new Vector3(0, 4.5f * yWeight);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-2 * 360f * Time.deltaTime));
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
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (3 * 360f * Time.deltaTime));

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
                        flyProg = 1;
                        hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 4f, 2f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                        SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 4f, 2f, InputType.STANDARD_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough);
                        currentCurve = SamuraiSliceNtr.instance.LaunchCurve;

                        Jukebox.PlayOneShotGame("samuraiSliceNtr/holy_mackerel" + UnityEngine.Random.Range(1, 4), pitch: UnityEngine.Random.Range(0.95f, 1.05f), volume: 1f/4);
                    }
                    else 
                    {
                        flyProg = 2;
                        launchProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 2f, InputType.STANDARD_ALT_DOWN, LaunchSuccess, LaunchMiss, LaunchThrough);
                        SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 2f, InputType.STANDARD_ALT_DOWN, DoLaunchAutoplay, LaunchThrough, LaunchThrough);
                        //autoplay: unstep
                        SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 1.75f, InputType.STANDARD_ALT_UP, DoUnStepAutoplay, LaunchThrough, LaunchThrough);
                        currentCurve = null;

                        Jukebox.PlayOneShotGame("samuraiSliceNtr/holy_mackerel" + UnityEngine.Random.Range(1, 4), pitch: UnityEngine.Random.Range(0.95f, 1.05f), volume: 0.8f);
                    }
                    break;
                case (int) SamuraiSliceNtr.ObjectType.Demon:
                    flyProg = 1;
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 4f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 4f, InputType.STANDARD_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough);
                    currentCurve = SamuraiSliceNtr.instance.LaunchHighCurve;
                    break;
                default:
                    flyProg = 1;
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough);
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

        void DoUnStepAutoplay(PlayerActionEvent caller, float state)
        {
            if (SamuraiSliceNtr.instance.player.stepping)
            {
                SamuraiSliceNtr.instance.DoUnStep();
            }
        }

        public void LaunchSuccess(PlayerActionEvent caller, float state)
        {
            launchProg.Disable();
            DoLaunch();
            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchImpact", pitch: UnityEngine.Random.Range(0.85f, 1.05f));

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
            if (UnityEngine.Random.Range(0f, 1f) >= 0.5f)
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_just00", pitch: UnityEngine.Random.Range(0.95f, 1.05f));
            else
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_just01", pitch: UnityEngine.Random.Range(0.95f, 1.05f));

            currentCurve = SamuraiSliceNtr.instance.DebrisRightCurve;

            var mobj = GameObject.Instantiate(SamuraiSliceNtr.instance.objectPrefab, SamuraiSliceNtr.instance.objectHolder);
            var mobjDat = mobj.GetComponent<NtrSamuraiObject>();
            mobjDat.startBeat = caller.startBeat + caller.timer;
            mobjDat.type = type;
            mobjDat.isDebris = true;
            mobjDat.flyProg = -1;

            mobj.transform.position = transform.position;
            mobj.transform.rotation = transform.rotation;
            mobj.SetActive(true);

            this.startBeat = caller.startBeat + caller.timer;
            if (type == (int) SamuraiSliceNtr.ObjectType.Demon)
            {
                anim.Play("ObjDemonDebris01");
            }

            if (holdingCash > 0)
            {
                moneyBurst.Emit(holdingCash);
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_scoreMany", pitch: UnityEngine.Random.Range(0.95f, 1.05f));
            }
        }

        public void HitMiss(PlayerActionEvent caller)
        {
            missedHit = true;
        }
    }
}