using System;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Common;

namespace HeavenStudio.Games
{

    public class PlayerActionEvent : MonoBehaviour
    {
        static List<PlayerActionEvent> allEvents = new List<PlayerActionEvent>();
        public static bool EnableAutoplayCheat = true;
        public delegate void ActionEventCallback(PlayerActionEvent caller);
        public delegate void ActionEventCallbackState(PlayerActionEvent caller, float state);
        public delegate bool ActionEventHittableQuery();

        public ActionEventCallbackState OnHit; //Function to trigger when an input has been done perfectly
        public ActionEventCallback OnMiss; //Function to trigger when an input has been missed
        public ActionEventCallback OnBlank; //Function to trigger when an input has been recorded while this is pending
        public ActionEventHittableQuery IsHittable; //Checks if an input can be hit. Returning false will skip button checks.

        public ActionEventCallback OnDestroy; //Function to trigger whenever this event gets destroyed. /!\ Shouldn't be used for a minigame! Use OnMiss instead /!\

        public PlayerInput.InputAction InputAction;

        public double startBeat;
        public double timer;
        public float weight = 1f;

        public bool isEligible = true;
        public bool canHit = true; //Indicates if you can still hit the cue or not. If set to false, it'll guarantee a miss
        public bool enabled = true; //Indicates if the PlayerActionEvent is enabled. If set to false, it'll not trigger any events and destroy itself AFTER it's not relevant anymore
        public bool triggersAutoplay = true;
        public string minigame;
        bool lockedByEvent = false;
        bool markForDeletion = false;

        float pitchWhenHit = 1f;

        public bool autoplayOnly = false; //Indicates if the input event only triggers when it's autoplay. If set to true, NO Miss or Blank events will be triggered when you're not autoplaying.

        public bool noAutoplay = false; //Indicates if this PlayerActionEvent is recognized by the autoplay. /!\ Overrides autoPlayOnly /!\

        public bool perfectOnly = false; //Indicates that the input only recognize perfect inputs.

        public bool countsForAccuracy = true; //Indicates if the input counts for the accuracy or not. If set to false, it'll not be counted in the accuracy calculation

        public bool missable = false; //Indicates if the miss input counts for the accuracy or not. If set to true, it'll not be counted in the accuracy calculation

        public void setHitCallback(ActionEventCallbackState OnHit)
        {
            this.OnHit = OnHit;
        }

        public void setMissCallback(ActionEventCallback OnMiss)
        {
            this.OnMiss = OnMiss;
        }

        public void setHittableQuery(ActionEventHittableQuery IsHittable)
        {
            this.IsHittable = IsHittable;
        }

        public void Enable() { enabled = true; }
        public void Disable() { enabled = false; }
        public void QueueDeletion() { markForDeletion = true; }

        public bool IsCorrectInput(out double dt)
        {
            dt = 0;
            if (InputAction != null)
            {
                return PlayerInput.GetIsAction(InputAction, out dt);
            }
            return false;
        }

        public void CanHit(bool canHit)
        {
            this.canHit = canHit;
        }

        public void Start()
        {
            allEvents.Add(this);
        }

        public void Update()
        {
            Conductor cond = Conductor.instance;
            GameManager gm = GameManager.instance;
            if (markForDeletion) CleanUp();
            if (!cond.NotStopped()) CleanUp(); // If the song is stopped entirely in the editor, destroy itself as we don't want duplicates

            if (noAutoplay && autoplayOnly) autoplayOnly = false;
            if (noAutoplay && triggersAutoplay) triggersAutoplay = false;
            if (!enabled) return;
            if (minigame != GameManager.instance.currentGame) return;

            double normalizedTime = GetNormalizedTime();
            if (gm.autoplay && gm.canInput)
            {
                AutoplayInput(normalizedTime);
                return;
            }

            //BUGFIX: ActionEvents destroyed too early
            if (normalizedTime > Minigame.NgLateTime(cond.SongPitch)) Miss();

            if (lockedByEvent)
            {
                return;
            }
            if (!CheckEventLock())
            {
                return;
            }

            if (!autoplayOnly && (IsHittable == null || IsHittable != null && IsHittable()) && IsCorrectInput(out double dt))
            {
                normalizedTime -= dt;
                if (IsExpectingInputNow())
                {
                    double stateProg = ((normalizedTime - Minigame.JustEarlyTime()) / (Minigame.JustLateTime() - Minigame.JustEarlyTime()) - 0.5f) * 2;
                    Hit(stateProg, normalizedTime);
                }
                else
                {
                    Blank();
                }
            }
        }

        public void LateUpdate()
        {
            if (markForDeletion)
            {
                allEvents.Remove(this);
                OnDestroy(this);
                Destroy(this.gameObject);
            }
            foreach (PlayerActionEvent evt in allEvents)
            {
                evt.lockedByEvent = false;
            }
        }

        private bool CheckEventLock()
        {
            foreach (PlayerActionEvent toCompare in allEvents)
            {
                if (toCompare == this) continue;
                if (toCompare.autoplayOnly) continue;
                if (InputAction != null)
                {
                    if (toCompare.InputAction == null) continue;
                    int catIdx = (int)PlayerInput.CurrentControlStyle;
                    if (toCompare.InputAction != null
                        && toCompare.InputAction.inputLockCategory[catIdx] != InputAction.inputLockCategory[catIdx]) continue;
                }

                double t1 = this.startBeat + this.timer;
                double t2 = toCompare.startBeat + toCompare.timer;
                double songPos = Conductor.instance.songPositionInBeatsAsDouble;

                // compare distance between current time and the events
                // events that happen at the exact same time with the exact same inputs will return true
                if (Math.Abs(t1 - songPos) > Math.Abs(t2 - songPos))
                    return false;
                else if (t1 != t2)  // if they are the same time, we don't want to lock the event
                    toCompare.lockedByEvent = true;
            }
            return true;
        }

