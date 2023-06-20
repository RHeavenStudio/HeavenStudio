using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Util;
using Starpelly;

using HeavenStudio.Common;

namespace HeavenStudio.Games
{

    public class PlayerActionEvent : MonoBehaviour
    {
        static List<PlayerActionEvent> allEvents = new List<PlayerActionEvent>();
        public static bool EnableAutoplayCheat = true;
        public delegate void ActionEventCallback(PlayerActionEvent caller);
        public delegate void ActionEventCallbackState(PlayerActionEvent caller, float state);

        public ActionEventCallbackState OnHit; //Function to trigger when an input has been done perfectly
        public ActionEventCallback OnMiss; //Function to trigger when an input has been missed
        public ActionEventCallback OnBlank; //Function to trigger when an input has been recorded while this is pending

        public ActionEventCallback OnDestroy; //Function to trigger whenever this event gets destroyed. /!\ Shouldn't be used for a minigame! Use OnMiss instead /!\

        public double startBeat;
        public double timer;

        public bool isEligible = true;
        public bool canHit  = true; //Indicates if you can still hit the cue or not. If set to false, it'll guarantee a miss
        public bool enabled = true; //Indicates if the PlayerActionEvent is enabled. If set to false, it'll not trigger any events and destroy itself AFTER it's not relevant anymore
        public bool triggersAutoplay = true;
        bool lockedByEvent = false;
        bool markForDeletion = false;

        public bool autoplayOnly = false; //Indicates if the input event only triggers when it's autoplay. If set to true, NO Miss or Blank events will be triggered when you're not autoplaying.

        public bool noAutoplay = false; //Indicates if this PlayerActionEvent is recognized by the autoplay. /!\ Overrides autoPlayOnly /!\

        public InputType inputType; //The type of input. Check the InputType class to see a list of all of them

        public bool perfectOnly = false; //Indicates that the input only recognize perfect inputs.
        
        public bool countsForAccuracy = true; //Indicates if the input counts for the accuracy or not. If set to false, it'll not be counted in the accuracy calculation

        public void setHitCallback(ActionEventCallbackState OnHit)
        { 
            this.OnHit = OnHit;
        }

        public void setMissCallback(ActionEventCallback OnMiss)
        {
            this.OnMiss = OnMiss;
        }

        public void Enable()  { enabled = true; }
        public void Disable() { enabled = false; }
        public void QueueDeletion() { markForDeletion = true; }

        public bool IsCorrectInput() =>
            //General inputs, both down and up
            (PlayerInput.Pressed() && inputType.HasFlag(InputType.STANDARD_DOWN)) ||
            (PlayerInput.AltPressed() && inputType.HasFlag(InputType.STANDARD_ALT_DOWN)) ||
            (PlayerInput.GetAnyDirectionDown() && inputType.HasFlag(InputType.DIRECTION_DOWN)) ||
            (PlayerInput.PressedUp() && inputType.HasFlag(InputType.STANDARD_UP)) ||
            (PlayerInput.AltPressedUp() && inputType.HasFlag(InputType.STANDARD_ALT_UP)) ||
            (PlayerInput.GetAnyDirectionUp() && inputType.HasFlag(InputType.DIRECTION_UP)) ||
            //Specific directional inputs
            (PlayerInput.GetSpecificDirectionDown(PlayerInput.DOWN) && inputType.HasFlag(InputType.DIRECTION_DOWN_DOWN)) ||
            (PlayerInput.GetSpecificDirectionDown(PlayerInput.UP) && inputType.HasFlag(InputType.DIRECTION_UP_DOWN)) ||
            (PlayerInput.GetSpecificDirectionDown(PlayerInput.LEFT) && inputType.HasFlag(InputType.DIRECTION_LEFT_DOWN)) ||
            (PlayerInput.GetSpecificDirectionDown(PlayerInput.RIGHT) && inputType.HasFlag(InputType.DIRECTION_RIGHT_DOWN)) ||

            (PlayerInput.GetSpecificDirectionUp(PlayerInput.DOWN) && inputType.HasFlag(InputType.DIRECTION_DOWN_UP)) ||
            (PlayerInput.GetSpecificDirectionUp(PlayerInput.UP) && inputType.HasFlag(InputType.DIRECTION_UP_UP)) ||
            (PlayerInput.GetSpecificDirectionUp(PlayerInput.LEFT) && inputType.HasFlag(InputType.DIRECTION_LEFT_UP)) ||
            (PlayerInput.GetSpecificDirectionUp(PlayerInput.RIGHT) && inputType.HasFlag(InputType.DIRECTION_RIGHT_UP));

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
            if (markForDeletion) CleanUp();
            if(!Conductor.instance.NotStopped()) CleanUp(); // If the song is stopped entirely in the editor, destroy itself as we don't want duplicates

            if (noAutoplay && autoplayOnly) autoplayOnly = false;
            if (noAutoplay && triggersAutoplay) triggersAutoplay = false;
            if (!enabled) return;

            double normalizedTime = GetNormalizedTime();
            if (GameManager.instance.autoplay)
            {
                AutoplayInput(normalizedTime);
                return;
            }

