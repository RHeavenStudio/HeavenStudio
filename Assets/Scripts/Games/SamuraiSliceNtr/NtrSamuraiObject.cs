using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_NtrSamurai
{
    public class NtrSamuraiObject : MonoBehaviour
    {
        [Header("Objects")]
        public ParticleSystem moneyBurst;
        public ParticleSystem pickelBurst;
        public Animator anim;
        public NtrSamuraiObject secondHalf;

        [Header("Transforms")]
        public Transform doubleLaunchPos;
        public Transform heldPos;
        
        public float startBeat;
        public int type;
        public bool isDebris = false;
        public int holdingCash = 1;

        BezierCurve3D currentCurve;
        int flyProg = 0;
        bool flying = true;
        bool missedLaunch = false;
        bool missedHit = false;

        PlayerActionEvent launchProg;
        PlayerActionEvent hitProg;

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
                    case (int) SamuraiSliceNtr.ObjectType.Melon2B2T:
                        anim.Play("ObjMelonPickelDebris02");
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
                    case (int) SamuraiSliceNtr.ObjectType.Melon2B2T:
                        anim.Play("ObjMelonPickel");
                        break;
                    default:
                        anim.Play("ObjMelon");
                        break;
                }

                launchProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, LaunchSuccess, LaunchMiss, LaunchThrough);
                //autoplay: launch anim
                SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat, 2f, InputType.STANDARD_ALT_DOWN, DoLaunchAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;
                //autoplay: unstep
                SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat, 1.75f, InputType.STANDARD_ALT_UP, DoUnStepAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;

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
                    case -3:    // near miss on board launch
                        flyPos = cond.GetPositionFromBeat(startBeat, 2f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (180f * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;
                    case -2:    // being carried by a child
                        flyPos = cond.GetPositionFromBeat(startBeat + 2f, 4f);
                        if (heldPos == null || flyPos > 1f)
                        {
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        transform.position = heldPos.position;
                        break;
                    case -1:    // sliced by samurai, falling towards child
                        flyPos = cond.GetPositionFromBeat(startBeat, 1f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + ((isDebris? 360f : -360f) * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_catch");
                            if (!isDebris)
                            {
                                NtrSamuraiChild child = SamuraiSliceNtr.instance.CreateChild(startBeat + 1f);
                                heldPos = child.DebrisPosR;
                                secondHalf.heldPos = child.DebrisPosL;
                            }
                            flyProg = -2;
                            GetComponent<SpriteRenderer>().sortingOrder = 7;
                            return;
                        }
                        break;
                    case 2:     // fish first bounce
                        float jumpPos = cond.GetPositionFromBeat(launchProg.startBeat, 2f);
                        float yMul = jumpPos * 2f - 1f;
                        float yWeight = -(yMul*yMul) + 1f;
                        transform.position = doubleLaunchPos.position + new Vector3(0, 4.5f * yWeight);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-2 * 360f * Time.deltaTime));
                        
                        if (jumpPos > 2f)
                        {
                            // missed...
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;
                    case 1:     // launched from board to samurai
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
                            // missed...
                            GameObject.Destroy(gameObject);
                            return;
                        }
                        break;

                    default:    // object initial spawn, flying towards board
                        flyPos = cond.GetPositionFromBeat(launchProg.startBeat, 3f);
                        transform.position = currentCurve.GetPoint(flyPos);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime));

                        if (flyPos > 1f)
                        {
                            // missed...
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
                        SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 2f, InputType.STANDARD_ALT_DOWN, DoLaunchAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;
                        //autoplay: unstep
                        SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 1.75f, InputType.STANDARD_ALT_UP, DoUnStepAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;
                        currentCurve = null;

                        Jukebox.PlayOneShotGame("samuraiSliceNtr/holy_mackerel" + UnityEngine.Random.Range(1, 4), pitch: UnityEngine.Random.Range(0.95f, 1.05f), volume: 0.8f);
                    }
                    break;
                case (int) SamuraiSliceNtr.ObjectType.Demon:
                    flyProg = 1;
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 4f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 4f, InputType.STANDARD_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;
                    currentCurve = SamuraiSliceNtr.instance.LaunchHighCurve;
                    break;
                default:
                    flyProg = 1;
                    hitProg = SamuraiSliceNtr.instance.ScheduleInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, HitSuccess, HitMiss, LaunchThrough);
                    SamuraiSliceNtr.instance.ScheduleAutoplayInput(startBeat + 2f, 2f, InputType.STANDARD_DOWN, DoSliceAutoplay, LaunchThrough, LaunchThrough).countsForAccuracy = false;
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
            if (state <= -1f || state >= 1f)
            {
                startBeat = launchProg.startBeat + 2f;
                currentCurve = SamuraiSliceNtr.instance.NgLaunchCurve;
                flyProg = -3;
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_launchImpact", pitch: 2f);
                launchProg.Disable();
                return;
            }
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
            if (state <= -1f || state >= 1f)
            {
                startBeat = hitProg.startBeat + hitProg.timer;
                currentCurve = SamuraiSliceNtr.instance.NgDebrisCurve;
                flyProg = -3;
                Jukebox.PlayOneShotGame("samuraiSliceNtr/ntrSamurai_ng");
                hitProg.Disable();
                return;
            }
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
            mobj.GetComponent<SpriteRenderer>().sortingOrder = 4;
            mobj.SetActive(true);

            secondHalf = mobjDat;

            this.startBeat = caller.startBeat + caller.timer;
            if (type == (int) SamuraiSliceNtr.ObjectType.Demon)
                anim.Play("ObjDemonDebris01");
            else if (type == (int) SamuraiSliceNtr.ObjectType.Melon2B2T)
            {
                Jukebox.PlayOneShotGame("samuraiSliceNtr/melon_dig");
                pickelBurst.Play();
                anim.Play("ObjMelonPickelDebris01");
            }

            if (holdingCash > 0)
            {
                moneyBurst.Emit(holdingCash);
                Jukebox.PlayOneShotGame((holdingCash > 2) ? "samuraiSliceNtr/ntrSamurai_scoreMany" : "samuraiSliceNtr/ntrSamurai_ng", pitch: UnityEngine.Random.Range(0.95f, 1.05f));
            }
        }

        public void HitMiss(PlayerActionEvent caller)
        {
            missedHit = true;
        }
    }
}