        private void AutoplayInput(double normalizedTime, bool autoPlay = false)
        {
            if (triggersAutoplay && (GameManager.instance.autoplay || autoPlay) && normalizedTime >= 1f - (Time.deltaTime * 0.5f))
            {
                AutoplayEvent();
                if (!autoPlay)
                    TimelineAutoplay();
            }
        }

        // TODO: move this to timeline code instead
        private void TimelineAutoplay()
        {
            if (Editor.Editor.instance == null) return;
            if (!GameManager.instance.canInput) return;
            if (Editor.Track.Timeline.instance != null && !Editor.Editor.instance.fullscreen)
            {
                Editor.Track.Timeline.instance.AutoplayBTN.GetComponent<Animator>().Play("Ace", 0, 0);
            }
        }

        public bool IsExpectingInputNow()
        {
            if (IsHittable != null)
            {
                if (!IsHittable()) return false;
            }
            if (!enabled) return false;
            if (!isEligible) return false;

            double normalizedBeat = GetNormalizedTime();
            return normalizedBeat > Minigame.NgEarlyTime() && normalizedBeat < Minigame.NgLateTime();
        }

        double GetNormalizedTime()
        {
            var cond = Conductor.instance;
            double currTime = cond.songPositionAsDouble;
            double targetTime = cond.GetSongPosFromBeat(startBeat + timer);

            // HS timing window uses 1 as the middle point instead of 0
            return 1 + (currTime - targetTime);
        }

        //For the Autoplay
        public void AutoplayEvent()
        {
            if (!GameManager.instance.canInput)
            {
                CleanUp();
                return;
            }
            if (EnableAutoplayCheat)
            {
                Hit(0f, 1f);
            }
            else
            {
                double normalizedBeat = GetNormalizedTime();
                double stateProg = ((normalizedBeat - Minigame.JustEarlyTime()) / (Minigame.JustLateTime() - Minigame.JustEarlyTime()) - 0.5f) * 2;
                Hit(stateProg, normalizedBeat);
            }
        }

        //The state parameter is either -1 -> Early, 0 -> Perfect, 1 -> Late
        public void Hit(double state, double time)
        {
            GameManager gm = GameManager.instance;
            if (OnHit != null && enabled)
            {
                if (canHit)
                {
                    CleanUp();
                    pitchWhenHit = Conductor.instance.SongPitch;
                    double normalized = time - 1f;
                    int offset = Mathf.CeilToInt((float)normalized * 1000);
                    if (gm.canInput)
                    {
                        gm.AvgInputOffset = offset;
                    }
                    state = System.Math.Max(-1.0, System.Math.Min(1.0, state));

                    if (countsForAccuracy && gm.canInput && !(noAutoplay || autoplayOnly) && isEligible)
                    {
                        gm.ScoreInputAccuracy(startBeat + timer, TimeToAccuracy(time, pitchWhenHit), time > 1.0, time, weight, true);
                        if (state >= 1f || state <= -1f)
                        {
                            GoForAPerfect.instance.Miss();
                            SectionMedalsManager.instance.MakeIneligible();
                        }
                        else
                        {
                            GoForAPerfect.instance.Hit();
                        }
                    }
                    OnHit(this, (float)state);
                }
                else
                {
                    Blank();
                }
            }
        }

        double TimeToAccuracy(double time, float pitch = -1)
        {
            if (pitch < 0) pitch = pitchWhenHit;
            if (time >= Minigame.AceEarlyTime(pitch) && time <= Minigame.AceLateTime(pitch))
            {
                // Ace
                return 1.0;
            }

            double state = 0;
            if (time >= Minigame.JustEarlyTime(pitch) && time <= Minigame.JustLateTime(pitch))
            {
                // Good Hit
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.AceLateTime(pitch)) / (Minigame.JustLateTime(pitch) - Minigame.AceLateTime(pitch)));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.JustEarlyTime(pitch)) / (Minigame.AceEarlyTime(pitch) - Minigame.JustEarlyTime(pitch)));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
            }
            else
            {
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.JustLateTime(pitch)) / (Minigame.NgLateTime(pitch) - Minigame.JustLateTime(pitch)));
                    state *= Minigame.rankOkThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.JustEarlyTime(pitch)) / (Minigame.AceEarlyTime(pitch) - Minigame.JustEarlyTime(pitch)));
                    state *= Minigame.rankOkThreshold;
                }
            }
            return state;
        }

        public void Miss()
        {
            GameManager gm = GameManager.instance;
            CleanUp();
            if (OnMiss != null && enabled && !autoplayOnly)
            {
                OnMiss(this);
            }

            if (countsForAccuracy && !missable && gm.canInput && !(noAutoplay || autoplayOnly))
            {
                gm.ScoreInputAccuracy(startBeat + timer, 0, true, 2.0, weight, false);
                GoForAPerfect.instance.Miss();
                SectionMedalsManager.instance.MakeIneligible();
            }
        }

        public void Blank()
        {
            if (OnBlank != null && enabled && !autoplayOnly)
            {
                OnBlank(this);
            }
        }

        public void CleanUp()
        {
            if (markForDeletion) return;
            markForDeletion = true;
        }
    }
}