            //BUGFIX: ActionEvents destroyed too early
            if (normalizedTime > Minigame.EndTime()) Miss();

            if (lockedByEvent)
            {
                return;
            }
            if (!CheckEventLock())
            {
                return;
            }
            
            if (!autoplayOnly && IsCorrectInput())
            {
                if (IsExpectingInputNow())
                {
                    double stateProg = ((normalizedTime - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
                    Hit(stateProg, normalizedTime);
                }
                else
                {
                    Blank();
                }
            }
        }

        public void LateUpdate() {
            if (markForDeletion) {
                CleanUp();
                Destroy(this.gameObject);
            }
            foreach (PlayerActionEvent evt in allEvents)
            {
                evt.lockedByEvent = false;
            }
        }

        private bool CheckEventLock()
        {
            foreach(PlayerActionEvent toCompare in allEvents)
            {
                if (toCompare == this) continue;
                if (toCompare.autoplayOnly) continue;
                if ((toCompare.inputType & this.inputType) == 0) continue;
                if (!toCompare.IsExpectingInputNow()) continue;

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
            if (triggersAutoplay && (GameManager.instance.autoplay || autoPlay) && GameManager.instance.canInput && normalizedTime >= 1f - (Time.deltaTime*0.5f))
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
            if (Editor.Track.Timeline.instance != null && !Editor.Editor.instance.fullscreen)
            {
                Editor.Track.Timeline.instance.AutoplayBTN.GetComponent<Animator>().Play("Ace", 0, 0);
            }
        }

        public bool IsExpectingInputNow()
        {
            double normalizedBeat = GetNormalizedTime();
            return normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.EndTime();
        }

        double GetNormalizedTime()
        {
            var cond = Conductor.instance;
            double currTime = cond.songPositionAsDouble;
            double targetTime = cond.GetSongPosFromBeat(startBeat + timer);
            double min = targetTime - 1f;
            double max = targetTime + 1f;
            return 1f + (((currTime - min) / (max - min))-0.5f)*2;
        }

        //For the Autoplay
        public void AutoplayEvent()
        {
            if (EnableAutoplayCheat)
            {
                Hit(0f, 1f);
            }
            else
            {
                double normalizedBeat = GetNormalizedTime();
                double stateProg = ((normalizedBeat - Minigame.PerfectTime()) / (Minigame.LateTime() - Minigame.PerfectTime()) - 0.5f) * 2;
                Hit(stateProg, normalizedBeat);
            }
        }

        //The state parameter is either -1 -> Early, 0 -> Perfect, 1 -> Late
        public void Hit(double state, double time) 
        {
            if (OnHit != null && enabled)
            {
                if(canHit)
                {
                    double normalized = time - 1f;
                    int offset = Mathf.CeilToInt((float)normalized * 1000);
                    GameManager.instance.AvgInputOffset = offset;
                    state = System.Math.Max(-1.0, System.Math.Min(1.0, state));
                    OnHit(this, (float) state);

                    CleanUp();
                    if (countsForAccuracy && !(noAutoplay || autoplayOnly) && isEligible)
                    {
                        GameManager.instance.ScoreInputAccuracy(TimeToAccuracy(time), time > 1.0, time);
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
                } else
                {
                   Blank();
                }
            }
        }

        double TimeToAccuracy(double time)
        {
            if (time >= Minigame.AceStartTime() && time <= Minigame.AceEndTime())
            {
                // Ace
                return 1.0;
            }

            double state = 0;
            if (time >= Minigame.PerfectTime() && time <= Minigame.LateTime())
            {
                // Good Hit
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.AceEndTime()) / (Minigame.LateTime() - Minigame.AceEndTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.PerfectTime()) / (Minigame.AceStartTime() - Minigame.PerfectTime()));
                    state *= 1.0 - Minigame.rankHiThreshold;
                    state += Minigame.rankHiThreshold;
                }
            }
            else
            {
                if (time > 1.0)
                {
                    // late half of timing window
                    state = 1.0 - ((time - Minigame.LateTime()) / (Minigame.EndTime() - Minigame.LateTime()));
                    state *= Minigame.rankOkThreshold;
                }
                else
                {
                    //early half of timing window
                    state = ((time - Minigame.PerfectTime()) / (Minigame.AceStartTime() - Minigame.PerfectTime()));
                    state *= Minigame.rankOkThreshold;
                }
            }
            return state;
        }

        public void Miss()
        {
            if (OnMiss != null && enabled && !autoplayOnly)
            {
                OnMiss(this);
            }

            CleanUp();
            if (countsForAccuracy && !(noAutoplay || autoplayOnly))
            {
                GameManager.instance.ScoreInputAccuracy(0, true, 2.0, 1.0, false);
                GoForAPerfect.instance.Miss();
                SectionMedalsManager.instance.MakeIneligible();
            }
        }

        public void Blank()
        {
            if(OnBlank != null && enabled && !autoplayOnly)
            {
                OnBlank(this);
            }
        }

        public void CleanUp()
        {
            if (markForDeletion) return;
            allEvents.Remove(this);
            OnDestroy(this);
            markForDeletion = true;
        }
    }
}